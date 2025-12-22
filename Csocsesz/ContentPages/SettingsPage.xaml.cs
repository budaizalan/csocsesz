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
        PlayerRedPicker.SelectedIndex = 0;
        PlayerBluePicker.ItemsSource = DataStore.Players;
        PlayerBluePicker.SelectedIndex = 1;
        AutoSideSwitch.IsToggled = AppSettings.changingSide;
        SaveTestMatchesSwitch.IsToggled = AppSettings.sendTestMatches;

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
        if (int.TryParse(entry.Text, out int result)) AppSettings.pushUpsMultiplier = result;
        else
        {
            AppSettings.pushUpsMultiplier = 3;
            entry.Text = "3";
        }
    }
    void OnSaveTestMatchesChanged(object sender, ToggledEventArgs e)
    {
        AppSettings.sendTestMatches = e.Value;
    }
}