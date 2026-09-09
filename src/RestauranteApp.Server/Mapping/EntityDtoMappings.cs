using RestauranteApp.Domain.Entities;
using RestauranteApp.Shared.Dtos;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Server.Mapping;

/// <summary>
/// Mapeos entidad -> DTO. Deliberadamente simples (sin AutoMapper) para el MVP: son pocas
/// entidades y mantenerlos explícitos hace evidente qué datos cruzan hacia los clientes.
/// </summary>
public static class EntityDtoMappings
{
    public static MesaDto ToDto(this Mesa mesa) => new(
        mesa.Id,
        mesa.Nombre,
        mesa.ZonaId,
        mesa.Zona?.Nombre ?? string.Empty,
        mesa.Capacidad,
        mesa.Estado,
        mesa.OrdenActivaId,
        mesa.OrdenActiva?.HoraApertura);

    public static ZonaDto ToDto(this Zona zona) => new(
        zona.Id,
        zona.Nombre,
        zona.Mesas.Select(m => m.ToDto()).ToList());

    public static ItemOrdenDto ToDto(this ItemOrden item) => new(
        item.Id,
        item.ItemMenuId,
        item.NombreItem,
        item.Cantidad,
        item.Notas,
        item.Estacion,
        item.Estado,
        item.PrecioUnitario,
        string.IsNullOrEmpty(item.ModificadorIdsCsv)
            ? Array.Empty<int>()
            : item.ModificadorIdsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList());

    public static ComandaDto ToDto(this Comanda comanda) => new(
        comanda.Id,
        comanda.OrdenId,
        comanda.Orden?.MesaId ?? 0,
        comanda.Orden?.Mesa?.Nombre ?? string.Empty,
        comanda.Estacion,
        comanda.HoraEnvio,
        comanda.EstadoImpresion,
        comanda.Items.Select(i => i.ToDto()).ToList());

    public static OrdenDto ToDto(this Orden orden) => new(
        orden.Id,
        orden.Folio,
        orden.MesaId,
        orden.Mesa?.Nombre ?? string.Empty,
        orden.MeseroId,
        orden.Mesero?.Nombre ?? string.Empty,
        orden.Estado,
        orden.HoraApertura,
        orden.Items.Select(i => i.ToDto()).ToList(),
        orden.RowVersion);

    public static ModificadorItemDto ToDto(this ModificadorItem m) => new(m.Id, m.Nombre, m.PrecioExtra);

    public static ItemMenuDto ToDto(this ItemMenu item) => new(
        item.Id,
        item.Nombre,
        item.Descripcion,
        item.Precio,
        item.Estacion,
        item.Disponible,
        item.Modificadores.Select(m => m.ToDto()).ToList(),
        string.IsNullOrEmpty(item.Alergenos)
            ? Array.Empty<string>()
            : item.Alergenos.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList());

    public static CategoriaMenuDto ToDto(this CategoriaMenu categoria) => new(
        categoria.Id,
        categoria.Nombre,
        categoria.Orden,
        categoria.Items.Select(i => i.ToDto()).ToList());

    public static TicketCuentaDto ToTicketDto(this Cuenta cuenta) => new(
        cuenta.OrdenId,
        cuenta.Orden?.Folio ?? 0,
        cuenta.Orden?.Mesa?.Nombre ?? string.Empty,
        cuenta.Orden?.Mesero?.Nombre ?? string.Empty,
        cuenta.Orden?.Items.Select(i => i.ToDto()).ToList() ?? new List<ItemOrdenDto>(),
        cuenta.Subtotal,
        cuenta.Impuestos,
        cuenta.Propina,
        cuenta.Total,
        cuenta.Pagos.FirstOrDefault()?.Metodo ?? MetodoPago.Efectivo,
        cuenta.HoraCobro ?? DateTime.UtcNow);
}
