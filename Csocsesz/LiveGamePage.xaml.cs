using Csocsesz;
using Csocsesz.Classes;

using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Maui.Devices;

using Microsoft.Maui.Dispatching;

namespace Csocsesz;

public partial class LiveGamePage : ContentPage
{
    #region HTTP
    // HTTP Kliens és API URL
    private readonly HttpClient _httpClient = new HttpClient();
    private const string ApiUrl = "https://csocsesz-backend-g2bsedgra6b8aydz.swedencentral-01.azurewebsites.net/api/matches";
    // Feltételezem, a /api/users helyett /api/matchresults-ra kéne küldeni
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
        MatchResults results = new MatchResults(winnerId: winner.id, loserId: loser.id, loserGoals: loser.inGame.goals);
        // feltételezve, hogy a gyõztes góljai fixen 10-ek

        // Elindítjuk a feltöltést egy háttérszálon (Task.Run), 
        // hogy ne fagyjon le a felhasználói felület, amíg várunk a szerverre.
        Task.Run(() => UploadMatchResultAsync(results));
    }
    #endregion

    private bool gameWon = false;
    private bool started = false;
    private List<Side> scores = new List<Side>();

    private Player player1 = new Player("694077cfe93c946a4ce8fdaf", "Hugo", 0, 0, 0, 0, 0, 0, Side.red);
    private Player player2 = new Player("694077dbe93c946a4ce8fdb1", "Zalan", 0, 0, 0, 0, 0, 0, Side.blue);
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
        ExitButton.BackgroundColor = Color.FromHex("#1FDB48");
        if (side == Side.red)
        {
            BlueButton.BackgroundColor = Color.FromHex("#FF0000");
            RedButton.BackgroundColor = Color.FromHex("#FF0000");
            if (Grid.GetRow(BlueButton) == 0)
            {
                BCBgoalLabel.Text = "RED";
                RCBgoalLabel.Text = "WON";
            }
            else
            {
                RCBgoalLabel.Text = "RED";
                BCBgoalLabel.Text = "WON";
            }
        }
        else
        {
            BlueButton.BackgroundColor = Color.FromHex("#2121E3");
            RedButton.BackgroundColor = Color.FromHex("#2121E3");
            if (Grid.GetRow(BlueButton) == 0)
            {
                BCBgoalLabel.Text = "BLUE";
                RCBgoalLabel.Text = "WON";
            }
            else
            {
                RCBgoalLabel.Text = "BLUE";
                BCBgoalLabel.Text = "WON";
            }
        }
        if (GetPlayerBySide(side).name == "Zalan") hugoImage.Source = ImageSource.FromFile("hugosad_icon.png");
        else zalanImage.Source = ImageSource.FromFile("zalansad_icon.png");

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
    }
    private async void ExitButtonClicked(object sender, EventArgs e)
    {
        if(gameWon) SaveGame();
        await Navigation.PushAsync(new MainPage(), false);
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
            BlueButton.BackgroundColor = Color.FromHex("#2121E3");
            RedButton.BackgroundColor = Color.FromHex("#FF0000");
            ExitButton.BackgroundColor = Color.FromHex("#7f7f7f");
            NewGameButton.IsVisible = false;
            SwapButton.IsVisible = false;
            gameWon = false;
            hugoImage.Source = ImageSource.FromFile("hugo_icon.png");
            zalanImage.Source = ImageSource.FromFile("zalan_icon.png");
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
            BlueButton.BackgroundColor = Color.FromHex("#2121E3");
            RedButton.BackgroundColor = Color.FromHex("#FF0000");
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
        ExitButton.BackgroundColor = Color.FromHex("#7f7f7f");
        GetPlayerBySide(Side.red).inGame.goals = 0;
        GetPlayerBySide(Side.blue).inGame.goals = 0;
        UpdateCounterButtons();
        BlueButton.BackgroundColor = Color.FromHex("#2121E3");
        RedButton.BackgroundColor = Color.FromHex("#FF0000");
        gameWon = false;
        scores.Clear();

        hugoImage.Source = ImageSource.FromFile("hugo_icon.png");
        zalanImage.Source = ImageSource.FromFile("zalan_icon.png");
    }
    #endregion
}