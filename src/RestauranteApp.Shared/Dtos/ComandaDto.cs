using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Shared.Dtos;

/// <summary>Un envío puntual de ítems a una estación (cocina o bar) para una orden.</summary>
public record ComandaDto(
    int Id,
    int OrdenId,
    int MesaId,
    string MesaNombre,
    EstacionPreparacion Estacion,
    DateTime HoraEnvio,
    EstadoImpresion EstadoImpresion,
    IReadOnlyList<ItemOrdenDto> Items
);

public record TicketCuentaDto(
    int OrdenId,
    int Folio,
    string MesaNombre,
    string MeseroNombre,
    IReadOnlyList<ItemOrdenDto> Items,
    decimal Subtotal,
    decimal Impuestos,
    decimal Propina,
    decimal Total,
    MetodoPago MetodoPago,
    DateTime HoraCobro
);
