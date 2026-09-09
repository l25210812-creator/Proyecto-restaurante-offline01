using RestauranteApp.Domain.Exceptions;
using RestauranteApp.Domain.Services;
using RestauranteApp.Shared.Dtos;
using RestauranteApp.Shared.Enums;
using RestauranteApp.Tests.Fakes;

namespace RestauranteApp.Tests;

/// <summary>
/// Cubre el flujo principal del sistema: enviar una orden debe (a) guardarla en base de
/// datos y (b) generar una comanda por estación para imprimir, en la misma operación, y
/// debe respetar el bloqueo optimista si dos terminales chocan sobre la misma orden.
/// </summary>
public class OrdenServiceTests
{
    private static (OrdenService servicio, FakeDatabaseStore store, FakeImpresionService impresion, FakeNotificadorTiempoReal notificador)
        CrearServicio(FakeDatabaseStore? storeExistente = null)
    {
        var store = storeExistente ?? EscenarioPruebaFactory.CrearStoreConDatosBase();
        var unitOfWork = new FakeUnitOfWork(store);
        var impresion = new FakeImpresionService();
        var notificador = new FakeNotificadorTiempoReal();
        var servicio = new OrdenService(unitOfWork, impresion, notificador);
        return (servicio, store, impresion, notificador);
    }

    [Fact]
    public async Task EnviarOrden_GuardaEnBaseDeDatosYGeneraUnaComandaPorEstacion()
    {
        var (servicio, store, impresion, notificador) = CrearServicio();

        var items = new List<NuevoItemOrdenRequest>
        {
            new(EscenarioPruebaFactory.ItemTacosId, 2, "Sin cebolla", Array.Empty<int>()),
            new(EscenarioPruebaFactory.ItemCervezaId, 1, null, Array.Empty<int>())
        };

        var (orden, comandas) = await servicio.EnviarOrdenAsync(
            EscenarioPruebaFactory.MesaId, EscenarioPruebaFactory.MeseroId, items, rowVersionEsperado: null);

        // (a) quedó guardada en el "servidor" (nuestro store fake hace las veces de BD)
        Assert.True(store.Ordenes.ContainsKey(orden.Id));
        Assert.Equal(EstadoOrden.EnviadaACocina, store.Ordenes[orden.Id].Estado);
        Assert.Equal(EstadoMesa.Ocupada, store.Mesas[EscenarioPruebaFactory.MesaId].Estado);

        // (b) una comanda por estación: cocina (tacos) y bar (cerveza)
        Assert.Equal(2, comandas.Count);
        Assert.Contains(comandas, c => c.Estacion == EstacionPreparacion.Cocina && c.Items.Count == 1);
        Assert.Contains(comandas, c => c.Estacion == EstacionPreparacion.Bar && c.Items.Count == 1);

        // se intentó imprimir cada una
        Assert.Equal(2, impresion.ComandasImpresas.Count);
        Assert.Equal(2, notificador.ComandasNotificadas);
        Assert.True(notificador.MesasNotificadas >= 1);
    }

    [Fact]
    public async Task EnviarOrden_SiLaImpresionFalla_LaOrdenIgualQuedaGuardada()
    {
        var store = EscenarioPruebaFactory.CrearStoreConDatosBase();
        var unitOfWork = new FakeUnitOfWork(store);
        var impresionQueFalla = new FakeImpresionService { FallarSiempre = true };
        var notificador = new FakeNotificadorTiempoReal();
        var servicio = new OrdenService(unitOfWork, impresionQueFalla, notificador);

        var items = new List<NuevoItemOrdenRequest> { new(EscenarioPruebaFactory.ItemTacosId, 1, null, Array.Empty<int>()) };

        // No debe lanzar aunque la "impresora" esté simulada como apagada.
        var (orden, comandas) = await servicio.EnviarOrdenAsync(
            EscenarioPruebaFactory.MesaId, EscenarioPruebaFactory.MeseroId, items, rowVersionEsperado: null);

        Assert.True(store.Ordenes.ContainsKey(orden.Id));
        Assert.Single(comandas);
        Assert.Empty(impresionQueFalla.ComandasImpresas); // no se registró como impresa
    }

    [Fact]
    public async Task EnviarOrden_ConRowVersionDesactualizado_LanzaConflictoDeConcurrencia()
    {
        var store = EscenarioPruebaFactory.CrearStoreConDatosBase();

        // Dos terminales (dos OrdenService independientes) que comparten el mismo "servidor".
        var (servicioMesero1, _, _, _) = CrearServicio(store);
        var (servicioMesero2, _, _, _) = CrearServicio(store);

        var primerEnvio = new List<NuevoItemOrdenRequest> { new(EscenarioPruebaFactory.ItemTacosId, 1, null, Array.Empty<int>()) };

        // Mesero 1 abre la orden y la envía primero.
        var (ordenTrasPrimerEnvio, _) = await servicioMesero1.EnviarOrdenAsync(
            EscenarioPruebaFactory.MesaId, EscenarioPruebaFactory.MeseroId, primerEnvio, rowVersionEsperado: null);

        // Mesero 2 ya tenía cargada la orden ANTES del envío de Mesero 1 (RowVersion viejo)
        // y ahora intenta agregar algo con ese RowVersion desactualizado.
        var segundoEnvio = new List<NuevoItemOrdenRequest> { new(EscenarioPruebaFactory.ItemCervezaId, 1, null, Array.Empty<int>()) };
        var rowVersionViejo = new byte[16]; // distinto al que ya quedó guardado tras el primer envío

        await Assert.ThrowsAsync<ConflictoConcurrenciaException>(() =>
            servicioMesero2.EnviarOrdenAsync(
                EscenarioPruebaFactory.MesaId, EscenarioPruebaFactory.MeseroId, segundoEnvio, rowVersionViejo));

        // La orden en el "servidor" conserva únicamente lo del primer envío.
        var ordenFinal = store.Ordenes[ordenTrasPrimerEnvio.Id];
        Assert.Single(ordenFinal.Items);
    }
}
