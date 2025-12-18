using Csocsesz;
using Csocsesz.Classes;

using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Maui.Devices;

using Microsoft.Maui.Dispatching;

namespace Csocsesz.ContentPages;

public partial class LiveGamePage : ContentPage
{
    private bool gameWon = false;
    private bool started = false;
    private List<Side> scores = new List<Side>();

    private Player player1 = AppSettings.player1;
    private Player player2 = AppSettings.player2;

    #region HTTP
    // HTTP Kliens és API URL
    private readonly HttpClient _httpClient = new HttpClient();
    private const string ApiUrl = DataStore.apiMatchUrl;
    private async Task UploadMatchResultAsync(MatchResults matchResults)
    {
        // 1. Objektum sorosítása JSON stringgé
        string jsonContent = JsonSerializer.Serialize(matchResults);

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
    private void SaveGame()
    {
        if (!gameWon) return;
        Player winner = (GetPlayerBySide(Side.red).inGame.goals > GetPlayerBySide(Side.blue).inGame.goals) ? GetPlayerBySide(Side.red) : GetPlayerBySide(Side.blue);
        Player loser = (winner == GetPlayerBySide(Side.red)) ? GetPlayerBySide(Side.blue) : GetPlayerBySide(Side.red);

        // Létrehozzuk a MatchResults objektumot a szervernek:
        MatchResults results = new MatchResults
            (winnerId: winner.id, loserId: loser.id, loserGoals: loser.inGame.goals, 
            timeSpan: TimeSpan.FromSeconds(secondsElapsed), date: DateTime.Now, pushUpsMultiplier: AppSettings.pushUpsMultiplier);
        for (int i = 0; i < scores.Count; i++) results.goals[i] = scores[i];

        // Elindítjuk a feltöltést egy háttérszálon (Task.Run), 
        // hogy ne fagyjon le a felhasználói felület, amíg várunk a szerverre.
        Task.Run(() => UploadMatchResultAsync(results));
    }
    #endregion
    public LiveGamePage()
    {
        InitializeComponent();
    }

    private void UpdateCounterButtons()
    {
        BCBgoalLabel.Text = $"{GetPlayerBySide(Side.blue).inGame.goals}";
        RCBgoalLabel.Text = $"{GetPlayerBySide(Side.red).inGame.goals}";
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
        }
        else
        {
            BlueButton.BackgroundColor = DataStore.blue;
            RedButton.BackgroundColor = DataStore.blue;
            RCBgoalLabel.Text = "L";
            BCBgoalLabel.Text = "W";
        }
        if (GetPlayerBySide(side).name == "Zalan") hugoImage.Source = DataStore.hugoSadImage;
        else zalanImage.Source = DataStore.zalanSadImage;

        TimeSpan duration = TimeSpan.FromMilliseconds(1001);
        Vibration.Default.Vibrate(duration);

        StopTimer();
    }
    private Player GetPlayerBySide(Side side)
    {
        if ((side == Side.red && player1.inGame.side == Side.red) || (side == Side.blue && player1.inGame.side == Side.blue)) return player1;
        else return player2;
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

    #region Buttons
    private async void CounterButtonClicked(object sender, EventArgs e)
    {
        if (gameWon || !started) return;

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
        if (RCBgoalLabel.Text == "10") GameWon(Side.red);
        else if (BCBgoalLabel.Text == "10") GameWon(Side.blue);

        var clickedElement = sender as Frame;
        if (clickedElement != null)
        {
            await clickedElement.ScaleTo(0.95, 50, Easing.CubicOut);
            await clickedElement.ScaleTo(1.0, 150, Easing.CubicIn);
        }
        #region unimportant
        _ = ShakeButtonsIfCriticalScore();
        #endregion
    }
    private async void ExitButtonClicked(object sender, EventArgs e)
    {
        if(gameWon) SaveGame();
        await Shell.Current.GoToAsync("///PlayPage");
    }
    private void BackButtonClicked(object sender, EventArgs e)
    {
        if (!started) return;
        if (scores.Count == 0) return;
        if (scores[scores.Count() - 1] == Side.red) GetPlayerBySide(Side.red).inGame.goals--;
        else GetPlayerBySide(Side.blue).inGame.goals--;
        scores.RemoveAt(scores.Count() - 1);

        UpdateCounterButtons();
        if(gameWon)
        {
            StartTimer();
            BlueButton.BackgroundColor = DataStore.blue;
            RedButton.BackgroundColor = DataStore.red;
            ExitButton.BackgroundColor = DataStore.gray;
            NewGameButton.IsVisible = false;
            SwapButton.IsVisible = false;
            gameWon = false;
            hugoImage.Source = DataStore.hugoImage;
            zalanImage.Source = DataStore.zalanImage;
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
            UpdateCounterButtons();
            BlueButton.BackgroundColor = DataStore.blue;
            RedButton.BackgroundColor = DataStore.red;
        }
    }
    private void NewGameButtonClicked(object sender, EventArgs e)
    {
        if (!started) started = true;
        if(gameWon) SaveGame();

        if (player1.inGame.goals == 10)
        {
            if (player1.inGame.side == Side.red) RCBmatchLabel.Text = $"{++player1.inGame.matchWon}";
            else BCBmatchLabel.Text = $"{++player1.inGame.matchWon}";
        }
        else if (player2.inGame.goals == 10)
        {
            if (player2.inGame.side == Side.red) RCBmatchLabel.Text = $"{++player2.inGame.matchWon}";
            else BCBmatchLabel.Text = $"{++player2.inGame.matchWon}";
        }

        ResetTimer();
        StartTimer();
        NewGameButton.IsVisible = false;
        SwapButton.IsVisible = false;
        ExitButton.BackgroundColor = DataStore.gray;
        GetPlayerBySide(Side.red).inGame.goals = 0;
        GetPlayerBySide(Side.blue).inGame.goals = 0;
        UpdateCounterButtons();
        BlueButton.BackgroundColor = DataStore.blue;
        RedButton.BackgroundColor = DataStore.red;
        gameWon = false;
        scores.Clear();

        hugoImage.Source = DataStore.hugoImage;
        zalanImage.Source = DataStore.zalanImage;
    }
    #endregion

    #region unimportant
    private async Task ShakeButtonsIfCriticalScore()
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