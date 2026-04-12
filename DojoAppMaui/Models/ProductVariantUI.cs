using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DojoAppMaui.Models
{
    public class ProductVariantUI
    {
        public int Id { get; set; }
        public string Size { get; set; }

        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }

        public string Muestra => $"{Size} - {PrimaryColor} / {SecondaryColor}";
    }
}
