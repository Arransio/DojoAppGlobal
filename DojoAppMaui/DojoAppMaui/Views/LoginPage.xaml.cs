namespace DojoAppMaui.Views;

public partial class LoginPage : ContentPage
{
    public string Username { get; set; }
    public string Password { get; set; }

    public LoginPage()
    {
        InitializeComponent();
        BindingContext = this;
    }
}