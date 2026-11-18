namespace Csocsesz.Classes
{
    public class Goal
    {
        public Side side { get; set; }
        public DateTime time { get; set; }
        public Goal() { }
        public Goal(Side side, DateTime time)
        {
            this.side = side;
            this.time = time;
        }
    }
    public class Match
    {
        public int id { get; set; }
        public Mode mode { get; set; }
        public string winnerId { get; set; } = string.Empty;
        public Side winnerSide { get; set; }
        public string loserId { get; set; } = string.Empty;
        // Second team member, 2v2 only.
        public string? winnerId2 { get; set; }
        public string? loserId2 { get; set; }
        public int loserGoals { get; set; }
        // 10 normally; more when the game was played on past 10.
        public int winnerGoals { get; set; } = 10;
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public List<Goal> goals { get; set; } = new();
        public int punishmentMultiplier { get; set; }
    }
}
