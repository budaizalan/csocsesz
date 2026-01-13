using Csocsesz.Classes;

namespace Csocsesz.ContentPages;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
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
        PlayerRedPicker.ItemsSource = DataStore.Players;
        PlayerRedPicker.SelectedIndex = DataStore.Players.IndexOf(AppSettings.playerRed);
        PlayerBluePicker.ItemsSource = DataStore.Players;
        PlayerBluePicker.SelectedIndex = DataStore.Players.IndexOf(AppSettings.playerBlue);
        AutoSideSwitch.IsToggled = AppSettings.changingSide;
        SaveTestMatchesSwitch.IsToggled = AppSettings.sendTestMatches;

        List<Player> players = await DataManager.LoadPlayerDataBase();
        List<Match> matches = await DataManager.LoadMatchDataBase();
        List<Match> matchesBuffer = await DataManager.LoadMatchBufferDataBase();

        DataBaseStatsLabel.Text = 
            $"DS.P: {DataStore.Players.Count}, DS.M: {DataStore.Matches.Count}, " +
            $"JS.P: {players.Count}, JS.M: {matches.Count}, JS.MB: {matchesBuffer.Count}";

        Navbar.setButtonColor();
    }
    void OnPlayerRedChanged(object sender, EventArgs e)
    {
        if (DataStore.Players[PlayerRedPicker.SelectedIndex] == AppSettings.playerBlue)
        {
            PlayerBluePicker.SelectedItem = AppSettings.playerRed;
        }
        AppSettings.playerRed = (Player)PlayerRedPicker.SelectedItem;
    }
    void OnPlayerBlueChanged(object sender, EventArgs e)
    {
        if (DataStore.Players[PlayerBluePicker.SelectedIndex] == AppSettings.playerRed)
        {
            PlayerRedPicker.SelectedItem = AppSettings.playerBlue;
        }
        AppSettings.playerBlue = (Player)PlayerBluePicker.SelectedItem;
    }
    void OnAutoSideSwitchChanged(object sender, ToggledEventArgs e)
    {
        AppSettings.changingSide = e.Value;
    }
    private void OnPushUpsMultiplierChanged(object sender, EventArgs e)
    {
        var entry = (Entry)sender;
        if (int.TryParse(entry.Text, out int result)) AppSettings.punishmentMultiplier = result;
        else
        {
            AppSettings.punishmentMultiplier = 3;
            entry.Text = "3";
        }
    }
    void OnSaveTestMatchesChanged(object sender, ToggledEventArgs e)
    {
        AppSettings.sendTestMatches = e.Value;
    }

    private async void RefreshDataBaseButtonClicked(object sender, EventArgs e)
    {
        await DataService.LoadDataBases();
    }
}