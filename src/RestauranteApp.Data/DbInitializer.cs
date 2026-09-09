using Microsoft.EntityFrameworkCore;
using RestauranteApp.Domain.Entities;
using RestauranteApp.Domain.Services;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Data;

/// <summary>
/// Crea la base de datos (o aplica migraciones pendientes) y siembra datos mínimos
/// de ejemplo la primera vez que el servidor arranca, para poder probar el flujo
/// completo (login -> mesas -> menú -> enviar orden) sin capturar nada a mano.
/// </summary>
public static class DbInitializer
{
    public static async Task InicializarAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!await db.Restaurantes.AnyAsync())
        {
            db.Restaurantes.Add(new Restaurante
            {
                Nombre = "Mi Restaurante",
                Moneda = "MXN",
                PorcentajeImpuesto = 0.08m
            });
        }

        if (!await db.Meseros.AnyAsync())
        {
            db.Meseros.AddRange(
                new Mesero { Nombre = "Admin", NumeroMesero = 1, Rol = RolUsuario.Admin, PinHash = AutenticacionService.HashPin("1234") },
                new Mesero { Nombre = "Mesero Demo", NumeroMesero = 2, Rol = RolUsuario.Mesero, PinHash = AutenticacionService.HashPin("0000") },
                new Mesero { Nombre = "Cajero Demo", NumeroMesero = 3, Rol = RolUsuario.Cajero, PinHash = AutenticacionService.HashPin("0000") }
            );
        }

        if (!await db.Zonas.AnyAsync())
        {
            var salon = new Zona
            {
                Nombre = "Salón principal",
                Orden = 1,
                Mesas = Enumerable.Range(1, 8)
                    .Select(n => new Mesa { Nombre = $"M{n}", Capacidad = 4, Estado = EstadoMesa.Libre })
                    .ToList()
            };
            var terraza = new Zona
            {
                Nombre = "Terraza",
                Orden = 2,
                Mesas = Enumerable.Range(1, 4)
                    .Select(n => new Mesa { Nombre = $"T{n}", Capacidad = 2, Estado = EstadoMesa.Libre })
                    .ToList()
            };
            db.Zonas.AddRange(salon, terraza);
        }

        if (!await db.CategoriasMenu.AnyAsync())
        {
            db.CategoriasMenu.AddRange(
                new CategoriaMenu
                {
                    Nombre = "Entradas",
                    Orden = 1,
                    Items = new List<ItemMenu>
                    {
                        new() { Nombre = "Guacamole", Precio = 85, Estacion = EstacionPreparacion.Cocina },
                        new() { Nombre = "Alitas BBQ", Precio = 120, Estacion = EstacionPreparacion.Cocina }
                    }
                },
                new CategoriaMenu
                {
                    Nombre = "Platos fuertes",
                    Orden = 2,
                    Items = new List<ItemMenu>
                    {
                        new() { Nombre = "Milanesa de pollo", Precio = 165, Estacion = EstacionPreparacion.Cocina },
                        new() { Nombre = "Tacos de asada (orden)", Precio = 140, Estacion = EstacionPreparacion.Cocina }
                    }
                },
                new CategoriaMenu
                {
                    Nombre = "Bebidas",
                    Orden = 3,
                    Items = new List<ItemMenu>
                    {
                        new() { Nombre = "Michelada", Precio = 95, Estacion = EstacionPreparacion.Bar },
                        new() { Nombre = "Refresco", Precio = 35, Estacion = EstacionPreparacion.Bar },
                        new() { Nombre = "Agua fresca", Precio = 30, Estacion = EstacionPreparacion.Bar }
                    }
                },
                new CategoriaMenu
                {
                    Nombre = "Postres",
                    Orden = 4,
                    Items = new List<ItemMenu>
                    {
                        new() { Nombre = "Flan napolitano", Precio = 60, Estacion = EstacionPreparacion.Cocina }
                    }
                }
            );
        }

        await db.SaveChangesAsync();
    }
}
