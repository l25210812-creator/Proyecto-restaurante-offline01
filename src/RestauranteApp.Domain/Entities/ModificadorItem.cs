using RestauranteApp.Domain.Common;

namespace RestauranteApp.Domain.Entities;

/// <summary>Variante/extra de un ítem del menú (ej. "sin cebolla", "extra queso +$15").</summary>
public class ModificadorItem : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioExtra { get; set; }

    public int ItemMenuId { get; set; }
    public ItemMenu? ItemMenu { get; set; }
}
