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
        PlayerPicker.SelectedIndex = DataStore.Players.IndexOf(AppSettings.playerRed);
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

        double winRate = (double)player.stats.totalMatchWon / (player.stats.totalMatchWon + player.stats.totalMatchLost);
        WinRateLabel.Text = 
            $"(W:{player.stats.totalMatchWon}, L:{player.stats.totalMatchLost}) - {Math.Round(winRate * 100, 1)}%";

        double ratio= (double)player.stats.totalGoalsScored / player.stats.totalGoalsConceded;
        GoalScoredConcededRatioLabel.Text = 
            $"(S:{player.stats.totalGoalsScored}, C:{player.stats.totalGoalsConceded}) - {Math.Round(ratio, 1)}";

        double todo = player.stats.totalPunishmentAssigned - player.stats.totalPunishmentCompleted;
        PunishmentToDoLabel.Text =
            $"(C:{player.stats.totalPunishmentCompleted}, A:{player.stats.totalPunishmentAssigned}) - {todo}";
    }
}