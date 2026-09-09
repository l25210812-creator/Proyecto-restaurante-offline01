namespace RestauranteApp.Server.Printing;

/// <summary>Configuración de red de cada impresora térmica, leída de appsettings.json ("Impresoras").</summary>
public class ImpresorasOptions
{
    public const string Seccion = "Impresoras";

    public ImpresoraEndpoint Cocina { get; set; } = new();
    public ImpresoraEndpoint Bar { get; set; } = new();
    public ImpresoraEndpoint Caja { get; set; } = new();

    /// <summary>Milisegundos de espera al conectar/enviar antes de considerar la impresora caída.</summary>
    public int TimeoutMs { get; set; } = 3000;

    /// <summary>Intentos máximos que hace el trabajo en segundo plano antes de dejar de reintentar.</summary>
    public int MaxIntentos { get; set; } = 5;

    /// <summary>Segundos entre cada reintento del PrintQueueBackgroundService.</summary>
    public int IntervaloReintentoSegundos { get; set; } = 20;
}

public class ImpresoraEndpoint
{
    public string Nombre { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Puerto { get; set; } = 9100; // puerto estándar ESC/POS sobre red (RAW/JetDirect)
}
