using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Shared.Dtos;

public record ItemOrdenDto(
    int Id,
    int ItemMenuId,
    string NombreItem,
    int Cantidad,
    string? Notas,
    EstacionPreparacion Estacion,
    EstadoItemOrden Estado,
    decimal PrecioUnitario,
    IReadOnlyList<int> ModificadorIds
);
