namespace StudyBuddy.Models;

public class TaskItem
{
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

    /// <summary>
    /// Sorting: גבוהה=0, בינונית=1, נמוכה=2
    /// </summary>
    public int PrioritySortOrder => Priority switch
    {
        "גבוהה" => 0,
        "בינונית" => 1,
        _ => 2
    };

    /// <summary>
    /// Returns the parsed due date, or DateTime.MaxValue if invalid.
    /// </summary>
    public DateTime ParsedDate =>
        DateTime.TryParseExact(Date, "dd/MM/yyyy", null,
            System.Globalization.DateTimeStyles.None, out DateTime d)
            ? d
            : DateTime.MaxValue;

    /// <summary>
    /// Priority badge color: red=גבוהה, yellow=בינונית, green=נמוכה
    /// </summary>
    public string PriorityColor => Priority switch
    {
        "גבוהה" => "#dc2626",
        "בינונית" => "#ca8a04",
        _ => "#16a34a"
    };

    /// <summary>
    /// Priority badge background: light red/yellow/green
    /// </summary>
    public string PriorityBackgroundColor => Priority switch
    {
        "גבוהה" => "#fee2e2",
        "בינונית" => "#fef9c3",
        _ => "#d1fae5"
    };

    /// <summary>
    /// Calculates priority based on days until due date.
    /// ≤2 days = גבוהה, ≤7 days = בינונית, >7 days = נמוכה
    /// </summary>
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