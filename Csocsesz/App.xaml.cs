using Csocsesz.Classes;

namespace Csocsesz
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            loadDataBase();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        private void loadDataBase()
        {
            AppSettings.player1 = AppSettings.defaultPlayer1;
            AppSettings.player2 = AppSettings.defaultPlayer2;
            DataStore.Players.Add(AppSettings.player1);
            DataStore.Players.Add(AppSettings.player2);
        }
    }
}