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
    #endregion
    
    private void SaveMatchToBuffer(Side side)
    {
        if (AppSettings.sendTestMatches && secondsElapsed < 30) return;
        Player winner = GetPlayerBySide(side);
        Player loser = GetPlayerBySide(side == Side.red ? Side.blue : Side.blue);

        MatchResults results = new MatchResults
            (winner.id, side, loser.id, loser.inGame.goals, startTime, AppSettings.pushUpsMultiplier);
        for (int i = 0; i < goals.Count; i++) results.goals[i] = goals[i];

        matchBuffer.Add(results);
    }
    private void SaveBuffer()
    {
        if (matchBuffer.Count == 0) return;
        for (int i = 0; i < matchBuffer.Count; i++)
        {
            Task.Run(() => UploadMatchResultAsync(matchBuffer[i]));
        }
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

        if (RCBgoalLabel.Text == "10") GameWon(Side.red);
        else if (BCBgoalLabel.Text == "10") GameWon(Side.blue);

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
        SaveBuffer();
        await Navigation.PopModalAsync();
    }
    private void BackButtonClicked(object sender, EventArgs e)
    {
        if (!started) return;
        if (goals.Count == 0) return;

        if (goals[goals.Count() - 1].side == Side.red) GetPlayerBySide(Side.red).inGame.goals--;
        else GetPlayerBySide(Side.blue).inGame.goals--;
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
        if(playerRed.inGame.goals == 10)
        {
            RCBmatchLabel.Text = $"{++playerRed.inGame.matchWon}";
            SaveMatchToBuffer(Side.red);
        }
        else if (playerBlue.inGame.goals == 10)
        {
            BCBmatchLabel.Text = $"{++playerBlue.inGame.matchWon}";
            SaveMatchToBuffer(Side.blue);
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

    #region unimportant
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