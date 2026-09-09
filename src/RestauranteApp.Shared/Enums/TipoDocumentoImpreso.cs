namespace RestauranteApp.Shared.Enums;

/// <summary>
/// Distingue la comanda (va a cocina/bar cuando el mesero envía la orden)
/// del ticket final de cuenta (va a caja cuando se cobra, y se entrega al cliente).
/// </summary>
public enum TipoDocumentoImpreso
{
    Comanda = 0,
    TicketCuenta = 1
}
