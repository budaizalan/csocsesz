using Csocsesz;
using Csocsesz.Classes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Csocsesz.ContentPages;
public partial class LiveGamePage : ContentPage
{
    private Player playerRed;
    private Player playerBlue;
    private Player? playerRed2; // team mates, 2v2 only
    private Player? playerBlue2;

    private bool gameWon = false;
    private bool started = false;
    private bool paused = false;
    private bool extended = false;  // played on past 10; only END GAME finishes it
    private int continuedAt = -1;    // goals.Count when the 10 was passed

    private List<Goal> goals = new List<Goal>();
    private DateTime startTime;

    private List<Match> matchBuffer = new List<Match>();
    public LiveGamePage()
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
        playerRed = AppSettings.playerRed ?? new Player();
        playerBlue = AppSettings.playerBlue ?? new Player();
        bool two = AppSettings.mode == Mode.TwoVsTwo;
        playerRed2 = two ? AppSettings.playerRed2 : null;
        playerBlue2 = two ? AppSettings.playerBlue2 : null;

        if(playerRed != null && playerBlue != null)
        {
            playerRed.inGame.goals = 0;
            playerRed.inGame.matchWon = 0;
            playerBlue.inGame.goals = 0;
            playerBlue.inGame.matchWon = 0;
            RCBimage.Source = GetImageBySide(Side.red, true);
            BCBimage.Source = GetImageBySide(Side.blue, true);
            RCBnameLabel.Text = TeamName(Side.red);
            BCBnameLabel.Text = TeamName(Side.blue);
        }
        UpdateBar();
    }

    #region Saving Match functions
    private void SaveMatchToBuffer()
    {
        if (!AppSettings.sendTestMatches && secondsElapsed < 30) return;
        Side wSide = winnerSide();
        Player winner = GetPlayerBySide(wSide);
        Side lSide = wSide == Side.red ? Side.blue : Side.red;
        Player loser = GetPlayerBySide(lSide);
        // goals is cleared for the next game, so the match needs its own copy.
        Match results = new Match
        {
            mode = AppSettings.mode, winnerId2 = MateBySide(wSide)?.id, loserId2 = MateBySide(lSide)?.id,
            winnerId = winner.id, winnerSide = wSide, loserId = loser.id, loserGoals = loser.inGame.goals,
            winnerGoals = winner.inGame.goals, startTime = startTime, endTime = goals[goals.Count - 1].time,
            goals = new List<Goal>(goals), punishmentMultiplier = AppSettings.punishmentMultiplier
        };
        matchBuffer.Add(results);
    }
    private async Task SaveBuffer()
    {
        foreach (var item in matchBuffer) await DataService.AddMatch(item);
        matchBuffer.Clear();
    }
    #endregion

    #region Pointer Functions
    private Player GetPlayerBySide(Side side)
    {
        return side == Side.red ? playerRed : playerBlue;
    }
    private Player? MateBySide(Side side) => side == Side.red ? playerRed2 : playerBlue2;
    private string TeamName(Side side)
    {
        var mate = MateBySide(side);
        return mate == null ? GetPlayerBySide(side).name : $"{GetPlayerBySide(side).name} & {mate.name}";
    }
    private string GetImageBySide(Side side, bool normal)
    {
        if (normal) return GetPlayerBySide(side).normalImagePath;
        else return GetPlayerBySide(side).sadImagePath;
    }
    private Side winnerSide()
    {
        int red = playerRed.inGame.goals, blue = playerBlue.inGame.goals;
        if (red != blue) return red > blue ? Side.red : Side.blue;
        return goals[goals.Count - 1].side;
    }
    #endregion

    #region Subsidiary Functions
    private void UpdateCounterButtonsLabel()
    {
        BCBgoalLabel.Text = $"{playerBlue.inGame.goals}";
        RCBgoalLabel.Text = $"{playerRed.inGame.goals}";
        BCBmatchLabel.Text = $"{playerBlue.inGame.matchWon}";
        RCBmatchLabel.Text = $"{playerRed.inGame.matchWon}";
    }
    private void Vibrate()
    {
        TimeSpan duration = TimeSpan.FromMilliseconds(1001);
        Vibration.Default.Vibrate(duration);
    }
    private void GameWon(Side side, bool vibrate = true)
    {
        gameWon = true;

        ExitButton.BackgroundColor = DataStore.green;

        if (side == Side.red)
        {
            BlueButton.BackgroundColor = DataStore.red;
            RedButton.BackgroundColor = DataStore.red;
            RCBgoalLabel.Text = "W";
            BCBgoalLabel.Text = "L";
            BCBimage.Source = GetImageBySide(Side.blue, false);
        }
        else
        {
            BlueButton.BackgroundColor = DataStore.blue;
            RedButton.BackgroundColor = DataStore.blue;
            RCBgoalLabel.Text = "L";
            BCBgoalLabel.Text = "W";
            RCBimage.Source = GetImageBySide(Side.red, false);
        }

        if (vibrate) Vibrate();
        StopTimer();
        UpdateBar();
    }
    private void RestorePlayView()
    {
        BlueButton.BackgroundColor = DataStore.blue;
        RedButton.BackgroundColor = DataStore.red;
        ExitButton.BackgroundColor = DataStore.gray;
        RCBimage.Source = GetImageBySide(Side.red, true);
        BCBimage.Source = GetImageBySide(Side.blue, true);
        gameWon = false;
        UpdateBar();
    }
    // Which bottom bar buttons make sense right now.
    private void UpdateBar()
    {
        NewGameButton.IsVisible = (!started || gameWon) && !paused;
        EndGameButton.IsVisible = started && extended && !gameWon && !paused;
        PauseButton.IsVisible = started && !gameWon;
        PauseButton.Source = paused ? "resume_icon.svg" : "pause_icon.svg";
        UpdateOvertime();
    }
    // Overtime = setting on and someone is at 10+ without having won yet.
    private int overtimeToken;
    private bool overtimeOn;
    private void UpdateOvertime()
    {
        bool on = AppSettings.overtime && started && !gameWon && !extended
            && Math.Max(playerRed.inGame.goals, playerBlue.inGame.goals) >= 10;
        if (on == overtimeOn) return;
        overtimeOn = on;
        int id = ++overtimeToken;
        OvertimePulse.CancelAnimations();
        OvertimeLabel.CancelAnimations();
        if (!on)
        {
            OvertimePulse.Opacity = 0;
            OvertimeLabel.Opacity = 0;
            return;
        }
        _ = OvertimeEffect(id);
    }
    private async Task OvertimeEffect(int id)
    {
        // Text grows while fading in, then fades out; the red pulse keeps going until overtime ends.
        OvertimeLabel.Scale = 0.5;
        _ = OvertimeLabel.ScaleToAsync(1.5, 1200, Easing.CubicOut);
        await OvertimeLabel.FadeToAsync(1, 500);
        if (id != overtimeToken) return;
        await OvertimeLabel.FadeToAsync(0, 700);
        while (id == overtimeToken)
        {
            await OvertimePulse.FadeToAsync(0.5, 700, Easing.SinInOut);
            await OvertimePulse.FadeToAsync(0, 700, Easing.SinInOut);
        }
    }
    private async void PauseButtonClicked(object sender, EventArgs e)
    {
        if (!started || gameWon) return;
        paused = !paused;
        if (paused) StopTimer(); else StartTimer();
        UpdateBar();
    }
    private async void EndGameButtonClicked(object sender, EventArgs e)
    {
        if (playerRed.inGame.goals == playerBlue.inGame.goals)
        {
            await DisplayAlertAsync("Tied", "Someone has to be ahead to end the game.", "OK");
            return;
        }
        GameWon(winnerSide());
    }
    
    #region Timer
    private IDispatcherTimer? gameTimer; // Az id�z�t� objektum
    private int secondsElapsed = 0; // Eltelt id� m�sodpercben (a sz�ml�l�)
    private void StartTimer()
    {
        // Megakad�lyozzuk, hogy �jra elinduljon, ha m�r fut
        if (gameTimer != null && gameTimer.IsRunning)
        {
            return;
        }

        // 1. Az id�z�t� l�trehoz�sa
        gameTimer = Dispatcher.CreateTimer();

        // 2. Intervallum be�ll�t�sa 1 m�sodpercre
        gameTimer.Interval = TimeSpan.FromSeconds(1);

        // 3. Esem�nykezel� be�ll�t�sa (ez h�v�dik meg minden m�sodpercben)
        gameTimer.Tick += OnGameTimerTick;

        // 4. Az id�z�t� elind�t�sa
        gameTimer.Start();
    }
    private void StopTimer()
    {
        if (gameTimer != null && gameTimer.IsRunning)
        {
            gameTimer.Stop();

            // Opcion�lis: Szabad�tsd fel az er�forr�sokat
            gameTimer.Tick -= OnGameTimerTick;
            gameTimer = null;
        }
    }
    private void OnGameTimerTick(object? sender, EventArgs e)
    {
        secondsElapsed++; // N�velj�k a m�sodperceket

        // �talak�tjuk a m�sodperceket MM:SS form�tumra
        TimeSpan time = TimeSpan.FromSeconds(secondsElapsed);

        // Friss�tj�k a Label-t (felt�telezve, hogy TimerLabel a neve a Label-�dnek)
        TimerLabel.Text = time.ToString(@"mm\:ss");

        // Opcion�lis: itt teheted meg a GameWon ellen�rz�seket id�alap� meccsek eset�n
    }
    private void ResetTimer()
    {
        secondsElapsed = 0;
        TimerLabel.Text = "00:00";
    }
    #endregion
    #endregion

    #region Buttons
    private async void CounterButtonClicked(object sender, EventArgs e)
    {
        if (!started || paused) return;
        if (gameWon)
        {
            if (extended) return; // finished with END GAME
            // Pressing on after reaching 10 keeps the game going.
            extended = true;
            continuedAt = goals.Count;
            RestorePlayView();
            StartTimer();
        }

        var button = sender as Button;
        if (sender == RedButton)
        {
            GetPlayerBySide(Side.red).inGame.goals++;
            goals.Add(new Goal(Side.red, DateTime.Now));
        }
        else
        {
            GetPlayerBySide(Side.blue).inGame.goals++;
            goals.Add(new Goal(Side.blue, DateTime.Now));
        }

        UpdateCounterButtonsLabel();

        if (!extended)
        {
            int red = playerRed.inGame.goals, blue = playerBlue.inGame.goals;
            // Overtime: first to 10 only wins with a 2 goal lead, otherwise play on.
            int lead = AppSettings.overtime ? 2 : 1;
            if (red >= 10 && red - blue >= lead) GameWon(Side.red);
            else if (blue >= 10 && blue - red >= lead) GameWon(Side.blue);
        }
        UpdateOvertime();

        var clickedElement = sender as Border;
        if (clickedElement != null)
        {
            await clickedElement.ScaleToAsync(0.95, 50, Easing.CubicOut);
            await clickedElement.ScaleToAsync(1.0, 150, Easing.CubicIn);
        }
        #region unimportant
        _ = ShakeButtonsIf67Score();
        #endregion
    }
    private async void ExitButtonClicked(object sender, EventArgs e)
    {
        if (gameWon) SaveMatchToBuffer();
        await Leave();
    }
    // Games already finished with NEXT are saved; the running one is not.
    private async Task Leave()
    {
        StopTimer();
        overtimeToken++;
        await SaveBuffer();
        await DataService.LoadDataBases();
        await Navigation.PopModalAsync();
    }
    private void BackButtonClicked(object sender, EventArgs e)
    {
        if (!started || paused || goals.Count == 0) return;

        bool wasWon = gameWon;
        if (goals[goals.Count() - 1].side == Side.red) playerRed.inGame.goals--;
        else playerBlue.inGame.goals--;
        goals.RemoveAt(goals.Count() - 1);

        UpdateCounterButtonsLabel();

        if (extended && goals.Count == continuedAt)
        {
            // Undid the goal that went past 10: back to the plain win view.
            extended = false;
            continuedAt = -1;
            GameWon(winnerSide(), vibrate: false);
        }
        else if (wasWon)
        {
            StartTimer();
            RestorePlayView();
        }
        UpdateOvertime();
    }
    private void SwapButtonClicked(object sender, EventArgs e)
    {
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
        if(gameWon)
        {
            UpdateCounterButtonsLabel();
            BlueButton.BackgroundColor = DataStore.blue;
            RedButton.BackgroundColor = DataStore.red;
        }
    }
    private void NewGameButtonClicked(object sender, EventArgs e)
    {
        if (started && gameWon && winnerSide() == Side.blue)
        {
            BCBmatchLabel.Text = $"{++playerBlue.inGame.matchWon}";
            SaveMatchToBuffer();
        }
        else if (started && gameWon && winnerSide() == Side.red)
        {
            RCBmatchLabel.Text = $"{++playerRed.inGame.matchWon}";
            SaveMatchToBuffer();
        }

        ResetTimer();
        StartTimer();
        startTime = DateTime.Now;

        ExitButton.BackgroundColor = DataStore.gray;
        BlueButton.BackgroundColor = DataStore.blue;
        RedButton.BackgroundColor = DataStore.red;
        RCBimage.Source = GetImageBySide(Side.red, true);
        BCBimage.Source = GetImageBySide(Side.blue, true);

        GetPlayerBySide(Side.red).inGame.goals = 0;
        GetPlayerBySide(Side.blue).inGame.goals = 0;
        UpdateCounterButtonsLabel();

        gameWon = false;
        extended = false;
        continuedAt = -1;
        paused = false;
        goals.Clear();

        if(AppSettings.changingSide && started)
        {
            (playerRed, playerBlue) = (playerBlue, playerRed);
            (playerRed2, playerBlue2) = (playerBlue2, playerRed2);

            RCBimage.Source = GetImageBySide(Side.red, true);
            BCBimage.Source = GetImageBySide(Side.blue, true);
            RCBnameLabel.Text = TeamName(Side.red);
            BCBnameLabel.Text = TeamName(Side.blue);
            UpdateCounterButtonsLabel();
        }
        started = true;
        UpdateBar();
    }
    #endregion
    protected override bool OnBackButtonPressed()
    {
        _ = Leave();
        return true;
    }
    #region Unimportant
    private async Task ShakeButtonsIf67Score()
    {
        // Lek�rdezz�k a g�lokat
        int redGoals = GetPlayerBySide(Side.red).inGame.goals;
        int blueGoals = GetPlayerBySide(Side.blue).inGame.goals;

        // Meghat�rozzuk, ki van fel�l (0. sor) �s ki alul (2. sor)
        // A te k�dod alapj�n a gombok a 0. �s a 2. sorban cser�l�dnek
        bool isBlueTop = Grid.GetRow(BlueButton) == 0;

        // A felt�tel: 
        // (K�k van fel�l �S k�k=6, piros=7) VAGY (Piros van fel�l �S piros=6, k�k=7)
        while ((isBlueTop && blueGoals == 6 && redGoals == 7) ||
               (!isBlueTop && redGoals == 6 && blueGoals == 7))
        {
            if (gameWon || !started || paused) break;

            // Gyors oda-vissza mozg�s (10 pixel)
            var t1 = RedButton.TranslateToAsync(-20, 0, 125, Easing.Linear);
            var t2 = BlueButton.TranslateToAsync(20, 0, 125, Easing.Linear);
            await Task.WhenAll(t1, t2);

            var t3 = RedButton.TranslateToAsync(20, 0, 125, Easing.Linear);
            var t4 = BlueButton.TranslateToAsync(-20, 0, 125, Easing.Linear);
            await Task.WhenAll(t3, t4);

            // Friss�tj�k az adatokat a ciklus k�vetkez� k�r�hez
            redGoals = GetPlayerBySide(Side.red).inGame.goals;
            blueGoals = GetPlayerBySide(Side.blue).inGame.goals;
            isBlueTop = Grid.GetRow(BlueButton) == 0;
        }

        // Alaphelyzetbe �ll�t�s, ha m�r nem teljes�l a felt�tel
        await Task.WhenAll(
            RedButton.TranslateToAsync(0, 0, 125),
            BlueButton.TranslateToAsync(0, 0, 125)
        );
    }
    #endregion
}