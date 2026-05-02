using System.Collections.ObjectModel;
using System.Globalization;
using StudyBuddy.Models;
using StudyBuddy.Services;

namespace StudyBuddy;

public class CalendarEvent
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string TimeDisplay { get; set; } = "";
    public bool HasTime { get; set; }
    public string Icon { get; set; } = "";
    public Color IconBg { get; set; } = Colors.LightGray;
    public Color AccentColor { get; set; } = Colors.Gray;
    public DateTime Date { get; set; }
    public string EventType { get; set; } = "task"; // "task" or "meeting"
}

public class CalendarDateGroup : ObservableCollection<CalendarEvent>
{
    public DateTime Date { get; set; }
    public string DateDisplay { get; set; } = "";
    public string DayName { get; set; } = "";

    public CalendarDateGroup(DateTime date, IEnumerable<CalendarEvent> events) : base(events)
    {
        Date = date;
        DateDisplay = date.ToString("dd/MM");
        DayName = date.ToString("dddd", new CultureInfo("he-IL"));
    }
}

public partial class CalendarPage : ContentPage
{
    private static readonly CultureInfo HebrewCulture = new("he-IL");
    private readonly TaskFirestoreService _taskSvc = new();
    private readonly GroupFirestoreService _groupSvc = new();
    private readonly ObservableCollection<CalendarDateGroup> _groupedEvents = [];
    private DateTime _currentMonth;

    public CalendarPage()
    {
        InitializeComponent();
        EventsCollection.ItemsSource = _groupedEvents;

        var name = UserSession.FullName;
        if (!string.IsNullOrWhiteSpace(name))
            AvatarLabel.Text = name[0].ToString();

        _currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        BuildCalendarGrid();
    }

    private void BuildCalendarGrid()
    {
        MonthYearLabel.Text = _currentMonth.ToString("MMMM yyyy", HebrewCulture);
        CalendarGrid.Children.Clear();

        int daysInMonth = DateTime.DaysInMonth(_currentMonth.Year, _currentMonth.Month);
        var firstDay = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);

        // Hebrew calendar: Sunday=0 is rightmost (column 6), Saturday=6 is leftmost (column 0)
        int startColumn = 6 - (int)firstDay.DayOfWeek;

        int row = 0;
        int col = startColumn;

        for (int day = 1; day <= daysInMonth; day++)
        {
            var currentDate = new DateTime(_currentMonth.Year, _currentMonth.Month, day);
            bool isToday = currentDate.Date == DateTime.Today;

            View dayView;
            if (isToday)
            {
                var border = new Border
                {
                    WidthRequest = 36,
                    HeightRequest = 36,
                    BackgroundColor = Color.FromArgb("#0040a1"),
                    StrokeThickness = 0,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 18 },
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Content = new Label
                    {
                        Text = day.ToString(),
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                };
                dayView = border;
            }
            else
            {
                dayView = new Label
                {
                    Text = day.ToString(),
                    FontSize = 14,
                    TextColor = Color.FromArgb("#181c1e"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
            }

            Grid.SetRow(dayView, row);
            Grid.SetColumn(dayView, col);
            CalendarGrid.Children.Add(dayView);

            col--;
            if (col < 0)
            {
                col = 6;
                row++;
            }
        }
    }

    private void OnPrevMonthTapped(object sender, EventArgs e)
    {
        _currentMonth = _currentMonth.AddMonths(-1);
        BuildCalendarGrid();
    }

    private void OnNextMonthTapped(object sender, EventArgs e)
    {
        _currentMonth = _currentMonth.AddMonths(1);
        BuildCalendarGrid();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadEventsAsync();
    }

    private async Task LoadEventsAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        EmptyState.IsVisible = false;

        var allEvents = new List<CalendarEvent>();
        var today = DateTime.Today;
        var weekEnd = today.AddDays(7);

        try
        {
            // Load tasks
            var tasks = await _taskSvc.GetTasksAsync();
            var pendingTasks = tasks.Where(t => !t.IsCompleted && t.ParsedDate >= today)
                                    .OrderBy(t => t.ParsedDate);

            foreach (var task in pendingTasks)
            {
                allEvents.Add(new CalendarEvent
                {
                    Title = task.Title,
                    Subtitle = task.Category,
                    Date = task.ParsedDate,
                    HasTime = false,
                    Icon = "\u2705",
                    IconBg = Color.FromArgb("#dae2ff"),
                    AccentColor = Color.FromArgb(task.PriorityColor),
                    EventType = "task"
                });
            }

            // Count tasks this week
            var tasksThisWeek = pendingTasks.Count(t => t.ParsedDate <= weekEnd);
            TasksCountLabel.Text = tasksThisWeek.ToString();
        }
        catch { /* silent */ }

        try
        {
            // Load group meetings
            var groups = await _groupSvc.GetMyGroupsAsync();
            var upcomingMeetings = groups.Where(g => g.NextMeeting >= today)
                                         .OrderBy(g => g.NextMeeting);

            foreach (var group in upcomingMeetings)
            {
                allEvents.Add(new CalendarEvent
                {
                    Title = group.Title,
                    Subtitle = group.Subject,
                    Date = group.NextMeeting,
                    TimeDisplay = group.NextMeeting.ToString("HH:mm"),
                    HasTime = group.NextMeeting.Hour > 0 || group.NextMeeting.Minute > 0,
                    Icon = "\U0001F465",
                    IconBg = Color.FromArgb("#e8f5e9"),
                    AccentColor = group.SubjectColor,
                    EventType = "meeting"
                });
            }

            // Count meetings this week
            var meetingsThisWeek = upcomingMeetings.Count(g => g.NextMeeting <= weekEnd);
            MeetingsCountLabel.Text = meetingsThisWeek.ToString();
        }
        catch { /* silent */ }

        // Group events by date
        _groupedEvents.Clear();

        var grouped = allEvents
            .OrderBy(e => e.Date)
            .GroupBy(e => e.Date.Date)
            .Select(g => new CalendarDateGroup(g.Key, g));

        foreach (var group in grouped)
            _groupedEvents.Add(group);

        LoadingIndicator.IsVisible = false;
        LoadingIndicator.IsRunning = false;
        EmptyState.IsVisible = _groupedEvents.Count == 0;
    }

    private async void OnHomeNavTapped(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnTasksNavTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TasksPage());
    }

    private async void OnGroupsNavTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new GroupsPage());
    }
}
