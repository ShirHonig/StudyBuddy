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

        var ctx = SynchronizationContext.Current;

        try
        {
            await Task.Run(() => _auth.RegisterAsync(email!, password, fullName!, username!));

            // Insert MainPage before SignInPage, then pop SignInPage.
            // Stack becomes [LoginPage → MainPage] so back goes to login, not registration.
            void Navigate()
            {
                Navigation.InsertPageBefore(new MainPage(), this);
                Navigation.PopAsync();
            }

            if (ctx != null)
                ctx.Post(_ => Navigate(), null);
            else
                Navigate();
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
