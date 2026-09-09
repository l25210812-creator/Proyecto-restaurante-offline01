using RestauranteApp.Domain.Common;

namespace RestauranteApp.Domain.Entities;

/// <summary>La cuenta/cobro de una orden. Un ticket final se imprime a partir de aquí, en caja.</summary>
public class Cuenta : EntidadBase
{
    public int OrdenId { get; set; }
    public Orden? Orden { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Propina { get; set; }
    public decimal Total { get; set; }

    public DateTime? HoraCobro { get; set; }

    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public EstadoImpresionTicket EstadoImpresionTicket { get; set; } = EstadoImpresionTicket.Pendiente;
}

public enum EstadoImpresionTicket
{
    Pendiente = 0,
    Impreso = 1,
    Fallido = 2
}
