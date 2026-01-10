using System.Collections.ObjectModel;
namespace StudyBuddy;

public partial class TasksPage : ContentPage
{
    public ObservableCollection<string> MyTasks { get; set; }

    public TasksPage()
    {
        InitializeComponent();

        MyTasks = new ObservableCollection<string>();
        TasksListView.ItemsSource = MyTasks;
    }

    private void OnAddTaskClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(TaskEntry.Text))
        {
            MyTasks.Add(TaskEntry.Text);
            TaskEntry.Text = string.Empty;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}