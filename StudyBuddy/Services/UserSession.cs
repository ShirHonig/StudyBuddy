namespace StudyBuddy.Services;

/// <summary>Singleton that holds the currently signed-in user for the lifetime of the app.</summary>
public static class UserSession
{
    public static string Uid       { get; set; } = "default-user";
    public static string Email     { get; set; } = string.Empty;
    public static string FullName  { get; set; } = string.Empty;
    public static string Username  { get; set; } = string.Empty;
    public static string IdToken   { get; set; } = string.Empty;

    public static bool IsLoggedIn => !string.IsNullOrEmpty(IdToken);

    public static void Clear()
    {
        Uid = "default-user";
        Email = string.Empty;
        FullName = string.Empty;
        Username = string.Empty;
        IdToken = string.Empty;
    }
}
