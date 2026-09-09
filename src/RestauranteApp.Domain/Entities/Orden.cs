using RestauranteApp.Domain.Common;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Domain.Entities;

/// <summary>
/// La orden abierta de una mesa. RowVersion (heredado de EntidadBase) es la clave del
/// bloqueo optimista: cada UPDATE exitoso lo regenera, así que dos meseros que intenten
/// guardar la misma orden con el mismo RowVersion "viejo" provocan que el segundo falle
/// con ConflictoConcurrenciaException en vez de sobreescribir al primero.
/// </summary>
public class Orden : EntidadBase
{
    /// <summary>Folio correlativo legible para el cliente/ticket (no es el Id interno).</summary>
    public int Folio { get; set; }

    public int MesaId { get; set; }
    public Mesa? Mesa { get; set; }

    public int MeseroId { get; set; }
    public Mesero? Mesero { get; set; }

    public EstadoOrden Estado { get; set; } = EstadoOrden.Abierta;

    public DateTime HoraApertura { get; set; } = DateTime.UtcNow;
    public DateTime? HoraCierre { get; set; }

    public ICollection<ItemOrden> Items { get; set; } = new List<ItemOrden>();
    public ICollection<Comanda> Comandas { get; set; } = new List<Comanda>();
    public Cuenta? Cuenta { get; set; }

    /// <summary>Historial simple de traspasos de mesa/mesero, como texto append-only para el MVP.</summary>
    public string? HistorialTraspasos { get; set; }
}
