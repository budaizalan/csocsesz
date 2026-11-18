namespace Csocsesz.Classes;

public static class StatsCalculator
{
    // 1v1 and 2v2 are counted separately. Winner always has 10 goals.
    // Punishment = goals the loser was short of, times the multiplier of that match (every loser gets it).
    public static void Recalculate(List<Player> players, List<Match> matches)
    {
        var byId = players.ToDictionary(p => p.id);
        foreach (var p in players)
        {
            p.stats = new Stats { totalPunishmentCompleted = p.punishmentCompleted };
            p.stats2v2 = new Stats { totalPunishmentCompleted = p.punishmentCompleted2v2 };
        }

        foreach (var m in matches.OrderBy(m => m.startTime))
        {
            var winners = Team(byId, m.winnerId, m.winnerId2);
            var losers = Team(byId, m.loserId, m.loserId2);
            if (winners.Count == 0 || losers.Count == 0) continue;

            foreach (var w in winners)
            {
                var s = w.StatsFor(m.mode);
                s.totalMatchWon++;
                s.totalGoalsScored += m.winnerGoals;
                s.totalGoalsConceded += m.loserGoals;
                s.streak++;
            }
            foreach (var l in losers)
            {
                var s = l.StatsFor(m.mode);
                s.totalMatchLost++;
                s.totalGoalsScored += m.loserGoals;
                s.totalGoalsConceded += m.winnerGoals;
                s.totalPunishmentAssigned += (m.winnerGoals - m.loserGoals) * m.punishmentMultiplier;
                s.streak = 0;
            }
        }
    }

    // 2v2 stats of a fixed pair, in either order. No punishments here: they exist in 1v1 only.
    public static Stats TeamStats(IEnumerable<Match> matches, string a, string b)
    {
        var s = new Stats();
        bool IsTeam(string? x, string? y) => (x == a && y == b) || (x == b && y == a);
        foreach (var m in matches.Where(m => m.mode == Mode.TwoVsTwo).OrderBy(m => m.startTime))
        {
            if (IsTeam(m.winnerId, m.winnerId2))
            {
                s.totalMatchWon++; s.totalGoalsScored += m.winnerGoals; s.totalGoalsConceded += m.loserGoals; s.streak++;
            }
            else if (IsTeam(m.loserId, m.loserId2))
            {
                s.totalMatchLost++; s.totalGoalsScored += m.loserGoals; s.totalGoalsConceded += m.winnerGoals; s.streak = 0;
            }
        }
        return s;
    }

    private static List<Player> Team(Dictionary<string, Player> byId, string id1, string? id2) =>
        new[] { id1, id2 }.Where(id => id != null && byId.ContainsKey(id)).Select(id => byId[id!]).ToList();
}
