namespace DojoAppMaui.Services
{
	// ÚNICA fuente de la configuración de red de la app.
	// Ningún servicio ni vista debe escribir URLs: todos la reciben ya configurada
	// desde MauiProgram (IHttpClientFactory) usando estos valores.
	//
	// Fase 6 (despliegue): la URL pública de producción se añadirá aquí
	// (p. ej. con #if DEBUG emulador / #else producción) sin tocar nada más.
	public static class AppConfig
	{
#if ANDROID
		// 10.0.2.2 es el alias del localhost del PC anfitrión visto desde el
		// emulador Android (el emulador tiene su propia red virtual).
		public const string ApiBaseUrl = "http://10.0.2.2:5221/";
#else
		public const string ApiBaseUrl = "http://localhost:5221/";
#endif

		// Si el servidor no respondió en este tiempo, no va a responder:
		// mejor un error claro a los 15 s que la UI congelada los 100 s del default.
		public static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(15);
	}
}
