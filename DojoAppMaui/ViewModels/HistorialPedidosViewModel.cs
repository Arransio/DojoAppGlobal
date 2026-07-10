using System.Collections.ObjectModel;
using System.Diagnostics;
using DojoAppMaui.Services;

namespace DojoAppMaui.ViewModels;

// ViewModel del historial de pedidos del usuario.
//
// Patrón a seguir en el resto de pantallas (regla del boy scout de la Fase 4):
// el ViewModel es el dueño del estado (cargando / datos / vacío / error) y la
// vista solo se enlaza a él. Distingue "sin datos" (la carga fue bien pero no
// hay pedidos) de "error de red" (no se pudo cargar y se ofrece reintentar).
public class HistorialPedidosViewModel : BaseViewModel
{
    private readonly OrderReportService _orderReportService;

    public ObservableCollection<PedidoHistorialItem> Pedidos { get; } = new();

    // Comando de "Reintentar" del estado de error.
    public Command RecargarCommand { get; }

    private bool _sinPedidos;
    // La carga terminó bien y el usuario no tiene pedidos (estado vacío legítimo).
    public bool SinPedidos
    {
        get => _sinPedidos;
        set => SetProperty(ref _sinPedidos, value);
    }

    private string? _mensajeError;
    // Si tiene valor, la carga falló y la vista muestra el estado de error.
    public string? MensajeError
    {
        get => _mensajeError;
        set
        {
            if (SetProperty(ref _mensajeError, value))
                OnPropertyChanged(nameof(HayError));
        }
    }

    public bool HayError => !string.IsNullOrEmpty(MensajeError);

    // Lista visible solo cuando hay datos y ningún estado especial activo.
    public bool HayPedidos => !IsBusy && !HayError && !SinPedidos && Pedidos.Count > 0;

    public HistorialPedidosViewModel(OrderReportService orderReportService)
    {
        _orderReportService = orderReportService;
        RecargarCommand = new Command(async () => await CargarAsync(), () => !IsBusy);
    }

    public async Task CargarAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        MensajeError = null;
        SinPedidos = false;
        RecargarCommand.ChangeCanExecute();
        OnPropertyChanged(nameof(HayPedidos));

        try
        {
            var userId = await TokenStorage.GetUserId();
            if (userId is null || userId <= 0)
            {
                MensajeError = "No se pudo identificar tu sesión. Cierra sesión y vuelve a entrar.";
                return;
            }

            var pedidos = await _orderReportService.GetPedidosByUserAsync(userId.Value);

            Pedidos.Clear();
            foreach (var pedido in pedidos)
                Pedidos.Add(new PedidoHistorialItem(pedido));

            SinPedidos = Pedidos.Count == 0;
        }
        // El tipo de excepción dice qué pasó; nunca se inspecciona el texto del mensaje.
        catch (TaskCanceledException)
        {
            MensajeError = "El servidor tarda demasiado en responder. Inténtalo de nuevo.";
        }
        catch (HttpRequestException)
        {
            MensajeError = "No se pudo conectar con el servidor. Comprueba tu conexión.";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HistorialPedidosViewModel] Error inesperado: {ex}");
            MensajeError = "No se pudo cargar el historial de pedidos.";
        }
        finally
        {
            IsBusy = false;
            RecargarCommand.ChangeCanExecute();
            OnPropertyChanged(nameof(HayPedidos));
        }
    }
}

// Pedido listo para pintar: la vista no formatea nada, solo enlaza.
public class PedidoHistorialItem
{
    public PedidoHistorialItem(PedidoUsuarioDto pedido)
    {
        Titulo = $"Pedido nº {pedido.Id}";
        FechaTexto = pedido.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        TotalTexto = $"{pedido.TotalPrice:F2} €";
        EstaPagado = pedido.EstaPagado;
        EstadoTexto = pedido.EstaPagado ? "Pagado" : "Pendiente de pago";
    }

    public string Titulo { get; }
    public string FechaTexto { get; }
    public string TotalTexto { get; }
    public string EstadoTexto { get; }
    public bool EstaPagado { get; }
}
