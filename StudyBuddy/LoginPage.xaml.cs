namespace StudyBuddy;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage());
    }

    private async void OnSignInTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignInPage());
    }

    private async void OnSecretLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage());
    }

}