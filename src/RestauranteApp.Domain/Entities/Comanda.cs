using RestauranteApp.Domain.Common;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Domain.Entities;

/// <summary>
/// Un envío puntual de ítems a una estación (cocina o bar), generado automáticamente
/// cuando el mesero presiona "Enviar". Es el registro que respalda la impresión física
/// y permite reintentar si la impresora falló, sin perder ni duplicar la orden.
/// </summary>
public class Comanda : EntidadBase
{
    public int OrdenId { get; set; }
    public Orden? Orden { get; set; }

    public EstacionPreparacion Estacion { get; set; }

    public DateTime HoraEnvio { get; set; } = DateTime.UtcNow;

    public EstadoImpresion EstadoImpresion { get; set; } = EstadoImpresion.Pendiente;
    public int IntentosImpresion { get; set; }
    public DateTime? UltimoIntentoImpresion { get; set; }
    public string? UltimoErrorImpresion { get; set; }

    public ICollection<ItemOrden> Items { get; set; } = new List<ItemOrden>();
}
