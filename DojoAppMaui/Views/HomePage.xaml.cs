using System.Diagnostics;
using System.Text.Json;
using DojoAppMaui.Models;

namespace DojoAppMaui.Views;

public partial class HomePage : ContentPage
{
	private readonly ProductService _productService = new ProductService();

	public static List<CartItem> cart = new();

	private List<Colores> colors = new();

	private List<ProductVariant> allVariants = new();

	private List<Product> currentProducts = new();

	public string baseUrl = "http://10.0.2.2:5221/api/";

	public HomePage()
	{
		InitializeComponent();
	}

	private async void LoadProducts()
	{
		var products = await _productService.GetProducts();

		foreach (var product in products)
		{
			// Get variants for this product
			var variantsForProduct = allVariants
				.Where(v => v.ProductId == product.Id)
				.ToList();

			product.VariantsUI = variantsForProduct
				.Select(v => MapVariantToUI(v))
				.ToList();

			// Set available sizes
			var uniqueSizes = variantsForProduct
				.Select(v => v.Size)
				.Distinct()
				.OrderBy(s => s)
				.ToList();

			if (product.Name.ToLower().Contains("kimono"))
				product.AvailableSizes = new(new List<string> { "A1", "A2", "A3", "A4" });
			else
				product.AvailableSizes = new(uniqueSizes.Count > 0 ? uniqueSizes : new List<string> { "XS", "S", "M", "L", "XL" });

			// Keep old properties for compatibility
			product.Sizes = product.AvailableSizes.ToList();
		}

		currentProducts = products;
		ProductsCollectionView.ItemsSource = null;
		ProductsCollectionView.ItemsSource = products;

		foreach (var product in products)
		{
			Debug.WriteLine($"////////////Producto: {product.Name}");

			foreach (var v in product.VariantsUI)
			{
				Debug.WriteLine($"  {v.Muestra}");
			}
		}
	}

	private void OnProductHeaderClicked(object sender, EventArgs e)
	{
		var button = sender as Button;
		var product = button?.CommandParameter as Product;

		if (product != null)
		{
			// Toggle expansion
			product.IsExpanded = !product.IsExpanded;

			// Reset selections when collapsing
			if (!product.IsExpanded)
			{
				ResetProductSelection(product);
			}
		}
	}

	private void OnSizeStepClicked(object sender, EventArgs e)
	{
		var button = sender as Button;
		var selectedSize = button?.BindingContext as string;

		if (selectedSize == null)
			return;

		// Find the product by traversing the visual tree
		var product = FindProductInHierarchy(button);

		if (product != null)
		{
			product.SelectedSizeStep = selectedSize;
			product.SelectedVariant = null;
			product.SelectedPrimaryColor = null;
			product.SelectedSecondaryColor = null;
			product.AvailablePrimaryColors.Clear();
			product.AvailableSecondaryColors.Clear();

			// Load primary colors for selected size
			LoadPrimaryColorsForSize(product, selectedSize);
		}
	}

	private void LoadPrimaryColorsForSize(Product product, string selectedSize)
	{
		var variantsForSize = product.VariantsUI
			.Where(v => v.Size == selectedSize)
			.ToList();

		var primaryColors = new HashSet<string>();

		foreach (var variant in variantsForSize)
		{
			var apiVariant = allVariants.FirstOrDefault(av => av.Id == variant.Id);
			if (apiVariant != null)
			{
				var primaryColorId = apiVariant.Colors
					.FirstOrDefault(c => c.Role.ToLower() == "primary")
					?.ColorId;

				if (primaryColorId.HasValue)
				{
					var colorName = GetColorsName(primaryColorId.Value);
					primaryColors.Add(colorName);
				}
			}
		}

		product.AvailablePrimaryColors.Clear();
		foreach (var colorName in primaryColors.OrderBy(c => c))
		{
			var colorId = colors.FirstOrDefault(c => c.Name == colorName)?.Id ?? 0;
			product.AvailablePrimaryColors.Add(new ColorOption { Id = colorId, Name = colorName });
		}
	}

	private void OnPrimaryColorClicked(object sender, EventArgs e)
	{
		var button = sender as Button;
		var colorOption = button?.BindingContext as ColorOption;

		if (colorOption == null)
			return;

		var product = FindProductInHierarchy(button);

		if (product != null)
		{
			product.SelectedPrimaryColor = colorOption.Name;
			product.SelectedSecondaryColor = null;
			product.AvailableSecondaryColors.Clear();

			// Load secondary colors for selected size and primary color
			LoadSecondaryColorsForSizeAndPrimaryColor(product, product.SelectedSizeStep, colorOption.Name);
		}
	}

