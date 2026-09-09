using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using RestauranteApp.Shared.Enums;

namespace RestauranteApp.Client.Converters;

/// <summary>
/// Color provisional por estado de mesa, solo para distinguir de un vistazo en el MVP.
/// La paleta definitiva de colores/planos del restaurante se aplica en la fase de diseño visual.
/// </summary>
public class EstadoMesaColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) => value switch
    {
        EstadoMesa.Libre => new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x71)),
        EstadoMesa.Ocupada => new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C)),
        EstadoMesa.Reservada => new SolidColorBrush(Color.FromRgb(0xF1, 0xC4, 0x0F)),
        EstadoMesa.EnLimpieza => new SolidColorBrush(Color.FromRgb(0x95, 0xA5, 0xA6)),
        _ => new SolidColorBrush(Colors.LightGray)
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
