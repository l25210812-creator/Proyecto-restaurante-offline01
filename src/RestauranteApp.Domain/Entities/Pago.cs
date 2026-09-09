using RestauranteApp.Domain.Common;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Domain.Entities;

public class Pago : EntidadBase
{
    public int CuentaId { get; set; }
    public Cuenta? Cuenta { get; set; }

    public MetodoPago Metodo { get; set; }
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
