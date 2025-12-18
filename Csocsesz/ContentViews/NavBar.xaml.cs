using Csocsesz.Classes;
namespace Csocsesz.ContentViews;

public partial class NavBar : ContentView
{
	public NavBar()
	{
		InitializeComponent();
		setButtonColor();
	}
	private void setButtonColor() 
	{
		if (DataStore.pageIdx == 0) PlayTabButton.BackgroundColor = DataStore.red;
		else PlayTabButton.BackgroundColor = DataStore.gray;
        if (DataStore.pageIdx == 1) HistoryTabButton.BackgroundColor = DataStore.red;
        else HistoryTabButton.BackgroundColor = DataStore.gray;
        if (DataStore.pageIdx == 2) StatsTabButton.BackgroundColor = DataStore.red;
        else StatsTabButton.BackgroundColor = DataStore.gray;
        if (DataStore.pageIdx == 3) SettingsTabButton.BackgroundColor = DataStore.red;
        else SettingsTabButton.BackgroundColor = DataStore.gray;
    }
    private async void PlayTabButtonClicked(object sender, EventArgs e)
    {
        DataStore.pageIdx = 0;
        await Shell.Current.GoToAsync("///PlayPage");
    }
    private async void HistoryTabButtonClicked(object sender, EventArgs e)
    {
        DataStore.pageIdx = 1;
        await Shell.Current.GoToAsync("///HistoryPage");
    }
    private async void StatsTabButtonClicked(object sender, EventArgs e)
    {
        DataStore.pageIdx = 2;
        await Shell.Current.GoToAsync("///StatsPage");
    }
    private async void SettingsTabButtonClicked(object sender, EventArgs e)
    {
        DataStore.pageIdx = 3;
        await Shell.Current.GoToAsync("///SettingsPage");
    }
}