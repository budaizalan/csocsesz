namespace Csocsesz
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        private async void LiveGameButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LiveGamePage(), false);
        }
    }
}