using RestauranteApp.Domain.Common;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Domain.Entities;

public class Mesero : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public int NumeroMesero { get; set; }

    /// <summary>Hash del PIN de acceso (nunca se guarda en texto plano).</summary>
    public string PinHash { get; set; } = string.Empty;

    public RolUsuario Rol { get; set; } = RolUsuario.Mesero;
    public bool Activo { get; set; } = true;
}
