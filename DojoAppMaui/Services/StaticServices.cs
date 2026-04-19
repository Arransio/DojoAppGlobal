using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DojoAppMaui.Services
{
	internal class StaticServices
	{
	}

	public static class TokenStorage
	{
		public static async Task SaveToken(string token)
		{
			try
			{
				await SecureStorage.SetAsync("auth_token", token);
			}
			catch
			{
				//solo es un fallback para el emulador
				Preferences.Set("auth_token", token);
			}
		}

		public static async Task<string> GetToken()
		{
			try
			{
				return await SecureStorage.GetAsync("auth_token");
			}
			catch
			{
				return Preferences.Get("auth_token", null);

			}
		}

		public static async Task SaveUserId(int userId)
		{
			try
			{
				await SecureStorage.SetAsync("user_id", userId.ToString());
			}
			catch
			{
				Preferences.Set("user_id", userId);
			}
		}

		public static async Task<int?> GetUserId()
		{
			try
			{
				var userId = await SecureStorage.GetAsync("user_id");
				if (int.TryParse(userId, out var id))
					return id;
			}
			catch
			{
				if (Preferences.ContainsKey("user_id"))
				{
					return Preferences.Get("user_id", 0);
				}
			}
			return null;
		}

		public static async Task ClearAll()
			{
				try
				{
					SecureStorage.RemoveAll();
				}
				catch { }

				Preferences.Clear();
			}

			public static async Task SaveRole(string username)
			{
				// Rol calculado localmente: test1 es admin, otros son user
				// No es necesario guardar el rol, se calcula cuando se necesita
				await Task.CompletedTask;
			}

			public static async Task<string> GetRole()
			{
				try
				{
					var username = await GetUsername();
					if (!string.IsNullOrEmpty(username))
					{
						// Hardcodear: si es test1 es admin, sino user
						return username.Equals("test1", StringComparison.OrdinalIgnoreCase) ? "admin" : "user";
					}
				}
				catch { }

				return "user";
			}

			public static async Task SaveUsername(string username)
			{
				try
				{
					await SecureStorage.SetAsync("username", username);
				}
				catch
				{
					Preferences.Set("username", username);
				}
			}

			public static async Task<string> GetUsername()
			{
				try
				{
					return await SecureStorage.GetAsync("username");
				}
				catch
				{
					return Preferences.Get("username", null);
				}
			}
	}

}
