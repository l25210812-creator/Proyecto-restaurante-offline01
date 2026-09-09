using RestauranteApp.Domain.Services;
using RestauranteApp.Shared.Dtos;
using RestauranteApp.Shared.Enums;
using RestauranteApp.Tests.Fakes;

namespace RestauranteApp.Tests;

/// <summary>
/// El cierre de cuenta calcula el total correctamente y dispara la impresión del ticket
/// final en caja -- documento independiente y posterior a las comandas de cocina/bar.
/// </summary>
public class CuentaServiceTests
{
    [Fact]
    public async Task CerrarCuenta_CalculaTotalEImprimeTicketDeCaja()
    {
        var store = EscenarioPruebaFactory.CrearStoreConDatosBase();
        var unitOfWork = new FakeUnitOfWork(store);
        var impresion = new FakeImpresionService();
        var notificador = new FakeNotificadorTiempoReal();

        var ordenService = new OrdenService(unitOfWork, impresion, notificador);
        var cuentaService = new CuentaService(unitOfWork, impresion, notificador, porcentajeImpuesto: 0.08m);

        var items = new List<NuevoItemOrdenRequest>
        {
            new(EscenarioPruebaFactory.ItemTacosId, 2, null, Array.Empty<int>()), // 2 x 140 = 280
            new(EscenarioPruebaFactory.ItemCervezaId, 1, null, Array.Empty<int>()) // 1 x 60 = 60
        };
        var (orden, _) = await ordenService.EnviarOrdenAsync(
            EscenarioPruebaFactory.MesaId, EscenarioPruebaFactory.MeseroId, items, rowVersionEsperado: null);

        var cuenta = await cuentaService.CerrarCuentaAsync(orden.Id, MetodoPago.Efectivo, propina: 20);

        Assert.Equal(340m, cuenta.Subtotal); // 280 + 60
        Assert.Equal(27.20m, cuenta.Impuestos); // 8% de 340
        Assert.Equal(20m, cuenta.Propina);
        Assert.Equal(387.20m, cuenta.Total);

        Assert.Single(impresion.TicketsImpresos);
        Assert.Equal(EstadoMesa.EnLimpieza, store.Mesas[EscenarioPruebaFactory.MesaId].Estado);
        Assert.Equal(1, notificador.CuentasCerradasNotificadas);
    }
}
