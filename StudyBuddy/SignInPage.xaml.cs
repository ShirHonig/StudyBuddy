using StudyBuddy.Services;

namespace StudyBuddy;

public partial class SignInPage : ContentPage
{
    private readonly AuthService _auth = new();

    public SignInPage()
    {
        InitializeComponent();
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var fullName        = FullNameEntry.Text?.Trim();
        var email           = EmailEntry.Text?.Trim();
        var username        = UsernameEntry.Text?.Trim();
        var password        = PasswordEntry.Text;
        var confirmPassword = ConfirmPasswordEntry.Text;

        // ── Validation ────────────────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email)    ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("שגיאה", "יש למלא את כל השדות", "אישור");
            return;
        }

        if (password != confirmPassword)
        {
            await DisplayAlert("שגיאה", "הסיסמאות אינן תואמות", "אישור");
            return;
        }

        if (password.Length < 6)
        {
            await DisplayAlert("שגיאה", "הסיסמה חייבת להכיל לפחות 6 תווים", "אישור");
            return;
        }

        // ── Register ──────────────────────────────────────────────────────────
        RegisterButton.IsEnabled = false;
        RegisterButton.Text      = "יוצר חשבון...";

        try
        {
            await Task.Run(() => _auth.RegisterAsync(email!, password, fullName!, username!));

            // Set MainPage as the new root
            Application.Current!.MainPage = new NavigationPage(new MainPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("שגיאה", ex.Message, "אישור");
            RegisterButton.IsEnabled = true;
            RegisterButton.Text      = "צור חשבון";
        }
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
