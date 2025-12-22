using Csocsesz.Classes;
using static Csocsesz.ContentPages.HistoryPage;

namespace Csocsesz.ContentPages;

public partial class SelectedMatchPage : ContentPage
{
    public class GoalDisplay
    {
        public Side side { get; set; }
        public TimeSpan time { get; set; }

        public ImageSource image =>
            side == Side.red ? ImageSource.FromFile("redgoal_icon.svg") : ImageSource.FromFile("bluegoal_icon.svg");
        public string TimeText => $"{(int)time.TotalMinutes:00}:{time.Seconds:00}";
        public GoalDisplay(MatchResults match, Goal goal)
        {
            this.side = goal.side;
            this.time = goal.time - match.startTime;
        }
    }
    public class GoalGroup : List<GoalDisplay>
    {
        public string Date { get; private set; }

        public GoalGroup(string date, List<GoalDisplay> goals) : base(goals)
        {
            Date = date;
        }
    }
    public SelectedMatchPage()
	{
		InitializeComponent();
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Start();
    }
    private void Start()
    {
        var currentMatch = DataStore.selectedMatch;
        if (currentMatch == null || currentMatch.goals == null) return;

        MatchDateLabel.Text = currentMatch.startTime.ToString("MM.dd\nHH:mm");
        MatchResultLabel.Text = $"10 - {currentMatch.loserGoals}";
        var winner = DataStore.Players.FirstOrDefault(p => p.id == currentMatch.winnerId);
        var loser = DataStore.Players.FirstOrDefault(p => p.id == currentMatch.loserId);
        if (winner != null && loser != null)
        {
            MatchWinnerImage.Source = winner.inGame.normalImage;
            MatchLoserImage.Source = loser.inGame.normalImage;
            MatchWinnerNameLabel.Text = winner.name;
            MatchLoserNameLabel.Text = loser.name;
        }
        WinnerColor.Color = currentMatch.winnerSide == Side.red ? DataStore.red : DataStore.blue;
        LoserColor.Color = currentMatch.winnerSide == Side.red ? DataStore.blue : DataStore.red;

        var displayList = new List<GoalDisplay>();

        foreach (var goal in currentMatch.goals)
        {
            if(goal != null) displayList.Add(new GoalDisplay(currentMatch, goal));
        }

        var grouped = displayList
        .GroupBy(g => g.time.ToString("mm") + ":00")
        .Select(g => new GoalGroup(g.Key, g.OrderBy(x => x.time).ToList()))
        .OrderBy(g => g.Date)
        .ToList();

        GoalsCollectionView.ItemsSource = grouped;
    }
    private async void BackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}