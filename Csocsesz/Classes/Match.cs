using Csocsesz.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Csocsesz.Classes
{
    public class GoalBridge
    {
        public int id { get; set; }
        public int matchId { get; set; }
        public Side side { get; set; }
        public DateTime time { get; set; }
        public GoalBridge(Side side, DateTime time)
        {
            this.id = 0;
            this.side = side;
            this.time = time;
        }
    }
    public class Goal
    {
        public Side side { get; set; }
        public DateTime time { get; set; }
        public Goal(Side side, DateTime time)
        {
            this.side = side;
            this.time = time;
        }
    }
    public class MatchBridge
    {
        public int id { get; set; }
        public string winnerId { get; set; } = string.Empty;
        public Side winnerSide { get; set; }
        public string loserId { get; set; } = string.Empty;
        public int loserGoals { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public List<GoalBridge> goals { get; set; } = new();
        public int punishmentMultiplier { get; set; }
    }
    public class Match
    {
        public int id { get; set; }
        public string winnerId { get; set; } = string.Empty;
        public Side winnerSide { get; set; }
        public string loserId { get; set; } = string.Empty;
        public int loserGoals { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public List<Goal> goals { get; set; } = new();
        public int punishmentMultiplier { get; set; }

        public Match
            (
            int id,
            string winnerId, 
            Side winnerSide, 
            string loserId, 
            int loserGoals, 
            DateTime startTime, 
            DateTime endTime,
            List<Goal> goals,
            int punishmentMultiplier)
        {
            this.id = id;
            this.winnerId = winnerId;
            this.winnerSide = winnerSide;
            this.loserId = loserId;
            this.loserGoals = loserGoals;
            this.startTime = startTime;
            this.endTime = endTime;
            this.goals = goals;
            this.punishmentMultiplier = punishmentMultiplier;
        }
    }
}