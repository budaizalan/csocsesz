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
            LoadDataBases();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
        public void LoadDataBases()
        {
            LoadPlayerDataBase();
            LoadMatchDataBase();
            LoadFakeMatchDataBase();
        }
        public async void LoadMatchDataBase()
        {
            using HttpClient _httpClient = new HttpClient();
            // HTTP Kliens (érdemes osztályszinten tartani, de itt a példa kedvéért)
            const string MatchApiUrl = DataStore.apiMatchUrl;
            try
            {
                // 1. GET kérés küldése
                HttpResponseMessage response = await _httpClient.GetAsync(MatchApiUrl);
                // 2. Válasz ellenőrzése
                if (response.IsSuccessStatusCode)
                {
                    // 3. A tartalom beolvasása stringként
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    // 4. Deszerializáció (JSON -> Lista)
                    var options = new JsonSerializerOptions
                    {
                        // Ez fontos, ha az Enumok stringként jönnek a szerverről
                        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
                        PropertyNameCaseInsensitive = true // Segít, ha a JSON-ben kisbetű/nagybetű eltérés van
                    };

                    // Itt a változás: MatchRootResponse-t deszerializálunk!
                    var result = JsonSerializer.Deserialize<MatchRootResponse>(jsonResponse, options);

                    if (result?.matches != null)
                    {
                        DataStore.Matches.Clear();
                        foreach (var match in result.matches)
                        {
                            DataStore.Matches.Add(match);
                        }
                        Console.WriteLine($"Sikeres letöltés! {result.matches.Count} meccs betöltve.");
                    }
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Hiba a letöltésnél. Státuszkód: {response.StatusCode}. Válasz: {errorBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hálózati hiba a letöltés során: {ex.Message}");
            }
        }
        public async void LoadPlayerDataBase()
        {
            using HttpClient _httpClient = new HttpClient();
            const string PlayerApiUrl = DataStore.apiPlayerUrl;
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(PlayerApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    List<Player> players = JsonSerializer.Deserialize<List<Player>>(jsonResponse, options);

                    if (players != null)
                    {
                        DataStore.Players.Clear();
                        foreach (var player in players) DataStore.Players.Add(player);
                        Console.WriteLine($"Sikeres letöltés! {players.Count} player betöltve.");
                    }
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Hiba a letöltésnél. Státuszkód: {response.StatusCode}. Válasz: {errorBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hálózati hiba a letöltés során: {ex.Message}");
            }
            AppSettings.playerRed = DataStore.Players[DataStore.defaultPlayerRedIdx];
            AppSettings.playerBlue = DataStore.Players[DataStore.defaultPlayerBlueIdx];
            foreach (var player in DataStore.Players)
            {
                if (player.id == "694077cfe93c946a4ce8fdaf")
                {
                    player.inGame =
                        new InGame(0, 0, DataStore.hugoNormalImage, DataStore.hugoSadImage);
                }
                else if (player.id == "694077dbe93c946a4ce8fdb1")
                {
                    player.inGame =
                        new InGame(0, 0, DataStore.zalanNormalImage, DataStore.zalanSadImage);
                }
                else
                {
                    player.inGame =
                        new InGame(0, 0, DataStore.defaultNormalImage, DataStore.defaultSadImage);
                }
            }
        }
        private void LoadFakeMatchDataBase()
        {

        }
    }
}