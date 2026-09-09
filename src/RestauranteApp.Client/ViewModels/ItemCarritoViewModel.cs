using CommunityToolkit.Mvvm.ComponentModel;
using RestauranteApp.Shared.Dtos;

namespace RestauranteApp.Client.ViewModels;

/// <summary>Un renglón del "carrito" local en la terminal del mesero, antes de enviarse al servidor.</summary>
public partial class ItemCarritoViewModel : ObservableObject
{
    public ItemMenuDto ItemMenu { get; }

    [ObservableProperty]
    private int cantidad = 1;

    [ObservableProperty]
    private string? notas;

    public ItemCarritoViewModel(ItemMenuDto itemMenu)
    {
        ItemMenu = itemMenu;
    }

    public decimal Importe => ItemMenu.Precio * Cantidad;
}
