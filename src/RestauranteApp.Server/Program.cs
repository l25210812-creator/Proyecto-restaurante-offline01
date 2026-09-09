using Microsoft.EntityFrameworkCore;
using RestauranteApp.Data;
using RestauranteApp.Domain.Interfaces;
using RestauranteApp.Domain.Services;
using RestauranteApp.Server.Endpoints;
using RestauranteApp.Server.Hubs;
using RestauranteApp.Server.Printing;
using RestauranteApp.Server.Realtime;
using RestauranteApp.Shared.Realtime;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Base de datos: por defecto SQLite (cero instalación en el servidor). Para
// usar SQL Server Express, cambia "DatabaseProvider" a "SqlServer" en
// appsettings.json y ajusta "ConnectionStrings:Default" -- el resto del
// código (repositorios, servicios, endpoints) no cambia.
// ---------------------------------------------------------------------------
var proveedorBd = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "Sqlite";
var cadenaConexion = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=restaurante.db";

builder.Services.AddDbContext<AppDbContext>(opciones =>
{
    if (proveedorBd.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        opciones.UseSqlServer(cadenaConexion);
    else
        opciones.UseSqlite(cadenaConexion);
});

builder.Services.Configure<ImpresorasOptions>(builder.Configuration.GetSection(ImpresorasOptions.Seccion));

// ---- Dominio / aplicación ----
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IImpresionService, ImpresionService>();
builder.Services.AddScoped<INotificadorTiempoReal, SignalROrdenNotificador>();
builder.Services.AddScoped<IEscPosPrinterClient, EscPosTcpPrinterClient>();

builder.Services.AddScoped<OrdenService>();
builder.Services.AddScoped<MesaService>();
builder.Services.AddScoped<CuentaService>();
builder.Services.AddScoped<AutenticacionService>();

// Trabajo en segundo plano que reintenta comandas/tickets que no se pudieron imprimir.
builder.Services.AddHostedService<PrintQueueBackgroundService>();

// ---- SignalR (tiempo real dentro de la LAN, sin Internet) ----
builder.Services.AddSignalR();

// Las terminales (WPF) en otras PCs de la LAN llaman a este servidor por IP:puerto;
// se permite cualquier origen dentro de la red local ya que no hay exposición a Internet.
builder.Services.AddCors(opciones =>
{
    opciones.AddDefaultPolicy(politica =>
        politica.SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.InicializarAsync(db);
}

app.UseCors();

app.MapAuthEndpoints();
app.MapMesasEndpoints();
app.MapMenuEndpoints();
app.MapOrdenesEndpoints();
app.MapCuentasEndpoints();

app.MapHub<OrdenHub>(HubRoutes.OrdenHub);

app.MapGet("/", () => Results.Ok(new { servicio = "RestauranteApp.Server", estado = "activo" }));

app.Run();
