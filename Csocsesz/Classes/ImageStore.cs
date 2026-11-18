using Microsoft.Maui.Graphics.Platform;

namespace Csocsesz.Classes;

// Player pictures live as files in the app data folder; players only store the file name.
public static class ImageStore
{
    private static readonly string dir = Path.Combine(FileSystem.AppDataDirectory, "images");

    // Missing or empty name falls back to the bundled default face.
    public static string Resolve(string name, bool sad)
    {
        string path = Path.Combine(dir, Path.GetFileName(name ?? ""));
        if (!string.IsNullOrEmpty(name) && File.Exists(path)) return path;
        return sad ? "sadface_icon.png" : "normalface_icon.png";
    }

    // Downsizes to 512px (phone photos are huge) and stores under a fresh name, so the image cache can't show a stale picture.
    public static async Task<string> Save(Stream source, string playerId, bool sad)
    {
        Directory.CreateDirectory(dir);
        string name = $"{playerId}_{(sad ? "sad" : "normal")}_{DateTime.UtcNow.Ticks}.png";
        using var image = PlatformImage.FromStream(source);
        using var small = image.Downsize(512, true);
        using var file = File.Create(Path.Combine(dir, name));
        await small.SaveAsync(file, ImageFormat.Png);
        return name;
    }

    public static void Delete(string name)
    {
        try { File.Delete(Path.Combine(dir, Path.GetFileName(name ?? ""))); } catch { }
    }

    // For export: file name -> base64, only for the pictures players actually use.
    public static Dictionary<string, string> CollectAll(IEnumerable<Player> players)
    {
        var result = new Dictionary<string, string>();
        foreach (var name in players.SelectMany(p => new[] { p.normalImage, p.sadImage }).Where(n => !string.IsNullOrEmpty(n)).Distinct())
        {
            string path = Path.Combine(dir, Path.GetFileName(name));
            if (File.Exists(path)) result[name] = Convert.ToBase64String(File.ReadAllBytes(path));
        }
        return result;
    }

    public static void WriteAll(Dictionary<string, string> images)
    {
        Directory.CreateDirectory(dir);
        foreach (var (name, base64) in images)
            File.WriteAllBytes(Path.Combine(dir, Path.GetFileName(name)), Convert.FromBase64String(base64));
    }
}
