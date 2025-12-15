using System;
using System.Collections.Generic;
using System.Text;

namespace Csocsesz.Classes
{
    class Player
    {
        public string id { get; set; }
        public string name { get; set; }
        public Stats stats { get; set; }
        public InGame? inGame { get; set; }

        public Player(string id, string name, int streak, int totalGoals, int totalMatchWon, int totalMatchLost, int matchWon, int goals, Side side)
        {
            this.id = id;
            this.name = name;
            this.stats = new Stats(streak, totalGoals, totalMatchWon, totalMatchLost);
            this.inGame = new InGame(matchWon, goals, side);
        }
    }
    class Stats
    {
        public int streak { get; set; }
        public int totalGoals;
        static public int totalMatchWon { get; set; }
        static public int totalMatchLost { get; set; }
        public double winRate { get { return totalMatchWon / (totalMatchLost + totalMatchWon); } }
        public Stats(int steak, int totalGoals, int totalMatchWonn, int totalMatchLostt)
        {
            this.streak = steak;
            this.totalGoals = totalGoals;
            totalMatchWon = totalMatchWonn;
            totalMatchLost = totalMatchLostt;
        }
    }
    enum Side { red, blue };
    class InGame
    {
        public int matchWon { get; set; }
        public int goals { get; set; }
        public Side side { get; set; }
        public InGame(int matchWon, int goals, Side side)
        {
            this.matchWon = matchWon;
            this.goals = goals;
            this.side = side;
        }
    }


}
