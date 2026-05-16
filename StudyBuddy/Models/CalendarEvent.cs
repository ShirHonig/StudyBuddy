namespace StudyBuddy.Models;

public class CalendarEvent
{
    public string Title       { get; set; } = "";
    public string Subtitle    { get; set; } = "";
    public string TimeDisplay { get; set; } = "";
    public bool   HasTime     { get; set; }
    public string Icon        { get; set; } = "";
    public Color  IconBg      { get; set; } = Colors.LightGray;
    public Color  AccentColor { get; set; } = Colors.Gray;
    public DateTime Date      { get; set; }
    public string EventType   { get; set; } = "task";
}
