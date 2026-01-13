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
    private Player playerRed = AppSettings.playerRed;
    private Player playerBlue = AppSettings.playerBlue;

    private bool gameWon = false;
    private bool started = false;

    private List<Goal> goals = new List<Goal>();
    private DateTime startTime;

    private List<MatchResults> matchBuffer = new List<MatchResults>();
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
        playerRed.inGame.goals = 0;
        playerRed.inGame.matchWon = 0;
        playerBlue.inGame.goals = 0;
        playerBlue.inGame.matchWon = 0;
        RCBimage.Source = GetImageBySide(Side.red, true);
        BCBimage.Source = GetImageBySide(Side.blue, true);
        RCBnameLabel.Text = playerRed.name;
        BCBnameLabel.Text = playerBlue.name;
    }

    #region Saving Match functions
    private void SaveMatchToBuffer()
    {
        if (!AppSettings.sendTestMatches && secondsElapsed < 30) return;
        Side wSide = winnerSide();
        Player winner = GetPlayerBySide(wSide);
        Player loser = GetPlayerBySide(wSide == Side.red ? Side.blue : Side.red);
        MatchResults results = new MatchResults
            (winner.id, wSide, loser.id, loser.inGame.goals, startTime, AppSettings.pushUpsMultiplier);
        for (int i = 0; i < goals.Count; i++) results.goals[i] = goals[i];

        matchBuffer.Add(results);
    }
    private async void SaveBuffer()
    {
        if (matchBuffer.Count == 0) return;
        await DataManager.UploadMatchResultsList(new List<MatchResults>(matchBuffer));
        matchBuffer.Clear();
    }
    #endregion

    #region Pointer Functions
    private Player GetPlayerBySide(Side side)
    {
        return side == Side.red ? playerRed : playerBlue;
    }
    private string GetImageBySide(Side side, bool normal)
    {
        if (normal) return GetPlayerBySide(side).inGame.normalImage;
        else return GetPlayerBySide(side).inGame.sadImage;
    }
    private Side winnerSide()
    {
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
    private void GameWon(Side side)
    {
        gameWon = true;

        NewGameButton.IsVisible = true;
        SwapButton.IsVisible = true;
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

        Vibrate();
        StopTimer();
    }
    
    #region Timer
    private IDispatcherTimer? gameTimer; // Az idõzítõ objektum
    private int secondsElapsed = 0; // Eltelt idõ másodpercben (a számláló)
    private void StartTimer()
    {
        TimerLabel.FontSize = 30;
        // Megakadályozzuk, hogy újra elinduljon, ha már fut
        if (gameTimer != null && gameTimer.IsRunning)
        {
            return;
        }

        // 1. Az idõzítõ létrehozása
        gameTimer = Dispatcher.CreateTimer();

        // 2. Intervallum beállítása 1 másodpercre
        gameTimer.Interval = TimeSpan.FromSeconds(1);

        // 3. Eseménykezelõ beállítása (ez hívódik meg minden másodpercben)
        gameTimer.Tick += OnGameTimerTick;

        // 4. Az idõzítõ elindítása
        gameTimer.Start();
    }
    private void StopTimer()
    {
        TimerLabel.FontSize = 15;
        if (gameTimer != null && gameTimer.IsRunning)
        {
            gameTimer.Stop();

            // Opcionális: Szabadítsd fel az erõforrásokat
            gameTimer.Tick -= OnGameTimerTick;
            gameTimer = null;
        }
    }
    private void OnGameTimerTick(object? sender, EventArgs e)
    {
        secondsElapsed++; // Növeljük a másodperceket

        // Átalakítjuk a másodperceket MM:SS formátumra
        TimeSpan time = TimeSpan.FromSeconds(secondsElapsed);

        // Frissítjük a Label-t (feltételezve, hogy TimerLabel a neve a Label-ödnek)
        TimerLabel.Text = time.ToString(@"mm\:ss");

        // Opcionális: itt teheted meg a GameWon ellenõrzéseket idõalapú meccsek esetén
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
        if (gameWon || !started) return;

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

        if (playerRed.inGame.goals == 10) GameWon(Side.red);
        else if (playerBlue.inGame.goals == 10) GameWon(Side.blue);

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
        SaveBuffer();
        ((App)App.Current).LoadDataBases();
        await Navigation.PopModalAsync();
    }
    private void BackButtonClicked(object sender, EventArgs e)
    {
        if (!started || goals.Count == 0) return;

        if (goals[goals.Count() - 1].side == Side.red) playerRed.inGame.goals--;
        else playerBlue.inGame.goals--;
        goals.RemoveAt(goals.Count() - 1);

        UpdateCounterButtonsLabel();

        if(gameWon)
        {
            StartTimer();

            BlueButton.BackgroundColor = DataStore.blue;
            RedButton.BackgroundColor = DataStore.red;
            ExitButton.BackgroundColor = DataStore.gray;
            NewGameButton.IsVisible = false;
            SwapButton.IsVisible = false;

            RCBimage.Source = GetImageBySide(Side.red, true);
            BCBimage.Source = GetImageBySide(Side.blue, true);

            gameWon = false;
        }
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

        NewGameButton.IsVisible = false;
        SwapButton.IsVisible = false;
        ExitButton.BackgroundColor = DataStore.gray;
        BlueButton.BackgroundColor = DataStore.blue;
        RedButton.BackgroundColor = DataStore.red;
        RCBimage.Source = GetImageBySide(Side.red, true);
        BCBimage.Source = GetImageBySide(Side.blue, true);

        GetPlayerBySide(Side.red).inGame.goals = 0;
        GetPlayerBySide(Side.blue).inGame.goals = 0;
        UpdateCounterButtonsLabel();

        gameWon = false;
        goals.Clear();

        if(AppSettings.changingSide && started)
        {
            Player tempP = playerRed;
            playerRed = playerBlue;
            playerBlue = tempP;

            RCBimage.Source = GetImageBySide(Side.red, true);
            BCBimage.Source = GetImageBySide(Side.blue, true);
            RCBnameLabel.Text = playerRed.name;
            BCBnameLabel.Text = playerBlue.name;
            UpdateCounterButtonsLabel();
        }
        started = true;
    }
    #endregion

    #region Unimportant
    private async Task ShakeButtonsIf67Score()
    {
        // Lekérdezzük a gólokat
        int redGoals = GetPlayerBySide(Side.red).inGame.goals;
        int blueGoals = GetPlayerBySide(Side.blue).inGame.goals;

        // Meghatározzuk, ki van felül (0. sor) és ki alul (2. sor)
        // A te kódod alapján a gombok a 0. és a 2. sorban cserélõdnek
        bool isBlueTop = Grid.GetRow(BlueButton) == 0;

        // A feltétel: 
        // (Kék van felül ÉS kék=6, piros=7) VAGY (Piros van felül ÉS piros=6, kék=7)
        while ((isBlueTop && blueGoals == 6 && redGoals == 7) ||
               (!isBlueTop && redGoals == 6 && blueGoals == 7))
        {
            if (gameWon || !started) break;

            // Gyors oda-vissza mozgás (10 pixel)
            var t1 = RedButton.TranslateToAsync(-20, 0, 125, Easing.Linear);
            var t2 = BlueButton.TranslateToAsync(20, 0, 125, Easing.Linear);
            await Task.WhenAll(t1, t2);

            var t3 = RedButton.TranslateToAsync(20, 0, 125, Easing.Linear);
            var t4 = BlueButton.TranslateToAsync(-20, 0, 125, Easing.Linear);
            await Task.WhenAll(t3, t4);

            // Frissítjük az adatokat a ciklus következõ köréhez
            redGoals = GetPlayerBySide(Side.red).inGame.goals;
            blueGoals = GetPlayerBySide(Side.blue).inGame.goals;
            isBlueTop = Grid.GetRow(BlueButton) == 0;
        }

        // Alaphelyzetbe állítás, ha már nem teljesül a feltétel
        await Task.WhenAll(
            RedButton.TranslateToAsync(0, 0, 125),
            BlueButton.TranslateToAsync(0, 0, 125)
        );
    }
    #endregion
}