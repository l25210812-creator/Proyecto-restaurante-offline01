using RestauranteApp.Domain.Common;

namespace RestauranteApp.Domain.Entities;

/// <summary>Área física del restaurante (ej. "Terraza", "Salón principal", "Barra").</summary>
public class Zona : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }

    public ICollection<Mesa> Mesas { get; set; } = new List<Mesa>();
}
