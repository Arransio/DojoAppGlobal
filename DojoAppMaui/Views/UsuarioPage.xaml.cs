using System.Collections.Generic;
using System.Diagnostics;
using DojoAppMaui.Services;

namespace DojoAppMaui.Views;

public partial class UsuarioPage : ContentPage
{
    private string _cinturonSeleccionado = string.Empty;
    private int? _gradoSeleccionado;

    private bool _cinturonExpandido = true;
    private bool _gradoExpandido = true;

    private bool _hayCambios;
    private bool _cargando;

    private readonly Dictionary<string, Border> _swatches;
    private readonly Dictionary<string, VerticalStackLayout> _beltItems;
    private readonly Dictionary<int, Border> _gradoChips;
    private readonly Dictionary<int, Label> _gradoLabels;

    public UsuarioPage()
    {
        InitializeComponent();

        _swatches = new Dictionary<string, Border>
        {
            { "Blanco", BeltBlanco },
            { "Azul", BeltAzul },
            { "Morado", BeltMorado },
            { "Marron", BeltMarron },
            { "Negro", BeltNegro },
        };

        _beltItems = new Dictionary<string, VerticalStackLayout>
        {
            { "Blanco", BeltItemBlanco },
            { "Azul", BeltItemAzul },
            { "Morado", BeltItemMorado },
            { "Marron", BeltItemMarron },
            { "Negro", BeltItemNegro },
        };

        _gradoChips = new Dictionary<int, Border>
        {
            { 0, Grado0 }, { 1, Grado1 }, { 2, Grado2 }, { 3, Grado3 }, { 4, Grado4 },
        };

        _gradoLabels = new Dictionary<int, Label>
        {
            { 0, Grado0Label }, { 1, Grado1Label }, { 2, Grado2Label }, { 3, Grado3Label }, { 4, Grado4Label },
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _cargando = true;

        RenderAvatar();

        NombreEntry.Text = PerfilService.GetNombre();

        // Cinturón: si ya hay uno guardado, arrancamos colapsado mostrando solo el elegido.
        if (PerfilService.TieneCinturon())
        {
            _cinturonSeleccionado = PerfilService.GetCinturon();
            _cinturonExpandido = false;
        }
        else
        {
            _cinturonSeleccionado = string.Empty;
            _cinturonExpandido = true;
        }
        ActualizarCinturonUI();

        // Grados: igual que el cinturón.
        if (PerfilService.TieneGrado())
        {
            _gradoSeleccionado = PerfilService.GetGrado();
            _gradoExpandido = false;
        }
        else
        {
            _gradoSeleccionado = null;
            _gradoExpandido = true;
        }
        ActualizarGradoUI();

        _hayCambios = false;
        GuardarButton.IsVisible = false;

        _cargando = false;
    }

    // ---------- Nombre ----------

    private void OnNombreChanged(object sender, TextChangedEventArgs e) => MarcarCambios();

    // ---------- Cinturón ----------

    private void OnBeltTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not string key || string.IsNullOrEmpty(key))
            return;

        // Si está colapsado, el toque solo expande para poder cambiar.
        if (!_cinturonExpandido)
        {
            _cinturonExpandido = true;
            ActualizarCinturonUI();
            return;
        }

        // Expandido: elegimos y colapsamos.
        var cambia = key != _cinturonSeleccionado;
        _cinturonSeleccionado = key;
        _cinturonExpandido = false;
        ActualizarCinturonUI();

