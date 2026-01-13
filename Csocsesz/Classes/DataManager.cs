using System;
using System.Collections.Generic;
using System.Text;

using System.Text.Json;

namespace Csocsesz.Classes
{
    public static class DataManager
    {
        #region Player
        private static string playersFilePath = Path.Combine(FileSystem.AppDataDirectory, "players.json");
        public static async Task SavePlayerDataBase(List<Player> players)
        {
            string json = JsonSerializer.Serialize(players);
            await File.WriteAllTextAsync(playersFilePath, json);
        }

        public static List<Player> LoadPlayerDataBase()
        {
            if (!File.Exists(playersFilePath)) return new List<Player>();
            string json = File.ReadAllText(playersFilePath);
            return JsonSerializer.Deserialize<List<Player>>(json);
        }
        public static void DeletePlayerDataBase()
        {
            if (File.Exists(playersFilePath))
            {
                File.Delete(playersFilePath);
            }
        }
        #endregion

        #region Match
        private static string matchesFilePath = Path.Combine(FileSystem.AppDataDirectory, "matches.json");
        public static async Task SaveMatchDataBase(List<MatchResults> matches)
        {
            string json = JsonSerializer.Serialize(matches);
            await File.WriteAllTextAsync(matchesFilePath, json);
        }
        public static List<MatchResults> LoadMatchDataBase()
        {
            if (!File.Exists(matchesFilePath)) return new List<MatchResults>();
            string json = File.ReadAllText(matchesFilePath);
            //return JsonSerializer.Deserialize<List<MatchResults>>(json);

            List<MatchResults> matches = JsonSerializer.Deserialize<List<MatchResults>>(json);
            List<MatchResults> matchbuffer = LoadMatchBufferDataBase();
            if (matchbuffer.Count != 0) matches.AddRange(matchbuffer);

            return matches;

        }
        public static async Task UploadMatchResultsList(List<MatchResults> matchResultss)
        {
            // HTTP Kliens és API URL
            HttpClient _httpClient = new HttpClient();
            const string ApiUrl = DataStore.apiMatchUrl;
            // 1. Objektum sorosítása JSON stringgé
            var options = new JsonSerializerOptions
            {
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
                WriteIndented = true // opcionális, olvashatóbb JSON
            };
            bool success = true;

            List<MatchResults> allMatches = new List<MatchResults>(matchResultss);
            List<MatchResults> matchbuffer = LoadMatchBufferDataBase();
            if(matchbuffer.Count != 0) allMatches.AddRange(matchbuffer);

            foreach (MatchResults matchResults in allMatches)
            {
                string jsonContent = JsonSerializer.Serialize(matchResults, options);

                // 2. JSON tartalom létrehozása
                StringContent content = new StringContent(
                    jsonContent,
                    Encoding.UTF8,
                    "application/json" // Megmondjuk a szervernek, hogy JSON-t küldünk
                );

                try
                {
                    // 3. POST kérés küldése
                    HttpResponseMessage response = await _httpClient.PostAsync(ApiUrl, content);

                    // 4. Válasz ellenőrzése
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("=====================================================");
                        Console.WriteLine("Sikeres Match feltöltés!");
                        Console.WriteLine("=====================================================");
                        // Itt megjeleníthetsz egy üzenetet a felhasználónak
                    }
                    else
                    {
                        // Hibakezelés (pl. ha a szerver 400 Bad Request-et küld)
                        string errorBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("=====================================================");
                        Console.WriteLine($"Hiba a feltöltésnél. Státuszkód: {response.StatusCode}. Válasz: {errorBody}");
                        Console.WriteLine("=====================================================");
                    }
                }
                catch (Exception ex)
                {
                    // Hálózati hiba (pl. nincs internet, rossz URL)
                    Console.WriteLine("=====================================================");
                    Console.WriteLine($"Hiba a hálózati kérés során: {ex.Message}");
                    Console.WriteLine("=====================================================");
                    success = false;
                }
            }
            if (success) DeleteMatchBufferDataBase();
            else
            {
                await AppendMatchBuffereDataBase(matchResultss);
            }
        }
        public static void DeleteMatchDataBase()
        {
            if (File.Exists(matchesFilePath))
            {
                File.Delete(matchesFilePath);
            }
        }
        public static void PrintMatchToConsole(MatchResults match)
        {
            if (match == null) return;
            Console.WriteLine("=====================================================");
            Console.WriteLine("========================MATCH========================");
            Console.WriteLine("=====================================================");
            Console.WriteLine
            ($"wId:{match.winnerId}\n" +
            $"wS:{(match.winnerSide == Side.red ? "Red" : "Blue")}\n" +
            $"lId:{match.loserId}\n" +
            $"lGoals:{match.loserGoals}\n" +
            $"startTime:{match.startTime}\n" +
            $"Pmultiplier:{match.pushUpsMultiplier}\n");
            Console.WriteLine("Goals:");
            for (int j = 0; j < 20; j++)
            {
                if (match.goals[j] == null) break;
                Console.Write($"{(match.goals[j].side == Side.red ? "Red" : "Blue")}");
                Console.WriteLine($" - {match.goals[j].time}");
            }
            Console.WriteLine("=====================================================");
        }
        #endregion

        #region Match Buffer
        private static string matchesBufferFilePath = Path.Combine(FileSystem.AppDataDirectory, "matchesbuffer.json");
        public static async Task SaveMatchBuffereDataBase(List<MatchResults> matches)
        {
            string json = JsonSerializer.Serialize(matches);
            await File.WriteAllTextAsync(matchesBufferFilePath, json);
        }
        public static async Task AppendMatchBuffereDataBase(List<MatchResults> matches)
        {
            List<MatchResults> matchResultsBuffer = new List<MatchResults>();
            matchResultsBuffer.AddRange(LoadMatchBufferDataBase());
            matchResultsBuffer.AddRange(matches);
            DeleteMatchBufferDataBase();
            await SaveMatchBuffereDataBase(matchResultsBuffer);

            foreach(MatchResults match in matchResultsBuffer)
            {
                PrintMatchToConsole(match);
            }
            Console.WriteLine("=====================================================");
            Console.WriteLine($"{matchResultsBuffer.Count} Matches Added to JS MatchBuffer");
            Console.WriteLine("=====================================================");
        }

        public static List<MatchResults> LoadMatchBufferDataBase()
        {
            if (!File.Exists(matchesBufferFilePath)) return new List<MatchResults>();
            string json = File.ReadAllText(matchesBufferFilePath);
            return JsonSerializer.Deserialize<List<MatchResults>>(json);
        }
        public static void DeleteMatchBufferDataBase()
        {
            if (File.Exists(matchesBufferFilePath))
            {
                File.Delete(matchesBufferFilePath);
            }
        }
        #endregion
    }
}
