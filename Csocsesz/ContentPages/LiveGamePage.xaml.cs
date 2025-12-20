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

        playerRed.inGame.goals = 0;
        playerRed.inGame.matchWon = 0;
        playerBlue.inGame.goals = 0;
        playerBlue.inGame.matchWon = 0;
        RCBimage.Source = GetImageBySide(Side.red, true);
        BCBimage.Source = GetImageBySide(Side.blue, true);
    }

    #region Saving Match functions
    private async Task UploadMatchResultAsync(MatchResults matchResults)
    {
        // HTTP Kliens és API URL
        HttpClient _httpClient = new HttpClient();
        const string ApiUrl = DataStore.apiMatchUrl;
         // 1. Objektum sorosítása JSON stringgé
        var options = new JsonSerializerOptions
        {
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
            WriteIndented = true // opcionális, olvashatóbb JSON
        };

        string jsonContent = JsonSerializer.Serialize(matchResults, options);

        // 2. JSON tartalom létrehozása
        StringContent content = new StringContent(
            jsonContent,
            Encoding.UTF8,
            "application/json" // Megmondjuk a szervernek, hogy JSON-t küldünk
        );

        try
        {
            // 3. POST kérés küldése
            HttpResponseMessage response = await _httpClient.PostAsync(ApiUrl, content);

            // 4. Válasz ellenõrzése
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Sikeres feltöltés!");
                // Itt megjeleníthetsz egy üzenetet a felhasználónak
            }
            else
            {
                // Hibakezelés (pl. ha a szerver 400 Bad Request-et küld)
                string errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Hiba a feltöltésnél. Státuszkód: {response.StatusCode}. Válasz: {errorBody}");
            }
        }
        catch (Exception ex)
        {
            // Hálózati hiba (pl. nincs internet, rossz URL)
            Console.WriteLine($"Hiba a hálózati kérés során: {ex.Message}");
        }
    }
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
    private void SaveBuffer()
    {
        if (matchBuffer.Count == 0) return;
        Console.WriteLine("------------------------MATCHES------------------------");
        for (int i = 0; i < matchBuffer.Count; i++)
        {
            Console.WriteLine
                ($"wId:{matchBuffer[i].winnerId}\n" +
                $"wS:{(matchBuffer[i].winnerSide == Side.red ? "Red" : "Blue")}\n" +
                $"lId:{matchBuffer[i].loserId}\n" +
                $"lGoals:{matchBuffer[i].loserGoals}\n" +
                $"startTime:{matchBuffer[i].startTime}\n" +
                $"multiplier:{matchBuffer[i].pushUpsMultiplier}\n");
            Console.WriteLine("Goals:");
            for (int j = 0; j < 20; j++)
            {
                if (matchBuffer[i].goals[j] == null) break;
                Console.Write($"{(matchBuffer[i].goals[j].side == Side.red ? "Red" : "Blue")}");
                Console.WriteLine($" - {matchBuffer[i].goals[j].time}");
            }
            Task.Run(() => UploadMatchResultAsync(matchBuffer[i]));
        }
        Console.WriteLine("-------------------------------------------------------");
    }
    #endregion

    #region Pointer Functions
    private Player GetPlayerBySide(Side side)
    {
        return side == Side.red ? playerRed : playerBlue;
    }
    private ImageSource GetImageBySide(Side side, bool normal)
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

        var clickedElement = sender as Frame;
        if (clickedElement != null)
        {
            await clickedElement.ScaleTo(0.95, 50, Easing.CubicOut);
            await clickedElement.ScaleTo(1.0, 150, Easing.CubicIn);
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
            var t1 = RedButton.TranslateTo(-20, 0, 125, Easing.Linear);
            var t2 = BlueButton.TranslateTo(20, 0, 125, Easing.Linear);
            await Task.WhenAll(t1, t2);

            var t3 = RedButton.TranslateTo(20, 0, 125, Easing.Linear);
            var t4 = BlueButton.TranslateTo(-20, 0, 125, Easing.Linear);
            await Task.WhenAll(t3, t4);

            // Frissítjük az adatokat a ciklus következõ köréhez
            redGoals = GetPlayerBySide(Side.red).inGame.goals;
            blueGoals = GetPlayerBySide(Side.blue).inGame.goals;
            isBlueTop = Grid.GetRow(BlueButton) == 0;
        }

        // Alaphelyzetbe állítás, ha már nem teljesül a feltétel
        await Task.WhenAll(
            RedButton.TranslateTo(0, 0, 125),
            BlueButton.TranslateTo(0, 0, 125)
        );
    }
    #endregion
}