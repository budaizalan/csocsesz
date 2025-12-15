using Csocsesz;
using Csocsesz.Classes;

namespace Csocsesz;

public partial class LiveGamePage : ContentPage
{
    int leftCounter = 0;
    int rightCounter = 0;
    bool rbOrder = true;
    bool gameWon = false;
    List<int> scores = new List<int>();

    Player player1 = new Player(0, "Hugo", 0, 0, 0, 0, 0, Side.red);
    Player player2 = new Player(1, "Zalan", 0, 0, 0, 0, 0, Side.blue);
    public LiveGamePage()
    {
        InitializeComponent();
    }
    private void UpdateCounterButtons()
    {
        if (rbOrder)
        {
            LeftButton.Text = $"{leftCounter}";
            RightButton.Text = $"{rightCounter}";
        }
        else
        {
            LeftButton.Text = $"{rightCounter}";
            RightButton.Text = $"{leftCounter}";
        }
    }
    private void Goal(bool left)
    {
        if (left && rbOrder || !left && !rbOrder) leftCounter++;
        else rightCounter++;
        if (leftCounter == 10) GameWon(true);
        else if (rightCounter == 10) GameWon(false);
        else UpdateCounterButtons();
    }
    private void GameWon(bool red)
    {
        gameWon = true;
        if (red)
        {
            LeftButton.BackgroundColor = Color.FromHex("#FF0000");
            RightButton.BackgroundColor = Color.FromHex("#FF0000");
            LeftButton.Text = "RED";
            RightButton.Text = "WON";
        }
        else
        {
            LeftButton.BackgroundColor = Color.FromHex("#2121E3");
            RightButton.BackgroundColor = Color.FromHex("#2121E3");
            LeftButton.Text = "BLUE";
            RightButton.Text = "WON";
        }
    }
    private void CounterButtonClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (!gameWon)
        {
            if (rbOrder)
            {
                if (button == LeftButton) Goal(true);
                else Goal(false);
            }
            else
            {
                if (button == LeftButton) Goal(true);
                else Goal(false);
            }
        }
    }
    private async void ExitButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage(), false);
    }
    private void BackButtonClicked(object sender, EventArgs e)
    {

    }
    private void SwapButtonClicked(object sender, EventArgs e)
    {
        rbOrder = !rbOrder;
        if (rbOrder)
        {
            LeftButton.BackgroundColor = Color.FromHex("#FF0000");
            RightButton.BackgroundColor = Color.FromHex("#2121E3");
        }
        else
        {
            RightButton.BackgroundColor = Color.FromHex("#FF0000");
            LeftButton.BackgroundColor = Color.FromHex("#2121E3");
        }
        UpdateCounterButtons();
    }
    private void ResetButtonClicked(object sender, EventArgs e)
    {
        if (gameWon) return;
        leftCounter = 0; rightCounter = 0;
        UpdateCounterButtons();
        if (rbOrder)
        {
            LeftButton.BackgroundColor = Color.FromHex("#FF0000");
            RightButton.BackgroundColor = Color.FromHex("#2121E3");
        }
        else
        {
            RightButton.BackgroundColor = Color.FromHex("#FF0000");
            LeftButton.BackgroundColor = Color.FromHex("#2121E3");
        }
        gameWon = false;
    }
}