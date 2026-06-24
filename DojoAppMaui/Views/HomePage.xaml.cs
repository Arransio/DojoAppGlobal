using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using DojoAppMaui.Models;
using DojoAppMaui.Services;

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

		//foreach (var product in products)
		//{
		//	Debug.WriteLine($"////////////Producto: {product.Name}");

		//	foreach (var v in product.VariantsUI)
		//	{
		//		Debug.WriteLine($"  {v.Muestra}");
		//	}
		//}
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
			var client = new HttpClient();
			var response = await client.GetAsync($"{baseUrl}ProductVariants/ensure/{productId}/{size}");
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
			Debug.WriteLine($"[HomePage] Error al crear variante: {ex.Message}");
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
		UpdateCartSummary();
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

		// Cargar datos del usuario logueado en la cabecera
		await LoadUserInfo();

		// Verificar si el usuario es admin (localmente)
		await CheckAdminRoleLocally();
	}

	private async Task LoadUserInfo()
	{
		try
		{
			var username = await TokenStorage.GetUsername();

			if (string.IsNullOrWhiteSpace(username))
				username = "Invitado";

			UsernameLabel.Text = username;
			UserInitialLabel.Text = char.ToUpper(username.Trim()[0]).ToString();
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[HomePage] Error cargando usuario: {ex.Message}");
		}
	}

	private async Task CheckAdminRoleLocally()
	{
		try
		{
			// Verificar si es admin: solo si username es "test1"
			var isAdmin = false;

			// Obtener el username usando TokenStorage (maneja SecureStorage correctamente)
			var username = await TokenStorage.GetUsername();
			Debug.WriteLine($"[HomePage] Username obtenido: '{username}'");

			if (!string.IsNullOrEmpty(username))
			{
				isAdmin = username.Equals("test1", StringComparison.OrdinalIgnoreCase);
				Debug.WriteLine($"[HomePage] Es admin: {isAdmin}");
			}
			else
			{
				Debug.WriteLine($"[HomePage] Username es vacío o null");
			}

			// Mostrar botones solo si es admin
			var adminButton = this.FindByName<Button>("AdminReportButton");
			if (adminButton != null)
			{
				adminButton.IsVisible = isAdmin;
				Debug.WriteLine($"[HomePage] AdminButton visibility set to: {isAdmin}");
			}

			var previewButton = this.FindByName<Button>("AdminPreviewButton");
			if (previewButton != null)
				previewButton.IsVisible = isAdmin;
			else
			{
				Debug.WriteLine($"[HomePage] AdminButton not found");
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[HomePage] Error verificando rol: {ex.Message}");
			Debug.WriteLine($"[HomePage] Stack: {ex.StackTrace}");
		}
	}

	private async void OnVisualizarReporteClicked(object sender, EventArgs e)
	{
		var button = sender as Button;
		if (button == null) return;

		try
		{
			button.IsEnabled = false;
			var originalText = button.Text;
			button.Text = "Cargando...";

			var orderReportService = new OrderReportService();
			var pedidos = await orderReportService.GetAllPedidosAsync();

			button.IsEnabled = true;
			button.Text = originalText;

			if (pedidos.Count == 0)
			{
				await DisplayAlert("Sin pedidos", "No hay pedidos para mostrar", "OK");
				return;
			}

			var pdfService = new PdfGeneratorService();
			var html = pdfService.GenerateHtmlPreview(pedidos);
			await Navigation.PushAsync(new ReportePreviewPage(html, pedidos));
		}
		catch (Exception ex)
		{
			button.IsEnabled = true;
			button.Text = "👁 Visualizar Reporte";
			Debug.WriteLine($"[HomePage] Error visualizando reporte: {ex.Message}");
			await DisplayAlert("Error", $"Error al cargar el reporte: {ex.Message}", "OK");
		}
	}

	private async void OnAdminReportClicked(object sender, EventArgs e)
	{
		try
		{
			Debug.WriteLine("[HomePage] Generando reporte de pedidos");

			var button = sender as Button;
			if (button == null) return;

			button.IsEnabled = false;
			var originalText = button.Text;
			button.Text = "Generando...";

			// Obtener pedidos
			var orderReportService = new OrderReportService();
			var pedidos = await orderReportService.GetAllPedidosAsync();

			if (pedidos.Count == 0)
			{
				await DisplayAlert("Sin pedidos", "No hay pedidos para generar el reporte", "OK");
				button.IsEnabled = true;
				button.Text = originalText;
				return;
			}

			// Generar PDF
			var pdfService = new PdfGeneratorService();
			var filePath = await pdfService.GenerateOrdersPdfAsync(pedidos);

			button.IsEnabled = true;
			button.Text = originalText;

			// Mostrar opciones
			var action = await DisplayActionSheet(
				$"Reporte generado: {Path.GetFileName(filePath)}",
				"Cerrar",
				null,
				"Compartir",
				"Guardar en descargas"
			);

			if (action == "Compartir")
			{
				await Share.Default.RequestAsync(new ShareFileRequest
				{
					Title = "Reporte de pedidos",
					File = new ShareFile(filePath)
				});
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[HomePage] Error generando reporte: {ex.Message}");
			await DisplayAlert("Error", $"Error al generar reporte: {ex.Message}", "OK");

			var button = sender as Button;
			if (button != null)
			{
				button.IsEnabled = true;
				button.Text = "📊 Reporte";
			}
		}
	}
}