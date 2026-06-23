using DojoAppMaui.Services;
using DojoAppMaui.Views;

namespace DojoAppMaui;

public partial class App : Application
{

    public static CarritoService CarritoService { get; private set; }
    public App()
	{
		InitializeComponent();

        // Obsidian Redux: la identidad visual es modo oscuro.
        UserAppTheme = AppTheme.Dark;

        CarritoService = new CarritoService();

        MainPage = new NavigationPage(new LoginPage());
    }
}
