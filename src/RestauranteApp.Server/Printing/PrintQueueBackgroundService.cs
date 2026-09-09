using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RestauranteApp.Data;
using RestauranteApp.Domain.Entities;
using RestauranteApp.Domain.Exceptions;
using RestauranteApp.Domain.Interfaces;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Server.Printing;

/// <summary>
/// Cola de reintentos de impresión. Corre en segundo plano en el servidor y cada cierto
/// intervalo busca comandas/tickets que quedaron en estado "Reintentando"/"Fallido" (porque
/// la impresora estaba apagada o sin papel) y vuelve a intentarlos, hasta un máximo de
/// intentos configurado. La orden y el cobro YA estaban guardados en base de datos desde el
/// primer intento -- esto solo resuelve la parte de "que salga el papel".
/// </summary>
public class PrintQueueBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ImpresorasOptions _opciones;
    private readonly ILogger<PrintQueueBackgroundService> _logger;

    public PrintQueueBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<ImpresorasOptions> opciones,
        ILogger<PrintQueueBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _opciones = opciones.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalo = TimeSpan.FromSeconds(Math.Max(5, _opciones.IntervaloReintentoSegundos));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ReintentarPendientesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en la cola de reintentos de impresión.");
            }

            try
            {
                await Task.Delay(intervalo, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // El servidor se está apagando; salir del ciclo normalmente.
            }
        }
    }

    private async Task ReintentarPendientesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var impresionService = scope.ServiceProvider.GetRequiredService<IImpresionService>();
        var notificador = scope.ServiceProvider.GetRequiredService<INotificadorTiempoReal>();

        var comandasPendientes = await db.Comandas
            .Include(c => c.Orden).ThenInclude(o => o!.Mesa)
            .Include(c => c.Items)
            .Where(c => c.EstadoImpresion == EstadoImpresion.Reintentando && c.IntentosImpresion < _opciones.MaxIntentos)
            .ToListAsync(ct);

        foreach (var comanda in comandasPendientes)
        {
            try
            {
                await impresionService.EncolarComandaAsync(comanda, ct);
                await notificador.NotificarComandaRecibidaAsync(comanda, ct);
                _logger.LogInformation("Reintento de impresión exitoso para comanda #{Id}", comanda.Id);
            }
            catch (ImpresionFallidaException)
            {
                if (comanda.IntentosImpresion >= _opciones.MaxIntentos)
                {
                    comanda.EstadoImpresion = EstadoImpresion.Fallida;
                    await db.SaveChangesAsync(ct);
                    await notificador.NotificarAlertaAsync(
                        $"La comanda de la mesa {comanda.Orden?.Mesa?.Nombre} no pudo imprimirse tras {_opciones.MaxIntentos} intentos. Avisa al mesero manualmente.",
                        "error", ct);
                }
            }
        }

        var cuentasPendientes = await db.Cuentas
            .Include(c => c.Orden).ThenInclude(o => o!.Mesa)
            .Include(c => c.Orden).ThenInclude(o => o!.Mesero)
            .Include(c => c.Orden).ThenInclude(o => o!.Items)
            .Include(c => c.Pagos)
            .Where(c => c.EstadoImpresionTicket == EstadoImpresionTicket.Fallido)
            .ToListAsync(ct);

        foreach (var cuenta in cuentasPendientes)
        {
            try
            {
                await impresionService.EncolarTicketCuentaAsync(cuenta, ct);
                _logger.LogInformation("Reintento de impresión de ticket exitoso para cuenta #{Id}", cuenta.Id);
            }
            catch (ImpresionFallidaException)
            {
                // Se queda en Fallido; el siguiente ciclo lo vuelve a intentar. La caja puede
                // reimprimir manualmente desde la vista de cuenta si es urgente.
            }
        }
    }
}
