using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;
//using AndroidX.Browser.Trusted;
using DojoAppMaui.Services;
using DojoAppMaui.Views;


namespace DojoAppMaui.ViewModels;

public class LoginViewModel : BaseViewModel
{
	private readonly AuthService _authService;


	//Usuario
	private string username;
	public string Username
	{
		get => username;
		set => SetProperty(ref username, value);
	}

	//Contraseña
	private string password;
	public string Password
	{
		get => password;
		set => SetProperty(ref password, value);
	}

	public ICommand LoginCommand { get; }

	public LoginViewModel()
	{
		_authService = new AuthService();

		LoginCommand = new Command(async () =>
		{
			//await Application.Current.MainPage.DisplayAlert("DEBUG", "ANTES DE LOGIN", "OK");

			await Login();

			//await Application.Current.MainPage.DisplayAlert("DEBUG", "DESPUES DE LOGIN", "OK");
		});
	}

	private async Task Login()
	{
		if (IsBusy) return;

		try
		{
			var result = await _authService.LoginAsync(Username, Password);

			await TokenStorage.SaveToken(result.token);
			var token = await TokenStorage.GetToken();
			

			Application.Current.MainPage = new HomePage();
		}
		catch (Exception ex)
		{
			await Application.Current.MainPage.DisplayAlert("ERROR", ex.ToString(), "OK");
		}
		finally
		{
			IsBusy = false;
			
		}
	}
}