namespace RestauranteApp.Server.Printing;

/// <summary>
/// Envía bytes ESC/POS crudos a una impresora térmica de red (protocolo RAW/JetDirect,
/// puerto 9100 típico en impresoras Epson TM-T20/TM-T88 y similares). Aislado en su propia
/// interfaz para poder sustituirlo fácilmente por USB/serial en el futuro sin tocar
/// ImpresionService.
/// </summary>
public interface IEscPosPrinterClient
{
    Task ImprimirAsync(ImpresoraEndpoint impresora, byte[] datosEscPos, CancellationToken ct = default);
}
