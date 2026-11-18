namespace Csocsesz.Classes;

// Everything is local: one JSON file in the app data folder, export/import by file.
public static class DataService
{
    private static AppData data = new();

    public static async Task LoadDataBases()
    {
        var redId = AppSettings.playerRed?.id ?? DataStore.defaultPlayerRedId;
        var blueId = AppSettings.playerBlue?.id ?? DataStore.defaultPlayerBlueId;

        data = await DataManager.Load() ?? Seed();
        if (await SeedDefaultImages()) await DataManager.Save(data);
        Publish(redId, blueId, AppSettings.playerRed2?.id, AppSettings.playerBlue2?.id);
    }

    private static AppData Seed() => new()
    {
        players = new()
        {
            new Player { id = "hugo0001", name = "Hugo" },
            new Player { id = "zazzzzuska0001", name = "Zazzzzuska" },
            new Player { id = "guest0001", name = "Guest1" },
            new Player { id = "guest0002", name = "Guest2" },
        }
    };

    // The two original players get their pictures from bundled files, stored like any uploaded image.
    private static async Task<bool> SeedDefaultImages()
    {
        bool changed = false;
        foreach (var (id, normal, sad) in new[] { ("hugo0001", "hugo_icon.png", "hugosad_icon.png"), ("zazzzzuska0001", "zalan_icon.png", "zalansad_icon.png") })
        {
            var p = data.players.FirstOrDefault(x => x.id == id);
            if (p == null) continue;
            if (p.normalImage == "") { p.normalImage = await FromAsset(id, normal, false); changed |= p.normalImage != ""; }
            if (p.sadImage == "") { p.sadImage = await FromAsset(id, sad, true); changed |= p.sadImage != ""; }
        }
        return changed;
    }
    private static async Task<string> FromAsset(string playerId, string asset, bool sad)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(asset);
            return await ImageStore.Save(stream, playerId, sad);
        }
        catch { return ""; }
    }

    private static void Publish(string redId, string blueId, string? red2Id = null, string? blue2Id = null)
    {
        StatsCalculator.Recalculate(data.players, data.matches);
        DataStore.Players = data.players;
        DataStore.Matches = data.matches;

        var red = data.players.FirstOrDefault(p => p.id == redId);
        var blue = data.players.FirstOrDefault(p => p.id == blueId);
        if (red == null || blue == null || red == blue)
        {
            red = data.players.ElementAtOrDefault(0) ?? new Player();
            blue = data.players.ElementAtOrDefault(1) ?? new Player();
        }
        AppSettings.playerRed = red;
        AppSettings.playerBlue = blue;
        AppSettings.playerRed2 = data.players.FirstOrDefault(p => p.id == red2Id);
        AppSettings.playerBlue2 = data.players.FirstOrDefault(p => p.id == blue2Id);
    }

    private static async Task Commit()
    {
        StatsCalculator.Recalculate(data.players, data.matches);
        await DataManager.Save(data);
    }

    public static async Task AddMatch(Match match)
    {
        match.id = data.matches.Count == 0 ? 1 : data.matches.Max(m => m.id) + 1;
        data.matches.Add(match);
        await Commit();
    }

    public static async Task DeleteMatch(int matchId)
    {
        data.matches.RemoveAll(m => m.id == matchId);
        await Commit();
    }

    public static async Task AddPlayer(string name)
    {
        name = name.Trim();
        if (name.Length == 0) throw new ArgumentException("Name is empty.");
        if (data.players.Any(p => p.name.Equals(name, StringComparison.OrdinalIgnoreCase))) throw new ArgumentException($"{name} already exists.");
        data.players.Add(new Player { id = "p" + Guid.NewGuid().ToString("N")[..8], name = name });
        await Commit();
    }

    public static async Task ChangePunishmentCompleted(string playerId, int amount, Mode mode)
    {
        var player = data.players.FirstOrDefault(p => p.id == playerId);
        if (player == null) return;
        if (mode == Mode.TwoVsTwo) player.punishmentCompleted2v2 = Math.Max(0, player.punishmentCompleted2v2 + amount);
        else player.punishmentCompleted = Math.Max(0, player.punishmentCompleted + amount);
        await Commit();
    }

    // Pictures travel inside the export file, so an import on another device brings them along.
    public static string Export()
    {
        data.images = ImageStore.CollectAll(data.players);
        try { return DataManager.Serialize(data); }
        finally { data.images = new(); }
    }

    public static async Task SetPlayerImage(string playerId, Stream source, bool sad)
    {
        var player = data.players.FirstOrDefault(p => p.id == playerId);
        if (player == null) return;
        string name = await ImageStore.Save(source, playerId, sad);
        ImageStore.Delete(sad ? player.sadImage : player.normalImage);
        if (sad) player.sadImage = name; else player.normalImage = name;
        await Commit();
    }

    // Replaces everything with the file's content. Parse throws before anything is touched.
    public static async Task Import(string json)
    {
        var imported = DataManager.Parse(json);
        ImageStore.WriteAll(imported.images);
        imported.images = new();
        data = imported;
        await DataManager.Save(data);
        Publish(AppSettings.playerRed?.id ?? "", AppSettings.playerBlue?.id ?? "", AppSettings.playerRed2?.id, AppSettings.playerBlue2?.id);
    }
}
