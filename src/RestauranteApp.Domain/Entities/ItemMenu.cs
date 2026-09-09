using RestauranteApp.Domain.Common;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Domain.Entities;

public class ItemMenu : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }

    /// <summary>Determina a qué impresora/panel se manda este ítem al enviarse una orden.</summary>
    public EstacionPreparacion Estacion { get; set; }

    public bool Disponible { get; set; } = true;

    /// <summary>Alérgenos separados por coma (simple para el MVP; normalizar en fase futura).</summary>
    public string? Alergenos { get; set; }

    public int CategoriaMenuId { get; set; }
    public CategoriaMenu? CategoriaMenu { get; set; }

    public ICollection<ModificadorItem> Modificadores { get; set; } = new List<ModificadorItem>();
}
