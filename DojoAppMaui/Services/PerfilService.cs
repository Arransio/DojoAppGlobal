using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace DojoAppMaui.Services
{
    /// <summary>
    /// Almacena los datos del perfil del usuario (nombre, cinturón y grados)
    /// de forma local con Preferences. No son datos sensibles.
    /// </summary>
    public static class PerfilService
    {
        private const string KeyNombre = "perfil_nombre";
        private const string KeyCinturon = "perfil_cinturon";
        private const string KeyGrado = "perfil_grado";

        public static string GetNombre() => Preferences.Get(KeyNombre, string.Empty);
        public static void SaveNombre(string nombre) => Preferences.Set(KeyNombre, nombre ?? string.Empty);

        // Clave del color de cinturón: "Blanco", "Azul", "Morado", "Marron", "Negro"
        public static string GetCinturon() => Preferences.Get(KeyCinturon, string.Empty);
        public static void SaveCinturon(string cinturon) => Preferences.Set(KeyCinturon, cinturon ?? string.Empty);

        // Grados: 0 a 4 (ambos incluidos)
        public static int GetGrado() => Preferences.Get(KeyGrado, 0);
        public static void SaveGrado(int grado) => Preferences.Set(KeyGrado, grado);

        // ¿El usuario ya eligió alguna vez estos valores?
        public static bool TieneCinturon() => !string.IsNullOrEmpty(GetCinturon());
        public static bool TieneGrado() => Preferences.ContainsKey(KeyGrado);

        /// <summary>
        /// Color del cinturón a partir de su clave. Si no hay clave, devuelve blanco (default).
        /// Debe coincidir con los swatches de UsuarioPage.xaml.
        /// </summary>
        public static Color GetCinturonColor(string key) => key switch
        {
            "Azul" => Color.FromArgb("#1E66F5"),
            "Morado" => Color.FromArgb("#7C3AED"),
            "Marron" => Color.FromArgb("#8B5A2B"),
            "Negro" => Color.FromArgb("#000000"),
            _ => Color.FromArgb("#FFFFFF"), // Blanco por defecto
        };

        // El cinturón blanco (o ninguno elegido) lleva franjas negras; el resto, blancas.
        public static bool EsCinturonBlanco(string key) => string.IsNullOrEmpty(key) || key == "Blanco";
    }
}
