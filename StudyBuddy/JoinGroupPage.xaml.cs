using System.Collections.ObjectModel;

namespace StudyBuddy;

public partial class JoinGroupPage : ContentPage
{
    private readonly Services.GroupFirestoreService _svc = new();
    private readonly List<GroupItem> _allGroups = [];
    private readonly ObservableCollection<GroupItem> _displayed = [];

    public JoinGroupPage()
    {
        InitializeComponent();
        GroupsCollection.ItemsSource = _displayed;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadGroupsAsync();
    }

    private async Task LoadGroupsAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        EmptyLabel.IsVisible = false;

        try
        {
            var groups = await _svc.GetAllGroupsAsync();
            _allGroups.Clear();
            _allGroups.AddRange(groups);
            ApplySearch(SearchEntry.Text);
        }
        catch (Exception ex)
        {
            await DisplayAlert("שגיאת טעינה", $"{ex.GetType().Name}\n{ex.Message}", "אישור");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            EmptyLabel.IsVisible = _displayed.Count == 0;
        }
    }

    private void ApplySearch(string? query)
    {
        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allGroups
            : _allGroups.Where(g =>
                g.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                g.Subject.Contains(query, StringComparison.OrdinalIgnoreCase));

        _displayed.Clear();
        foreach (var g in filtered.OrderBy(g => g.NextMeeting))
            _displayed.Add(g);

        EmptyLabel.IsVisible = _displayed.Count == 0;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplySearch(e.NewTextValue);
    }

    private async void OnGroupSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not GroupItem group) return;

        // Clear selection so item can be tapped again
        GroupsCollection.SelectedItem = null;

        // Navigate to details page
        await Navigation.PushAsync(new GroupDetailsPage(group));
    }

    private async void OnBackTapped(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnHomeNavTapped(object sender, EventArgs e) => await Navigation.PopToRootAsync();
    private async void OnTasksNavTapped(object sender, EventArgs e) => await Navigation.PushAsync(new TasksPage());
    private async void OnGroupsNavTapped(object sender, EventArgs e) => await Navigation.PopAsync();
}
