namespace Csocsesz.ContentPages
{
    public partial class PlayPage : ContentPage
    {
        public PlayPage()
        {
            InitializeComponent();
        }
        private async void LiveGameButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new LiveGamePage());
        }
    }
}