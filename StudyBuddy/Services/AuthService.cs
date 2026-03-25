using System.Net.Http.Json;
using System.Text.Json;

namespace StudyBuddy.Services;

public class AuthService
{
    private const string ApiKey   = "AIzaSyDRqnwE4RRuEmJOJaXWY4_mVhrX5g9Rl80";
    private const string SignUpUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={ApiKey}";
    private const string SignInUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={ApiKey}";

    private static readonly HttpClient Http = new();
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.General);

    // ── Register ─────────────────────────────────────────────────────────────

    public async Task RegisterAsync(string email, string password, string fullName, string username)
    {
        var body = new { email, password, returnSecureToken = true };
        var response = await Http.PostAsJsonAsync(SignUpUrl, body, JsonOpts);
        var json = await response.Content.ReadAsStringAsync();
        System.Diagnostics.Debug.WriteLine($"[Auth] SignUp status={response.StatusCode} body={json}");

        if (!response.IsSuccessStatusCode)
        {
            var error = ParseFirebaseError(json);
            throw new Exception(error);
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Store session
        UserSession.Uid      = root.GetProperty("localId").GetString() ?? "default-user";
        UserSession.IdToken  = root.GetProperty("idToken").GetString() ?? string.Empty;
        UserSession.Email    = email;
        UserSession.FullName = fullName;
        UserSession.Username = username;

        // Save profile to Firestore
        await SaveProfileAsync(fullName, username, email);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    public async Task LoginAsync(string email, string password)
    {
        var body = new { email, password, returnSecureToken = true };
        var response = await Http.PostAsJsonAsync(SignInUrl, body, JsonOpts);
        var json = await response.Content.ReadAsStringAsync();
        System.Diagnostics.Debug.WriteLine($"[Auth] SignIn status={response.StatusCode} body={json}");

        if (!response.IsSuccessStatusCode)
        {
            var error = ParseFirebaseError(json);
            throw new Exception(error);
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        UserSession.Uid     = root.GetProperty("localId").GetString() ?? "default-user";
        UserSession.IdToken = root.GetProperty("idToken").GetString() ?? string.Empty;
        UserSession.Email   = email;
    }

    // ── Save user profile to Firestore ────────────────────────────────────────

    private static async Task SaveProfileAsync(string fullName, string username, string email)
    {
        var projectId = "studdybuddy-app522";
        var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/users/{UserSession.Uid}?key={ApiKey}";

        var body = new
        {
            fields = new
            {
                FullName = new { stringValue = fullName },
                Username = new { stringValue = username },
                Email    = new { stringValue = email    },
                CreatedAt= new { stringValue = DateTime.UtcNow.ToString("o") }
            }
        };

        var response = await Http.PatchAsJsonAsync(url, body, JsonOpts);
        System.Diagnostics.Debug.WriteLine($"[Auth] Profile save status={response.StatusCode}");
    }

    // ── Error helper ──────────────────────────────────────────────────────────

    private static string ParseFirebaseError(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var code = doc.RootElement
                          .GetProperty("error")
                          .GetProperty("message")
                          .GetString() ?? "UNKNOWN";

            return code switch
            {
                "EMAIL_EXISTS"              => "כתובת האימייל כבר בשימוש",
                "INVALID_EMAIL"             => "כתובת האימייל אינה תקינה",
                "WEAK_PASSWORD : Password should be at least 6 characters" => "הסיסמה חלשה מדי",
                "EMAIL_NOT_FOUND"           => "האימייל לא נמצא",
                "INVALID_PASSWORD"          => "סיסמה שגויה",
                "USER_DISABLED"             => "החשבון הושבת",
                "INVALID_LOGIN_CREDENTIALS" => "אימייל או סיסמה שגויים",
                _ => $"שגיאה: {code}"
            };
        }
        catch
        {
            return "אירעה שגיאה. נסה שוב.";
        }
    }
}
