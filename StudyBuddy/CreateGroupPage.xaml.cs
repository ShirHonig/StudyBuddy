namespace StudyBuddy;

public partial class CreateGroupPage : ContentPage
{
    private string  _selectedSubject = "";
    private string  _selectedColor   = "#737785";
    private Frame?  _activeSubjectFrame;
    private Frame?  _activeColorFrame;

    // All subject frames paired with their accent colors for bulk-reset
    private (Frame frame, string color)[] _subjectFrames = [];

    public CreateGroupPage()
    {
        InitializeComponent();

        _subjectFrames =
        [
            (MathFrame,    "#2563eb"),
            (CSFrame,      "#1e293b"),
            (PhysicsFrame, "#0ea5e9"),
            (ChemFrame,    "#8b5cf6"),
            (BioFrame,     "#16a34a"),
            (HistFrame,    "#f97316"),
            (GeoFrame,     "#84cc16"),
            (LitFrame,     "#ec4899"),
            (EngFrame,     "#0891b2"),
            (PsychoFrame,  "#9333ea"),
            (CivFrame,     "#dc2626"),
            (EconFrame,    "#0284c7"),
            (OtherFrame,   "#737785"),
        ];
    }

    // ── Subject selection ─────────────────────────────────────────────────────

    private void OnSubjectTapped(object sender, TappedEventArgs e)
    {
        // Reset previous selection
        if (_activeSubjectFrame is not null)
            _activeSubjectFrame.BackgroundColor = Colors.White;

        if (sender is not Frame tapped) return;

        var param  = e.Parameter as string ?? "";
        var parts  = param.Split('|');
        _selectedSubject = parts.Length > 0 ? parts[0] : "";
        _selectedColor   = parts.Length > 1 ? parts[1] : "#737785";

        // Highlight selected with tinted background
        tapped.BackgroundColor = Color.FromArgb(_selectedColor).WithAlpha(0.13f);
        _activeSubjectFrame = tapped;

        // Show color picker only for "אחר"
        ColorPickerSection.IsVisible = _selectedSubject == "אחר";
    }

    // ── Color picker (for "אחר") ──────────────────────────────────────────────

    private void OnColorTapped(object sender, TappedEventArgs e)
    {
        // Reset border on previous color circle
        if (_activeColorFrame is not null)
            _activeColorFrame.BorderColor = Colors.Transparent;

        if (sender is not Frame colorFrame) return;

        _selectedColor = e.Parameter as string ?? "#737785";
        colorFrame.BorderColor = Colors.White;
        colorFrame.Padding     = new Thickness(3);
        _activeColorFrame      = colorFrame;
    }

    // ── Create ────────────────────────────────────────────────────────────────

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        var name = GroupNameEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("שגיאה", "יש להזין שם קבוצה", "אישור");
            return;
        }
        if (string.IsNullOrWhiteSpace(_selectedSubject))
        {
            await DisplayAlert("שגיאה", "יש לבחור מקצוע", "אישור");
            return;
        }

        CreateButton.IsEnabled = false;
        CreateButton.Text      = "⏳ יוצר...";

        try
        {
            var fullName = Services.UserSession.FullName ?? "";
            var newGroup = new GroupItem
            {
                Subject        = _selectedSubject,
                SubjectColor   = Color.FromArgb(_selectedColor),
                Title          = name,
                Description    = DescriptionEditor.Text?.Trim() ?? "",
                CreatorUid     = Services.UserSession.Uid,
                CreatorEmail   = Services.UserSession.Email,
                CreatorInitial = fullName.Length > 0 ? fullName[0].ToString() : "?",
                CreatorName    = fullName.Length > 0 ? fullName : (Services.UserSession.Username ?? ""),
                AvatarBg       = Color.FromArgb(_selectedColor).WithAlpha(0.25f),
                AvatarFg       = Color.FromArgb(_selectedColor),
                NextMeeting    = MeetingDatePicker.Date,
                MembersList    =
                [
                    new MemberInfo
                    {
                        Uid   = Services.UserSession.Uid,
                        Email = Services.UserSession.Email,
                        Name  = fullName.Length > 0 ? fullName : (Services.UserSession.Username ?? "")
                    }
                ]
            };

            var svc = new Services.GroupFirestoreService();
            var id  = await svc.CreateGroupAsync(newGroup);
            newGroup.Id = id;
            MessagingCenter.Send(this, "GroupCreated", newGroup);
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("שגיאה", $"שגיאה: {ex.GetType().Name}\n{ex.Message}", "אישור");
            CreateButton.IsEnabled = true;
            CreateButton.Text      = "＋  צור קבוצה חדשה";
        }
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    private async void OnBackTapped(object sender, EventArgs e)  => await Navigation.PopAsync();
    private async void OnHomeNavTapped(object sender, EventArgs e) => await Navigation.PopToRootAsync();
    private async void OnTasksNavTapped(object sender, EventArgs e)
        => await Navigation.PushAsync(new TasksPage());
}
