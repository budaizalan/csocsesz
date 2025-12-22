using System;
using System.Collections.Generic;
using System.Text;

namespace Csocsesz.Classes
{
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
    public class MatchResults
    {
        public string winnerId { get; set; }
        public Side winnerSide { get; set; }
        public string loserId { get; set; }
        public int loserGoals { get; set; }
        public DateTime startTime { get; set; }
        public Goal[] goals { get; set; }
        public int pushUpsMultiplier { get; set; }

        public MatchResults(string winnerId, Side winnerSide, string loserId, int loserGoals, DateTime startTime, int pushUpsMultiplier)
        {
            this.winnerId = winnerId;
            this.winnerSide = winnerSide;
            this.loserId = loserId;
            this.loserGoals = loserGoals;
            this.startTime = startTime;
            this.goals = new Goal[20];
            this.pushUpsMultiplier = pushUpsMultiplier;
        }
    }
}
