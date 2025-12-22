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
                await Start();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        private async Task Start()
        {
            Navbar.setButtonColor();
        }
        private async void LiveGameButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new LiveGamePage());
        }
    }
}