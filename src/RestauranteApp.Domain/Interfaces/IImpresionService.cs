using RestauranteApp.Domain.Entities;

namespace RestauranteApp.Domain.Interfaces;

/// <summary>
/// Abstracción de impresión ESC/POS. La implementación real (TCP/USB hacia la impresora
/// térmica) vive en el proyecto Server; Domain solo conoce este contrato para poder
/// invocar la impresión desde OrdenService sin acoplarse a hardware.
/// </summary>
public interface IImpresionService
{
    /// <summary>
    /// Encola la impresión de una comanda en la estación correspondiente. No lanza
    /// excepción si la impresora falla: en su lugar deja la Comanda en estado
    /// Reintentando y un servicio en background (PrintQueueBackgroundService) reintenta.
    /// </summary>
    Task EncolarComandaAsync(Comanda comanda, CancellationToken ct = default);

    /// <summary>Encola la impresión del ticket final de cuenta en la impresora de caja.</summary>
    Task EncolarTicketCuentaAsync(Cuenta cuenta, CancellationToken ct = default);
}
