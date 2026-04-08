using DojoAppMaui.Views;

namespace DojoAppMaui;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new LoginPage();
	}
}
