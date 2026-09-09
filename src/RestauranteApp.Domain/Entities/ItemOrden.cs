using RestauranteApp.Domain.Common;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Domain.Entities;

public class ItemOrden : EntidadBase
{
    public int OrdenId { get; set; }
    public Orden? Orden { get; set; }

    public int ItemMenuId { get; set; }
    public ItemMenu? ItemMenu { get; set; }

    /// <summary>Copia del nombre/precio al momento de ordenar, para no depender de cambios futuros del menú.</summary>
    public string NombreItem { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }

    public int Cantidad { get; set; } = 1;
    public string? Notas { get; set; }

    public EstacionPreparacion Estacion { get; set; }
    public EstadoItemOrden Estado { get; set; } = EstadoItemOrden.Pendiente;

    /// <summary>Ids de ModificadorItem seleccionados, serializados simple (csv) para el MVP.</summary>
    public string? ModificadorIdsCsv { get; set; }

    public int? ComandaId { get; set; }
    public Comanda? Comanda { get; set; }
}
