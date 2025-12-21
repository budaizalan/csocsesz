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
            for (int i = 0; i < 5; i++)
            {
                MatchResults match = RandomMatch();
                DataStore.Matches.Add(match);
                PrintMatchToConsole(match);
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
        private void PrintMatchToConsole(MatchResults match)
        {
            if (match == null) return;

            Console.WriteLine("========================================");
            Console.WriteLine($"MECCS ÖSSZEFOGLALÓ - {match.startTime:yyyy.MM.dd HH:mm}");
            Console.WriteLine("========================================");

            // Győztes és Vesztes azonosítása
            Console.WriteLine($"GYŐZTES: {match.winnerId} ({match.winnerSide}) - 10 gól");
            Console.WriteLine($"VESZTES: {match.loserId} - {match.loserGoals} gól");
            Console.WriteLine($"Push-ups Multiplier: {match.pushUpsMultiplier}x");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("GÓLOK LISTÁJA:");

            DateTime? lastGoalTime = null;

            for (int i = 0; i < match.goals.Length; i++)
            {
                var goal = match.goals[i];
                if (goal == null) break; // Megállunk, ha elértük a tömb végét vagy az üres helyeket

                // Kiszámoljuk a kezdéstől eltelt időt (MM:SS formátumban)
                TimeSpan elapsed = goal.time - match.startTime;
                string timeStr = $"[{elapsed.Minutes:D2}:{elapsed.Seconds:D2}]";

                Console.WriteLine($"{i + 1}. gól | {timeStr} | Side: {goal.side.ToString().PadRight(4)}");

                lastGoalTime = goal.time;
            }

            if (lastGoalTime.HasValue)
            {
                TimeSpan duration = lastGoalTime.Value - match.startTime;
                Console.WriteLine("----------------------------------------");
                Console.WriteLine($"MECCS IDŐTARTAMA: {duration.Minutes} perc {duration.Seconds} másodperc");
            }

            Console.WriteLine("========================================\n\n");
        }
    }
}