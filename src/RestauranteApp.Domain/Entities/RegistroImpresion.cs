using RestauranteApp.Domain.Common;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Domain.Entities;

/// <summary>
/// Bitácora de cada intento de impresión (comanda o ticket), independiente de la entidad
/// de negocio, para poder auditar fallas de impresoras a lo largo del tiempo.
/// </summary>
public class RegistroImpresion : EntidadBase
{
    public TipoDocumentoImpreso Tipo { get; set; }

    /// <summary>Id de la Comanda o de la Cuenta, según Tipo.</summary>
    public int ReferenciaId { get; set; }

    public string NombreImpresora { get; set; } = string.Empty;
    public DateTime FechaIntento { get; set; } = DateTime.UtcNow;
    public bool Exitoso { get; set; }
    public string? Error { get; set; }
}
