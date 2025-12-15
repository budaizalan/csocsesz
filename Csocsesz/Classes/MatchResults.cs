using System;
using System.Collections.Generic;
using System.Text;

namespace Csocsesz.Classes
{
    internal class MatchResults
    {
        public string winnerId { get; set; }
        public string loserId { get; set; }
        public int loserGoals { get; set; }

        public MatchResults(string winnerId, string loserId, int loserGoals)
        {
            this.winnerId = winnerId;
            this.loserId = loserId;
            this.loserGoals = loserGoals;
        }
    }
}
