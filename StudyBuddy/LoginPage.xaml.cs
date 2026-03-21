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
    // For demonstration purposes, the login button simply navigates to the MainPage.


}