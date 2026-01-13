using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Csocsesz.Classes;
public class PlayerBridge
{
    public string id { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public string? normalImage { get; set; }
    public string? sadImage { get; set; }
    public int streak { get; set; }
    public int totalMatchWon { get; set; }
    public int totalMatchLost { get; set; }
    public int totalGoalsScored { get; set; }
    public int totalGoalsConceded { get; set; }
    public int totalPunishmentAssigned { get; set; }
    public int totalPunishmentCompleted { get; set; }
    public PlayerBridge() { }
}
public class Player
{
    public string id { get; set; }
    public string name { get; set; }
    public string normalImage { get; set; } = string.Empty;
    public string sadImage { get; set; } = string.Empty;
    public Stats stats { get; set; }
    public InGame? inGame { get; set; }
    public Player() { }
    public Player(string id, string name, string normalImage, string sadImage,
            int streak, int totalMatchWon, int totalGoalsLost, int totalGoalsScored,
            int totalGoalsConceded, int totalPunishmentAssigned, int totalPunishmentCompleted)
    {
        this.id = id;
        this.name = name;
        this.normalImage = normalImage;
        this.sadImage = sadImage;
        this.stats = new Stats
            (
            streak,
            totalMatchWon,
            totalGoalsLost,
            totalGoalsScored,
            totalGoalsConceded,
            totalPunishmentAssigned,
            totalPunishmentCompleted
            );
        this.inGame = new InGame(0, 0);
    }
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
    public Stats() { }
    public Stats
        (
            int streak,
            int totalMatchWon,
            int totalMatchLost,
            int totalGoalsScored,
            int totalGoalsConceded,
            int totalPunishmentAssigned,
            int totalPunishmentCompleted
        )
    {
        this.streak = streak;
        this.totalMatchWon = totalMatchWon;
        this.totalMatchLost = totalMatchLost;
        this.totalGoalsScored = totalGoalsScored;
        this.totalGoalsConceded = totalGoalsConceded;
        this.totalPunishmentAssigned = totalPunishmentAssigned;
        this.totalPunishmentCompleted = totalPunishmentCompleted;
    }
}
public enum Side { red, blue };
public class InGame
{
    public int matchWon { get; set; }
    public int goals { get; set; }
    public InGame(int matchWon, int goals)
    {
        this.matchWon = matchWon;
        this.goals = goals;
    }
}
