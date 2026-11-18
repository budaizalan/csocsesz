using System.Text.Json.Serialization;

namespace Csocsesz.Classes;
public class Player
{
    public string id { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public string normalImage { get; set; } = string.Empty; // file name in ImageStore
    public string sadImage { get; set; } = string.Empty;
    [JsonIgnore] public string normalImagePath => ImageStore.Resolve(normalImage, false);
    [JsonIgnore] public string sadImagePath => ImageStore.Resolve(sadImage, true);
    // Only this is stored; every other stat is derived from the matches (see StatsCalculator).
    public int punishmentCompleted { get; set; }
    public int punishmentCompleted2v2 { get; set; }
    [JsonIgnore] public Stats stats { get; set; } = new();
    [JsonIgnore] public Stats stats2v2 { get; set; } = new();
    public Stats StatsFor(Mode mode) => mode == Mode.TwoVsTwo ? stats2v2 : stats;
    [JsonIgnore] public InGame inGame { get; set; } = new();
}
public class Stats
{
    public int streak { get; set; }
    public int totalMatchWon { get; set; }
    public int totalMatchLost { get; set; }
    public int totalGoalsScored { get; set; }
    public int totalGoalsConceded { get; set; }
    public int totalPunishmentAssigned { get; set; }
    public int totalPunishmentCompleted { get; set; }
}
public enum Side { red, blue };
public enum Mode { OneVsOne, TwoVsTwo };
public class InGame
{
    public int matchWon { get; set; }
    public int goals { get; set; }
}
