using DojoAppMaui.Views;

namespace DojoAppMaui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Application.Current.MainPage = new LoginPage();

		Routing.RegisterRoute("login", typeof(LoginPage));
	}
}
