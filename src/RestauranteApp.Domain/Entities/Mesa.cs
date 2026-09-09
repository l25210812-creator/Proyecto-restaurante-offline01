using RestauranteApp.Domain.Common;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Domain.Entities;

public class Mesa : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public int Capacidad { get; set; }
    public EstadoMesa Estado { get; set; } = EstadoMesa.Libre;

    public int ZonaId { get; set; }
    public Zona? Zona { get; set; }

    public int? OrdenActivaId { get; set; }
    public Orden? OrdenActiva { get; set; }
}
