using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using DojoAppMaui.Models;
using DojoAppMaui.Services;

namespace DojoAppMaui.Views;

public partial class PedidosPage : ContentPage
{
	// Resuelto desde DI: llega con HttpClient configurado (URL base, timeout, token).
	private readonly ProductService _productService = ServiceHelper.GetService<ProductService>();

	private List<Colores> colors = new();

	private List<ProductVariant> allVariants = new();

	private List<Product> currentProducts = new();

	public PedidosPage()
	{
		InitializeComponent();
	}

	// Orquesta la carga completa del catálogo con sus estados visuales:
	// cargando → (datos | vacío | error con reintentar).
	private async Task LoadCatalogAsync()
	{
		LoadingView.IsVisible = true;
		LoadingIndicator.IsRunning = true;
		ErrorView.IsVisible = false;
		EmptyView.IsVisible = false;
		ProductsCollectionView.IsVisible = false;

		try
		{
			await LoadColors();
			await LoadVariants();
			await LoadProducts();

			// Catálogo vacío legítimo ≠ error: cada uno tiene su pantalla.
			var hayProductos = currentProducts.Count > 0;
			EmptyView.IsVisible = !hayProductos;
			ProductsCollectionView.IsVisible = hayProductos;
		}
		// El tipo de excepción dice qué pasó; nunca se inspecciona el texto del mensaje.
		catch (TaskCanceledException)
		{
			ErrorMessageLabel.Text = "El servidor tarda demasiado en responder. Inténtalo de nuevo.";
			ErrorView.IsVisible = true;
		}
		catch (HttpRequestException)
		{
			ErrorMessageLabel.Text = "No se pudo conectar con el servidor. Comprueba tu conexión.";
			ErrorView.IsVisible = true;
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[PedidosPage] Error inesperado cargando el catálogo: {ex}");
			ErrorMessageLabel.Text = "No se pudo cargar el catálogo.";
			ErrorView.IsVisible = true;
		}
		finally
		{
			LoadingView.IsVisible = false;
			LoadingIndicator.IsRunning = false;
		}
	}

	private async void OnReintentarClicked(object sender, EventArgs e)
	{
		await LoadCatalogAsync();
	}

	private async Task LoadProducts()
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
			var variant = await _productService.EnsureVariantAsync(productId, size);

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

		App.CarritoService.AddItem(new CartItem
		{
			ProductId = product.Id,
			ProductName = product.Name,
			UnitPrice = product.Price,
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
		colors = await _productService.GetColorsAsync();
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
		allVariants = await _productService.GetVariantsAsync();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		// LoadCatalogAsync captura sus propias excepciones: este async void no puede lanzar.
		await LoadCatalogAsync();

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
