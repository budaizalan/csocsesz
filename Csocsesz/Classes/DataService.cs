using Csocsesz.Classes;
using System.Net.Http.Json;

namespace Csocsesz.Classes;
public static class DataService
{
    private static readonly HttpClient _httpClient = new HttpClient();

    private static bool dataLoaded = false;
    private static bool online = true;

    public static async Task LoadDataBases()
    {
        DataStore.Players = await LoadPlayerDataBase();
        DataStore.Matches = await LoadMatchDataBase();
        if(online)
        {
            await DataManager.SavePlayerDataBase(DataStore.Players);
            await DataManager.SaveMatchDataBase(DataStore.Matches);
        }
        else await DataManager.LoadOfflineDataBases();
        dataLoaded = true;

        if (DataStore.Players.Count >= 2)
        {
            AppSettings.playerRed = DataStore.Players.FirstOrDefault(p => p.id == DataStore.defaultPlayerRedId);
            AppSettings.playerBlue = DataStore.Players.FirstOrDefault(p => p.id == DataStore.defaultPlayerBlueId);
            if (AppSettings.playerRed == null || AppSettings.playerBlue == null)
            {
                AppSettings.playerRed = DataStore.Players[0];
                AppSettings.playerBlue = DataStore.Players[1];
            }
        }
        else
        {
            AppSettings.playerRed = new Player();
            AppSettings.playerBlue = new Player();
        }
    }

    #region ONLINE

    #region Load
    public static async Task<List<Player>> LoadPlayerDataBase()
    {
        try
        {
            var serverplayers = await _httpClient.GetFromJsonAsync<List<PlayerBridge>>($"{DataStore.BaseUrl}player");
            List<Player> players = new List<Player>();
            if(serverplayers != null)
            {
                foreach(var p in serverplayers)
                {
                    Player player = new Player
                                    (
                                        p.id,
                                        p.name,
                                        p.normalImage ?? "",
                                        p.sadImage ?? "",
                                        p.streak,
                                        p.totalMatchWon,
                                        p.totalMatchLost,
                                        p.totalGoalsScored,
                                        p.totalGoalsConceded,
                                        p.totalPunishmentAssigned,
                                        p.totalPunishmentCompleted
                                    );
                    players.Add(player);
                }
            }
            await Shell.Current.DisplayAlertAsync("Success", $"Successfully loaded: {players.Count} players.", "OK");
            return players ?? new List<Player>();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"Failed to load players: {ex.Message}", "OK");
            online = false;
            return new List<Player>();
        }
    }

    public static async Task<List<Match>> LoadMatchDataBase()
    {
        try
        {
            var serverMatches = await _httpClient.GetFromJsonAsync<List<MatchBridge>>($"{DataStore.BaseUrl}match");
            List<Match> matches = new List<Match>();

            if (serverMatches != null)
            {
                foreach (var m in serverMatches)
                {
                    Match match = new Match
                    (
                        m.id,
                        m.winnerId,
                        m.winnerSide,
                        m.loserId,
                        m.loserGoals,
                        m.startTime,
                        m.endTime,
                        new List<Goal>(),
                        m.punishmentMultiplier
                    );
                    if (m.goals != null)
                    {
                        foreach (var g in m.goals)
                        {
                            Goal newGoal = new Goal(g.side, g.time);
                            match.goals.Add(newGoal);
                        }
                    }
                    matches.Add(match);
                }
            }
            await Shell.Current.DisplayAlertAsync("Success", $"Successfully loaded: {matches.Count} matches.", "OK");
            return matches;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"Failed to load matches: {ex.Message}", "OK");
            online = false;
            return new List<Match>();
        }
    }
    #endregion

    #region Upload
    public static async Task<bool> PostPlayer(Player player)
    {
        try
        {
            var bridge = new PlayerBridge
            {
                id = player.id,
                name = player.name,
                normalImage = player.normalImage,
                sadImage = player.sadImage,
                streak = player.stats.streak,
                totalMatchWon = player.stats.totalMatchWon,
                totalMatchLost = player.stats.totalMatchLost,
                totalGoalsScored = player.stats.totalGoalsScored,
                totalGoalsConceded = player.stats.totalGoalsConceded,
                totalPunishmentAssigned = player.stats.totalPunishmentAssigned,
                totalPunishmentCompleted = player.stats.totalPunishmentCompleted
            };

            var response = await _httpClient.PostAsJsonAsync($"{DataStore.BaseUrl}player", bridge);

            if (response.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlertAsync("Success", "Player saved successfully.", "OK");
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            await Shell.Current.DisplayAlertAsync("Error", $"Server error: {error}", "OK");
            return false;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"Failed to post player: {ex.Message}", "OK");
            return false;
        }
    }
    public static async Task<bool> PostMatch(Match match)
    {
        try
        {
            var bridge = new MatchBridge
            {
                id = 0,
                winnerId = match.winnerId,
                winnerSide = match.winnerSide,
                loserId = match.loserId,
                loserGoals = match.loserGoals,
                startTime = match.startTime,
                endTime = match.endTime,
                punishmentMultiplier = match.punishmentMultiplier,
                goals = match.goals.Select(g => new GoalBridge(g.side, g.time)).ToList()
            };

            var response = await _httpClient.PostAsJsonAsync($"{DataStore.BaseUrl}match", bridge);

            if (response.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlertAsync("Success", "Match saved successfully.", "OK");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"Failed to post match: {ex.Message}", "OK");
            return false;
        }
    }
    #endregion

    #region Delete
    public static async Task<bool> DeleteMatch(int matchId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{DataStore.BaseUrl}match/{matchId}");

            if (response.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlertAsync("Success", "Match deleted successfully.", "OK");
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            await Shell.Current.DisplayAlertAsync("Error", $"Failed to delete match: {error}", "OK");
            return false;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"Connection error: {ex.Message}", "OK");
            return false;
        }
    }
    #endregion

    #endregion
}