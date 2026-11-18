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
        string? keep = (ImagePlayerPicker.SelectedItem as Player)?.id;
        ImagePlayerPicker.ItemsSource = DataStore.Players.ToList();
        ImagePlayerPicker.SelectedItem = ((List<Player>)ImagePlayerPicker.ItemsSource).FirstOrDefault(p => p.id == keep)
            ?? ((List<Player>)ImagePlayerPicker.ItemsSource).FirstOrDefault();
        AutoSideSwitch.IsToggled = AppSettings.changingSide;
        SaveTestMatchesSwitch.IsToggled = AppSettings.sendTestMatches;
        OvertimeSwitch.IsToggled = AppSettings.overtime;
        MultiplierEntry.Text = AppSettings.punishmentMultiplier.ToString();

        DataBaseStatsLabel.Text = $"Players: {DataStore.Players.Count}, Matches: {DataStore.Matches.Count}";

        OnImagePlayerChanged(null, EventArgs.Empty);

        Navbar.setButtonColor();
    }
    private void OnImagePlayerChanged(object? sender, EventArgs e)
    {
        var p = ImagePlayerPicker.SelectedItem as Player;
        NormalPreview.Source = p?.normalImagePath;
        SadPreview.Source = p?.sadImagePath;
    }
    private async void ChangeImageButtonClicked(object sender, EventArgs e)
    {
        if (ImagePlayerPicker.SelectedItem is not Player p) return;
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select an image",
                FileTypes = FilePickerFileType.Images
            });
            if (file == null) return;
            using var stream = await file.OpenReadAsync();
            await DataService.SetPlayerImage(p.id, stream, ((Button)sender).Text == "SAD");
            OnImagePlayerChanged(null, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Image failed", ex.Message, "OK");
        }
    }
    private async void AddPlayerButtonClicked(object sender, EventArgs e)
    {
        string? name = await DisplayPromptAsync("New Player", "Name:", "ADD", "Cancel", maxLength: 20);
        if (string.IsNullOrWhiteSpace(name)) return;
        try
        {
            await DataService.AddPlayer(name);
            await Start();
        }
        catch (ArgumentException ex)
        {
            await DisplayAlertAsync("Player", ex.Message, "OK");
        }
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
    void OnOvertimeChanged(object sender, ToggledEventArgs e)
    {
        AppSettings.overtime = e.Value;
    }
    void OnSaveTestMatchesChanged(object sender, ToggledEventArgs e)
    {
        AppSettings.sendTestMatches = e.Value;
    }

    private async void ExportButtonClicked(object sender, EventArgs e)
    {
        try
        {
            string name = $"csocsesz-{DateTime.Now:yyyyMMdd-HHmm}.json";
            // Share needs package identity, which the unpackaged Windows build lacks: save to Documents there.
            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Csocsesz");
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, name);
                await File.WriteAllTextAsync(path, DataService.Export());
                await DisplayAlertAsync("Exported", path, "OK");
            }
            else
            {
                string path = Path.Combine(FileSystem.CacheDirectory, name);
                await File.WriteAllTextAsync(path, DataService.Export());
                await Share.Default.RequestAsync(new ShareFileRequest("Csocsesz export", new ShareFile(path)));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }
    private async void ImportButtonClicked(object sender, EventArgs e)
    {
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Select a Csocsesz export (.json)" });
            if (file == null) return;
            if (!await DisplayAlertAsync("Import", "This replaces ALL current players and matches. Continue?", "Replace", "Cancel")) return;

            using var stream = await file.OpenReadAsync();
            using var reader = new StreamReader(stream);
            await DataService.Import(await reader.ReadToEndAsync());
            await Start();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Import failed", ex.Message, "OK");
        }
    }
}
