using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using DojoAppMaui.Models;
using DojoAppMaui.Services;

namespace DojoAppMaui.Views;

public partial class PedidosPage : ContentPage
{
	private readonly ProductService _productService = new ProductService();

	// Cliente único para toda la página (crear uno por llamada agota sockets).
	// AuthHttpHandler añade el token y gestiona los 401 de sesión caducada.
	private readonly HttpClient _httpClient = new HttpClient(new AuthHttpHandler());

	private List<Colores> colors = new();

	private List<ProductVariant> allVariants = new();

	private List<Product> currentProducts = new();

	public string baseUrl = "http://10.0.2.2:5221/api/";

	public PedidosPage()
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
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Distinct()
				.OrderBy(s => s)
				.ToList();

			if (product.Name.ToLower().Contains("kimono"))
				product.AvailableSizes = new(new List<string> { "A1", "A2", "A3", "A4" });
			else
				product.AvailableSizes = new(new List<string> { "XS", "S", "M", "L", "XL" });

			// Keep old properties for compatibility
			product.Sizes = product.AvailableSizes.ToList();
		}

		currentProducts = products;
		ProductsCollectionView.ItemsSource = null;
		ProductsCollectionView.ItemsSource = products;
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

	private async void OnSizeStepClicked(object sender, EventArgs e)
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
			product.SelectedPrimaryColor = null;
			product.SelectedSecondaryColor = null;
			product.SelectedPrimaryColorId = 0;
			product.SelectedSecondaryColorId = 0;
			product.AvailablePrimaryColors.Clear();
			product.AvailableSecondaryColors.Clear();

			// La variante = talla. Buscamos la variante de esa talla.
			product.SelectedVariant = product.VariantsUI
				.FirstOrDefault(v => v.Size == selectedSize);

			// Si no hay variante local, la creamos en el servidor para este producto y talla.
			if (product.SelectedVariant == null)
			{
				product.SelectedVariant = await EnsureVariantAsync(product.Id, selectedSize);
			}

			// Los colores ya no dependen de la variante: ofrecemos todos.
			FillColorOptions(product.AvailablePrimaryColors);
		}
	}

	private async Task<ProductVariantUI?> EnsureVariantAsync(int productId, string size)
	{
		try
		{
			var response = await _httpClient.GetAsync($"{baseUrl}ProductVariants/ensure/{productId}/{size}");
			if (!response.IsSuccessStatusCode)
				return null;

			var json = await response.Content.ReadAsStringAsync();
			var variant = JsonSerializer.Deserialize<ProductVariant>(json, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});

			if (variant != null)
				return new ProductVariantUI { Id = variant.Id, Size = variant.Size };
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[PedidosPage] Error al crear variante: {ex.Message}");
		}
		return null;
	}

	private void OnPrimaryColorClicked(object sender, TappedEventArgs e)
	{
		var border = sender as Border;
		var colorOption = border?.BindingContext as ColorOption;

		if (colorOption == null)
			return;

		var product = FindProductInHierarchy(border);

		if (product != null)
		{
			product.SelectedPrimaryColor = colorOption.Name;
			product.SelectedPrimaryColorId = colorOption.Id;
			product.SelectedSecondaryColor = null;
			product.SelectedSecondaryColorId = 0;
			product.AvailableSecondaryColors.Clear();

			// El color secundario también es libre: ofrecemos todos.
			FillColorOptions(product.AvailableSecondaryColors);
		}
	}

	private void OnSecondaryColorClicked(object sender, TappedEventArgs e)
	{
		var border = sender as Border;
		var colorOption = border?.BindingContext as ColorOption;

		if (colorOption == null)
			return;

		var product = FindProductInHierarchy(border);

		if (product != null)
		{
			product.SelectedSecondaryColor = colorOption.Name;
			product.SelectedSecondaryColorId = colorOption.Id;
		}
	}

	private async void OnAddToCartClicked(object sender, EventArgs e)
	{
		var button = sender as Button;
		var product = button?.CommandParameter as Product;

		if (product?.SelectedVariant == null || string.IsNullOrEmpty(product.SelectedSizeStep)
			|| product.SelectedPrimaryColorId <= 0 || product.SelectedSecondaryColorId <= 0)
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

		App.CarritoService.AddItem(new CartItem
		{
			Product = cartProduct,
			Size = product.SelectedSizeStep,
			ProductVariantId = product.SelectedVariant.Id,
			PrimaryColorId = product.SelectedPrimaryColorId,
			SecondaryColorId = product.SelectedSecondaryColorId,
			PrimaryColorName = product.SelectedPrimaryColor,
			SecondaryColorName = product.SelectedSecondaryColor
		});

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
		NavBar.RefreshBadge();
	}

	private void ResetProductSelection(Product product)
	{
		product.SelectedSizeStep = null;
		product.SelectedPrimaryColor = null;
		product.SelectedSecondaryColor = null;
		product.SelectedPrimaryColorId = 0;
		product.SelectedSecondaryColorId = 0;
		product.SelectedVariant = null;
		product.AvailablePrimaryColors.Clear();
		product.AvailableSecondaryColors.Clear();
	}

	private Product? FindProductInHierarchy(View? view)
	{
		var current = view;

		while (current != null)
		{
			if (current is Border border && border.BindingContext is Product product)
			{
				return product;
			}

			current = current.Parent as View;
		}

		return null;
	}

	private async Task LoadColors()
	{
		var httpResponse = await _httpClient.GetAsync($"{baseUrl}Colors");

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
		// Una variante es ahora solo talla.
		return new ProductVariantUI
		{
			Id = variant.Id,
			Size = variant.Size
		};
	}

	// Vuelca la lista global de colores en una colección de opciones.
	private void FillColorOptions(ObservableCollection<ColorOption> target)
	{
		target.Clear();
		foreach (var color in colors.OrderBy(c => c.Name))
		{
			target.Add(new ColorOption { Id = color.Id, Name = color.Name });
		}
	}

	private async Task LoadVariants()
	{
		var response = await _httpClient.GetStringAsync($"{baseUrl}ProductVariants");

		allVariants = JsonSerializer.Deserialize<List<ProductVariant>>(response, new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		}) ?? new List<ProductVariant>();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		await LoadColors();
		await LoadVariants();
		LoadProducts();

		// Cargar datos del usuario logueado en la cabecera
		await LoadUserInfo();
	}

	private async Task LoadUserInfo()
	{
		try
		{
			// Preferimos el nombre y apellidos del perfil; si no, el username de la sesión.
			var nombrePerfil = PerfilService.GetNombre();
			var display = !string.IsNullOrWhiteSpace(nombrePerfil)
				? nombrePerfil
				: await TokenStorage.GetUsername();

			if (string.IsNullOrWhiteSpace(display))
				display = "Invitado";

			UsernameLabel.Text = display;

			// Avatar: círculo del color del cinturón con una franja por cada grado.
			RenderBeltAvatar();
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[PedidosPage] Error cargando usuario: {ex.Message}");
		}
	}

	private void RenderBeltAvatar()
	{
		var cinturon = PerfilService.GetCinturon();
		var grados = PerfilService.GetGrado();

		AvatarBorder.BackgroundColor = PerfilService.GetCinturonColor(cinturon);

		// Cinturón blanco (o sin elegir) -> franjas negras; resto -> blancas.
		var stripeColor = PerfilService.EsCinturonBlanco(cinturon) ? Colors.Black : Colors.White;

		AvatarStripes.Children.Clear();
		for (int i = 0; i < grados; i++)
		{
			AvatarStripes.Children.Add(new BoxView
			{
				Color = stripeColor,
				BackgroundColor = stripeColor,
				HeightRequest = 3,
				HorizontalOptions = LayoutOptions.Fill
			});
		}
	}
}
