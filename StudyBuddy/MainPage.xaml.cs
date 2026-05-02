using System.Collections.ObjectModel;
using System.Globalization;
using StudyBuddy.Models;
using StudyBuddy.Services;

namespace StudyBuddy;

public partial class MainPage : ContentPage
{
    private static readonly CultureInfo HebrewCulture = new("he-IL");
    private readonly TaskFirestoreService _taskSvc = new();
    private readonly GroupFirestoreService _groupSvc = new();

    public MainPage()
    {
        InitializeComponent();

        var name = UserSession.FullName;
        if (!string.IsNullOrWhiteSpace(name))
        {
            GreetingLabel.Text  = $"היי, {name}!";
            AvatarLabel.Text    = name[0].ToString();
        }

        DateLabel.Text = DateTime.Now.ToString("dd/MM/yyyy, dddd", HebrewCulture);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        try
        {
            var tasks = await _taskSvc.GetTasksAsync();
            var pending = tasks.Where(t => !t.IsCompleted).ToList();

            TaskCountLabel.Text = $"{tasks.Count} משימות";
            TaskSubLabel.Text   = $"{pending.Count} פתוחות";

            // Show up to 5 upcoming non-completed tasks
            var upcoming = pending
                .OrderBy(t => t.ParsedDate)
                .Take(5)
                .ToList();
            UpcomingTasksCollection.ItemsSource = new ObservableCollection<TaskItem>(upcoming);
        }
        catch { /* silent — dashboard is best-effort */ }

        try
        {
            var groups = await _groupSvc.GetMyGroupsAsync();
            GroupCountLabel.Text = $"{groups.Count} קבוצות";
        }
        catch { /* silent */ }
    }

    private async void OnTasksClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TasksPage());
    }

    private async void OnGroupsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new GroupsPage());
    }

    private async void OnScheduleClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CalendarPage());
    }

    private async void OnCalendarNavTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CalendarPage());
    }
}
