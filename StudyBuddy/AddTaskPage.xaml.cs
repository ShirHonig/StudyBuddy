using StudyBuddy.Models;
using StudyBuddy.Services;

namespace StudyBuddy;

public partial class AddTaskPage : ContentPage
{
    private string _selectedPriority = "נמוכה";
    private readonly TaskItem? _editingTask;
    private readonly bool _isEditMode;
    private readonly TaskFirestoreService _taskService = new();

    private static readonly Dictionary<string, string> CategoryColors = new()
    {
        ["מתמטיקה"] = "#2563eb",
        ["מדעי המחשב"] = "#9333ea",
        ["היסטוריה"] = "#ea580c",
        ["אנגלית"] = "#0891b2",
        ["פיזיקה"] = "#4f46e5",
        ["ביולוגיה"] = "#16a34a",
        ["כללי"] = "#6b7280"
    };

    private static readonly List<string> CategoryList =
    [
        "מתמטיקה", "מדעי המחשב", "היסטוריה",
        "אנגלית", "פיזיקה", "ביולוגיה", "כללי"
    ];

    /// <summary>
    /// Constructor for adding a new task.
    /// </summary>
    public AddTaskPage()
    {
        InitializeComponent();
        _isEditMode = false;
        DueDatePicker.Date = DateTime.Now;
    }

    /// <summary>
    /// Constructor for editing an existing task.
    /// </summary>
    public AddTaskPage(TaskItem taskToEdit) : this()
    {
        _isEditMode = true;
        _editingTask = taskToEdit;

        PageTitleLabel.Text = "עריכת משימה";
        PageSubtitleLabel.Text = "ערוך את הפרטים ולחץ על שמור";
        BreadcrumbLastLabel.Text = "עריכת משימה";
        SectionHeaderIcon.Text = "✏️";
        SectionHeaderText.Text = "עריכת פרטים";
        SubmitButton.Text = "💾  שמור שינויים";

        TitleEntry.Text = taskToEdit.Title;
        DescriptionEditor.Text = taskToEdit.Description;

        int catIndex = CategoryList.IndexOf(taskToEdit.Category);
        if (catIndex >= 0)
            CategoryPicker.SelectedIndex = catIndex;

        if (DateTime.TryParseExact(taskToEdit.Date, "dd/MM/yyyy", null,
                System.Globalization.DateTimeStyles.None, out DateTime parsed))
            DueDatePicker.Date = parsed;

        // Show the existing priority
        UpdateAutoPriorityDisplay(taskToEdit.Priority);
    }

