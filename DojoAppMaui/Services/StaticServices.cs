using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DojoAppMaui.Services
{
	public static class TokenStorage
	{
		// Claves de sesión (las únicas que borra ClearSession; el perfil usa claves perfil_*)
		private const string KeyToken = "auth_token";
		private const string KeyUserId = "user_id";
		private const string KeyUsername = "username";
		private const string KeyRole = "user_role";

		public static async Task SaveToken(string token)
		{
			try
			{
				await SecureStorage.SetAsync(KeyToken, token);
			}
			catch
			{
				//solo es un fallback para el emulador
				Preferences.Set(KeyToken, token);
			}
		}

		public static async Task<string> GetToken()
		{
			try
			{
				return await SecureStorage.GetAsync(KeyToken);
			}
			catch
			{
				return Preferences.Get(KeyToken, null);
			}
		}

		public static async Task SaveUserId(int userId)
		{
			try
			{
				await SecureStorage.SetAsync(KeyUserId, userId.ToString());
			}
			catch
			{
				Preferences.Set(KeyUserId, userId);
			}
		}

		public static async Task<int?> GetUserId()
		{
			try
			{
				var userId = await SecureStorage.GetAsync(KeyUserId);
				if (int.TryParse(userId, out var id))
					return id;
			}
			catch
			{
				if (Preferences.ContainsKey(KeyUserId))
				{
					return Preferences.Get(KeyUserId, 0);
				}
			}
			return null;
		}

		// El rol viene del backend (LoginResponse.Role) y se persiste aquí.
		public static async Task SaveRole(string role)
		{
			try
			{
				await SecureStorage.SetAsync(KeyRole, role ?? "user");
			}
			catch
			{
				Preferences.Set(KeyRole, role ?? "user");
			}
		}

		public static async Task<string> GetRole()
		{
			try
			{
				var role = await SecureStorage.GetAsync(KeyRole);
				if (!string.IsNullOrEmpty(role))
					return role;
			}
			catch
			{
				var role = Preferences.Get(KeyRole, null);
				if (!string.IsNullOrEmpty(role))
					return role;
			}

			return "user";
		}

		public static async Task SaveUsername(string username)
		{
			try
			{
				await SecureStorage.SetAsync(KeyUsername, username);
			}
			catch
			{
				Preferences.Set(KeyUsername, username);
			}
		}

		public static async Task<string> GetUsername()
		{
			try
			{
				return await SecureStorage.GetAsync(KeyUsername);
			}
			catch
			{
				return Preferences.Get(KeyUsername, null);
			}
		}

		// Cierra la sesión borrando SOLO las claves de sesión.
		// No usa Preferences.Clear() para conservar el perfil local (perfil_*)
		// y el usuario recordado del login.
		public static Task ClearSession()
		{
			foreach (var key in new[] { KeyToken, KeyUserId, KeyUsername, KeyRole })
			{
				try { SecureStorage.Remove(key); } catch { }
				try { Preferences.Remove(key); } catch { }
			}

			return Task.CompletedTask;
		}

		// Expiración (claim exp) de un JWT, o null si el token no es un JWT válido
		// (p. ej. el token del modo demo). Usado por el auto-login.
		public static DateTime? GetTokenExpiryUtc(string token)
		{
			try
			{
				var parts = token?.Split('.');
				if (parts == null || parts.Length != 3)
					return null;

				var payload = parts[1].Replace('-', '+').Replace('_', '/');
				switch (payload.Length % 4)
				{
					case 2: payload += "=="; break;
					case 3: payload += "="; break;
				}

				var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
				using var doc = JsonDocument.Parse(json);

				if (doc.RootElement.TryGetProperty("exp", out var exp) && exp.TryGetInt64(out var expSeconds))
					return DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
			}
			catch
			{
				// Token ilegible → se trata como no válido
			}

			return null;
		}
	}
}
