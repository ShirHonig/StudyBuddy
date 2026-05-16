namespace StudyBuddy.Models;

public class TaskItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = "#9333ea";
    public string Date { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsUrgent { get; set; }
    public string Status { get; set; } = "בתהליך";
    public string Priority { get; set; } = "נמוכה";

    public int PrioritySortOrder => Priority switch
    {
        "גבוהה" => 0,
        "בינונית" => 1,
        _ => 2
    };

    public DateTime ParsedDate =>
        DateTime.TryParseExact(Date, "dd/MM/yyyy", null,
            System.Globalization.DateTimeStyles.None, out DateTime d)
            ? d
            : DateTime.MaxValue;

    public string PriorityColor => Priority switch
    {
        "גבוהה" => "#dc2626",
        "בינונית" => "#ca8a04",
        _ => "#16a34a"
    };

    public string PriorityBackgroundColor => Priority switch
    {
        "גבוהה" => "#fee2e2",
        "בינונית" => "#fef9c3",
        _ => "#d1fae5"
    };

    public static string CalculatePriorityFromDate(DateTime dueDate)
    {
        int daysUntil = (dueDate.Date - DateTime.Now.Date).Days;
        return daysUntil switch
        {
            <= 2 => "גבוהה",
            <= 7 => "בינונית",
            _ => "נמוכה"
        };
    }
}