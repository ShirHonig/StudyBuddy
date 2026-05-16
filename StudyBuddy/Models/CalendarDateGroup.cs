using System.Collections.ObjectModel;
using System.Globalization;

namespace StudyBuddy.Models;

public class CalendarDateGroup : ObservableCollection<CalendarEvent>
{
    public DateTime Date        { get; set; }
    public string   DateDisplay { get; set; } = "";
    public string   DayName     { get; set; } = "";

    public CalendarDateGroup(DateTime date, IEnumerable<CalendarEvent> events) : base(events)
    {
        Date        = date;
        DateDisplay = date.ToString("dd/MM");
        DayName     = date.ToString("dddd", new CultureInfo("he-IL"));
    }
}
