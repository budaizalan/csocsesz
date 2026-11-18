using System.Text.Json;

namespace Csocsesz.Classes;

// The whole app state, one JSON file. Same shape is used for export/import.
public class AppData
{
    public int version { get; set; } = 2;
    public List<Player> players { get; set; } = new();
    public List<Match> matches { get; set; } = new();
    // Only filled inside an export file: picture file name -> base64.
    public Dictionary<string, string> images { get; set; } = new();
}

public static class DataManager
{
    private static readonly string filePath = Path.Combine(FileSystem.AppDataDirectory, "csocsesz.json");
    private static readonly JsonSerializerOptions options = new() { WriteIndented = true };

    public static async Task Save(AppData data)
    {
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(data, options));
    }

    public static async Task<AppData?> Load()
    {
        if (!File.Exists(filePath)) return null;
        return Parse(await File.ReadAllTextAsync(filePath));
    }

    // Throws on garbage input, so a bad import file can't wipe the data.
    public static AppData Parse(string json)
    {
        var data = JsonSerializer.Deserialize<AppData>(json) ?? throw new InvalidDataException("Empty file.");
        if (data.players == null || data.matches == null) throw new InvalidDataException("Not a Csocsesz export.");
        return data;
    }

    public static string Serialize(AppData data) => JsonSerializer.Serialize(data, options);
}
