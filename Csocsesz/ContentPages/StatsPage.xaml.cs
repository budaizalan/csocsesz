using Csocsesz.Classes;

namespace Csocsesz.ContentPages;

public partial class StatsPage : ContentPage
{
    private int punishments = 0;
    private Mode mode = AppSettings.mode;
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
        Player2Picker.ItemsSource = DataStore.Players;
        Player2Picker.SelectedItem = AppSettings.playerRed2 ?? DataStore.Players.FirstOrDefault(p => p != AppSettings.playerRed);
        UpdateLabels();

        Navbar.setButtonColor();
    }
    private void ModeButtonClicked(object sender, EventArgs e)
    {
        mode = sender == Mode2v2Button ? Mode.TwoVsTwo : Mode.OneVsOne;
        punishments = 0;
        UpdateLabels();
    }
    void OnPlayerChanged(object sender, EventArgs e)
    {
        UpdateLabels();
    }
    private async void PunishmentPlusButtonClicked(object sender, EventArgs e)
    {
        punishments++;
        UpdateLabels();
    }
    private async void PunishmentMinusButtonClicked(object sender, EventArgs e)
    {
        punishments--;
        UpdateLabels();
    }
    private async void PunishmentCompleteButtonClicked(object sender, EventArgs e)
    {
        if (punishments == 0) return;
        Player player = (Player)PlayerPicker.SelectedItem;
        await DataService.ChangePunishmentCompleted(player.id, punishments, mode);
        punishments = 0;
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        Player player = (Player)PlayerPicker.SelectedItem;
        if (player == null) return;
        bool two = mode == Mode.TwoVsTwo;
        Player2Picker.IsVisible = two;
        Grid.SetColumnSpan(PlayerPicker, two ? 1 : 2);
        PlayerHeaderLabel.Text = two ? "Team" : "Player";
        // 2v2 stats belong to the pair, not to one player.
        var player2 = Player2Picker.SelectedItem as Player;
        var stats = !two ? player.StatsFor(mode)
            : player2 == null || player2 == player ? new Stats()
            : StatsCalculator.TeamStats(DataStore.Matches, player.id, player2.id);
        Mode1v1Button.BackgroundColor = mode == Mode.OneVsOne ? DataStore.red : DataStore.gray;
        Mode2v2Button.BackgroundColor = mode == Mode.TwoVsTwo ? DataStore.red : DataStore.gray;
        PunishmentSection.IsVisible = mode == Mode.OneVsOne; // punishments are 1v1 only

        WinStreakLabel.Text = $"{stats.streak}";

        int played = stats.totalMatchWon + stats.totalMatchLost;
        double winRate = played == 0 ? 0 : (double)stats.totalMatchWon / played;
        WinRateLabel.Text = 
            $"(W:{stats.totalMatchWon}, L:{stats.totalMatchLost}) - {Math.Round(winRate * 100, 1)}%";

        double ratio = stats.totalGoalsConceded == 0 ? 0 : (double)stats.totalGoalsScored / stats.totalGoalsConceded;
        GoalScoredConcededRatioLabel.Text = 
            $"(S:{stats.totalGoalsScored}, C:{stats.totalGoalsConceded}) - {Math.Round(ratio, 1)}";

        double todo = stats.totalPunishmentAssigned - stats.totalPunishmentCompleted;
        PunishmentToDoLabel.Text =
            $"(C:{stats.totalPunishmentCompleted}, A:{stats.totalPunishmentAssigned}) - {todo}";

        PunishmentCounter.Text = punishments.ToString();
        PunishmentCompleteButton.Text = "COMPLETE";
    }
}