    /// <summary>
    /// Called when the user picks a date — auto-calculates priority.
    /// </summary>
    private void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        string autoPriority = TaskItem.CalculatePriorityFromDate(e.NewDate);
        UpdateAutoPriorityDisplay(autoPriority);
        PriorityPickerSection.IsVisible = false;
    }

    /// <summary>
    /// Shows the auto-calculated priority badge below the date.
    /// </summary>
    private void UpdateAutoPriorityDisplay(string priority)
    {
        _selectedPriority = priority;
        AutoPriorityFrame.IsVisible = true;
        AutoPriorityLabel.Text = priority;

        switch (priority)
        {
            case "גבוהה":
                AutoPriorityFrame.BackgroundColor = Color.FromArgb("#fee2e2");
                AutoPriorityLabel.TextColor = Color.FromArgb("#dc2626");
                break;
            case "בינונית":
                AutoPriorityFrame.BackgroundColor = Color.FromArgb("#fef9c3");
                AutoPriorityLabel.TextColor = Color.FromArgb("#ca8a04");
                break;
            default:
                AutoPriorityFrame.BackgroundColor = Color.FromArgb("#d1fae5");
                AutoPriorityLabel.TextColor = Color.FromArgb("#16a34a");
                break;
        }
    }

    /// <summary>
    /// Tap on the auto-priority badge → show/hide the priority picker.
    /// </summary>
    private void OnAutoPriorityTapped(object sender, EventArgs e)
    {
        PriorityPickerSection.IsVisible = !PriorityPickerSection.IsVisible;
        if (PriorityPickerSection.IsVisible)
            HighlightPriority(_selectedPriority);
    }

    private void OnPriorityLowClicked(object sender, EventArgs e)
    {
        HighlightPriority("נמוכה");
        UpdateAutoPriorityDisplay("נמוכה");
        PriorityPickerSection.IsVisible = false;
    }

    private void OnPriorityMedClicked(object sender, EventArgs e)
    {
        HighlightPriority("בינונית");
        UpdateAutoPriorityDisplay("בינונית");
        PriorityPickerSection.IsVisible = false;
    }

    private void OnPriorityHighClicked(object sender, EventArgs e)
    {
        HighlightPriority("גבוהה");
        UpdateAutoPriorityDisplay("גבוהה");
        PriorityPickerSection.IsVisible = false;
    }

    private void HighlightPriority(string priority)
    {
        _selectedPriority = priority;

        PriorityLowFrame.BorderColor = Color.FromArgb("#86efac");
        PriorityLowFrame.BackgroundColor = Color.FromArgb("#d1fae5");
        PriorityMedFrame.BorderColor = Color.FromArgb("#fde68a");
        PriorityMedFrame.BackgroundColor = Color.FromArgb("#fef9c3");
        PriorityHighFrame.BorderColor = Color.FromArgb("#fca5a5");
        PriorityHighFrame.BackgroundColor = Color.FromArgb("#fee2e2");

        switch (priority)
        {
            case "נמוכה":
                PriorityLowFrame.BorderColor = Color.FromArgb("#16a34a");
                PriorityLowFrame.BackgroundColor = Color.FromArgb("#bbf7d0");
                break;
            case "בינונית":
                PriorityMedFrame.BorderColor = Color.FromArgb("#ca8a04");
                PriorityMedFrame.BackgroundColor = Color.FromArgb("#fef08a");
                break;
            case "גבוהה":
                PriorityHighFrame.BorderColor = Color.FromArgb("#dc2626");
                PriorityHighFrame.BackgroundColor = Color.FromArgb("#fecaca");
                break;
        }
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleEntry.Text))
        {
            await DisplayAlert("שגיאה", "יש להזין כותרת למשימה", "אישור");
            return;
        }

        string category = CategoryPicker.SelectedItem as string ?? "כללי";
        CategoryColors.TryGetValue(category, out string? catColor);

        SubmitButton.IsEnabled = false;
        SubmitButton.Text = "שומר...";

        // Capture the main-thread SynchronizationContext before any await
        var uiContext = SynchronizationContext.Current;

        try
        {
            if (_isEditMode && _editingTask is not null)
            {
                _editingTask.Title = TitleEntry.Text.Trim();
                _editingTask.Description = DescriptionEditor.Text?.Trim() ?? string.Empty;
                _editingTask.Category = category;
                _editingTask.CategoryColor = catColor ?? "#9333ea";
                _editingTask.Date = DueDatePicker.Date.ToString("dd/MM/yyyy");
                _editingTask.Priority = _selectedPriority;
                _editingTask.IsUrgent = _selectedPriority == "גבוהה";

                await _taskService.UpdateTaskAsync(_editingTask);
            }
            else
            {
                var newTask = new TaskItem
                {
                    Title = TitleEntry.Text.Trim(),
                    Description = DescriptionEditor.Text?.Trim() ?? string.Empty,
                    Category = category,
                    CategoryColor = catColor ?? "#9333ea",
                    Date = DueDatePicker.Date.ToString("dd/MM/yyyy"),
                    TeacherName = string.Empty,
                    Priority = _selectedPriority,
                    IsUrgent = _selectedPriority == "גבוהה",
                    Status = "בתהליך"
                };

                await _taskService.AddTaskAsync(newTask);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Firebase] Save failed: {ex}");
            await DisplayAlert("שגיאה", $"שמירה נכשלה:\n{ex.Message}", "אישור");
            SubmitButton.IsEnabled = true;
            SubmitButton.Text = "＋  הוסף משימה";
            return;
        }

        // Navigate back on the main thread regardless of which thread Firebase resumed on
        if (uiContext != null)
            uiContext.Post(_ => Navigation.PopAsync(), null);
        else
            await Navigation.PopAsync();

        await Navigation.PopAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}