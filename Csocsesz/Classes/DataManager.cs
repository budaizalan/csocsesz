using System;
using System.Collections.Generic;
using System.Text;

using System.Text.Json;

namespace Csocsesz.Classes;
public static class DataManager
{
    public static async Task LoadOfflineDataBases()
    {
        DataStore.Players = await LoadPlayerDataBase();
        DataStore.Matches = await LoadMatchDataBase();
    }
    #region Player
    private static string playersFilePath = Path.Combine(FileSystem.AppDataDirectory, "players.json");
    public static async Task SavePlayerDataBase(List<Player> players)
    {
        string json = JsonSerializer.Serialize(players);
        await File.WriteAllTextAsync(playersFilePath, json);
    }
    public static async Task<List<Player>> LoadPlayerDataBase()
    {
        if (!File.Exists(playersFilePath)) return new List<Player>();
        string json = File.ReadAllText(playersFilePath);
        return JsonSerializer.Deserialize<List<Player>>(json);
    }
    public static async Task DeletePlayerDataBase()
    {
        if (File.Exists(playersFilePath)) File.Delete(playersFilePath);
    }
    #endregion

    #region Match
    private static string matchesFilePath = Path.Combine(FileSystem.AppDataDirectory, "matches.json");
    public static async Task SaveMatchDataBase(List<Match> matches)
    {
        string json = JsonSerializer.Serialize(matches);
        await File.WriteAllTextAsync(matchesFilePath, json);
    }
    public static async Task<List<Match>> LoadMatchDataBase()
    {
        if (!File.Exists(matchesFilePath)) return new List<Match>();
        string json = File.ReadAllText(matchesFilePath);
        //return JsonSerializer.Deserialize<List<MatchResults>>(json);

        List<Match> matches = JsonSerializer.Deserialize<List<Match>>(json);
        List<Match> matchbuffer = await LoadMatchBufferDataBase();
        if (matchbuffer.Count != 0) matches.AddRange(matchbuffer);

        return matches;

    }
    public static async Task DeleteMatchDataBase()
    {
        if (File.Exists(matchesFilePath)) File.Delete(matchesFilePath);
    }
    #endregion

    #region Match Buffer
    private static string matchesBufferFilePath = Path.Combine(FileSystem.AppDataDirectory, "matchesbuffer.json");
    public static async Task SaveMatchBuffereDataBase(List<Match> matches)
    {
        string json = JsonSerializer.Serialize(matches);
        await File.WriteAllTextAsync(matchesBufferFilePath, json);
    }
    public static async Task AppendMatchBufferDataBase(List<Match> matches)
    {
        List<Match> matchResultsBuffer = new List<Match>();
        matchResultsBuffer.AddRange(await LoadMatchBufferDataBase());
        matchResultsBuffer.AddRange(matches);
        await DeleteMatchBufferDataBase();
        await SaveMatchBuffereDataBase(matchResultsBuffer);

        Console.WriteLine("=====================================================");
        Console.WriteLine($"{matchResultsBuffer.Count} Matches Added to JS MatchBuffer");
        Console.WriteLine("=====================================================");
    }
    public static async Task<List<Match>> LoadMatchBufferDataBase()
    {
        if (!File.Exists(matchesBufferFilePath)) return new List<Match>();
        string json = File.ReadAllText(matchesBufferFilePath);
        return JsonSerializer.Deserialize<List<Match>>(json);
    }
    public static async Task DeleteMatchBufferDataBase()
    {
        if (File.Exists(matchesBufferFilePath)) File.Delete(matchesBufferFilePath);
    }
    #endregion
}
