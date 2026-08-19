using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FarlaTweaks.Core.Database;
using FarlaTweaks.Core.Diagnostics;
using FarlaTweaks.Core.Models;
using FarlaTweaks.Core.Persistence;
using FarlaTweaks.Core.Recommendations;

namespace FarlaTweaks.App;

public partial class MainWindow : Window
{
    private readonly SystemProfileCollector _profileCollector = new();
    private readonly ProfileStore _profileStore = new();
    private readonly UserPreferencesStore _preferencesStore = new();
    private readonly TweakCatalogLoader _catalogLoader = new();
    private readonly RecommendationEngine _recommendationEngine = new();
    private SystemProfile? _profile;
    private UserPreferences? _preferences;
    private bool _analysisRunning;

    public MainWindow()
    {
        InitializeComponent();
        UpdateGreeting();
        Loaded += MainWindow_OnLoaded;
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= MainWindow_OnLoaded;

        try
        {
            _preferences = await _preferencesStore.LoadAsync();
            _profile = await _profileStore.LoadAsync();

            if (_preferences is null || !_preferences.OnboardingCompleted)
            {
                var wizard = new SetupWizard { Owner = this };
                var completed = wizard.ShowDialog() == true;
                if (!completed)
                    return;

                _preferences = await _preferencesStore.LoadAsync();
            }

            if (_profile is not null)
                await ApplyProfileToDashboardAsync(_profile, persisted: true);
            else
                await AnalyzeAsync();
        }
        catch
        {
            SystemStateText.Text = "READY";
        }
    }

    private void UpdateGreeting()
    {
        var hour = DateTime.Now.Hour;
        GreetingText.Text = hour switch
        {
            >= 5 and < 12 => "GOOD MORNING, MATHÉO",
            >= 12 and < 18 => "GOOD AFTERNOON, MATHÉO",
            _ => "GOOD EVENING, MATHÉO"
        };
    }

    private async void StartAnalysisButton_OnClick(object sender, RoutedEventArgs e)
    {
        await AnalyzeAsync();
    }

    private async Task AnalyzeAsync()
    {
        if (_analysisRunning)
            return;

        _analysisRunning = true;
        StartAnalysisButton.IsEnabled = false;
        StartAnalysisButton.Content = "ANALYZING...";
        ReviewRecommendationsButton.IsEnabled = false;
        ScoreStatusText.Text = "ANALYZING";
        SystemStateText.Text = "SCANNING";
        CopilotStatusText.Text = "Reading your system. Nothing is being changed.";

        try
        {
            _profile = await Task.Run(_profileCollector.Collect);
            await _profileStore.SaveAsync(_profile);
            await ApplyProfileToDashboardAsync(_profile, persisted: false);
        }
        catch (Exception ex)
        {
            ScoreStatusText.Text = "SCAN FAILED";
            SystemStateText.Text = "ERROR";
            AnalysisDescriptionText.Text = ex.Message;
            CopilotStatusText.Text = "Analysis failed. No changes were made.";
            StartAnalysisButton.Content = "TRY AGAIN";
        }
        finally
        {
            _analysisRunning = false;
            StartAnalysisButton.IsEnabled = true;
            if (StartAnalysisButton.Content.ToString() == "ANALYZING...")
                StartAnalysisButton.Content = "REFRESH ANALYSIS";
        }
    }

