namespace Csocsesz.Classes;

// Everything is local: one JSON file in the app data folder, export/import by file.
public static class DataService
{
    private static AppData data = new();

    // Pages await this so they never read players before the first load is done.
    public static Task Loading = Task.CompletedTask;

    public static async Task LoadDataBases()
    {
        var redId = AppSettings.playerRed?.id ?? DataStore.defaultPlayerRedId;
        var blueId = AppSettings.playerBlue?.id ?? DataStore.defaultPlayerBlueId;

        data = await DataManager.Load() ?? await Restore() ?? Seed();
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
        await Task.Run(WriteBackup);
    }

    // The backup must never break saving, so failures are swallowed.
    private static void WriteBackup()
    {
        try { Backup.Write(Export()); } catch { }
    }

    // No local data (fresh install): take the last backup, if there is one.
    private static async Task<AppData?> Restore()
    {
        try
        {
            var json = Backup.Read();
            if (json == null) return null;
            var restored = DataManager.Parse(json);
            ImageStore.WriteAll(restored.images);
            restored.images = new();
            await DataManager.Save(restored);
            return restored;
        }
        catch { return null; }
    }

    public static async Task AddMatch(Match match)
    {
        match.id = data.matches.Count == 0 ? 1 : data.matches.Max(m => m.id) + 1;
        data.matches.Add(match);
        await Commit();
    }

    // Sets the final score. Fewer goals for a side: its last goals are removed; more: the extra goals
    // land on the match's last timestamp. The winner is whoever ends up ahead.
    public static async Task EditMatch(int matchId, int red, int blue)
    {
        var m = data.matches.FirstOrDefault(x => x.id == matchId);
        if (m == null) return;
        if (red < 0 || blue < 0 || red == blue) throw new ArgumentException("Goals can't be negative and the game can't end in a draw.");
        m.goals ??= new();
        foreach (var (side, target) in new[] { (Side.red, red), (Side.blue, blue) })
        {
            int have = m.goals.Count(g => g.side == side);
            for (; have > target; have--) m.goals.RemoveAt(m.goals.FindLastIndex(g => g.side == side));
            for (; have < target; have++) m.goals.Add(new Goal(side, m.endTime));
        }
        var winSide = red > blue ? Side.red : Side.blue;
        if (winSide != m.winnerSide)
        {
            (m.winnerId, m.loserId) = (m.loserId, m.winnerId);
            (m.winnerId2, m.loserId2) = (m.loserId2, m.winnerId2);
        }
        m.winnerSide = winSide;
        m.winnerGoals = Math.Max(red, blue);
        m.loserGoals = Math.Min(red, blue);
        if (m.goals.Count > 0) m.endTime = m.goals[^1].time;
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

    public static async Task RenamePlayer(string playerId, string name)
    {
        name = name.Trim();
        if (name.Length == 0) throw new ArgumentException("Name is empty.");
        var player = data.players.FirstOrDefault(p => p.id == playerId);
        if (player == null) return;
        if (data.players.Any(p => p.id != playerId && p.name.Equals(name, StringComparison.OrdinalIgnoreCase))) throw new ArgumentException($"{name} already exists.");
        player.name = name;
        await Commit();
    }

    private static bool Involves(Match m, string id) => m.winnerId == id || m.loserId == id || m.winnerId2 == id || m.loserId2 == id;
    public static int MatchCount(string playerId) => data.matches.Count(m => Involves(m, playerId));

    // The player's matches go with them, otherwise history would point at nobody.
    public static async Task DeletePlayer(string playerId)
    {
        if (data.players.Count <= 2) throw new ArgumentException("At least 2 players are needed.");
        var player = data.players.FirstOrDefault(p => p.id == playerId);
        if (player == null) return;
        data.matches.RemoveAll(m => Involves(m, playerId));
        data.players.Remove(player);
        ImageStore.Delete(player.normalImage);
        ImageStore.Delete(player.sadImage);
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
        await Task.Run(WriteBackup);
        Publish(AppSettings.playerRed?.id ?? "", AppSettings.playerBlue?.id ?? "", AppSettings.playerRed2?.id, AppSettings.playerBlue2?.id);
    }
}
