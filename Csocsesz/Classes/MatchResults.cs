using System;
using System.Collections.Generic;
using System.Text;

namespace Csocsesz.Classes
{
    internal class MatchResults
    {
        public int winnerId { get; set; }
        public int loserId { get; set; }
        public int loserGoals { get; set; }

        public MatchResults(int winnerId, int loserId, int loserGoals)
        {
            this.winnerId = winnerId;
            this.loserId = loserId;
            this.loserGoals = loserGoals;
        }
    }
}
