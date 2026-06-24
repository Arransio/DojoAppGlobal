using PdfSharpCore.Fonts;

namespace DojoAppMaui.Services
{
    public class MauiFontResolver : IFontResolver
    {
        public string DefaultFontName => "OpenSans";

        private static readonly Dictionary<string, byte[]> FontCache = new();

        public byte[] GetFont(string faceName)
        {
            if (FontCache.TryGetValue(faceName, out var cached))
                return cached;

            var resourceName = faceName.EndsWith("-Bold")
                ? "DojoAppMaui.Resources.Fonts.OpenSans-Semibold.ttf"
                : "DojoAppMaui.Resources.Fonts.OpenSans-Regular.ttf";

            var assembly = typeof(MauiFontResolver).Assembly;
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new Exception($"Fuente no encontrada: {resourceName}");

            var bytes = new byte[stream.Length];
            _ = stream.Read(bytes, 0, bytes.Length);
            FontCache[faceName] = bytes;
            return bytes;
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var faceName = isBold ? "OpenSans-Bold" : "OpenSans";
            return new FontResolverInfo(faceName);
        }
    }
}
