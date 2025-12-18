using System;
using System.Collections.Generic;
using System.Text;

namespace Csocsesz.Classes
{
    public class MatchResults
    {
        public string winnerId { get; set; }
        public string loserId { get; set; }
        public int loserGoals { get; set; }
        public TimeSpan timeSpan { get; set; }
        public DateTime date { get; set; }
        public Side[] goals { get; set; }
        public int pushUpsMultiplier { get; set; }

        public MatchResults(string winnerId, string loserId, int loserGoals, TimeSpan timeSpan, DateTime date, int pushUpsMultiplier)
        {
            this.winnerId = winnerId;
            this.loserId = loserId;
            this.loserGoals = loserGoals;
            this.timeSpan = timeSpan;
            this.date = date;
            this.goals = new Side[20];
            this.pushUpsMultiplier = pushUpsMultiplier;
        }
    }
}
