using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RestauranteApp.Data;
using RestauranteApp.Domain.Entities;
using RestauranteApp.Domain.Exceptions;
using RestauranteApp.Domain.Interfaces;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Server.Printing;

/// <summary>
/// Implementación de IImpresionService (contrato de Domain). Resuelve la impresora física
/// según la estación, arma los bytes ESC/POS y actualiza el estado de impresión de la
/// Comanda/Cuenta en la MISMA base de datos y (normalmente) el mismo DbContext que ya usó
/// OrdenService/CuentaService para guardar la orden -- por eso puede simplemente mutar la
/// entidad recibida y volver a guardar, sin duplicar la orden.
/// </summary>
public class ImpresionService : IImpresionService
{
    private readonly AppDbContext _db;
    private readonly IEscPosPrinterClient _printerClient;
    private readonly ImpresorasOptions _opciones;
    private readonly ILogger<ImpresionService> _logger;

    public ImpresionService(
        AppDbContext db,
        IEscPosPrinterClient printerClient,
        IOptions<ImpresorasOptions> opciones,
        ILogger<ImpresionService> logger)
    {
        _db = db;
        _printerClient = printerClient;
        _opciones = opciones.Value;
        _logger = logger;
    }

    public async Task EncolarComandaAsync(Comanda comanda, CancellationToken ct = default)
    {
        var impresora = comanda.Estacion == EstacionPreparacion.Cocina ? _opciones.Cocina : _opciones.Bar;
        var datos = EscPosCommandBuilder.ConstruirComanda(comanda);

        try
        {
            await _printerClient.ImprimirAsync(impresora, datos, ct);
            comanda.EstadoImpresion = EstadoImpresion.Impresa;
            comanda.UltimoErrorImpresion = null;
            RegistrarIntento(TipoDocumentoImpreso.Comanda, comanda.Id, impresora.Nombre, exitoso: true, error: null);
        }
        catch (ImpresionFallidaException ex)
        {
            comanda.EstadoImpresion = EstadoImpresion.Reintentando;
            comanda.UltimoErrorImpresion = ex.Message;
            RegistrarIntento(TipoDocumentoImpreso.Comanda, comanda.Id, impresora.Nombre, exitoso: false, error: ex.Message);

            comanda.IntentosImpresion++;
            comanda.UltimoIntentoImpresion = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            throw;
        }

        comanda.IntentosImpresion++;
        comanda.UltimoIntentoImpresion = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task EncolarTicketCuentaAsync(Cuenta cuenta, CancellationToken ct = default)
    {
        var nombreRestaurante = (await _db.Restaurantes.FirstOrDefaultAsync(ct))?.Nombre ?? "Restaurante";
        var datos = EscPosCommandBuilder.ConstruirTicketCuenta(cuenta, nombreRestaurante);

        try
        {
            await _printerClient.ImprimirAsync(_opciones.Caja, datos, ct);
            cuenta.EstadoImpresionTicket = EstadoImpresionTicket.Impreso;
            RegistrarIntento(TipoDocumentoImpreso.TicketCuenta, cuenta.Id, _opciones.Caja.Nombre, exitoso: true, error: null);
        }
        catch (ImpresionFallidaException ex)
        {
            cuenta.EstadoImpresionTicket = EstadoImpresionTicket.Fallido;
            RegistrarIntento(TipoDocumentoImpreso.TicketCuenta, cuenta.Id, _opciones.Caja.Nombre, exitoso: false, error: ex.Message);
            await _db.SaveChangesAsync(ct);
            throw;
        }

        await _db.SaveChangesAsync(ct);
    }

    private void RegistrarIntento(TipoDocumentoImpreso tipo, int referenciaId, string impresora, bool exitoso, string? error)
    {
        _db.RegistrosImpresion.Add(new RegistroImpresion
        {
            Tipo = tipo,
            ReferenciaId = referenciaId,
            NombreImpresora = impresora,
            Exitoso = exitoso,
            Error = error
        });

        if (!exitoso)
            _logger.LogWarning("Fallo de impresión en {Impresora} ({Tipo} #{Id}): {Error}", impresora, tipo, referenciaId, error);
    }
}
