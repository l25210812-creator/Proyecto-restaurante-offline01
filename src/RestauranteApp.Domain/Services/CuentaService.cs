using RestauranteApp.Domain.Entities;
using RestauranteApp.Domain.Interfaces;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Domain.Services;

/// <summary>
/// Cierre de cuenta: calcula el total, registra el/los pago(s) y dispara la impresión
/// del ticket final en la impresora de caja — este paso es independiente y posterior
/// a las comandas de cocina/bar, y su resultado es lo que se le entrega al cliente.
/// </summary>
public class CuentaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImpresionService _impresionService;
    private readonly INotificadorTiempoReal _notificador;
    private readonly decimal _porcentajeImpuesto;

    public CuentaService(
        IUnitOfWork unitOfWork,
        IImpresionService impresionService,
        INotificadorTiempoReal notificador,
        decimal porcentajeImpuesto = 0.08m)
    {
        _unitOfWork = unitOfWork;
        _impresionService = impresionService;
        _notificador = notificador;
        _porcentajeImpuesto = porcentajeImpuesto;
    }

    public async Task<Cuenta> CerrarCuentaAsync(
        int ordenId,
        MetodoPago metodoPago,
        decimal propina,
        CancellationToken ct = default)
    {
        var orden = await _unitOfWork.Ordenes.ObtenerConItemsAsync(ordenId, ct)
            ?? throw new InvalidOperationException($"No existe la orden {ordenId}.");

        if (orden.Estado == EstadoOrden.Cobrada)
            throw new InvalidOperationException("Esta orden ya fue cobrada.");

        var subtotal = orden.Items
            .Where(i => i.Estado != EstadoItemOrden.Cancelado)
            .Sum(i => i.PrecioUnitario * i.Cantidad);
        var impuestos = Math.Round(subtotal * _porcentajeImpuesto, 2);
        var total = subtotal + impuestos + propina;

        var cuenta = new Cuenta
        {
            OrdenId = orden.Id,
            Subtotal = subtotal,
            Impuestos = impuestos,
            Propina = propina,
            Total = total,
            HoraCobro = DateTime.UtcNow,
            Pagos = new List<Pago> { new() { Metodo = metodoPago, Monto = total, Fecha = DateTime.UtcNow } }
        };

        orden.Cuenta = cuenta;
        orden.Estado = EstadoOrden.Cobrada;
        orden.HoraCierre = DateTime.UtcNow;

        var mesa = await _unitOfWork.Mesas.ObtenerPorIdAsync(orden.MesaId, ct);
        if (mesa is not null)
        {
            mesa.Estado = EstadoMesa.EnLimpieza;
            mesa.OrdenActivaId = null;
            await _unitOfWork.Mesas.ActualizarAsync(mesa, ct);
        }

        _unitOfWork.Ordenes.MarcarModificada(orden);
        await _unitOfWork.GuardarCambiosAsync(ct);

        // El ticket físico para el cliente se imprime después de confirmar el cobro en BD.
        try
        {
            await _impresionService.EncolarTicketCuentaAsync(cuenta, ct);
        }
        catch
        {
            // Igual que con las comandas: el cobro ya quedó registrado; la impresión
            // del ticket entra a su propia cola de reintentos si la impresora de caja falla.
        }

        await _notificador.NotificarCuentaCerradaAsync(orden.Id, orden.MesaId, ct);
        if (mesa is not null)
            await _notificador.NotificarMesaActualizadaAsync(mesa, ct);

        return cuenta;
    }
}
