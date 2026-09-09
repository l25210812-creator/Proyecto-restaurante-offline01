using RestauranteApp.Domain.Entities;
using RestauranteApp.Domain.Exceptions;
using RestauranteApp.Domain.Interfaces;
using RestauranteApp.Shared.Dtos;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Domain.Services;

/// <summary>
/// Corazón del flujo principal descrito en el prompt del proyecto:
///   mesero (terminal táctil) → arma la orden → presiona "Enviar a cocina"
///     → (a) se guarda/actualiza en la base de datos del servidor
///     → (b) se imprime automáticamente en la estación correspondiente (cocina/bar)
///   ambas cosas se disparan desde la misma acción; si la impresora falla, el pedido
///   YA quedó guardado en base de datos y solo la impresión entra a cola de reintentos.
///   Al cobrar, se genera e imprime el ticket final en caja, para el cliente.
/// </summary>
public class OrdenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImpresionService _impresionService;
    private readonly INotificadorTiempoReal _notificador;

    public OrdenService(IUnitOfWork unitOfWork, IImpresionService impresionService, INotificadorTiempoReal notificador)
    {
        _unitOfWork = unitOfWork;
        _impresionService = impresionService;
        _notificador = notificador;
    }

    /// <summary>
    /// Abre una orden en la mesa si no existe una activa, agrega los ítems nuevos que
    /// trae el mesero desde su terminal táctil y ejecuta el envío: guarda en BD e
    /// imprime comandas por estación en la misma operación lógica.
    /// </summary>
    /// <param name="rowVersionEsperado">
    /// RowVersion que el cliente tenía al momento de armar la orden. Si alguien más
    /// modificó la orden mientras tanto, EF Core detecta el choque al guardar y este
    /// método propaga ConflictoConcurrenciaException para que el mesero recargue y reintente.
    /// </param>
    public async Task<(Orden Orden, IReadOnlyList<Comanda> ComandasGeneradas)> EnviarOrdenAsync(
        int mesaId,
        int meseroId,
        IReadOnlyList<NuevoItemOrdenRequest> itemsNuevos,
        byte[]? rowVersionEsperado,
        CancellationToken ct = default)
    {
        if (itemsNuevos.Count == 0)
            throw new InvalidOperationException("La orden debe incluir al menos un ítem para poder enviarse a cocina.");

        var mesa = await _unitOfWork.Mesas.ObtenerPorIdAsync(mesaId, ct)
            ?? throw new InvalidOperationException($"No existe la mesa {mesaId}.");

        var orden = await _unitOfWork.Ordenes.ObtenerAbiertaPorMesaAsync(mesaId, ct);

        var esOrdenNueva = orden is null;
        if (orden is null)
        {
            orden = new Orden
            {
                Folio = await _unitOfWork.Ordenes.SiguienteFolioAsync(ct),
                MesaId = mesaId,
                MeseroId = meseroId,
                Estado = EstadoOrden.Abierta,
                HoraApertura = DateTime.UtcNow
            };
        }
        else if (rowVersionEsperado is not null && !rowVersionEsperado.SequenceEqual(orden.RowVersion))
        {
            // Protección temprana además del chequeo de EF Core al guardar: evita construir
            // comandas sobre una versión de la orden que el cliente ya no tiene actualizada.
            throw new ConflictoConcurrenciaException(
                $"La orden de la mesa {mesa.Nombre} fue modificada por otra terminal. Recarga la orden e intenta de nuevo.");
        }

        var nuevosItemOrden = new List<ItemOrden>();
        foreach (var itemReq in itemsNuevos)
        {
            var itemMenu = await _unitOfWork.Menu.ObtenerItemPorIdAsync(itemReq.ItemMenuId, ct)
                ?? throw new InvalidOperationException($"El ítem de menú {itemReq.ItemMenuId} no existe.");

            if (!itemMenu.Disponible)
                throw new InvalidOperationException($"El ítem '{itemMenu.Nombre}' no está disponible en este momento.");

            nuevosItemOrden.Add(new ItemOrden
            {
                ItemMenuId = itemMenu.Id,
                NombreItem = itemMenu.Nombre,
                PrecioUnitario = itemMenu.Precio,
                Cantidad = itemReq.Cantidad,
                Notas = itemReq.Notas,
                Estacion = itemMenu.Estacion,
                Estado = EstadoItemOrden.Pendiente,
                ModificadorIdsCsv = itemReq.ModificadorIds.Count > 0 ? string.Join(',', itemReq.ModificadorIds) : null
            });
        }

        foreach (var item in nuevosItemOrden)
            orden.Items.Add(item);

        orden.Estado = EstadoOrden.EnviadaACocina;

        // Una comanda por estación (cocina/bar) que agrupa SOLO los ítems nuevos de este envío;
        // los ítems ya enviados antes no se vuelven a mandar a la impresora.
        var comandasNuevas = nuevosItemOrden
            .GroupBy(i => i.Estacion)
            .Select(grupo => new Comanda
            {
                Estacion = grupo.Key,
                HoraEnvio = DateTime.UtcNow,
                EstadoImpresion = EstadoImpresion.Pendiente,
                Items = grupo.ToList()
            })
            .ToList();

        foreach (var comanda in comandasNuevas)
            orden.Comandas.Add(comanda);

        if (esOrdenNueva)
        {
            await _unitOfWork.Ordenes.AgregarAsync(orden, ct);
            mesa.Estado = EstadoMesa.Ocupada;
            mesa.OrdenActivaId = orden.Id; // EF resuelve el Id tras SaveChanges por relación de navegación
            await _unitOfWork.Mesas.ActualizarAsync(mesa, ct);
        }
        else
        {
            _unitOfWork.Ordenes.MarcarModificada(orden);
        }

        // --- Punto de atomicidad: todo lo anterior se persiste en una sola transacción. ---
        // Si esto lanza ConflictoConcurrenciaException, NADA se guardó y NADA se imprime.
        await _unitOfWork.GuardarCambiosAsync(ct);

        // --- A partir de aquí la orden YA está seguro en base de datos. ---
        // La impresión es "best effort con reintento": si falla, no revertimos el guardado.
        foreach (var comanda in comandasNuevas)
        {
            try
            {
                await _impresionService.EncolarComandaAsync(comanda, ct);
            }
            catch (ImpresionFallidaException)
            {
                // El propio IImpresionService ya deja la comanda en EstadoImpresion.Reintentando
                // y la agrega a la cola de reintentos; no se propaga el error al mesero porque
                // su orden ya quedó registrada correctamente.
            }

            await _notificador.NotificarComandaRecibidaAsync(comanda, ct);
        }

        await _notificador.NotificarOrdenActualizadaAsync(orden, ct);
        await _notificador.NotificarMesaActualizadaAsync(mesa, ct);

        return (orden, comandasNuevas);
    }

    /// <summary>
    /// Cocina/bar usan esto para mover un ítem por su ciclo de vida (pendiente -> en
    /// preparación -> listo -> entregado), lo que a su vez notifica en tiempo real a la
    /// terminal del mesero para que sepa cuándo pasar a recoger el platillo.
    /// </summary>
    public async Task<ItemOrden> CambiarEstadoItemAsync(int itemOrdenId, EstadoItemOrden nuevoEstado, CancellationToken ct = default)
    {
        var orden = await _unitOfWork.Ordenes.ObtenerPorItemOrdenIdAsync(itemOrdenId, ct)
            ?? throw new InvalidOperationException($"No se encontró el ítem de orden {itemOrdenId}.");

        var item = orden.Items.First(i => i.Id == itemOrdenId);
        item.Estado = nuevoEstado;

        _unitOfWork.Ordenes.MarcarModificada(orden);
        await _unitOfWork.GuardarCambiosAsync(ct);

        await _notificador.NotificarItemOrdenActualizadoAsync(item, ct);
        return item;
    }

    /// <summary>Traspasa una orden abierta de una mesa a otra (ej. el cliente se cambió de lugar).</summary>
    public async Task TraspasarMesaAsync(int ordenId, int mesaDestinoId, CancellationToken ct = default)
    {
        var orden = await _unitOfWork.Ordenes.ObtenerConItemsAsync(ordenId, ct)
            ?? throw new InvalidOperationException($"No existe la orden {ordenId}.");

        var mesaOrigen = await _unitOfWork.Mesas.ObtenerPorIdAsync(orden.MesaId, ct)
            ?? throw new InvalidOperationException($"No existe la mesa origen {orden.MesaId}.");
        var mesaDestino = await _unitOfWork.Mesas.ObtenerPorIdAsync(mesaDestinoId, ct)
            ?? throw new InvalidOperationException($"No existe la mesa destino {mesaDestinoId}.");

        if (mesaDestino.Estado == EstadoMesa.Ocupada && mesaDestino.OrdenActivaId != orden.Id)
            throw new InvalidOperationException($"La mesa {mesaDestino.Nombre} ya está ocupada por otra orden.");

        var nota = $"[{DateTime.UtcNow:u}] Traspaso de mesa {mesaOrigen.Nombre} -> {mesaDestino.Nombre}";
        orden.HistorialTraspasos = string.IsNullOrEmpty(orden.HistorialTraspasos)
            ? nota
            : orden.HistorialTraspasos + Environment.NewLine + nota;

        orden.MesaId = mesaDestino.Id;

        mesaOrigen.Estado = EstadoMesa.EnLimpieza;
        mesaOrigen.OrdenActivaId = null;

        mesaDestino.Estado = EstadoMesa.Ocupada;
        mesaDestino.OrdenActivaId = orden.Id;

        _unitOfWork.Ordenes.MarcarModificada(orden);
        await _unitOfWork.Mesas.ActualizarAsync(mesaOrigen, ct);
        await _unitOfWork.Mesas.ActualizarAsync(mesaDestino, ct);

        await _unitOfWork.GuardarCambiosAsync(ct);

        await _notificador.NotificarMesaActualizadaAsync(mesaOrigen, ct);
        await _notificador.NotificarMesaActualizadaAsync(mesaDestino, ct);
        await _notificador.NotificarOrdenActualizadaAsync(orden, ct);
    }
}
