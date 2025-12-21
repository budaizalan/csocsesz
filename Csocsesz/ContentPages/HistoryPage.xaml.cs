using Csocsesz.Classes;

namespace Csocsesz.ContentPages;

public partial class HistoryPage : ContentPage
{
	public HistoryPage()
	{
		InitializeComponent();
	}
    public class MatchDisplay
    {
        public MatchResults Match { get; set; }
        public Player Winner { get; set; }
        public Player Loser { get; set; }
        public Color WinnerColor => 
            Match.winnerSide == Side.red ? DataStore.red : DataStore.blue;
        public Color LoserColor =>
            Match.winnerSide == Side.red ? DataStore.blue : DataStore.red;
        public string ResultText => $"10 - {Match.loserGoals}";
        public string DateText => Match.startTime.ToString("MM.dd\nHH:mm");

        public MatchDisplay(MatchResults match, Player winner, Player loser)
        {
            this.Match = match;
            this.Winner = winner;
            this.Loser = loser;
        }
    }
    public class MatchGroup : List<MatchDisplay>
    {
        public string Date { get; private set; }

        public MatchGroup(string date, List<MatchDisplay> matches) : base(matches)
        {
            Date = date;
        }
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Start();
    }
    private void Start()
	{
        var displayList = new List<MatchDisplay>();

        foreach (var match in DataStore.Matches)
        {
            var winner = DataStore.Players.FirstOrDefault(p => p.id == match.winnerId);
            var loser = DataStore.Players.FirstOrDefault(p => p.id == match.loserId);

            if (winner != null && loser != null)
            {
                displayList.Add(new MatchDisplay(match, winner, loser));
            }
        }

        var grouped = displayList
            .GroupBy(m => m.Match.startTime.ToString("yyyy. MM. dd."))
            .Select(g => new MatchGroup(g.Key, g.OrderByDescending(x => x.Match.startTime).ToList()))
            .OrderByDescending(g => g.Date) // Legfrissebb nap legyen legfelül
            .ToList();

        MatchesCollectionView.ItemsSource = grouped;
    }
    private async void OnMatchTapped(object sender, EventArgs e)
    {
        var border = (Border)sender;
        var clickedMatch = border.BindingContext as MatchDisplay;

        if (border != null)
        {
            await border.ScaleToAsync(0.95, 50, Easing.CubicOut);
            await border.ScaleToAsync(1.0, 150, Easing.CubicIn);
        }

        if (clickedMatch != null)
        {
            //DisplayAlert("Meccs részletei", $"{selectedMatch.Winner.name} nyert {selectedMatch.ResultText} arányban!", "OK");
            DataStore.selectedMatch = clickedMatch.Match;
            await Navigation.PushModalAsync(new SelectedMatchPage());
        }
    }
}