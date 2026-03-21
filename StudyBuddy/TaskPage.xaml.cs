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

    public TasksPage()
    {
        InitializeComponent();

        DateLabel.Text = DateTime.Now.ToString("dd/MM/yyyy, dddd", HebrewCulture);
        AllTasks = [];

        RefreshSortedList();
        UpdateStats();
        HighlightStatCard(StatTotalFrame);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        MessagingCenter.Subscribe<AddTaskPage, TaskItem>(this, "TaskAdded", (sender, task) =>
        {
            AllTasks.Add(task);
            RefreshSortedList();
            UpdateStats();
        });

        MessagingCenter.Subscribe<AddTaskPage, TaskItem>(this, "TaskEdited", (sender, task) =>
        {
            RefreshSortedList();
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

            RefreshSortedList();
            UpdateStats();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Firebase] Load failed: {ex}");
            await DisplayAlert("שגיאה", "טעינת המשימות מ-Firebase נכשלה.", "אישור");
        }
    }

    // ==================== SORTING ====================

    private void RefreshSortedList()
    {
        var sorted = AllTasks
            .OrderBy(t => t.IsCompleted ? 1 : 0)
            .ThenBy(t => t.PrioritySortOrder)
            .ThenBy(t => t.ParsedDate)
            .ToList();
        TasksListView.ItemsSource = new ObservableCollection<TaskItem>(sorted);
    }

    private void DisplayFiltered(IEnumerable<TaskItem> items)
    {
        var sorted = items
            .OrderBy(t => t.IsCompleted ? 1 : 0)
            .ThenBy(t => t.PrioritySortOrder)
            .ThenBy(t => t.ParsedDate)
            .ToList();
        TasksListView.ItemsSource = new ObservableCollection<TaskItem>(sorted);
    }

    private void UpdateStats()
    {
        TotalCountLabel.Text = AllTasks.Count.ToString();
        InProgressCountLabel.Text = AllTasks.Count(t => !t.IsCompleted).ToString();
        CompletedCountLabel.Text = AllTasks.Count(t => t.IsCompleted).ToString();
        UrgentCountLabel.Text = AllTasks.Count(t => t.IsUrgent).ToString();
    }

    // ==================== STAT CARD HIGHLIGHT ====================

    /// <summary>
    /// Sets a white border on the selected stat card and clears the others.
    /// </summary>
    private void HighlightStatCard(Frame selectedFrame)
    {
        StatTotalFrame.BorderColor = Colors.Transparent;
        StatInProgressFrame.BorderColor = Colors.Transparent;
        StatCompletedFrame.BorderColor = Colors.Transparent;
        StatUrgentFrame.BorderColor = Colors.Transparent;

        selectedFrame.BorderColor = Colors.White;
    }

    // ==================== TAB HIGHLIGHT ====================

    /// <summary>
    /// Sets the purple active style on the selected tab button and resets the others.
    /// </summary>
    private void HighlightTab(Button selectedButton)
    {
        // Reset all tabs
        TabAllButton.BackgroundColor = Color.FromArgb("#f3f0ff");
        TabAllButton.TextColor = Color.FromArgb("#9333ea");
        TabTodayButton.BackgroundColor = Color.FromArgb("#f3f0ff");
        TabTodayButton.TextColor = Color.FromArgb("#9333ea");
        TabWeekButton.BackgroundColor = Color.FromArgb("#f3f0ff");
        TabWeekButton.TextColor = Color.FromArgb("#9333ea");

        // Highlight selected
        selectedButton.BackgroundColor = Color.FromArgb("#9333ea");
        selectedButton.TextColor = Colors.White;
    }

    // ==================== CHECKBOX (mark complete) ====================

    private async void OnCheckBoxChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.BindingContext is TaskItem task)
        {
            task.IsCompleted = e.Value;
            task.Status = e.Value ? "הושלם" : "בתהליך";

            RefreshSortedList();
            UpdateStats();
            await _taskService.UpdateTaskAsync(task);
        }
    }

    // ==================== TAP TO EDIT ====================

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

    // ==================== SWIPE ACTIONS ====================

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
            RefreshSortedList();
            UpdateStats();
        }
    }

    // ==================== STAT CARD TAPS (filter) ====================

    private void OnStatTotalTapped(object sender, EventArgs e)
    {
        HighlightStatCard(StatTotalFrame);
        RefreshSortedList();
    }

    private void OnStatInProgressTapped(object sender, EventArgs e)
    {
        HighlightStatCard(StatInProgressFrame);
        DisplayFiltered(AllTasks.Where(t => !t.IsCompleted));
    }

    private void OnStatCompletedTapped(object sender, EventArgs e)
    {
        HighlightStatCard(StatCompletedFrame);
        DisplayFiltered(AllTasks.Where(t => t.IsCompleted));
    }

    private void OnStatUrgentTapped(object sender, EventArgs e)
    {
        HighlightStatCard(StatUrgentFrame);
        DisplayFiltered(AllTasks.Where(t => t.IsUrgent));
    }

    // ==================== TAB FILTERS ====================

    private void OnTabAllClicked(object sender, EventArgs e)
    {
        HighlightTab(TabAllButton);
        RefreshSortedList();
    }

    private void OnTabTodayClicked(object sender, EventArgs e)
    {
        HighlightTab(TabTodayButton);
        string today = DateTime.Now.ToString("dd/MM/yyyy");
        DisplayFiltered(AllTasks.Where(t => t.Date == today));
    }

    private void OnTabWeekClicked(object sender, EventArgs e)
    {
        HighlightTab(TabWeekButton);
        DateTime weekFromNow = DateTime.Now.AddDays(7);
        DisplayFiltered(AllTasks.Where(t => t.ParsedDate <= weekFromNow));
    }

    // ==================== NAVIGATION ====================

    private async void OnAddTaskClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddTaskPage());
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}