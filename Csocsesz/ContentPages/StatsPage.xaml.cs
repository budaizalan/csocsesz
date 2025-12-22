using Csocsesz.Classes;

namespace Csocsesz.ContentPages;

public partial class StatsPage : ContentPage
{
	public StatsPage()
	{
		InitializeComponent();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Task.Yield();

        try
        {
            await Start();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }
    private async Task Start()
    {
        PlayerPicker.ItemsSource = DataStore.Players;
        PlayerPicker.SelectedIndex = 0;
        UpdateLabels();

        Navbar.setButtonColor();
    }
    void OnPlayerChanged(object sender, EventArgs e)
    {
        UpdateLabels();
    }
    private void UpdateLabels()
    {
        Player player = (Player)PlayerPicker.SelectedItem;
        WinStreakLabel.Text = $"{player.stats.streak}";
        TotalGoalsLabel.Text = $"{player.stats.totalGoals}";
        TotalMatchWonLabel.Text = $"{player.stats.totalMatchWon}";
        TotalMatchLostLabel.Text = $"{player.stats.totalMatchLost}";
        WinRateLabel.Text = $"{Math.Round(player.stats.winRate*100, 0)}%";
    }
}