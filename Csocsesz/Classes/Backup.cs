#if ANDROID
using Android.Content;
using Android.OS;
using Android.Provider;
#endif

namespace Csocsesz.Classes;

// A copy of all data (export format, with pictures) outside the app's private folder,
// so uninstalling / reinstalling the app doesn't lose matches.
// Android: Downloads/Csocsesz/csocsesz-backup.json via MediaStore (API 29+, no permission needed).
// Elsewhere: Documents/Csocsesz. shortcut: Android 9 and older get no backup, add storage permission if ever needed.
public static class Backup
{
    private const string Name = "csocsesz-backup.json";

#if ANDROID
    private static readonly Android.Net.Uri Downloads = MediaStore.Downloads.ExternalContentUri!;

    private static Android.Net.Uri? Find()
    {
        var resolver = Platform.AppContext.ContentResolver!;
        using var cursor = resolver.Query(Downloads, new[] { "_id" }, "_display_name=?", new[] { Name }, null);
        if (cursor != null && cursor.MoveToFirst()) return ContentUris.WithAppendedId(Downloads, cursor.GetLong(0));
        return null;
    }

    public static void Write(string json)
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.Q) return;
        var resolver = Platform.AppContext.ContentResolver!;
        var uri = Find();
        if (uri == null)
        {
            var values = new ContentValues();
            values.Put("_display_name", Name);
            values.Put("mime_type", "application/json");
            values.Put("relative_path", "Download/Csocsesz");
            uri = resolver.Insert(Downloads, values);
        }
        if (uri == null) return;
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        using var stream = resolver.OpenOutputStream(uri, "wt")!;
        stream.Write(bytes, 0, bytes.Length);
    }

    public static string? Read()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.Q) return null;
        var uri = Find();
        if (uri == null) return null;
        var stream = Platform.AppContext.ContentResolver!.OpenInputStream(uri);
        if (stream == null) return null;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
#else
    private static string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Csocsesz", Name);

    public static void Write(string json)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, json);
    }

    public static string? Read() => File.Exists(FilePath) ? File.ReadAllText(FilePath) : null;
#endif
}
