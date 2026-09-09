using RestauranteApp.Domain.Entities;

namespace RestauranteApp.Tests.Fakes;

/// <summary>
/// "Base de datos" en memoria compartida entre los repositorios fake, usada solo en
/// pruebas unitarias. Simula lo esencial de EF Core que a OrdenService/CuentaService les
/// importa: cada Obtener*Async devuelve una COPIA independiente (como haría un DbContext
/// nuevo por request), y GuardarCambiosAsync es quien compara RowVersion y decide si hay
/// conflicto de concurrencia -- así probamos el bloqueo optimista sin levantar SQLite.
/// </summary>
public class FakeDatabaseStore
{
    public Dictionary<int, Orden> Ordenes { get; } = new();
    public Dictionary<int, Mesa> Mesas { get; } = new();
    public Dictionary<int, Mesero> Meseros { get; } = new();
    public Dictionary<int, ItemMenu> ItemsMenu { get; } = new();

    private int _siguienteOrdenId = 1;
    private int _siguienteItemOrdenId = 1;
    private int _siguienteComandaId = 1;

    public int SiguienteOrdenId() => _siguienteOrdenId++;
    public int SiguienteItemOrdenId() => _siguienteItemOrdenId++;
    public int SiguienteComandaId() => _siguienteComandaId++;

    public static byte[] NuevoRowVersion() => Guid.NewGuid().ToByteArray();

    /// <summary>Copia superficial suficiente para las pruebas: sin referencias circulares de navegación.</summary>
    public static Orden Clonar(Orden origen) => new()
    {
        Id = origen.Id,
        Folio = origen.Folio,
        MesaId = origen.MesaId,
        MeseroId = origen.MeseroId,
        Estado = origen.Estado,
        HoraApertura = origen.HoraApertura,
        HoraCierre = origen.HoraCierre,
        RowVersion = (byte[])origen.RowVersion.Clone(),
        HistorialTraspasos = origen.HistorialTraspasos,
        Items = origen.Items.Select(ClonarItem).ToList(),
        Comandas = origen.Comandas.Select(ClonarComanda).ToList()
    };

    private static ItemOrden ClonarItem(ItemOrden i) => new()
    {
        Id = i.Id,
        OrdenId = i.OrdenId,
        ItemMenuId = i.ItemMenuId,
        NombreItem = i.NombreItem,
        PrecioUnitario = i.PrecioUnitario,
        Cantidad = i.Cantidad,
        Notas = i.Notas,
        Estacion = i.Estacion,
        Estado = i.Estado,
        ModificadorIdsCsv = i.ModificadorIdsCsv,
        ComandaId = i.ComandaId,
        RowVersion = (byte[])i.RowVersion.Clone()
    };

    private static Comanda ClonarComanda(Comanda c) => new()
    {
        Id = c.Id,
        OrdenId = c.OrdenId,
        Estacion = c.Estacion,
        HoraEnvio = c.HoraEnvio,
        EstadoImpresion = c.EstadoImpresion,
        IntentosImpresion = c.IntentosImpresion,
        UltimoIntentoImpresion = c.UltimoIntentoImpresion,
        UltimoErrorImpresion = c.UltimoErrorImpresion,
        RowVersion = (byte[])c.RowVersion.Clone(),
        Items = c.Items.Select(ClonarItem).ToList()
    };
}
