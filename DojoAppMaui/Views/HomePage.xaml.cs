using DojoAppMaui.Models;

namespace DojoAppMaui.Views;

public partial class HomePage : ContentPage
{
    private readonly ProductService _productService = new ProductService();

    private List<(Product product, string size)> cart = new();
    public HomePage()
	{
		InitializeComponent();
        LoadProducts();
    }
    private async void LoadProducts()
    {
        var products = await _productService.GetProducts();

        foreach (var product in products)
        {
            if (product.Name.ToLower().Contains("kimono"))
                product.Sizes = new List<string> { "A1", "A2", "A3", "A4" };
            else
                product.Sizes = new List<string> { "XS", "S", "M", "L", "XL" };
        }

        ProductsCollectionView.ItemsSource = null;
        ProductsCollectionView.ItemsSource = products;
    }

    private void OnSizeClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var selectedSize = button.BindingContext as string;

        var layout = button.Parent as Layout;

       
        foreach (var child in layout.Children)
        {
            if (child is Button btn)
            {
                btn.BackgroundColor = Colors.LightGray;
                btn.TextColor = Colors.Black;
            }
        }

        button.BackgroundColor = Colors.Green;
        button.TextColor = Colors.White;

        var product = (button.Parent.Parent as BindableObject).BindingContext as Product;

        if (product != null)
        {
            product.SelectedSize = selectedSize;

            Console.WriteLine($"Producto: {product.Name} - Talla: {selectedSize}");
        }
    }

    private async void OnAddToCartClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var product = button.BindingContext as Product;

        if (product?.SelectedSize == null)
        {
            Console.WriteLine("Selecciona una talla primero");
            return;
        }

        cart.Add((product, product.SelectedSize));
 
        var originalColor = button.BackgroundColor;
        var originalTextColor = button.TextColor;
        var originalText = button.Text;

        button.IsEnabled = false;

        button.Text = "Añadido ✔";
        button.BackgroundColor = Colors.Green;
        button.TextColor = Colors.White;

        await Task.Delay(500);

        // restaurar estado
        button.Text = originalText;
        button.BackgroundColor = originalColor;
        button.TextColor = originalTextColor;
        button.IsEnabled = true;

        UpdateCartSummary();
    }

    private void UpdateCartSummary()
    {
        var totalItems = cart.Count;
        var totalPrice = cart.Sum(x => x.product.Price);

        CartSummaryLabel.Text = $"{totalItems} items - {totalPrice}€";
    }
    private void OnCartClicked(object sender, EventArgs e)
    {
        Console.WriteLine("Abrir pantalla carrito");
    }

}