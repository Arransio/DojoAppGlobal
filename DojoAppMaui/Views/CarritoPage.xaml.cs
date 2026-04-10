using System.Linq;
using DojoAppMaui.Models;

namespace DojoAppMaui.Views;

public partial class CarritoPage : ContentPage
{
    private List<CartItem> cart;

    public CarritoPage()
    {
        InitializeComponent();
        LoadCarrito();
    }

    private void LoadCarrito()
    {
        var items = App.CarritoService.GetItems();

        CartCollection.ItemsSource = null;
        CartCollection.ItemsSource = items.ToList();

        double total = App.CarritoService.GetTotal();
        TotalLabel.Text = $"Total: {total:F2} €";
    }

    private void OnRemoveClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var item = button?.BindingContext as CartItem;

        if (item != null)
        {
            App.CarritoService.RemoveItem(item);

            LoadCarrito();
        }
    }
}

public class CartItem
{
    public Product Product { get; set; }
    public string Size { get; set; }
}