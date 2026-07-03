using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace DojoAppMaui.Services
{
    /// <summary>
    /// Almacena los datos del perfil del usuario (nombre, cinturón, grados y foto)
    /// de forma local. Los textos van en Preferences y la foto se guarda como archivo
    /// en la carpeta privada de la app (solo se guarda su ruta en Preferences).
    /// No son datos sensibles.
    /// </summary>
    public static class PerfilService
    {
        private const string KeyNombre = "perfil_nombre";
        private const string KeyCinturon = "perfil_cinturon";
        private const string KeyGrado = "perfil_grado";
        private const string KeyFoto = "perfil_foto_path";

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

        // ---------- Foto de perfil ----------

        /// <summary>Ruta local de la foto de perfil, o cadena vacía si no hay.</summary>
        public static string GetFotoPath() => Preferences.Get(KeyFoto, string.Empty);

        /// <summary>¿Hay una foto guardada y el archivo sigue existiendo?</summary>
        public static bool TieneFoto()
        {
            var path = GetFotoPath();
            return !string.IsNullOrEmpty(path) && File.Exists(path);
        }

        /// <summary>
        /// Copia la imagen elegida (cámara o galería) a la carpeta privada de la app
        /// y guarda su ruta. Devuelve la ruta del archivo copiado.
        /// </summary>
        public static async Task<string> SaveFotoAsync(FileResult foto)
        {
            // Borramos la foto anterior para no acumular archivos.
            BorrarFoto();

            // Nombre único: así el control Image no reutiliza una imagen cacheada.
            var nombre = $"perfil_{Guid.NewGuid():N}.jpg";
            var destino = Path.Combine(FileSystem.AppDataDirectory, nombre);

            using (var origen = await foto.OpenReadAsync())
            using (var salida = File.Create(destino))
            {
                await origen.CopyToAsync(salida);
            }

            Preferences.Set(KeyFoto, destino);
            return destino;
        }

        /// <summary>Elimina la foto de perfil (archivo y referencia).</summary>
        public static void BorrarFoto()
        {
            var actual = Preferences.Get(KeyFoto, string.Empty);
            if (!string.IsNullOrEmpty(actual) && File.Exists(actual))
            {
                try { File.Delete(actual); } catch { /* si no se puede borrar, seguimos */ }
            }
            Preferences.Remove(KeyFoto);
        }

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
