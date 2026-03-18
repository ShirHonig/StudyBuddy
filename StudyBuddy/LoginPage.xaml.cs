#if ANDROID
using Plugin.Firebase.Auth;
using Firebase;
#endif

namespace StudyBuddy;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("שגיאה", "יש להזין אימייל וסיסמה", "אישור");
            return;
        }

#if ANDROID
        try
        {
            await CrossFirebaseAuth.Current.SignInWithEmailAndPasswordAsync(email, password);
            await Navigation.PushAsync(new MainPage());
        }
        catch (Exception ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            await DisplayAlert("שגיאה", $"ההתחברות נכשלה: {message}", "אישור");
        }
#else
        await Navigation.PushAsync(new MainPage());
#endif
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("שגיאה", "יש להזין אימייל וסיסמה", "אישור");
            return;
        }

        if (password.Length < 6)
        {
            await DisplayAlert("שגיאה", "הסיסמה חייבת להכיל לפחות 6 תווים", "אישור");
            return;
        }

#if ANDROID
        try
        {
            await CrossFirebaseAuth.Current.CreateUserAsync(email, password);
            await DisplayAlert("הצלחה", "החשבון נוצר בהצלחה!", "אישור");
            await Navigation.PushAsync(new MainPage());
        }
        catch (Exception ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            await DisplayAlert("שגיאה", $"ההרשמה נכשלה: {message}", "אישור");
        }
#else
        await DisplayAlert("שגיאה", "Firebase Auth is only supported on Android", "OK");
#endif
    }
}