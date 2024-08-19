using System.Collections.ObjectModel;

namespace Countdown1;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        ThemeSwitch.IsToggled = Application.Current.UserAppTheme == AppTheme.Dark;
        DifficultyPicker.SelectedIndex = Preferences.Get("Difficulty", 1);
        UpdateBestScore();
    }
    private void UpdateBestScore()
    {
        int bestScore = Preferences.Get("BestScore", 0);
        BestScoreLabel.Text = $"Best Score: {bestScore}";
    }

    // Add this method
    private void OnDifficultyChanged(object sender, EventArgs e)
    {
        Preferences.Set("Difficulty", DifficultyPicker.SelectedIndex);
    }
    private void OnThemeSwitchToggled(object sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            Application.Current.UserAppTheme = AppTheme.Dark;
        }
        else
        {
            Application.Current.UserAppTheme = AppTheme.Light;
        }

        // Force the application to re-apply the theme
        Application.Current.MainPage.DisplayAlert("Theme Changed", "The theme has been updated.", "OK");
    }

    private async void OnViewHistoryClicked(object sender, EventArgs e)
    {
        try
        {
            string path = Path.Combine(FileSystem.AppDataDirectory, "game_results.txt");
            if (File.Exists(path))
            {
                string history = await File.ReadAllTextAsync(path);
                await DisplayAlert("Game History", history, "OK");
            }
            else
            {
                await DisplayAlert("Game History", "No game history available.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load game history: {ex.Message}", "OK");
        }
    }

    private async void OnClearHistoryClicked(object sender, EventArgs e)
    {
        try
        {
            string path = Path.Combine(FileSystem.AppDataDirectory, "game_results.txt");
            if (File.Exists(path))
            {
                File.Delete(path);
                await DisplayAlert("Success", "Game history has been cleared.", "OK");
            }
            else
            {
                await DisplayAlert("Info", "No game history to clear.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to clear game history: {ex.Message}", "OK");
        }
    }

    private async void OnBackToGameClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}