        if (cambia)
            MarcarCambios();
    }

    private void ActualizarCinturonUI()
    {
        foreach (var kv in _swatches)
        {
            var esSeleccionado = kv.Key == _cinturonSeleccionado;

            // Visibilidad: expandido -> todos; colapsado -> solo el elegido.
            _beltItems[kv.Key].IsVisible = _cinturonExpandido || esSeleccionado;

            // Anillo de selección.
            kv.Value.Stroke = esSeleccionado ? Res("AccentSalmon") : Res("Hairline");
            kv.Value.StrokeThickness = esSeleccionado ? 3 : 1;
        }
    }

    // ---------- Grados ----------

    private void OnGradoTapped(object sender, TappedEventArgs e)
    {
        if (!int.TryParse(e.Parameter?.ToString(), out var grado))
            return;

        if (!_gradoExpandido)
        {
            _gradoExpandido = true;
            ActualizarGradoUI();
            return;
        }

        var cambia = grado != _gradoSeleccionado;
        _gradoSeleccionado = grado;
        _gradoExpandido = false;
        ActualizarGradoUI();

        if (cambia)
            MarcarCambios();
    }

    private void ActualizarGradoUI()
    {
        foreach (var kv in _gradoChips)
        {
            var esSeleccionado = _gradoSeleccionado.HasValue && kv.Key == _gradoSeleccionado.Value;

            kv.Value.IsVisible = _gradoExpandido || esSeleccionado;

            kv.Value.BackgroundColor = esSeleccionado ? Res("AccentSalmon") : Res("SurfaceLowest");
            kv.Value.Stroke = esSeleccionado ? Res("AccentSalmon") : Res("Hairline");
            _gradoLabels[kv.Key].TextColor = esSeleccionado ? Res("ObsidianBg") : Res("TextPrimary");
        }
    }

    // ---------- Guardar / cambios ----------

    private void MarcarCambios()
    {
        if (_cargando)
            return;

        _hayCambios = true;
        GuardarButton.IsVisible = true;
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        PerfilService.SaveNombre(NombreEntry.Text?.Trim() ?? string.Empty);

        if (!string.IsNullOrEmpty(_cinturonSeleccionado))
            PerfilService.SaveCinturon(_cinturonSeleccionado);

        if (_gradoSeleccionado.HasValue)
            PerfilService.SaveGrado(_gradoSeleccionado.Value);

        _hayCambios = false;
        GuardarButton.IsVisible = false;

        await DisplayAlert("Perfil", "Datos guardados correctamente.", "OK");
    }

    private async void OnHistorialClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Historial de pedidos", "Esta sección estará disponible próximamente.", "OK");
    }

    private async void OnCerrarSesionClicked(object sender, EventArgs e)
    {
        var confirmar = await DisplayAlert(
            "Cerrar sesión",
            "¿Seguro que quieres cerrar la sesión?",
            "Sí, cerrar", "Cancelar");

        if (!confirmar)
            return;

        // Borra solo la sesión (token, usuario, rol); el perfil local se conserva
        await TokenStorage.ClearSession();

        Application.Current.MainPage = new NavigationPage(new LoginPage());
    }

    // ---------- Foto de perfil ----------

    private void RenderAvatar()
    {
        if (PerfilService.TieneFoto())
        {
            AvatarImage.Source = ImageSource.FromFile(PerfilService.GetFotoPath());
            AvatarImage.IsVisible = true;
            AvatarPlaceholder.IsVisible = false;
            AvatarHintLabel.Text = "Toca para cambiar la foto";
        }
        else
        {
            AvatarImage.Source = null;
            AvatarImage.IsVisible = false;
            AvatarPlaceholder.IsVisible = true;
            AvatarHintLabel.Text = "Toca para añadir foto";
        }
    }

    private async void OnAvatarTapped(object sender, TappedEventArgs e)
    {
        var opciones = PerfilService.TieneFoto()
            ? new[] { "Hacer foto", "Elegir de galería", "Quitar foto" }
            : new[] { "Hacer foto", "Elegir de galería" };

        var accion = await DisplayActionSheet("Foto de perfil", "Cancelar", null, opciones);

        try
        {
            FileResult foto = null;

            switch (accion)
            {
                case "Hacer foto":
                    if (!MediaPicker.Default.IsCaptureSupported)
                    {
                        await DisplayAlert("Foto de perfil", "Este dispositivo no permite hacer fotos.", "OK");
                        return;
                    }
                    foto = await MediaPicker.Default.CapturePhotoAsync();
                    break;

                case "Elegir de galería":
                    foto = await MediaPicker.Default.PickPhotoAsync();
                    break;

                case "Quitar foto":
                    PerfilService.BorrarFoto();
                    RenderAvatar();
                    NavBar.RefreshUsuarioAvatar();
                    return;

                default:
                    return; // Cancelar
            }

            // El usuario canceló la cámara o el selector.
            if (foto == null)
                return;

            await PerfilService.SaveFotoAsync(foto);
            RenderAvatar();
            NavBar.RefreshUsuarioAvatar();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UsuarioPage] Error con la foto de perfil: {ex.Message}");
            await DisplayAlert("Foto de perfil", "No se pudo guardar la foto.", "OK");
        }
    }

    private static Color Res(string key)
        => Application.Current is not null
           && Application.Current.Resources.TryGetValue(key, out var value)
           && value is Color color
            ? color
            : Colors.Transparent;
}
