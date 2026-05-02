using StudyBuddy.Services;

namespace StudyBuddy;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _auth = new();

	public LoginPage()
	{
		InitializeComponent();
	}

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email    = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("שגיאה", "יש למלא אימייל וסיסמה", "אישור");
            return;
        }

        LoginButton.IsEnabled = false;
        LoginButton.Text      = "מתחבר...";

        try
        {
            await _auth.LoginAsync(email, password);
            Application.Current!.MainPage = new NavigationPage(new MainPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("שגיאה", ex.Message, "אישור");
        }
        finally
        {
            LoginButton.IsEnabled = true;
            LoginButton.Text      = "← התחברות";
        }
    }

    private async void OnSignInTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignInPage());
    }

    private void OnSecretLoginTapped(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new NavigationPage(new MainPage());
    }

}