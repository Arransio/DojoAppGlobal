using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using DojoAppMaui.Views;

namespace DojoAppMaui.Services
{
	// Handler común para los servicios de datos:
	// - Añade el token Bearer a cada petición.
	// - Si el backend responde 401 (token caducado o inválido), cierra la sesión
	//   y devuelve al usuario a la pantalla de login con un aviso.
	//
	// NO lo usa AuthService: su 401 significa "credenciales incorrectas", no sesión caducada.
	public class AuthHttpHandler : DelegatingHandler
	{
		// Evita redirigir varias veces si fallan varias llamadas a la vez.
		// int en lugar de bool para poder usar Interlocked (0 = libre, 1 = redirigiendo).
		private static int _redirectingToLogin;

		// Sin InnerHandler propio: lo asigna IHttpClientFactory al construir la cadena.

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			// Fallo rápido sin conexión: un aviso inmediato en vez de esperar el timeout.
			if (Connectivity.NetworkAccess != NetworkAccess.Internet)
				throw new HttpRequestException("Sin conexión a internet");

			if (request.Headers.Authorization == null)
			{
				var token = await TokenStorage.GetToken();
				if (!string.IsNullOrEmpty(token))
					request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			}

			var response = await base.SendAsync(request, cancellationToken);

			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				Debug.WriteLine($"[AuthHttpHandler] 401 en {request.RequestUri} → sesión caducada");
				await HandleSessionExpiredAsync();
			}

			return response;
		}

		private static async Task HandleSessionExpiredAsync()
		{
			// Operación atómica: solo el primer 401 concurrente pasa de aquí.
			if (Interlocked.CompareExchange(ref _redirectingToLogin, 1, 0) != 0)
				return;

			await TokenStorage.ClearSession();

			await MainThread.InvokeOnMainThreadAsync(async () =>
			{
				try
				{
					// Si ya estamos en el login no hace falta redirigir
					var currentRoot = (Application.Current?.MainPage as NavigationPage)?.RootPage;
					if (currentRoot is LoginPage)
						return;

					Application.Current.MainPage = new NavigationPage(new LoginPage());

					await Application.Current.MainPage.DisplayAlert(
						"Sesión caducada",
						"Tu sesión ha caducado. Por favor, inicia sesión de nuevo.",
						"OK");
				}
				finally
				{
					Interlocked.Exchange(ref _redirectingToLogin, 0);
				}
			});
		}
	}
}
