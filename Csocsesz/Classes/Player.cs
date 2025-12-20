using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Csocsesz.Classes
{
    public class Player
    {
        [JsonPropertyName("_id")]
        public string id { get; set; }
        public string name { get; set; }
        public Stats stats { get; set; }
        public InGame? inGame { get; set; }
        public Player() { }
        public Player(string id, string name, 
            int streak, int totalGoals, int totalMatchWon, int totalMatchLost, 
            int matchWon, int goals, ImageSource normalImage, ImageSource sadImage)
        {
            this.id = id;
            this.name = name;
            this.stats = new Stats(streak, totalGoals, totalMatchWon, totalMatchLost);
            this.inGame = new InGame(matchWon, goals, normalImage, sadImage);
        }
    }
    public class Stats
    {
        public int streak { get; set; }
        public int totalGoals;
        public int totalMatchWon { get; set; }
        public int totalMatchLost { get; set; }

        // A winRate-nél figyelj: double osztás kell, különben 0 lesz az eredmény
        public double winRate
        {
            get
            {
                if (totalMatchWon + totalMatchLost == 0) return 0;
                return (double)totalMatchWon / (totalMatchLost + totalMatchWon);
            }
        }
        public Stats() { }
        public Stats(int steak, int totalGoals, int totalMatchWonn, int totalMatchLostt)
        {
            this.streak = steak;
            this.totalGoals = totalGoals;
            this.totalMatchWon = totalMatchWonn;
            this.totalMatchLost = totalMatchLostt;
        }
    }
    public enum Side {red, blue};
    public class InGame
    {
        public int matchWon { get; set; }
        public int goals { get; set; }
        public ImageSource normalImage{get; set; }
        public ImageSource sadImage{get; set; }
        public InGame(int matchWon, int goals, ImageSource normalImage, ImageSource sadImage)
        {
            this.matchWon = matchWon;
            this.goals = goals;
            this.normalImage = normalImage;
            this.sadImage = sadImage;
        }
    }
}
