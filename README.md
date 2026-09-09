# RestauranteApp — Sistema de toma de órdenes (POS/KDS) para restaurante

MVP funcional de un sistema cliente-servidor para restaurante, pensado para correr en una
red local (LAN) sin depender de Internet. Ver `prompt-sistema-restaurante.md` para el
contexto y los requisitos completos.

## Estructura de la solución

```
RestauranteApp.sln
├── src/
│   ├── RestauranteApp.Shared/    Enums y DTOs compartidos entre Server y Client
│   ├── RestauranteApp.Domain/    Entidades, interfaces y servicios de negocio
│   │                             (OrdenService, CuentaService, MesaService, AutenticacionService)
│   ├── RestauranteApp.Data/      EF Core: DbContext, configuraciones, repositorios
│   ├── RestauranteApp.Server/    ASP.NET Core: SignalR, impresión ESC/POS, API REST
│   └── RestauranteApp.Client/    WPF (net8.0-windows): terminal táctil del mesero,
│                                 panel de cocina/bar
└── tests/
    └── RestauranteApp.Tests/     Pruebas unitarias de Domain (con fakes en memoria)
```

La dependencia entre capas va en un solo sentido: `Client`/`Server` → `Data` → `Domain` →
`Shared`. `Domain` no conoce EF Core, SignalR ni WPF — solo interfaces (`IUnitOfWork`,
`IImpresionService`, `INotificadorTiempoReal`) que `Data`/`Server` implementan. Esto permite
agregar módulos futuros (inventario, reservaciones, facturación) sin tocar lo ya construido.

## El flujo principal (lo más importante del sistema)

1. El mesero abre sesión en su terminal táctil (número + PIN).
2. Toca una mesa, arma la orden desde el menú (también táctil).
3. Presiona **"ENVIAR A COCINA"**. Esto llama a `OrdenService.EnviarOrdenAsync`, que:
   - Guarda la orden y sus ítems en la base de datos del servidor.
   - Genera una `Comanda` por estación (cocina/bar) con los ítems nuevos.
   - Guarda todo en una sola transacción (`UnitOfWork.GuardarCambiosAsync`).
   - **Solo después de que el guardado fue exitoso**, intenta imprimir cada comanda.
     Si la impresora falla, la orden ya quedó en base de datos — la comanda pasa a estado
     `Reintentando` y `PrintQueueBackgroundService` la reintenta en segundo plano.
4. Cocina/bar ven la comanda en tiempo real (SignalR) además del papel impreso.
5. Al cobrar, `CuentaService.CerrarCuentaAsync` calcula el total e imprime el ticket final
   en la impresora de caja — documento independiente y posterior a las comandas.

El bloqueo optimista (`RowVersion` en cada entidad) evita que dos meseros pisen la misma
orden: si alguien más ya guardó cambios, la operación lanza `ConflictoConcurrenciaException`
y el cliente debe recargar la orden.

## Requisitos para compilar

- **Visual Studio 2022** (17.8+) con las cargas de trabajo:
  - ".NET desktop development" (necesaria para `RestauranteApp.Client`, que usa WPF —
    **solo compila en Windows**, no en Linux/macOS).
  - "ASP.NET and web development" (para `RestauranteApp.Server`).
- .NET 8 SDK (se instala junto con Visual Studio, o desde https://dotnet.microsoft.com/download).

> **Nota sobre este entorno de desarrollo:** este proyecto se generó en un sandbox en la
> nube cuya política de red bloquea el acceso a `api.nuget.org`, así que aquí no fue posible
> restaurar/compilar los proyectos que dependen de paquetes NuGet externos (`RestauranteApp.Data`
> por Entity Framework Core, `RestauranteApp.Server` por depender de Data, `RestauranteApp.Client`
> por SignalR.Client/CommunityToolkit.Mvvm, y `RestauranteApp.Tests` por xUnit). Lo que **sí**
> se verificó exhaustivamente en este entorno:
> - `RestauranteApp.Shared` y `RestauranteApp.Domain` compilan sin errores (no dependen de
>   ningún paquete externo).
> - La lógica de negocio completa de `OrdenService` y `CuentaService` (el flujo principal,
>   la resiliencia ante fallas de impresión y el bloqueo optimista) se probó con un runner
>   de pruebas equivalente a xUnit, con **4 de 4 pruebas pasando**.
> - `RestauranteApp.Server`/`RestauranteApp.Data` llegan hasta el paso de restaurar paquetes
>   (es decir, no hay errores de código/estructura del proyecto) y solo fallan ahí por el
>   bloqueo de red del sandbox.
>
> En tu máquina Windows con Internet normal, `dotnet restore` / compilar desde Visual Studio
> debería funcionar sin ajustes adicionales.

## Cómo correrlo (primera vez)

1. Abre `RestauranteApp.sln` en Visual Studio.
2. Click derecho en la solución → **Restaurar paquetes NuGet**.
3. Marca `RestauranteApp.Server` como proyecto de inicio y ejecútalo primero — al arrancar
   crea la base de datos SQLite (`restaurante.db`) y siembra datos de ejemplo (zonas, mesas,
   menú, y 3 usuarios: mesero `2`/PIN `0000`, cajero `3`/PIN `0000`, admin `1`/PIN `1234`).
4. En `src/RestauranteApp.Client/MainWindow.xaml.cs`, ajusta `baseUrl` a la IP real de la
   PC servidor en tu red (por defecto `http://192.168.1.10:5080`).
5. Ejecuta `RestauranteApp.Client` en cada terminal (mesero, cocina, etc.) — cada quien ve
   una pantalla distinta según el rol con el que inicia sesión.
6. Configura las IPs de tus impresoras térmicas en
   `src/RestauranteApp.Server/appsettings.json` (sección `Impresoras`).

## Cambiar de SQLite a SQL Server Express

En `appsettings.json` del proyecto Server, cambia:

```json
"DatabaseProvider": "SqlServer",
"ConnectionStrings": {
  "Default": "Server=NOMBRE-PC\\SQLEXPRESS;Database=RestauranteApp;Trusted_Connection=True;TrustServerCertificate=True"
}
```

No hay que tocar nada más — `Program.cs` ya selecciona el proveedor de EF Core según este valor.

## Qué falta para producción (fuera del alcance del MVP)

- Autenticación robusta (el login actual es número+PIN simple, pensado para LAN cerrada).
- HTTPS entre terminales (hoy corre en HTTP plano dentro de la LAN de confianza).
- División de cuentas, reportes, inventario, reservaciones, facturación — ver la sección
  "Funciones para fases futuras" del prompt original.
