using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Shared.Dtos;

public record OrdenDto(
    int Id,
    int Folio,
    int MesaId,
    string MesaNombre,
    int MeseroId,
    string MeseroNombre,
    EstadoOrden Estado,
    DateTime HoraApertura,
    IReadOnlyList<ItemOrdenDto> Items,
    byte[] RowVersion
);

/// <summary>Lo que manda el cliente (mesero) al presionar "Enviar a cocina".</summary>
public record NuevoItemOrdenRequest(int ItemMenuId, int Cantidad, string? Notas, IReadOnlyList<int> ModificadorIds);

public record EnviarOrdenRequest(int? OrdenId, int MesaId, int MeseroId, IReadOnlyList<NuevoItemOrdenRequest> Items, byte[]? RowVersion);

/// <summary>Resultado del envío: confirma qué se guardó en BD y qué pasó con la impresión de cada estación.</summary>
public record EnviarOrdenResultado(OrdenDto Orden, IReadOnlyList<ComandaDto> ComandasGeneradas);
