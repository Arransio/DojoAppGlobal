using DojoAppMaui.Services;
using DojoAppMaui.Views;

namespace DojoAppMaui;

public partial class App : Application
{

    public static CarritoService CarritoService { get; private set; }
    public App()
	{
		InitializeComponent();

        CarritoService = new CarritoService();

        MainPage = new NavigationPage(new LoginPage());
    }
}
