namespace StudyBuddy;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    // מעבר למסך המטלות
    private async void OnTasksClicked(object sender, EventArgs e)
    {
        // ודאי שיצרת את TasksPage.xaml לפני כן
        await Navigation.PushAsync(new TasksPage());
    }

    // מעבר למסך הקבוצות
    private async void OnGroupsClicked(object sender, EventArgs e)
    {
        // ודאי שיצרת את GroupsPage.xaml לפני כן
        await Navigation.PushAsync(new GroupsPage());
    }

    // כפתור זמני
    private async void OnScheduleClicked(object sender, EventArgs e)
    {
        await DisplayAlert("מערכת שעות", "הפיצ'ר יהיה זמין בקרוב", "אישור");
    }
}
