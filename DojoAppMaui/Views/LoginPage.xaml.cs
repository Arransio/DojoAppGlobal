using System.Runtime.CompilerServices;
using DojoAppMaui.ViewModels;

namespace DojoAppMaui.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
		BindingContext = new LoginViewModel();
	}

	public async void OnLoginClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new HomePage());
    }
}