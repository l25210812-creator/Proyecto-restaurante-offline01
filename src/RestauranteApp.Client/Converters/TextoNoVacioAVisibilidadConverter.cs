using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RestauranteApp.Client.Converters;

/// <summary>Oculta un TextBlock cuando el texto (ej. notas de un ítem) viene vacío o nulo.</summary>
public class TextoNoVacioAVisibilidadConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
