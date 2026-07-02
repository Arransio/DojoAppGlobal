using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DojoAppMaui.Services;

namespace DojoAppMaui.Views;

public partial class PagosPage : ContentPage
{
    private readonly OrderReportService _service = new();
    private readonly ObservableCollection<PagoRow> _rows = new();

    public PagosPage()
    {
        InitializeComponent();
        PagosCollection.ItemsSource = _rows;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarPagosAsync();
    }

    private async Task CargarPagosAsync()
    {
        try
        {
            var pedidos = await _service.GetAllPedidosAsync();

            // Desengancha los handlers anteriores antes de reconstruir la lista.
            foreach (var row in _rows)
                row.PropertyChanged -= OnRowChanged;
            _rows.Clear();

            // Solo mostramos los pedidos aún pendientes de pago; los ya pagados
            // desaparecen de la cola de gestión.
            foreach (var pedido in pedidos.Where(p => !p.EstaPagado))
            {
                var row = new PagoRow(pedido.Id, pedido.UserName, pedido.TotalPrice, pedido.EstaPagado);
                row.PropertyChanged += OnRowChanged;
                _rows.Add(row);
            }

            ActualizarBotonGuardar();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar los pagos: {ex.Message}", "OK");
        }
    }

    private void OnRowChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PagoRow.IsPaid))
            ActualizarBotonGuardar();
    }

    private void ActualizarBotonGuardar()
    {
        GuardarBar.IsVisible = _rows.Any(r => r.TieneCambios);
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        var cambios = _rows.Where(r => r.TieneCambios)
            .Select(r => new PaymentUpdateDto { PedidoId = r.PedidoId, IsPaid = r.IsPaid })
            .ToList();

        if (cambios.Count == 0)
            return;

        try
        {
            GuardarButton.IsEnabled = false;
            GuardarButton.Text = "...";

            await _service.UpdatePaymentsAsync(cambios);

            // Los pedidos marcados como pagados desaparecen de la lista.
            foreach (var row in _rows.Where(r => r.IsPaid).ToList())
            {
                row.PropertyChanged -= OnRowChanged;
                _rows.Remove(row);
            }

            // El resto (los que se hayan desmarcado) consolidan su estado guardado.
            foreach (var row in _rows.Where(r => r.TieneCambios).ToList())
                row.ConsolidarEstado();

            ActualizarBotonGuardar();
            GuardarButton.IsEnabled = true;
            GuardarButton.Text = "💾 GUARDAR CAMBIOS";

            await DisplayAlert("Guardado", "Los cambios de pago se guardaron correctamente.", "OK");
        }
        catch (Exception ex)
        {
            GuardarButton.IsEnabled = true;
            GuardarButton.Text = "💾 GUARDAR CAMBIOS";
            await DisplayAlert("Error", $"No se pudieron guardar los cambios: {ex.Message}", "OK");
        }
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await CargarPagosAsync();
        PagosRefresh.IsRefreshing = false;
    }

    private void OnVolverClicked(object sender, EventArgs e)
    {
        Controls.BottomNavBar.SetRoot(new ReporteMenuPage());
    }
}

// Fila de la lista de pagos: un pedido con su estado marcable.
public class PagoRow : INotifyPropertyChanged
{
    public int PedidoId { get; }
    public string UserName { get; }
    public string Subtitulo { get; }

    private bool _originalIsPaid;
    private bool _isPaid;

    public PagoRow(int pedidoId, string userName, decimal total, bool isPaid)
    {
        PedidoId = pedidoId;
        UserName = string.IsNullOrWhiteSpace(userName) ? $"Usuario {pedidoId}" : userName;
        Subtitulo = $"Pedido #{pedidoId} · {total:0.00} €";
        _originalIsPaid = isPaid;
        _isPaid = isPaid;
    }

    public bool IsPaid
    {
        get => _isPaid;
        set
        {
            if (_isPaid == value)
                return;
            _isPaid = value;
            OnPropertyChanged();
        }
    }

    public bool TieneCambios => _isPaid != _originalIsPaid;

    // Tras guardar, el estado actual pasa a ser el de referencia.
    public void ConsolidarEstado() => _originalIsPaid = _isPaid;

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
