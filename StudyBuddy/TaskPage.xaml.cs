using System.Collections.ObjectModel;
using System.Globalization;
using StudyBuddy.Models;
using StudyBuddy.Services;

namespace StudyBuddy;

public partial class TasksPage : ContentPage
{
    private static readonly CultureInfo HebrewCulture = new("he-IL");
    private readonly TaskFirestoreService _taskService = new();

    public ObservableCollection<TaskItem> AllTasks { get; set; }
    private string _searchQuery = "";
    private Func<IEnumerable<TaskItem>, IEnumerable<TaskItem>> _activeFilter = items => items;

    public TasksPage()
    {
        InitializeComponent();

        DateLabel.Text = DateTime.Now.ToString("dd/MM/yyyy, dddd", HebrewCulture);
        AllTasks = [];

        ApplyCurrentFilters();
        UpdateStats();
        HighlightStatCard(StatTotalFrame);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        MessagingCenter.Subscribe<AddTaskPage, TaskItem>(this, "TaskAdded", (sender, task) =>
        {
            AllTasks.Add(task);
            ApplyCurrentFilters();
            UpdateStats();
        });

        MessagingCenter.Subscribe<AddTaskPage, TaskItem>(this, "TaskEdited", (sender, task) =>
        {
            ApplyCurrentFilters();
            UpdateStats();
        });

        await LoadTasksAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        MessagingCenter.Unsubscribe<AddTaskPage, TaskItem>(this, "TaskAdded");
        MessagingCenter.Unsubscribe<AddTaskPage, TaskItem>(this, "TaskEdited");
    }

