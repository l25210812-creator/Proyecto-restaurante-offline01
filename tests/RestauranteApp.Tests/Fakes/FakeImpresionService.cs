using RestauranteApp.Domain.Entities;
using RestauranteApp.Domain.Exceptions;
using RestauranteApp.Domain.Interfaces;

namespace RestauranteApp.Tests.Fakes;

/// <summary>Registra qué se "imprimió" y permite simular una impresora caída sin tocar hardware/red.</summary>
public class FakeImpresionService : IImpresionService
{
    public List<Comanda> ComandasImpresas { get; } = new();
    public List<Cuenta> TicketsImpresos { get; } = new();
    public bool FallarSiempre { get; set; }

    public Task EncolarComandaAsync(Comanda comanda, CancellationToken ct = default)
    {
        if (FallarSiempre)
            throw new ImpresionFallidaException("Impresora simulada apagada.");

        ComandasImpresas.Add(comanda);
        return Task.CompletedTask;
    }

    public Task EncolarTicketCuentaAsync(Cuenta cuenta, CancellationToken ct = default)
    {
        if (FallarSiempre)
            throw new ImpresionFallidaException("Impresora de caja simulada apagada.");

        TicketsImpresos.Add(cuenta);
        return Task.CompletedTask;
    }
}
