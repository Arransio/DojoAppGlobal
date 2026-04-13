using System.Diagnostics;
using System.Text.Json;
using Bumptech.Glide.Load.Model.Stream;
using DojoAppMaui.Models;
using Org.Apache.Http;

namespace DojoAppMaui.Views;

public partial class HomePage : ContentPage
{
    private readonly ProductService _productService = new ProductService();

    public static List<CartItem> cart = new();

    private List<Colores> colors = new();

    private List<ProductVariant> allVariants = new();

    public string baseUrl= "http://10.0.2.2:5221/api/";


   
    public HomePage()
	{
		InitializeComponent();
       
       
    }
    private async void LoadProducts()
    {
        var products = await _productService.GetProducts();

        foreach (var product in products)
        {
            var variantsForProduct = allVariants
                .Where(v => v.ProductId == product.Id)
                .Select(v => MapVariantToUI(v))
                .ToList();

            product.VariantsUI = variantsForProduct;
        }

        foreach (var product in products)
        {
            if (product.Name.ToLower().Contains("kimono"))
                product.Sizes = new List<string> { "A1", "A2", "A3", "A4" };
            else
                product.Sizes = new List<string> { "XS", "S", "M", "L", "XL" };
        }

        ProductsCollectionView.ItemsSource = null;
        ProductsCollectionView.ItemsSource = products;

        //DEBUGGG   

        foreach (var product in products)
        {
            Debug.WriteLine($"////////////Producto: {product.Name}");

            foreach (var v in product.VariantsUI)
            {
                Debug.WriteLine($"  {v.Muestra}");
            }
        }
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
                btn.BackgroundColor = Microsoft.Maui.Graphics.Colors.LightGray;
                btn.TextColor = Microsoft.Maui.Graphics.Colors.Black;
            }
        }

        button.BackgroundColor = Microsoft.Maui.Graphics.Colors.Green;
        button.TextColor = Microsoft.Maui.Graphics.Colors.White;

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

        App.CarritoService.AddItem(product, product.SelectedSize);

        //reseteo color botones tallas por contenedor post addtocart
		var parentLayout = button.Parent as VerticalStackLayout;

		foreach (var child in parentLayout.Children)
		{
			if (child is HorizontalStackLayout sizeContainer)
			{
				foreach (var sizeChild in sizeContainer.Children)
				{
					if (sizeChild is Button sizeBtn)
					{
						sizeBtn.BackgroundColor = (Color)Application.Current.Resources["Primary"];
						sizeBtn.TextColor = Colors.White;
					}
				}
			}
		}


		var originalColor = button.BackgroundColor;
        var originalTextColor = button.TextColor;
        var originalText = button.Text;  

       
		button.IsEnabled = false;

        button.Text = "Añadido";
        button.BackgroundColor = Microsoft.Maui.Graphics.Colors.Green;
        button.TextColor = Microsoft.Maui.Graphics.Colors.White;

        await Task.Delay(500);


        // restaurar estado
        button.Text = originalText;
        button.BackgroundColor = originalColor;
        button.TextColor = originalTextColor;
        button.IsEnabled = true;      



		UpdateCartSummary();
    }



    //Metodo para cargas los colores y mapearlos para el front
    //URL REFERENCIA = "http://10.0.2.2:5221/api/products";

    private async Task LoadColors() 
    {
        var client = new HttpClient();

        var httpResponse = await client.GetAsync($"{baseUrl}Colors");

        //Borrar para la app real, lineas de debug
        if (!httpResponse.IsSuccessStatusCode)
        {
            Debug.WriteLine("////////////ERROR API: " + httpResponse.StatusCode);
            return;
        }

        var response = await httpResponse.Content.ReadAsStringAsync();


        colors = JsonSerializer.Deserialize<List<Colores>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<Colores>();

        //Debug.WriteLine("///////COLORES TOTALES CARGADOS:" + colors.Count);
        //Debug.WriteLine("///////OLOR CON ID 1 CARGADO:" + GetColorsName(1));
    }

    private string GetColorsName(int colorId)
    {
        return colors.FirstOrDefault(c => c.Id == colorId)?.Name ?? "Color Desconocido";
    }

    //mapeamos
    private ProductVariantUI MapVariantToUI(ProductVariant variant)
    {
        string primary = "";
        string secondary = "";

        foreach (var color in variant.Colors)
        {
            if (color.Role.ToLower() == "primary")
                primary = GetColorsName(color.ColorId);

            if (color.Role.ToLower() == "secondary")
                secondary = GetColorsName(color.ColorId);
        }

        return new ProductVariantUI
        {
            Id = variant.Id,
            Size = variant.Size,
            PrimaryColor = primary,
            SecondaryColor = secondary
        };
    }


    private async Task LoadVariants()
    {
        var client = new HttpClient();

        var response = await client.GetStringAsync($"{baseUrl}ProductVariants");

        allVariants = JsonSerializer.Deserialize<List<ProductVariant>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<ProductVariant>();
    }


    private void UpdateCartSummary()
    {
        int count = App.CarritoService.GetCount();
        double total = App.CarritoService.GetTotal();

        CartSummaryLabel.Text = $"{count} items - {total}€";
    }
    private async void OnCartClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CarritoPage());
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadColors();
        await LoadVariants();
              LoadProducts();
        UpdateCartSummary();

    }


}