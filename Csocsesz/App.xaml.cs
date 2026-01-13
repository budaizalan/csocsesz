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
        protected override async void OnStart()
        {
            try
            {
                LoadDataBases();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba az indításkor: {ex.Message}");
            }
        }
        public void LoadDataBases()
        {
            LoadPlayerDataBase();
            LoadMatchDataBase();
        }
        public async Task LoadMatchDataBase() // void helyett Task, hogy lehessen await-elni!
        {
            using HttpClient _httpClient = new HttpClient();
            const string MatchApiUrl = DataStore.apiMatchUrl;
            bool success = true;

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(MatchApiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions
                    {
                        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
                        PropertyNameCaseInsensitive = true,
                        // Ez segít, ha mégis maradna némi típuseltérés a számoknál
                        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                    };
                    var result = JsonSerializer.Deserialize<List<MatchResults>>(jsonResponse, options);

                    if (result != null)
                    {
                        DataStore.Matches.Clear();
                        foreach (var match in result)
                        {
                            DataStore.Matches.Add(match);
                        }
                        Console.WriteLine("=====================================================");
                        Console.WriteLine($"Sikeres letöltés! {result.Count} Match ONLINE betöltve.");
                        Console.WriteLine("=====================================================");
                        DataManager.DeleteMatchDataBase();
                        await DataManager.SaveMatchDataBase(result);
                    }
                    else
                    {
                        string errorBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("=====================================================");
                        Console.WriteLine($"Hiba a Match letöltésnél. Státuszkód: {response.StatusCode}. Válasz: {errorBody}");
                        Console.WriteLine("=====================================================");
                        success = false;
                    }
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("=====================================================");
                    Console.WriteLine($"Hiba a Match letöltésnél. Státuszkód: {response.StatusCode}. Válasz: {errorBody}");
                    Console.WriteLine("=====================================================");
                    success = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("=====================================================");
                Console.WriteLine($"Hálózati hiba a Match letöltés során: {ex.Message}");
                Console.WriteLine("=====================================================");
                success = false;
            }

            if (!success)
            {
                // Ha nem sikerült az online, jöhet a mentett offline verzió
                LoadOfflineMatchDataBase();
            }
        }
        public async void LoadPlayerDataBase()
        {
            using HttpClient _httpClient = new HttpClient();
            const string PlayerApiUrl = DataStore.apiPlayerUrl;
            bool success = true;
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
                        Console.WriteLine("=====================================================");
                        Console.WriteLine($"Sikeres letöltés! {players.Count} player ONLINE betöltve.");
                        Console.WriteLine("=====================================================");
                        if (players.Count >= 2)
                        {
                            AppSettings.playerRed = DataStore.Players[DataStore.defaultPlayerRedIdx];
                            AppSettings.playerBlue = DataStore.Players[DataStore.defaultPlayerBlueIdx];
                        }
                        foreach (var player in DataStore.Players)
                        {
                            if (player.id == "694077cfe93c946a4ce8fdaf")
                            {
                                player.inGame =
                                    new InGame(0, 0, "hugo_icon.svg", "hugosad_icon.svg");
                            }
                            else if (player.id == "694077dbe93c946a4ce8fdb1")
                            {
                                player.inGame =
                                    new InGame(0, 0, "zalan_icon.svg", "zalansad_icon.svg");
                            }
                            else
                            {
                                player.inGame =
                                    new InGame(0, 0, "normalface_icon.svg", "sadface_icon.svg");
                            }
                        }
                        DataManager.DeletePlayerDataBase();
                        await DataManager.SavePlayerDataBase(players);
                    }
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("=====================================================");
                    Console.WriteLine($"Hiba a Player letöltésnél. Státuszkód: {response.StatusCode}. Válasz: {errorBody}");
                    Console.WriteLine("=====================================================");
                    success = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("=====================================================");
                Console.WriteLine($"Hálózati hiba a Player letöltés során: {ex.Message}");
                Console.WriteLine("=====================================================");
                success = false;
            }
            if(!success) LoadOfflinePlayerDataBase();
        }
        public void LoadOfflinePlayerDataBase()
        {
            DataStore.Players = DataManager.LoadPlayerDataBase();
            Console.WriteLine("======================================================");
            Console.WriteLine($"{DataStore.Players.Count} db player OFFLINE betöltve!");
            Console.WriteLine("======================================================");
            if (DataStore.Players.Count >= 2)
            {
                AppSettings.playerRed = DataStore.Players[DataStore.defaultPlayerRedIdx];
                AppSettings.playerBlue = DataStore.Players[DataStore.defaultPlayerBlueIdx];
            }
            foreach (var player in DataStore.Players)
            {
                if (player.id == "694077cfe93c946a4ce8fdaf")
                {
                    player.inGame =
                        new InGame(0, 0, "hugo_icon.svg", "hugosad_icon.svg");
                }
                else if (player.id == "694077dbe93c946a4ce8fdb1")
                {
                    player.inGame =
                        new InGame(0, 0, "zalan_icon.svg", "zalansad_icon.svg");
                }
                else
                {
                    player.inGame =
                        new InGame(0, 0, "default_icon.svg", "defaultsad_icon.svg");
                }
            }
        }
        public void LoadOfflineMatchDataBase()
        {
            DataStore.Matches = DataManager.LoadMatchDataBase();
            Console.WriteLine("=====================================================");
            Console.WriteLine($"{DataStore.Matches.Count} db meccs OFFLINE betöltve!");
            Console.WriteLine("=====================================================");
        }
        #region Test
        private void LoadFakePlayerDatabase()
        {
            DataStore.Players.Add
                (new Player("694077cfe93c946a4ce8fdaf", "Hugo", 0, 0, 0, 0, 0, 0,"hugo_icon.png", "hugosad_icon.png"));
            DataStore.Players.Add
                (new Player("694077dbe93c946a4ce8fdb1", "Zazzzzuska", 0, 0, 0, 0, 0, 0, "zalan_icon.png", "zalansad_icon.png"));
            AppSettings.playerRed = DataStore.Players[DataStore.defaultPlayerRedIdx];
            AppSettings.playerBlue = DataStore.Players[DataStore.defaultPlayerBlueIdx];
        }
        private void LoadFakeMatchDataBase()
        {
            for (int i = 0; i < 5; i++)
            {
                MatchResults match = RandomMatch();
                DataStore.Matches.Add(match);
                //PrintMatchToConsole(match);
            }
        }
        private MatchResults RandomMatch()
        {
            Random rnd = new Random();
            string hID = "694077cfe93c946a4ce8fdaf";
            string zID = "694077dbe93c946a4ce8fdb1";

            // 1. Véletlenszerűen eldöntjük, ki a győztes (50-50%)
            bool isHugoWinner = rnd.Next(0, 2) == 0;
            string winnerId = isHugoWinner ? hID : zID;
            string loserId = isHugoWinner ? zID : hID;

            // 2. A győztes oldala is legyen véletlen
            Side winnerSide = (Side)rnd.Next(0, 2);
            Side loserSide = winnerSide == Side.red ? Side.blue : Side.red;

            // 3. Vesztes góljai (0-9 között, hiszen 10-nél vége a meccsnek)
            int loserGoals = rnd.Next(0, 10);
            int totalGoals = 10 + loserGoals;

            // 4. Meccs kezdete (az elmúlt 24 órában valmikor)
            DateTime matchStart = DateTime.Now.AddHours(-rnd.Next(1, 24*3));

            // 5. MatchResults létrehozása
            MatchResults match = new MatchResults(
                winnerId,
                winnerSide,
                loserId,
                loserGoals,
                matchStart,
                3 // Multiplier fixen 3, vagy rnd.Next(1, 6)
            );

            // 6. Gólok legyártása véletlenszerű sorrendben
            List<Side> goalOrder = new List<Side>();
            for (int i = 0; i < 10; i++) goalOrder.Add(winnerSide);
            for (int i = 0; i < loserGoals; i++) goalOrder.Add(loserSide);

            // Megkeverjük a gólokat
            goalOrder = goalOrder.OrderBy(x => rnd.Next()).ToList();

            // Feltöltjük a match.goals tömböt (figyelve, hogy nálad fix 20-as a tömbméret)
            for (int i = 0; i < goalOrder.Count; i++)
            {
                // Minden gól között eltelik 10-60 másodperc
                DateTime goalTime = matchStart.AddSeconds(i * rnd.Next(10, 61));
                match.goals[i] = new Goal(goalOrder[i], goalTime);
            }

            return match;
        }
        #endregion
    }
}