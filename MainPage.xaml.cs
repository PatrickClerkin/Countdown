using System.Timers;

namespace Countdown1;

public partial class MainPage : ContentPage
{
    private int player1Score = 0;
    private int player2Score = 0;
    private List<char> currentLetters = new List<char>();
    private List<int> currentNumbers = new List<int>();
    private System.Timers.Timer? timer;
    private int timeRemaining;
    private Random random = new Random();
    private HashSet<string> dictionary = new HashSet<string>();
    private int currentRound = 1;
    private bool isLetterRound = true;
    private bool isTwoPlayerMode = false;
    private int currentPlayer = 1;

    private const string consonants = "BCDFGHJKLMNPQRSTVWXYZ";
    private const string vowels = "AEIOU";
    private readonly int[] smallNumbers = { 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10 };
    private readonly int[] largeNumbers = { 25, 50, 75, 100 };

    public MainPage()
    {
        InitializeComponent();
        LoadDictionary();
        InitializeGame();
    }

    private async void LoadDictionary()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("dictionary.txt");
            using var reader = new StreamReader(stream);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                dictionary.Add(line.Trim().ToUpper());
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load dictionary: {ex.Message}", "OK");
        }
    }

    private void InitializeGame()
    {
        player1Score = 0;
        player2Score = 0;
        currentRound = 1;
        Player1ScoreLabel.Text = player1Score.ToString();
        Player2ScoreLabel.Text = player2Score.ToString();
        RoundLabel.Text = currentRound.ToString();
        StartLetterRound();
    }

    private int GetTimeForDifficulty()
    {
        int difficulty = Preferences.Get("Difficulty", 1);
        switch (difficulty)
        {
            case 0: return 45; // Easy
            case 2: return 20; // Hard
            default: return 30; // Medium
        }
    }

    private void UpdateBestScore(int score)
    {
        int currentBest = Preferences.Get("BestScore", 0);
        if (score > currentBest)
        {
            Preferences.Set("BestScore", score);
        }
    }

    private async void StartLetterRound()
    {
        isLetterRound = true;
        currentLetters.Clear();
        LettersLabel.Text = "";
        WordInputEntry.Text = "";
        CalculationInputEntry.IsVisible = false;
        WordInputEntry.IsVisible = true;
        NumbersLabel.Text = "";
        timeRemaining = GetTimeForDifficulty();
        TimerLabel.Text = timeRemaining.ToString();

        timer?.Stop();
        timer = new System.Timers.Timer(1000);
        timer.Elapsed += Timer_Elapsed;
        timer.Start();

        await AnimateTimerLabel();
    }

    private async void StartNumberRound()
    {
        isLetterRound = false;
        currentNumbers.Clear();
        NumbersLabel.Text = "";
        CalculationInputEntry.Text = "";
        WordInputEntry.IsVisible = false;
        CalculationInputEntry.IsVisible = true;
        LettersLabel.Text = "";
        timeRemaining = GetTimeForDifficulty();
        TimerLabel.Text = timeRemaining.ToString();

        timer?.Stop();
        timer = new System.Timers.Timer(1000);
        timer.Elapsed += Timer_Elapsed;
        timer.Start();

        int target = random.Next(100, 1000);
        NumbersLabel.Text = $"Target: {target}\n";

        await AnimateTimerLabel();
    }

    private async Task AnimateTimerLabel()
    {
        await TimerLabel.ScaleTo(1.2, 250, Easing.SpringOut);
        await TimerLabel.ScaleTo(1, 250, Easing.SpringIn);
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (timeRemaining > 0)
        {
            timeRemaining--;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TimerLabel.Text = timeRemaining.ToString();
                AnimateTimerLabel();
            });
        }
        else
        {
            timer?.Stop();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Time's up!", "The round is over.", "OK");
                NextRound();
            });
        }
    }

    private void NextRound()
    {
        currentRound++;
        if (currentRound > 5)
        {
            EndGame();
        }
        else
        {
            RoundLabel.Text = currentRound.ToString();
            if (currentRound % 2 == 1)
            {
                StartLetterRound();
            }
            else
            {
                StartNumberRound();
            }
        }
    }

    private async void EndGame()
    {
        timer?.Stop();
        string winner = player1Score > player2Score ? "Player 1" : (player2Score > player1Score ? "Player 2" : "It's a tie");
        await DisplayAlert("Game Over", $"{winner} wins!\nPlayer 1 Score: {player1Score}\nPlayer 2 Score: {player2Score}", "OK");
        UpdateBestScore(Math.Max(player1Score, player2Score));
        await SaveGameResult();
        InitializeGame();
    }

    private async Task SaveGameResult()
    {
        try
        {
            string result = $"Date: {DateTime.Now}, Player 1: {player1Score}, Player 2: {player2Score}";
            string path = Path.Combine(FileSystem.AppDataDirectory, "game_results.txt");
            File.AppendAllText(path, result + Environment.NewLine);
            await DisplayAlert("Save Successful", "Game result has been saved.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Save Error", $"Failed to save game result: {ex.Message}", "OK");
        }
    }

    private void NewGame_Clicked(object sender, EventArgs e)
    {
        InitializeGame();
    }

    private async void AddConsonant_Clicked(object sender, EventArgs e)
    {
        if (isLetterRound && currentLetters.Count < 9)
        {
            char consonant = consonants[random.Next(consonants.Length)];
            currentLetters.Add(consonant);
            await UpdateLettersDisplay(consonant);
        }
    }

    private async void AddVowel_Clicked(object sender, EventArgs e)
    {
        if (isLetterRound && currentLetters.Count < 9)
        {
            char vowel = vowels[random.Next(vowels.Length)];
            currentLetters.Add(vowel);
            await UpdateLettersDisplay(vowel);
        }
    }

    private async void AddSmallNumber_Clicked(object sender, EventArgs e)
    {
        if (!isLetterRound && currentNumbers.Count < 6)
        {
            int number = smallNumbers[random.Next(smallNumbers.Length)];
            currentNumbers.Add(number);
            await UpdateNumbersDisplay(number);
        }
    }

    private async void AddLargeNumber_Clicked(object sender, EventArgs e)
    {
        if (!isLetterRound && currentNumbers.Count < 6)
        {
            int number = largeNumbers[random.Next(largeNumbers.Length)];
            currentNumbers.Add(number);
            await UpdateNumbersDisplay(number);
        }
    }

    private async Task UpdateLettersDisplay(char newLetter)
    {
        LettersLabel.Text = new string(currentLetters.ToArray());
        await AnimateNewLetter(newLetter);
    }

    private async Task AnimateNewLetter(char letter)
    {
        var label = new Label
        {
            Text = letter.ToString(),
            FontSize = 48,
            Opacity = 0,
            TranslationY = -50
        };

        var container = (Layout)LettersLabel.Parent;
        container.Children.Add(label);

        await Task.WhenAll(
            label.FadeTo(1, 500),
            label.TranslateTo(0, 0, 500, Easing.SpringOut)
        );

        container.Children.Remove(label);
    }

    private async Task UpdateNumbersDisplay(int newNumber)
    {
        NumbersLabel.Text += $"{newNumber} ";
        await AnimateNewNumber(newNumber);
    }

    private async Task AnimateNewNumber(int number)
    {
        var label = new Label
        {
            Text = number.ToString(),
            FontSize = 36,
            Opacity = 0,
            TranslationY = -50
        };

        var container = (Layout)NumbersLabel.Parent;
        container.Children.Add(label);

        await Task.WhenAll(
            label.FadeTo(1, 500),
            label.TranslateTo(0, 0, 500, Easing.SpringOut)
        );

        container.Children.Remove(label);
    }

    private async void SubmitWord_Clicked(object sender, EventArgs e)
    {
        if (isLetterRound)
        {
            string submittedWord = WordInputEntry.Text.ToUpper();

            if (IsValidWord(submittedWord))
            {
                int wordScore = CalculateWordScore(submittedWord);
                UpdatePlayerScore(wordScore);
                await DisplayAlert("Valid Word", $"You scored {wordScore} points.", "OK");
            }
            else
            {
                await DisplayAlert("Invalid Word", "Please try again.", "OK");
            }

            WordInputEntry.Text = "";
        }
    }

    private async void SubmitCalculation_Clicked(object sender, EventArgs e)
    {
        if (!isLetterRound)
        {
            string calculation = CalculationInputEntry.Text;
            int target = int.Parse(NumbersLabel.Text.Split(':')[1].Trim().Split('\n')[0]);

            if (IsValidCalculation(calculation, target))
            {
                int calculationScore = CalculateCalculationScore(calculation, target);
                UpdatePlayerScore(calculationScore);
                await DisplayAlert("Valid Calculation", $"You scored {calculationScore} points.", "OK");
            }
            else
            {
                await DisplayAlert("Invalid Calculation", "Please try again.", "OK");
            }

            CalculationInputEntry.Text = "";
        }
    }

    private void UpdatePlayerScore(int score)
    {
        if (isTwoPlayerMode)
        {
            if (currentPlayer == 1)
            {
                player1Score += score;
                Player1ScoreLabel.Text = player1Score.ToString();
                currentPlayer = 2;
            }
            else
            {
                player2Score += score;
                Player2ScoreLabel.Text = player2Score.ToString();
                currentPlayer = 1;
            }
        }
        else
        {
            player1Score += score;
            Player1ScoreLabel.Text = player1Score.ToString();
        }
    }

    private bool IsValidWord(string word)
    {
        if (!dictionary.Contains(word))
        {
            return false;
        }

        List<char> availableLetters = new List<char>(currentLetters);
        foreach (char c in word)
        {
            if (!availableLetters.Remove(c))
            {
                return false;
            }
        }

        return true;
    }

    private int CalculateWordScore(string word)
    {
        int baseScore = word.Length;
        if (word.Length == currentLetters.Count)
        {
            baseScore += 50; // Bonus for using all letters
        }
        return baseScore;
    }

    private bool IsValidCalculation(string calculation, int target)
    {
        try
        {
            int result = EvaluateExpression(calculation);
            return result == target;
        }
        catch
        {
            return false;
        }
    }

    private int EvaluateExpression(string expression)
    {
        var tokens = expression.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        int result = int.Parse(tokens[0]);
        for (int i = 1; i < tokens.Length; i += 2)
        {
            int nextNum = int.Parse(tokens[i + 1]);
            switch (tokens[i])
            {
                case "+": result += nextNum; break;
                case "-": result -= nextNum; break;
                case "*": result *= nextNum; break;
                case "/": result /= nextNum; break;
                default: throw new InvalidOperationException("Invalid operator");
            }
        }
        return result;
    }

    private int CalculateCalculationScore(string calculation, int target)
    {
        int result = EvaluateExpression(calculation);
        int difference = Math.Abs(target - result);
        if (difference == 0)
        {
            return 10;
        }
        else if (difference <= 5)
        {
            return 7;
        }
        else if (difference <= 10)
        {
            return 5;
        }
        return 0;
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage());
    }
}