    private async Task LoadTasksAsync()
    {
        try
        {
            var tasks = await _taskService.GetTasksAsync();
            AllTasks.Clear();
            foreach (var task in tasks)
                AllTasks.Add(task);

            ApplyCurrentFilters();
            UpdateStats();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Firebase] Load failed: {ex}");
            await DisplayAlert("שגיאה", "טעינת המשימות מ-Firebase נכשלה.", "אישור");
        }
    }

    private void ApplyCurrentFilters()
    {
        var items = _activeFilter(AllTasks);
        var searched = ApplySearch(items);
        var sorted = searched
            .OrderBy(t => t.IsCompleted ? 1 : 0)
            .ThenBy(t => t.PrioritySortOrder)
            .ThenBy(t => t.ParsedDate)
            .ToList();
        TasksListView.ItemsSource = new ObservableCollection<TaskItem>(sorted);
    }

    private IEnumerable<TaskItem> ApplySearch(IEnumerable<TaskItem> items)
    {
        if (string.IsNullOrWhiteSpace(_searchQuery))
            return items;
        return items.Where(t =>
            (t.Title?.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (t.Category?.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (t.TeacherName?.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ?? false));
    }

    private void UpdateStats()
    {
        TotalCountLabel.Text = AllTasks.Count.ToString();
        InProgressCountLabel.Text = AllTasks.Count(t => !t.IsCompleted).ToString();
        CompletedCountLabel.Text = AllTasks.Count(t => t.IsCompleted).ToString();
        UrgentCountLabel.Text = AllTasks.Count(t => t.IsUrgent).ToString();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchQuery = e.NewTextValue ?? "";
        ApplyCurrentFilters();
    }

    private void HighlightStatCard(Frame selectedFrame)
    {
        StatTotalFrame.BorderColor = Colors.Transparent;
        StatInProgressFrame.BorderColor = Colors.Transparent;
        StatCompletedFrame.BorderColor = Colors.Transparent;
        StatUrgentFrame.BorderColor = Colors.Transparent;

        selectedFrame.BorderColor = Colors.White;
    }

    private void HighlightTab(Button selectedButton)
    {
        TabAllButton.BackgroundColor = Color.FromArgb("#e5e8eb");
        TabAllButton.TextColor = Color.FromArgb("#424654");
        TabTodayButton.BackgroundColor = Color.FromArgb("#e5e8eb");
        TabTodayButton.TextColor = Color.FromArgb("#424654");
        TabWeekButton.BackgroundColor = Color.FromArgb("#e5e8eb");
        TabWeekButton.TextColor = Color.FromArgb("#424654");

        selectedButton.BackgroundColor = Color.FromArgb("#0040a1");
        selectedButton.TextColor = Colors.White;
    }

    private async void OnCheckBoxChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.BindingContext is TaskItem task)
        {
            task.IsCompleted = e.Value;
            task.Status = e.Value ? "הושלם" : "בתהליך";

            ApplyCurrentFilters();
            UpdateStats();
            await _taskService.UpdateTaskAsync(task);
        }
    }

    private async void OnTaskCardTapped(object sender, EventArgs e)
    {
        TaskItem? task = null;

        if (e is TappedEventArgs tapped && tapped.Parameter is TaskItem t1)
            task = t1;
        else if (sender is BindableObject bo && bo.BindingContext is TaskItem t2)
            task = t2;

        if (task is null)
            return;

        bool wantToEdit = await DisplayAlert(
            "עריכת משימה",
            $"האם תרצה לערוך את המשימה \"{task.Title}\"?",
            "כן", "לא");

        if (wantToEdit)
            await Navigation.PushAsync(new AddTaskPage(task));
    }

    private async void OnSwipeEditInvoked(object sender, EventArgs e)
    {
        TaskItem? task = null;

        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is TaskItem t)
            task = t;

        if (task is null)
            return;

        await Navigation.PushAsync(new AddTaskPage(task));
    }

    private async void OnSwipeDeleteInvoked(object sender, EventArgs e)
    {
        TaskItem? task = null;

        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is TaskItem t)
            task = t;

        if (task is null)
            return;

        bool confirm = await DisplayAlert(
            "מחיקת משימה",
            $"האם אתה בטוח שברצונך למחוק את \"{task.Title}\"?",
            "מחק", "ביטול");

        if (confirm)
        {
            await _taskService.DeleteTaskAsync(task.Id);
            AllTasks.Remove(task);
            ApplyCurrentFilters();
            UpdateStats();
        }
    }

    private void OnStatTotalTapped(object sender, EventArgs e)
    {
        HighlightStatCard(StatTotalFrame);
        _activeFilter = items => items;
        ApplyCurrentFilters();
    }

    private void OnStatInProgressTapped(object sender, EventArgs e)
    {
        HighlightStatCard(StatInProgressFrame);
        _activeFilter = items => items.Where(t => !t.IsCompleted);
        ApplyCurrentFilters();
    }

    private void OnStatCompletedTapped(object sender, EventArgs e)
    {
        HighlightStatCard(StatCompletedFrame);
        _activeFilter = items => items.Where(t => t.IsCompleted);
        ApplyCurrentFilters();
    }

    private void OnStatUrgentTapped(object sender, EventArgs e)
    {
        HighlightStatCard(StatUrgentFrame);
        _activeFilter = items => items.Where(t => t.IsUrgent);
        ApplyCurrentFilters();
    }

    private void OnTabAllClicked(object sender, EventArgs e)
    {
        HighlightTab(TabAllButton);
        _activeFilter = items => items;
        ApplyCurrentFilters();
        HighlightStatCard(StatTotalFrame);
    }

    private void OnTabTodayClicked(object sender, EventArgs e)
    {
        HighlightTab(TabTodayButton);
        string today = DateTime.Now.ToString("dd/MM/yyyy");
        _activeFilter = items => items.Where(t => t.Date == today);
        ApplyCurrentFilters();
    }

    private void OnTabWeekClicked(object sender, EventArgs e)
    {
        HighlightTab(TabWeekButton);
        DateTime weekFromNow = DateTime.Now.AddDays(7);
        _activeFilter = items => items.Where(t => t.ParsedDate <= weekFromNow);
        ApplyCurrentFilters();
    }

    private async void OnAddTaskClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddTaskPage());
    }

    private async void OnHomeNavTapped(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnGroupsNavTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new GroupsPage());
    }

    private async void OnCalendarNavTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CalendarPage());
    }
}
