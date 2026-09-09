using RestauranteApp.Domain.Common;

namespace RestauranteApp.Domain.Entities;

/// <summary>Configuración general única del restaurante (fila única en la tabla).</summary>
public class Restaurante : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public string ZonaHoraria { get; set; } = "America/Tijuana";
    public string Moneda { get; set; } = "MXN";
    public decimal PorcentajeImpuesto { get; set; } = 0.08m;
}
