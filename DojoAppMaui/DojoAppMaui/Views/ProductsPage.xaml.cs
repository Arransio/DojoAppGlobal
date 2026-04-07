using DojoAppMaui.Services;
using DojoAppMaui.ViewModels;

namespace DojoAppMaui.Views;

public partial class ProductsPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public ProductsPage()
    {
        InitializeComponent();

        _viewModel = new MainViewModel(new ApiService());
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadData();
    }
}