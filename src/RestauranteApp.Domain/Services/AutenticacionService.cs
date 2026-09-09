using System.Security.Cryptography;
using System.Text;
using RestauranteApp.Domain.Entities;
using RestauranteApp.Domain.Interfaces;

namespace RestauranteApp.Domain.Services;

/// <summary>
/// Login simple de mesero por número + PIN, pensado para una terminal táctil
/// (teclado numérico en pantalla, sin usuario/contraseña tradicionales).
/// </summary>
public class AutenticacionService
{
    private readonly IUnitOfWork _unitOfWork;

    public AutenticacionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Mesero> LoginAsync(int numeroMesero, string pin, CancellationToken ct = default)
    {
        var mesero = await _unitOfWork.Meseros.ObtenerPorNumeroAsync(numeroMesero, ct)
            ?? throw new UnauthorizedAccessException("Número de mesero o PIN incorrectos.");

        if (!mesero.Activo)
            throw new UnauthorizedAccessException("Este usuario está inactivo.");

        if (!VerificarPin(pin, mesero.PinHash))
            throw new UnauthorizedAccessException("Número de mesero o PIN incorrectos.");

        return mesero;
    }

    public static string HashPin(string pin)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(pin));
        return Convert.ToHexString(bytes);
    }

    private static bool VerificarPin(string pinIngresado, string pinHashAlmacenado)
        => HashPin(pinIngresado).Equals(pinHashAlmacenado, StringComparison.OrdinalIgnoreCase);
}
