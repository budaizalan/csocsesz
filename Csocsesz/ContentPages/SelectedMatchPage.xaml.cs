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
        public GoalDisplay(Match match, Goal goal)
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
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Task.Yield();

        try
        {
            await Start();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }
    private async Task Start()
    {
        var currentMatch = DataStore.selectedMatch;
        if (currentMatch == null || currentMatch.goals == null) return;

        MatchDateLabel.Text = currentMatch.startTime.ToString("MM.dd\nHH:mm");
        MatchResultLabel.Text = $"{currentMatch.winnerGoals} - {currentMatch.loserGoals}";
        var winner = DataStore.Players.FirstOrDefault(p => p.id == currentMatch.winnerId);
        var loser = DataStore.Players.FirstOrDefault(p => p.id == currentMatch.loserId);
        if (winner != null && loser != null)
        {
            MatchWinnerImage.Source1 = winner.normalImagePath;
            MatchLoserImage.Source1 = loser.sadImagePath;
            MatchWinnerImage.Source2 = DataStore.Players.FirstOrDefault(p => p.id == currentMatch.winnerId2)?.normalImagePath;
            MatchLoserImage.Source2 = DataStore.Players.FirstOrDefault(p => p.id == currentMatch.loserId2)?.sadImagePath;
            MatchWinnerNameLabel.Text = MatchDisplay.TeamNames(currentMatch.winnerId, currentMatch.winnerId2);
            MatchLoserNameLabel.Text = MatchDisplay.TeamNames(currentMatch.loserId, currentMatch.loserId2);
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
    private async void ModifyButtonClicked(object sender, EventArgs e)
    {
        var m = DataStore.selectedMatch;
        if (m == null) return;
        // Names per side, whoever won.
        string winners = MatchDisplay.TeamNames(m.winnerId, m.winnerId2).Replace("\n", " & ");
        string losers = MatchDisplay.TeamNames(m.loserId, m.loserId2).Replace("\n", " & ");
        bool redWon = m.winnerSide == Side.red;
        int red = redWon ? m.winnerGoals : m.loserGoals, blue = redWon ? m.loserGoals : m.winnerGoals;
        string? r = await DisplayPromptAsync("Red goals", redWon ? winners : losers, "NEXT", "Cancel", keyboard: Keyboard.Numeric, initialValue: red.ToString());
        if (r == null) return;
        string? b = await DisplayPromptAsync("Blue goals", redWon ? losers : winners, "SAVE", "Cancel", keyboard: Keyboard.Numeric, initialValue: blue.ToString());
        if (b == null) return;
        try
        {
            if (!int.TryParse(r, out int newRed) || !int.TryParse(b, out int newBlue)) throw new ArgumentException("Enter whole numbers.");
            await DataService.EditMatch(m.id, newRed, newBlue);
            await Start();
        }
        catch (ArgumentException ex)
        {
            await DisplayAlertAsync("Modify", ex.Message, "OK");
        }
    }
    private async void DeleteButtonClicked(object sender, EventArgs e)
    {
        var currentMatch = DataStore.selectedMatch;
        await DataService.DeleteMatch(currentMatch.id);
        await DataService.LoadDataBases();
        await Navigation.PopModalAsync();
    }
}