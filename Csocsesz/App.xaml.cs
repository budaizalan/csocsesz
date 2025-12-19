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
            AppSettings.playerRed = DataStore.defaultPlayerRed;
            AppSettings.playerBlue = DataStore.defaultPlayerBlue;
            AppSettings.redImage = DataStore.defaultRedImage;
            AppSettings.redSadImage = DataStore.defaultRedSadImage;
            AppSettings.blueImage = DataStore.defaultBlueImage;
            AppSettings.blueSadImage = DataStore.defaultBlueSadImage;

            DataStore.Players.Add(AppSettings.playerRed);
            DataStore.Players.Add(AppSettings.playerBlue);
        }
    }
}