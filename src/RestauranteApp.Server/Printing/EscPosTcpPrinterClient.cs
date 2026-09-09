using System.Net.Sockets;
using Microsoft.Extensions.Options;
using RestauranteApp.Domain.Exceptions;

namespace RestauranteApp.Server.Printing;

/// <summary>
/// Implementación real: abre un socket TCP a la impresora y le manda los bytes ESC/POS
/// directamente (así trabajan la mayoría de impresoras térmicas de cocina/bar en LAN).
/// </summary>
public class EscPosTcpPrinterClient : IEscPosPrinterClient
{
    private readonly ImpresorasOptions _opciones;
    private readonly ILogger<EscPosTcpPrinterClient> _logger;

    public EscPosTcpPrinterClient(IOptions<ImpresorasOptions> opciones, ILogger<EscPosTcpPrinterClient> logger)
    {
        _opciones = opciones.Value;
        _logger = logger;
    }

    public async Task ImprimirAsync(ImpresoraEndpoint impresora, byte[] datosEscPos, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(impresora.Host))
            throw new ImpresionFallidaException($"La impresora '{impresora.Nombre}' no tiene IP configurada.");

        using var cliente = new TcpClient();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_opciones.TimeoutMs);

        try
        {
            await cliente.ConnectAsync(impresora.Host, impresora.Puerto, cts.Token);
            await using var stream = cliente.GetStream();
            await stream.WriteAsync(datosEscPos, cts.Token);
            await stream.FlushAsync(cts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new ImpresionFallidaException(
                $"Tiempo de espera agotado al conectar con la impresora '{impresora.Nombre}' ({impresora.Host}:{impresora.Puerto}). ¿Está encendida y en la red?");
        }
        catch (SocketException ex)
        {
            _logger.LogWarning(ex, "No se pudo imprimir en {Impresora}", impresora.Nombre);
            throw new ImpresionFallidaException(
                $"No se pudo conectar con la impresora '{impresora.Nombre}' ({impresora.Host}:{impresora.Puerto}).", ex);
        }
    }
}
