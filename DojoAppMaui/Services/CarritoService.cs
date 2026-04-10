using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DojoAppMaui.Services;

using DojoAppMaui.Models;
using DojoAppMaui.Views;

public class CarritoService
{
    private List<CartItem> cart = new();

    public List<CartItem> GetItems()
    {
        return cart;
    }

    public void AddItem(Product product, string size)
    {
        cart.Add(new CartItem
        {
            Product = product,
            Size = size
        });
    }

    public void RemoveItem(CartItem item)
    {
        cart.Remove(item);
    }

    public double GetTotal()
    {
        return cart.Sum(x => x.Product.Price);
    }

    public int GetCount()
    {
        return cart.Count;
    }
}