    private async Task ApplyProfileToDashboardAsync(SystemProfile profile, bool persisted)
    {
        var preferences = _preferences ?? new UserPreferences();
        var primaryGame = string.IsNullOrWhiteSpace(preferences.PrimaryGame) ? "Fortnite" : preferences.PrimaryGame;
        PrimaryGameText.Text = primaryGame;
        GameStatusText.Text = $"{primaryGame.ToUpperInvariant()}  /  READYING";

        var effectiveCapabilities = profile.Capabilities
            .Concat(preferences.Dependencies)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var effectiveProfile = profile with { Capabilities = effectiveCapabilities };
        var tweaks = await _catalogLoader.LoadAsync();
        var recommendations = _recommendationEngine.Build(effectiveProfile, tweaks, effectiveCapabilities);

        var score = CalculateScore(profile);
        ScoreText.Text = score.ToString();
        ScoreStatusText.Text = "ANALYZED";
        SystemStateText.Text = "READY";
        ScoreDescriptionText.Text = persisted
            ? "Farla remembered this system profile. Refresh the analysis after hardware or Windows changes."
            : "System profile captured. The score describes system readiness, not guaranteed FPS.";

        CpuText.Text = $"CPU  {Compact(profile.Cpu)}";
        GpuText.Text = $"GPU  {Compact(profile.Gpu)}";
        MemoryText.Text = $"MEMORY  {profile.RamGb} GB  ·  {profile.Architecture}";
        DisplayText.Text = $"DISPLAY  {Compact(profile.PrimaryDisplay)}  ·  {profile.RefreshRateHz} Hz";

        var performance = Clamp(45 + Math.Min(profile.RamGb, 32) + (IsKnown(profile.Gpu) ? 12 : 0));
        var stability = Clamp(profile.Architecture.Equals("X64", StringComparison.OrdinalIgnoreCase) ? 92 : 72);
        var gaming = Clamp((profile.RefreshRateHz >= 144 ? 92 : profile.RefreshRateHz >= 120 ? 86 : 70) + (IsKnown(profile.Gpu) ? 4 : 0));
        var network = 80;

        PerformanceBar.Value = performance;
        StabilityBar.Value = stability;
        GamingBar.Value = gaming;
        NetworkBar.Value = network;
        PerformanceValueText.Text = $"{performance}";
        StabilityValueText.Text = $"{stability}";
        GamingValueText.Text = $"{gaming}";
        NetworkValueText.Text = $"{network}";

        RecommendationTitleText.Text = recommendations.Count == 0
            ? "No compatible changes are waiting for review."
            : $"{recommendations.Count} compatible changes are waiting for review.";
        AnalysisDescriptionText.Text = $"Detected {Compact(profile.Cpu)}, {Compact(profile.Gpu)}, {profile.RamGb} GB RAM. Farla excluded incompatible catalog entries based on your setup.";
        CopilotStatusText.Text = recommendations.Count == 0
            ? "Profile analyzed. Farla has no compatible action requiring your attention."
            : $"Profile analyzed. {recommendations.Count} recommendations are ready to review.";
        ReviewRecommendationsButton.IsEnabled = recommendations.Count > 0;
    }

    private static int CalculateScore(SystemProfile profile)
    {
        var score = 50;
        score += Math.Min(profile.RamGb, 32);
        if (profile.Architecture.Equals("X64", StringComparison.OrdinalIgnoreCase))
            score += 8;
        if (IsKnown(profile.Cpu))
            score += 4;
        if (IsKnown(profile.Gpu))
            score += 6;
        if (profile.RefreshRateHz >= 144)
            score += 5;
        else if (profile.RefreshRateHz >= 120)
            score += 3;
        return Clamp(score);
    }

    private static bool IsKnown(string value) => !string.IsNullOrWhiteSpace(value) && !value.Equals("Unknown", StringComparison.OrdinalIgnoreCase);

    private static string Compact(string value) => string.IsNullOrWhiteSpace(value) ? "Unknown" : value;

    private static int Clamp(int value) => Math.Clamp(value, 0, 100);

    private void OptimizeButton_OnClick(object sender, RoutedEventArgs e)
    {
        var review = new RecommendationReview { Owner = this };
        review.ShowDialog();
    }

    private void HistoryButton_OnClick(object sender, RoutedEventArgs e)
    {
        var history = new HistoryWindow { Owner = this };
        history.ShowDialog();
    }

    private void SetupButton_OnClick(object sender, RoutedEventArgs e)
    {
        var wizard = new SetupWizard { Owner = this };
        if (wizard.ShowDialog() == true)
            _ = ReloadAfterSetupAsync();
    }

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        var settings = new SettingsWindow { Owner = this };
        if (settings.ShowDialog() == true)
            _ = ReloadAfterSetupAsync();
    }

    private void GamesButton_OnClick(object sender, RoutedEventArgs e)
    {
        var games = new GamesWindow { Owner = this };
        games.ShowDialog();
    }

    private void MonitorButton_OnClick(object sender, RoutedEventArgs e)
    {
        var monitor = new MonitorWindow { Owner = this };
        monitor.ShowDialog();
    }

    private async Task ReloadAfterSetupAsync()
    {
        _preferences = await _preferencesStore.LoadAsync();
        await AnalyzeAsync();
    }

    private void WindowDragArea_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