	private void LoadSecondaryColorsForSizeAndPrimaryColor(Product product, string? selectedSize, string primaryColorName)
	{
		if (string.IsNullOrEmpty(selectedSize))
			return;

		var matchingVariants = product.VariantsUI
			.Where(v => v.Size == selectedSize && v.PrimaryColor == primaryColorName)
			.ToList();

		var secondaryColors = new HashSet<string>();

		foreach (var variant in matchingVariants)
		{
			var apiVariant = allVariants.FirstOrDefault(av => av.Id == variant.Id);
			if (apiVariant != null)
			{
				var secondaryColorId = apiVariant.Colors
					.FirstOrDefault(c => c.Role.ToLower() == "secondary")
					?.ColorId;

				if (secondaryColorId.HasValue)
				{
					var colorName = GetColorsName(secondaryColorId.Value);
					secondaryColors.Add(colorName);
				}
			}
		}

		product.AvailableSecondaryColors.Clear();
		foreach (var colorName in secondaryColors.OrderBy(c => c))
		{
			var colorId = colors.FirstOrDefault(c => c.Name == colorName)?.Id ?? 0;
			product.AvailableSecondaryColors.Add(new ColorOption { Id = colorId, Name = colorName });
		}
	}

	private void OnSecondaryColorClicked(object sender, EventArgs e)
	{
		var button = sender as Button;
		var colorOption = button?.BindingContext as ColorOption;

		if (colorOption == null)
			return;

		var product = FindProductInHierarchy(button);

		if (product != null)
		{
			product.SelectedSecondaryColor = colorOption.Name;

			// Find and store the selected variant
			var selectedVariant = product.VariantsUI.FirstOrDefault(v =>
				v.Size == product.SelectedSizeStep &&
				v.PrimaryColor == product.SelectedPrimaryColor &&
				v.SecondaryColor == product.SelectedSecondaryColor);

			product.SelectedVariant = selectedVariant;
		}
	}

	private async void OnAddToCartClicked(object sender, EventArgs e)
	{
		var button = sender as Button;
		var product = button?.CommandParameter as Product;

		if (product?.SelectedVariant == null || string.IsNullOrEmpty(product.SelectedSizeStep))
		{
			Console.WriteLine("Selecciona talla y colores primero");
			return;
		}

		// Create a temporary product with the selected variant info for cart
		var cartProduct = new Product
		{
			Id = product.Id,
			Name = product.Name,
			Price = product.Price,
			SelectedSize = product.SelectedSizeStep,
			SelectedVariant = product.SelectedVariant
		};

		App.CarritoService.AddItem(cartProduct, product.SelectedSizeStep);

		// Visual feedback
		var originalColor = button.BackgroundColor;
		var originalTextColor = button.TextColor;
		var originalText = button.Text;

		button.IsEnabled = false;
		button.Text = "Añadido";
		button.BackgroundColor = Microsoft.Maui.Graphics.Colors.Green;
		button.TextColor = Microsoft.Maui.Graphics.Colors.White;

		await Task.Delay(500);

		// Restore state
		button.Text = originalText;
		button.BackgroundColor = originalColor;
		button.TextColor = originalTextColor;
		button.IsEnabled = true;

		// Reset selection
		ResetProductSelection(product);
		UpdateCartSummary();
	}

	private void ResetProductSelection(Product product)
	{
		product.SelectedSizeStep = null;
		product.SelectedPrimaryColor = null;
		product.SelectedSecondaryColor = null;
		product.SelectedVariant = null;
		product.AvailablePrimaryColors.Clear();
		product.AvailableSecondaryColors.Clear();
	}

	private Product? FindProductInHierarchy(View? view)
	{
		var current = view;

		while (current != null)
		{
			if (current is Frame frame && frame.BindingContext is Product product)
			{
				return product;
			}

			current = current.Parent as View;
		}

		return null;
	}

	private async Task LoadColors()
	{
		var client = new HttpClient();

		var httpResponse = await client.GetAsync($"{baseUrl}Colors");

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
	}

	private string GetColorsName(int colorId)
	{
		return colors.FirstOrDefault(c => c.Id == colorId)?.Name ?? "Color Desconocido";
	}

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