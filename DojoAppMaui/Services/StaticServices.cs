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
	}

}
