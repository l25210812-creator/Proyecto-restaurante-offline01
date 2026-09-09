using System.Text;
using RestauranteApp.Domain.Entities;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Server.Printing;

/// <summary>
/// Construye los bytes ESC/POS de una comanda (cocina/bar) o de un ticket de cuenta (caja).
/// Formato deliberadamente simple (texto + cortes) para el MVP; el ticket con logo/diseño
/// del restaurante se afina cuando se trabaje la identidad visual.
/// </summary>
public static class EscPosCommandBuilder
{
    private const byte ESC = 0x1B;
    private const byte GS = 0x1D;

    private static readonly byte[] Inicializar = { ESC, (byte)'@' };
    private static readonly byte[] NegritaOn = { ESC, (byte)'E', 1 };
    private static readonly byte[] NegritaOff = { ESC, (byte)'E', 0 };
    private static readonly byte[] TextoGrandeOn = { GS, (byte)'!', 0x11 };
    private static readonly byte[] TextoNormal = { GS, (byte)'!', 0x00 };
    private static readonly byte[] CentrarOn = { ESC, (byte)'a', 1 };
    private static readonly byte[] AlinearIzquierda = { ESC, (byte)'a', 0 };
    private static readonly byte[] CortarPapel = { GS, (byte)'V', 1 };

    public static byte[] ConstruirComanda(Comanda comanda)
    {
        var sb = new StringBuilder();
        sb.AppendLine(comanda.Estacion == EstacionPreparacion.Cocina ? "*** COCINA ***" : "*** BARRA ***");
        sb.AppendLine($"Mesa: {comanda.Orden?.Mesa?.Nombre}   Folio: {comanda.Orden?.Folio}");
        sb.AppendLine($"Hora: {comanda.HoraEnvio:HH:mm:ss}");
        sb.AppendLine(new string('-', 32));

        foreach (var item in comanda.Items)
        {
            sb.AppendLine($"{item.Cantidad}x {item.NombreItem}");
            if (!string.IsNullOrWhiteSpace(item.Notas))
                sb.AppendLine($"   Nota: {item.Notas}");
        }

        sb.AppendLine(new string('-', 32));
        sb.AppendLine(" ");
        sb.AppendLine(" ");

        return Envolver(centrarEncabezado: true, textoGrandeEncabezado: true, sb.ToString());
    }

    public static byte[] ConstruirTicketCuenta(Cuenta cuenta, string nombreRestaurante)
    {
        var sb = new StringBuilder();
        sb.AppendLine(nombreRestaurante);
        sb.AppendLine($"Folio: {cuenta.Orden?.Folio}   Mesa: {cuenta.Orden?.Mesa?.Nombre}");
        sb.AppendLine($"Mesero: {cuenta.Orden?.Mesero?.Nombre}");
        sb.AppendLine(new string('-', 32));

        foreach (var item in cuenta.Orden?.Items ?? Enumerable.Empty<ItemOrden>())
        {
            var importe = item.PrecioUnitario * item.Cantidad;
            sb.AppendLine($"{item.Cantidad}x {item.NombreItem,-18} ${importe,6:0.00}");
        }

        sb.AppendLine(new string('-', 32));
        sb.AppendLine($"Subtotal:{cuenta.Subtotal,20:0.00}");
        sb.AppendLine($"Impuestos:{cuenta.Impuestos,19:0.00}");
        if (cuenta.Propina > 0)
            sb.AppendLine($"Propina:{cuenta.Propina,21:0.00}");
        sb.AppendLine($"TOTAL:{cuenta.Total,23:0.00}");
        sb.AppendLine(new string('-', 32));
        sb.AppendLine("¡Gracias por su visita!");
        sb.AppendLine(" ");
        sb.AppendLine(" ");

        return Envolver(centrarEncabezado: true, textoGrandeEncabezado: false, sb.ToString());
    }

    private static byte[] Envolver(bool centrarEncabezado, bool textoGrandeEncabezado, string cuerpo)
    {
        using var ms = new MemoryStream();
        ms.Write(Inicializar);

        if (centrarEncabezado) ms.Write(CentrarOn);
        if (textoGrandeEncabezado) ms.Write(TextoGrandeOn);
        ms.Write(NegritaOn);

        // Solo la primera línea (encabezado) usa el formato grande/negrita/centrado;
        // el resto del cuerpo va en texto normal alineado a la izquierda.
        var lineas = cuerpo.Replace("\r\n", "\n").Split('\n');
        if (lineas.Length > 0)
        {
            EscribirLinea(ms, lineas[0]);
        }

        ms.Write(TextoNormal);
        ms.Write(NegritaOff);
        ms.Write(AlinearIzquierda);

        for (var i = 1; i < lineas.Length; i++)
            EscribirLinea(ms, lineas[i]);

        ms.Write(CortarPapel);
        return ms.ToArray();
    }

    private static void EscribirLinea(Stream ms, string linea)
    {
        var bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(linea + "\n");
        ms.Write(bytes, 0, bytes.Length);
    }
}
