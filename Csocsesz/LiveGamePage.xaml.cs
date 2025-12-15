using Csocsesz;
using Csocsesz.Classes;

namespace Csocsesz;

public partial class LiveGamePage : ContentPage
{
    bool gameWon = false;
    List<Side> scores = new List<Side>();

    Player player1 = new Player(0, "Hugo", 0, 0, 0, 0, 0, Side.red);
    Player player2 = new Player(1, "Zalan", 0, 0, 0, 0, 0, Side.blue);
    public LiveGamePage()
    {
        InitializeComponent();
    }
    private void UpdateCounterButtons()
    {
        BlueButton.Text = $"{GetPlayerBySide(Side.blue).inGame.goals}";
        RedButton.Text = $"{GetPlayerBySide(Side.red).inGame.goals}";
    }
    private void GameWon(Side side)
    {
        gameWon = true;
        if (side == Side.red)
        {
            BlueButton.BackgroundColor = Color.FromHex("#FF0000");
            RedButton.BackgroundColor = Color.FromHex("#FF0000");
            if (Grid.GetRow(BlueButton) == 0)
            {
                BlueButton.Text = "RED";
                RedButton.Text = "WON";
            }
            else
            {
                RedButton.Text = "RED";
                BlueButton.Text = "WON";
            }
        }
        else
        {
            BlueButton.BackgroundColor = Color.FromHex("#2121E3");
            RedButton.BackgroundColor = Color.FromHex("#2121E3");
            if (Grid.GetRow(BlueButton) == 0)
            {
                BlueButton.Text = "BLUE";
                RedButton.Text = "WON";
            }
            else
            {
                RedButton.Text = "BLUE";
                BlueButton.Text = "WON";
            }
        }
    }
    private void CounterButtonClicked(object sender, EventArgs e)
    {
        if (gameWon) return;
        var button = sender as Button;
        if (sender == RedButton)
        {
            GetPlayerBySide(Side.red).inGame.goals++;
            scores.Add(Side.red);
        }
        else
        {
            GetPlayerBySide(Side.blue).inGame.goals++;
            scores.Add(Side.blue);
        }
        UpdateCounterButtons();
        if (RedButton.Text == "10") GameWon(Side.red);
        else if (BlueButton.Text == "10") GameWon(Side.blue);
    }
    private Player GetPlayerBySide(Side side)
    {
        if ((side == Side.red && player1.inGame.side == Side.red) || (side == Side.blue && player1.inGame.side == Side.blue)) return player1;
        else return player2;
    }

    //----------------------------------------------------------------------------------------------------
    //BUTTONS---------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------
    private async void ExitButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage(), false);
    }
    private void BackButtonClicked(object sender, EventArgs e)
    {
        if (scores.Count == 0) return;
        if (scores[scores.Count() - 1] == Side.red) GetPlayerBySide(Side.red).inGame.goals--;
        else GetPlayerBySide(Side.blue).inGame.goals--;
        scores.RemoveAt(scores.Count() - 1);

        UpdateCounterButtons();
        if(gameWon)
        {
            BlueButton.BackgroundColor = Color.FromHex("#2121E3");
            RedButton.BackgroundColor = Color.FromHex("#FF0000");
            gameWon = false;
        }
    }
    private void SwapButtonClicked(object sender, EventArgs e)
    {
        if (gameWon) return;
        if (Grid.GetRow(BlueButton) == 0)
        {
            Grid.SetRow(BlueButton, 2);
            Grid.SetRow(RedButton, 0);
        }
        else
        {
            Grid.SetRow(BlueButton, 0);
            Grid.SetRow(RedButton, 2);
        }
    }
    private void ResetButtonClicked(object sender, EventArgs e)
    {
        GetPlayerBySide(Side.red).inGame.goals = 0;
        GetPlayerBySide(Side.blue).inGame.goals = 0;
        UpdateCounterButtons();
        BlueButton.BackgroundColor = Color.FromHex("#2121E3");
        RedButton.BackgroundColor = Color.FromHex("#FF0000");
        gameWon = false;
    }
}