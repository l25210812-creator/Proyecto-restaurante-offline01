using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Shared.Dtos;

/// <summary>Representación de una mesa para pintar el mapa de zonas en el cliente.</summary>
public record MesaDto(
    int Id,
    string Nombre,
    int ZonaId,
    string ZonaNombre,
    int Capacidad,
    EstadoMesa Estado,
    int? OrdenActivaId,
    DateTime? HoraAperturaOrden
);

public record ZonaDto(int Id, string Nombre, IReadOnlyList<MesaDto> Mesas);
