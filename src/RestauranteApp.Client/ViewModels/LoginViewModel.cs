using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestauranteApp.Client.Services;

namespace RestauranteApp.Client.ViewModels;

/// <summary>
/// Login por número de mesero + PIN, pensado para teclado numérico en pantalla táctil
/// (ver Views/LoginView.xaml): el mesero toca dígitos grandes, no usa teclado físico.
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public event EventHandler<LoginResultado>? LoginExitoso;

    [ObservableProperty]
    private string numeroMesero = string.Empty;

    [ObservableProperty]
    private string pin = string.Empty;

    [ObservableProperty]
    private string? mensajeError;

    [ObservableProperty]
    private bool cargando;

    public LoginViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [RelayCommand]
    private void AgregarDigitoMesero(string digito) => NumeroMesero += digito;

    [RelayCommand]
    private void AgregarDigitoPin(string digito) => Pin += digito;

    [RelayCommand]
    private void BorrarPin() => Pin = Pin.Length > 0 ? Pin[..^1] : Pin;

    [RelayCommand]
    private async Task IngresarAsync()
    {
        if (!int.TryParse(NumeroMesero, out var numero))
        {
            MensajeError = "Ingresa tu número de mesero.";
            return;
        }

        try
        {
            Cargando = true;
            MensajeError = null;
            var resultado = await _apiClient.LoginAsync(numero, Pin);
            LoginExitoso?.Invoke(this, resultado);
        }
        catch (Exception ex)
        {
            MensajeError = ex.Message;
            Pin = string.Empty;
        }
        finally
        {
            Cargando = false;
        }
    }
}
