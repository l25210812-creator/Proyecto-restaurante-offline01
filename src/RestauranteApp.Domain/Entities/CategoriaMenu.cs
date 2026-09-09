using RestauranteApp.Domain.Common;

namespace RestauranteApp.Domain.Entities;

public class CategoriaMenu : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }

    public ICollection<ItemMenu> Items { get; set; } = new List<ItemMenu>();
}
