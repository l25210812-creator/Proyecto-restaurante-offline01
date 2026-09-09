using RestauranteApp.Domain.Entities;
using RestauranteApp.Domain.Interfaces;

namespace RestauranteApp.Tests.Fakes;

/// <summary>No-op que solo cuenta cuántas veces se notificó cada evento, para aserciones simples.</summary>
public class FakeNotificadorTiempoReal : INotificadorTiempoReal
{
    public int MesasNotificadas { get; private set; }
    public int ComandasNotificadas { get; private set; }
    public int OrdenesNotificadas { get; private set; }
    public int ItemsNotificados { get; private set; }
    public int CuentasCerradasNotificadas { get; private set; }
    public List<(string Mensaje, string Severidad)> Alertas { get; } = new();

    public Task NotificarMesaActualizadaAsync(Mesa mesa, CancellationToken ct = default)
    {
        MesasNotificadas++;
        return Task.CompletedTask;
    }

    public Task NotificarComandaRecibidaAsync(Comanda comanda, CancellationToken ct = default)
    {
        ComandasNotificadas++;
        return Task.CompletedTask;
    }

    public Task NotificarOrdenActualizadaAsync(Orden orden, CancellationToken ct = default)
    {
        OrdenesNotificadas++;
        return Task.CompletedTask;
    }

    public Task NotificarItemOrdenActualizadoAsync(ItemOrden item, CancellationToken ct = default)
    {
        ItemsNotificados++;
        return Task.CompletedTask;
    }

    public Task NotificarCuentaCerradaAsync(int ordenId, int mesaId, CancellationToken ct = default)
    {
        CuentasCerradasNotificadas++;
        return Task.CompletedTask;
    }

    public Task NotificarAlertaAsync(string mensaje, string severidad, CancellationToken ct = default)
    {
        Alertas.Add((mensaje, severidad));
        return Task.CompletedTask;
    }
}
