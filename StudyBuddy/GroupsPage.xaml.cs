using System.Collections.ObjectModel;

namespace StudyBuddy;

public class GroupItem
{
    public string Subject      { get; set; } = "";
    public Color  SubjectColor { get; set; } = Colors.Gray;
    public string Title        { get; set; } = "";
    public string Description  { get; set; } = "";
    public string CreatorInitial { get; set; } = "";
    public string CreatorName    { get; set; } = "";
    public Color  AvatarBg { get; set; } = Colors.LightGray;
    public Color  AvatarFg { get; set; } = Colors.Black;
    public DateTime NextMeeting { get; set; }
    public string NextMeetingText => $"📅 הפגישה הבאה: {NextMeeting:dd/MM/yyyy}";
}

public partial class GroupsPage : ContentPage
{
    private readonly List<GroupItem> _allGroups;
    private readonly ObservableCollection<GroupItem> _displayed = [];
    private string _activeFilter = "הכל";

    public GroupsPage()
    {
        InitializeComponent();

        _allGroups =
        [
            new GroupItem
            {
                Subject = "מדעי המחשב", SubjectColor = Color.FromArgb("#0040a1"),
                Title = "מבני נתונים - הכנה למבחן ג'",
                Description = "מתמקדים בעצי חיפוש, גרפים ואלגוריתמי מיון. פותרים מבחנים משנים קודמות ומשתפים סיכומים.",
                CreatorInitial = "א", CreatorName = "איתי לוי",
                AvatarBg = Color.FromArgb("#dae2ff"), AvatarFg = Color.FromArgb("#0040a1"),
                NextMeeting = new DateTime(2026, 3, 28)
            },
            new GroupItem
            {
                Subject = "מתמטיקה", SubjectColor = Color.FromArgb("#ff9100"),
                Title = "חדו\"א 2מ - קבוצת תרגול",
                Description = "נפגשים פעמיים בשבוע בספריה המרכזית לעבור על תרגילי הבית.",
                CreatorInitial = "מ", CreatorName = "מיה כהן",
                AvatarBg = Color.FromArgb("#dee0ff"), AvatarFg = Color.FromArgb("#4858ab"),
                NextMeeting = new DateTime(2026, 3, 27)
            },
            new GroupItem
            {
                Subject = "ביולוגיה", SubjectColor = Color.FromArgb("#4caf50"),
                Title = "גנטיקה א' - סיכומי מאמרים",
                Description = "חלוקת עבודה על קריאת מאמרים שבועיים ודיון בממצאים.",
                CreatorInitial = "ד", CreatorName = "דניאל גולד",
                AvatarBg = Color.FromArgb("#88d982"), AvatarFg = Color.FromArgb("#002204"),
                NextMeeting = new DateTime(2026, 4, 1)
            },
            new GroupItem
            {
                Subject = "פסיכולוגיה", SubjectColor = Color.FromArgb("#9c27b0"),
                Title = "מבוא לפסיכולוגיה חברתית",
                Description = "קבוצת למידה למבחן האמצע. דגש על תיאוריות קלאסיות.",
                CreatorInitial = "נ", CreatorName = "נועה ארז",
                AvatarBg = Color.FromArgb("#96a5ff"), AvatarFg = Color.FromArgb("#27378a"),
                NextMeeting = new DateTime(2026, 3, 30)
            },
            new GroupItem
            {
                Subject = "כלכלה", SubjectColor = Color.FromArgb("#2196f3"),
                Title = "מיקרו כלכלה - פתרון תרגילים",
                Description = "עוברים יחד על דפי התרגול של ד\"ר שפירא.",
                CreatorInitial = "י", CreatorName = "יוסי חדד",
                AvatarBg = Color.FromArgb("#b2c5ff"), AvatarFg = Color.FromArgb("#001847"),
                NextMeeting = new DateTime(2026, 3, 29)
            },
        ];

        GroupsCollection.ItemsSource = _displayed;
        ApplyFilterAndSort();
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

    private async void OnCreateGroupClicked(object sender, EventArgs e)
    {
        await DisplayAlert("יצירת קבוצה", "בקרוב תוכל ליצור קבוצות למידה חדשות!", "אישור");
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
        await Navigation.PopAsync();
    }

    private async void OnTasksNavTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TasksPage());
    }
}
