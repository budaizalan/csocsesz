namespace Csocsesz
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            //StartContinuousRotation();
        }
        private async void LiveGameButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LiveGamePage(), false);
        }
        /*
        private async void StartContinuousRotation()
        {
            // Ez egy végtelen ciklus, ami folyamatosan fut, amíg az oldal nyitva van
            // Érdemes lehet egy Task-ban futtatni, ha a játéknak is van egy fő Task-ja

            // Figyelem: A True paraméter a WithAnimation opciót jelöli, ami a végtelenségig ismétli az animációt.
            // Ezzel elkerülhető a végtelen while ciklus!

            await AppIconImage.RotateTo(360, 1000, Easing.Linear);

            // A RotateTo metódus befejeződik 360 foknál. Ahhoz, hogy folytatódjon, 
            // vissza kell állítani a Rotation tulajdonságot 0-ra, 
            // és újra meghívni az animációt.

            while (true) // Végtelen ciklus
            {
                // Forgatás 360 fokra (a második paraméter az idő, ms-ben)
                // 1000 ms = 1 másodperc a teljes fordulat
                await AppIconImage.RotateTo(360, 3000, Easing.Linear);

                // Visszaállítás 0 fokra az új fordulat elindításához (azonnal, animáció nélkül)
                AppIconImage.Rotation = 0;
            }
        }
        */
    }
}