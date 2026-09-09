using RestauranteApp.Domain.Entities;
using RestauranteApp.Domain.Interfaces;

namespace RestauranteApp.Domain.Services;

/// <summary>Consultas y cambios de estado simples sobre mesas para el mapa de zonas.</summary>
public class MesaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificadorTiempoReal _notificador;

    public MesaService(IUnitOfWork unitOfWork, INotificadorTiempoReal notificador)
    {
        _unitOfWork = unitOfWork;
        _notificador = notificador;
    }

    public Task<IReadOnlyList<Mesa>> ObtenerMapaDeMesasAsync(CancellationToken ct = default)
        => _unitOfWork.Mesas.ObtenerTodasConZonaAsync(ct);

    public async Task MarcarLimpiaAsync(int mesaId, CancellationToken ct = default)
    {
        var mesa = await _unitOfWork.Mesas.ObtenerPorIdAsync(mesaId, ct)
            ?? throw new InvalidOperationException($"No existe la mesa {mesaId}.");

        mesa.Estado = Shared.Enums.EstadoMesa.Libre;
        await _unitOfWork.Mesas.ActualizarAsync(mesa, ct);
        await _unitOfWork.GuardarCambiosAsync(ct);
        await _notificador.NotificarMesaActualizadaAsync(mesa, ct);
    }
}
