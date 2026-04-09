using DojoAppMaui.Models;

namespace DojoAppMaui.Views;

public partial class HomePage : ContentPage
{
    private readonly ProductService _productService = new ProductService();
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
}