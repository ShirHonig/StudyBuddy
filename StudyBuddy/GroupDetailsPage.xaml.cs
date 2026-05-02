using System.Collections.ObjectModel;

namespace StudyBuddy;

public partial class GroupDetailsPage : ContentPage
{
    private readonly GroupItem _group;
    private readonly Services.GroupFirestoreService _svc = new();
    private readonly ObservableCollection<MemberInfo> _members = [];

    public GroupDetailsPage(GroupItem group)
    {
        InitializeComponent();
        _group = group;
        MembersCollection.ItemsSource = _members;
        PopulateUI();
    }

    private void PopulateUI()
    {
        // Subject with accent colors
        SubjectLabel.Text = _group.Subject;
        SubjectAccent.BackgroundColor = _group.SubjectColor;
        SubjectChip.BackgroundColor = _group.SubjectColor.WithAlpha(0.25f);
        SubjectLabel.TextColor = _group.SubjectColor;

        // Title & Description
        TitleLabel.Text = _group.Title;
        DescriptionLabel.Text = _group.Description;

        // Meeting Date
        MeetingDateLabel.Text = _group.NextMeeting.ToString("dd/MM/yyyy HH:mm");

        // Owner
        OwnerNameLabel.Text = _group.CreatorName;
        OwnerEmailLabel.Text = _group.CreatorEmail;
        OwnerInitialLabel.Text = _group.CreatorInitial;
        OwnerAvatar.BackgroundColor = _group.AvatarBg;
        OwnerInitialLabel.TextColor = _group.AvatarFg;

        // Members (exclude owner to avoid duplication)
        _members.Clear();
        foreach (var m in _group.MembersList.Where(m => m.Uid != _group.CreatorUid))
        {
            m.AvatarBg = _group.AvatarBg;
            m.AvatarFg = _group.AvatarFg;
            _members.Add(m);
        }

        MemberCountLabel.Text = $"({_group.MembersList.Count})";

        // Join button visibility
        var isMember = _group.MembersList.Any(m => m.Uid == Services.UserSession.Uid);
        JoinButtonContainer.IsVisible = !isMember;
        MemberBadge.IsVisible = isMember;
    }

    private async void OnJoinClicked(object sender, EventArgs e)
    {
        JoinButton.IsEnabled = false;
        JoinButton.Text = "מצטרף...";

        try
        {
            await _svc.JoinGroupAsync(_group);

            // Add to local list
            var newMember = new MemberInfo
            {
                Uid   = Services.UserSession.Uid,
                Email = Services.UserSession.Email,
                Name  = Services.UserSession.FullName,
                AvatarBg = _group.AvatarBg,
                AvatarFg = _group.AvatarFg
            };
            _group.MembersList.Add(newMember);
            _members.Add(newMember);

            // Add to calendar
            await AddMeetingToCalendarAsync();

            // Notify other pages
            MessagingCenter.Send(this, "GroupJoined", _group);

            // Update UI
            JoinButtonContainer.IsVisible = false;
            MemberBadge.IsVisible = true;
            MemberCountLabel.Text = $"({_group.MembersList.Count})";

            await DisplayAlert("הצטרפת!", $"הצטרפת בהצלחה לקבוצה: {_group.Title}\nהפגישה נוספה ללוח השנה", "אישור");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GroupDetails] JOIN error: {ex.Message}");
            JoinButton.IsEnabled = true;
            JoinButton.Text = "הצטרפות לקבוצה";
            await DisplayAlert("שגיאה", "לא הצלחנו להצטרף לקבוצה", "אישור");
        }
    }

    private async Task AddMeetingToCalendarAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.CalendarWrite>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.CalendarWrite>();
                if (status != PermissionStatus.Granted)
                    return;
            }

#if ANDROID
            var intent = new Android.Content.Intent(Android.Content.Intent.ActionInsert);
            intent.SetData(Android.Provider.CalendarContract.Events.ContentUri);
            intent.PutExtra(Android.Provider.CalendarContract.ExtraEventBeginTime,
                new DateTimeOffset(_group.NextMeeting).ToUnixTimeMilliseconds());
            intent.PutExtra(Android.Provider.CalendarContract.ExtraEventEndTime,
                new DateTimeOffset(_group.NextMeeting.AddHours(1)).ToUnixTimeMilliseconds());
            intent.PutExtra(Android.Provider.CalendarContract.Events.InterfaceConsts.Title,
                $"פגישת קבוצה: {_group.Title}");
            intent.PutExtra(Android.Provider.CalendarContract.Events.InterfaceConsts.Description,
                $"קבוצת לימוד - {_group.Subject}");

            Platform.CurrentActivity?.StartActivity(intent);
#endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Calendar] Error: {ex.Message}");
        }
    }

    private async void OnEmailTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is string email && !string.IsNullOrEmpty(email))
        {
            try
            {
                await Launcher.OpenAsync($"mailto:{email}");
            }
            catch { }
        }
    }

    private async void OnOwnerEmailTapped(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(_group.CreatorEmail))
        {
            try
            {
                await Launcher.OpenAsync($"mailto:{_group.CreatorEmail}");
            }
            catch { }
        }
    }

    private async void OnBackTapped(object sender, EventArgs e) => await Navigation.PopAsync();

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
        await Navigation.PopAsync();
    }
}
