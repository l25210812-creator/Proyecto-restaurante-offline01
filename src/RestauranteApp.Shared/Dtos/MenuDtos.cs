using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Shared.Dtos;

public record ModificadorItemDto(int Id, string Nombre, decimal PrecioExtra);

public record ItemMenuDto(
    int Id,
    string Nombre,
    string? Descripcion,
    decimal Precio,
    EstacionPreparacion Estacion,
    bool Disponible,
    IReadOnlyList<ModificadorItemDto> Modificadores,
    IReadOnlyList<string> Alergenos
);

public record CategoriaMenuDto(int Id, string Nombre, int Orden, IReadOnlyList<ItemMenuDto> Items);
