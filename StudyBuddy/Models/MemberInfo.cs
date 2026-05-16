namespace StudyBuddy.Models;

public class MemberInfo
{
    public string Uid     { get; set; } = "";
    public string Email   { get; set; } = "";
    public string Name    { get; set; } = "";
    public string Initial => string.IsNullOrEmpty(Name) ? "?" : Name[..1].ToUpper();
    public Color  AvatarBg { get; set; } = Color.FromArgb("#96a5ff");
    public Color  AvatarFg { get; set; } = Color.FromArgb("#27378a");
}
