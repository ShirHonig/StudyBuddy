using System.Collections.ObjectModel;
using StudyBuddy.Models;

namespace StudyBuddy;

public partial class GroupsPage : ContentPage
{
    private readonly List<GroupItem> _allGroups = [];
    private readonly ObservableCollection<GroupItem> _displayed = [];
    private readonly ObservableCollection<GroupItem> _myGroups = [];
    private readonly Services.GroupFirestoreService _svc = new();
    private string _activeFilter = "הכל";

    public GroupsPage()
    {
        InitializeComponent();
        GroupsCollection.ItemsSource = _displayed;
        MyGroupsCollection.ItemsSource = _myGroups;

        MessagingCenter.Subscribe<CreateGroupPage, GroupItem>(this, "GroupCreated", (_, g) =>
        {
            _allGroups.Add(g);
            ApplyFilterAndSort();
            RefreshMyGroups();
        });

        MessagingCenter.Subscribe<SearchGroupPage, GroupItem>(this, "GroupJoined", (_, g) =>
        {
            if (_allGroups.All(x => x.Id != g.Id))
                _allGroups.Add(g);
            else
            {
                var existing = _allGroups.FirstOrDefault(x => x.Id == g.Id);
                if (existing != null)
                {
                    existing.MembersList.Clear();
                    existing.MembersList.AddRange(g.MembersList);
                }
            }
            ApplyFilterAndSort();
            RefreshMyGroups();
        });

        MessagingCenter.Subscribe<GroupDetailsPage, GroupItem>(this, "GroupJoined", (_, g) =>
        {
            if (_allGroups.All(x => x.Id != g.Id))
                _allGroups.Add(g);
            else
            {
                var existing = _allGroups.FirstOrDefault(x => x.Id == g.Id);
                if (existing != null)
                {
                    existing.MembersList.Clear();
                    existing.MembersList.AddRange(g.MembersList);
                }
            }
            ApplyFilterAndSort();
            RefreshMyGroups();
        });
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadGroupsAsync();
    }

    private async Task LoadGroupsAsync()
    {
        _allGroups.Clear();
        ApplyFilterAndSort();
        try
        {
            var groups = await _svc.GetAllGroupsAsync();
            _allGroups.AddRange(groups);
            ApplyFilterAndSort();
            RefreshMyGroups();
        }
        catch (Exception ex)
        {
            await DisplayAlert("שגיאת טעינה", ex.Message, "אישור");
        }
    }

    private void RefreshMyGroups()
    {
        _myGroups.Clear();
        var myGroups = _allGroups
            .Where(g => g.MembersList.Any(m => m.Uid == Services.UserSession.Uid))
            .OrderBy(g => g.NextMeeting);

        foreach (var g in myGroups)
            _myGroups.Add(g);

        MyGroupsSection.IsVisible = _myGroups.Count > 0;
    }

    private void ApplyFilterAndSort()
    {
        var filtered = _activeFilter == "הכל"
            ? _allGroups
            : _allGroups.Where(g => g.Subject == _activeFilter);

        _displayed.Clear();
        foreach (var g in filtered.OrderBy(g => g.NextMeeting))
            _displayed.Add(g);
    }

    private async void OnMyGroupTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Frame frame || frame.BindingContext is not GroupItem group) return;
        await Navigation.PushAsync(new GroupDetailsPage(group));
    }

    private async void OnGroupCardTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Frame frame || frame.BindingContext is not GroupItem group) return;
        await Navigation.PushAsync(new GroupDetailsPage(group));
    }

    private async void OnCreateGroupClicked(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new CreateGroupPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("שגיאת ניווט", ex.ToString(), "אישור");
        }
    }

    private async void OnJoinGroupClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SearchGroupPage());
    }

    private void OnChipClicked(object sender, EventArgs e)
    {
        foreach (var child in ChipsContainer.Children)
        {
            if (child is Button btn)
            {
                btn.BackgroundColor = Color.FromArgb("#e5e8eb");
                btn.TextColor       = Color.FromArgb("#424654");
            }
        }

        if (sender is Button tapped)
        {
            tapped.BackgroundColor = Color.FromArgb("#0040a1");
            tapped.TextColor       = Colors.White;
            _activeFilter = tapped.Text;
        }

        ApplyFilterAndSort();
    }

    private async void OnHomeNavTapped(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnTasksNavTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TasksPage());
    }

    private async void OnCalendarNavTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CalendarPage());
    }
}
