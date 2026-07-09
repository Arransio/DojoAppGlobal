using Microsoft.Extensions.DependencyInjection;

namespace DojoAppMaui.Services
{
	// Puente hacia el contenedor de DI para código que se instancia con "new"
	// (la navegación actual crea las páginas a mano, así que no pueden recibir
	// servicios por constructor). Paso intermedio hasta que las páginas también
	// se resuelvan por DI.
	public static class ServiceHelper
	{
		public static T GetService<T>() where T : notnull =>
			IPlatformApplication.Current!.Services.GetRequiredService<T>();
	}
}
