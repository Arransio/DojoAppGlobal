using DojoAppMaui.Models;
using DojoAppMaui.Services;
using System.Collections.ObjectModel;


namespace DojoAppMaui.ViewModels
{
    public class MainViewModel
    {
        private readonly ApiService _apiService;

        public ObservableCollection<Product> Products { get; set; } = new();

        public MainViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadData()
        {
            var campaign = await _apiService.GetActiveCampaignAsync();

            Products.Clear();

            foreach (var product in campaign.Products)
            {
                Products.Add(product);
            }
        }
    }
}
