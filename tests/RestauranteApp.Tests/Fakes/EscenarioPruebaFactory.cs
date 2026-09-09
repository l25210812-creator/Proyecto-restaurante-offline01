using RestauranteApp.Domain.Entities;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Tests.Fakes;

/// <summary>Arma un escenario mínimo (una mesa libre + dos ítems de menú) reutilizable entre pruebas.</summary>
public static class EscenarioPruebaFactory
{
    public const int MesaId = 1;
    public const int MeseroId = 1;
    public const int ItemTacosId = 10;
    public const int ItemCervezaId = 20;

    public static FakeDatabaseStore CrearStoreConDatosBase()
    {
        var store = new FakeDatabaseStore();

        store.Mesas[MesaId] = new Mesa { Id = MesaId, Nombre = "M1", Capacidad = 4, Estado = EstadoMesa.Libre };

        store.Meseros[MeseroId] = new Mesero
        {
            Id = MeseroId,
            Nombre = "Mesero de prueba",
            NumeroMesero = 2,
            Rol = RolUsuario.Mesero,
            PinHash = "irrelevante-en-esta-prueba"
        };

        store.ItemsMenu[ItemTacosId] = new ItemMenu
        {
            Id = ItemTacosId,
            Nombre = "Tacos de asada",
            Precio = 140,
            Estacion = EstacionPreparacion.Cocina,
            Disponible = true
        };

        store.ItemsMenu[ItemCervezaId] = new ItemMenu
        {
            Id = ItemCervezaId,
            Nombre = "Cerveza",
            Precio = 60,
            Estacion = EstacionPreparacion.Bar,
            Disponible = true
        };

        return store;
    }
}
