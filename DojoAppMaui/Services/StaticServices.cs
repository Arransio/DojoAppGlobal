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

			public static async Task SaveRole(string role)
			{
				try
				{
					await SecureStorage.SetAsync("user_role", role);
				}
				catch
				{
					Preferences.Set("user_role", role);
				}
			}

			public static async Task<string> GetRole()
			{
				try
				{
					return await SecureStorage.GetAsync("user_role");
				}
				catch
				{
					return Preferences.Get("user_role", null);
				}
			}
	}

}
