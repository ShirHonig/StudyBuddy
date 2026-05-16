namespace StudyBuddy.Models;

public class GroupItem
{
    public string   Id             { get; set; } = "";
    public string   Subject        { get; set; } = "";
    public Color    SubjectColor   { get; set; } = Colors.Gray;
    public string   SubjectColorHex
    {
        get => $"#{(int)(SubjectColor.Red*255):X2}{(int)(SubjectColor.Green*255):X2}{(int)(SubjectColor.Blue*255):X2}";
        set => SubjectColor = Color.FromArgb(value);
    }
    public string   Title          { get; set; } = "";
    public string   Description    { get; set; } = "";
    public string   CreatorUid     { get; set; } = "";
    public string   CreatorEmail   { get; set; } = "";
    public string   CreatorInitial { get; set; } = "";
    public string   CreatorName    { get; set; } = "";
    public Color    AvatarBg       { get; set; } = Colors.LightGray;
    public Color    AvatarFg       { get; set; } = Colors.Black;
    public DateTime NextMeeting    { get; set; }
    public string   NextMeetingText => $"📅 הפגישה הבאה: {NextMeeting:dd/MM/yyyy}";
    public List<MemberInfo> MembersList { get; set; } = [];
    public string MemberCountText   => $"👥 {MembersList.Count} חברים";
    public bool   IsMember          => MembersList.Any(m => m.Uid == Services.UserSession.Uid);
    public string JoinButtonText    => IsMember ? "✓ חבר/ה" : "הצטרפות";
    public Color  JoinButtonBg      => IsMember ? Color.FromArgb("#4caf50") : Color.FromArgb("#0040a1");
    public bool   JoinButtonEnabled => !IsMember;
}
