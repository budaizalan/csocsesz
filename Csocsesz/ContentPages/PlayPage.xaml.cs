using Csocsesz.Classes;
using Csocsesz.ContentViews;

namespace Csocsesz.ContentPages
{
    public partial class PlayPage : ContentPage
    {
        public PlayPage()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await Task.Yield();

            try
            {
                foreach (var picker in new[] { RedPicker, Red2Picker, BluePicker, Blue2Picker })
                    picker.ItemsSource = DataStore.Players;
                RedPicker.SelectedItem = AppSettings.playerRed;
                BluePicker.SelectedItem = AppSettings.playerBlue;
                Red2Picker.SelectedItem = AppSettings.playerRed2;
                Blue2Picker.SelectedItem = AppSettings.playerBlue2;
                ShowMode();
                Navbar.setButtonColor();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        private void ShowMode()
        {
            bool two = AppSettings.mode == Mode.TwoVsTwo;
            Mode1v1Button.BackgroundColor = two ? DataStore.gray : DataStore.red;
            Mode2v2Button.BackgroundColor = two ? DataStore.red : DataStore.gray;
            Red2Picker.IsVisible = two;
            Blue2Picker.IsVisible = two;
            // 1v1: the single picker takes the whole row.
            Grid.SetColumnSpan(RedPicker, two ? 1 : 2);
            Grid.SetColumnSpan(BluePicker, two ? 1 : 2);
        }
        private void ModeButtonClicked(object sender, EventArgs e)
        {
            AppSettings.mode = sender == Mode2v2Button ? Mode.TwoVsTwo : Mode.OneVsOne;
            ShowMode();
        }
        private void SwapTeamsButtonClicked(object sender, EventArgs e)
        {
            (RedPicker.SelectedItem, BluePicker.SelectedItem) = (BluePicker.SelectedItem, RedPicker.SelectedItem);
            (Red2Picker.SelectedItem, Blue2Picker.SelectedItem) = (Blue2Picker.SelectedItem, Red2Picker.SelectedItem);
        }
        private async void LiveGameButtonClicked(object sender, EventArgs e)
        {
            var red = RedPicker.SelectedItem as Player;
            var blue = BluePicker.SelectedItem as Player;
            var red2 = Red2Picker.SelectedItem as Player;
            var blue2 = Blue2Picker.SelectedItem as Player;
            bool two = AppSettings.mode == Mode.TwoVsTwo;

            var chosen = two ? new[] { red, red2, blue, blue2 } : new[] { red, blue };
            if (chosen.Any(p => p == null) || chosen.Distinct().Count() != chosen.Length)
            {
                await DisplayAlertAsync("Teams", two ? "Pick 4 different players." : "Pick 2 different players.", "OK");
                return;
            }
            AppSettings.playerRed = red!;
            AppSettings.playerBlue = blue!;
            AppSettings.playerRed2 = two ? red2 : null;
            AppSettings.playerBlue2 = two ? blue2 : null;
            await Navigation.PushModalAsync(new LiveGamePage());
        }
    }
}
