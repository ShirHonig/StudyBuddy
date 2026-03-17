using System.Collections.ObjectModel;

namespace StudyBuddy;

public partial class GroupsPage : ContentPage
{
    public ObservableCollection<string> MyGroups { get; set; }

    public GroupsPage()
    {
        InitializeComponent();

        MyGroups = new ObservableCollection<string>();
        GroupsListView.ItemsSource = MyGroups;
    }

    private void OnAddGroupClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(GroupEntry.Text))
        {
            MyGroups.Add(GroupEntry.Text);
            GroupEntry.Text = string.Empty;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}