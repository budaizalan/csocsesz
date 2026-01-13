using Csocsesz.Classes;
using System.Text;
using System.Text.Json;

namespace Csocsesz
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
        protected override async void OnStart()
        {
            try
            {
                await DataService.LoadDataBases();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba az indításkor: {ex.Message}");
            }
        }
    }
}