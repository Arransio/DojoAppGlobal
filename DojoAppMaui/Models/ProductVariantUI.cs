using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DojoAppMaui.Models
{
    public class ProductVariantUI
    {
        // Una variante es ahora solo talla. El color se elige en el pedido.
        public int Id { get; set; }
        public string Size { get; set; }

        public string Muestra => Size;
    }
}
