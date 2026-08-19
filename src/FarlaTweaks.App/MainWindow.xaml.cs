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
    private readonly TweakCatalogLoader _catalogLoader = new();
    private readonly RecommendationEngine _recommendationEngine = new();
    private SystemProfile? _profile;

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
            _profile = await _profileStore.LoadAsync();
            if (_profile is not null)
                await ApplyProfileToDashboardAsync(_profile, persisted: true);
        }
        catch
        {
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
        StartAnalysisButton.IsEnabled = false;
        StartAnalysisButton.Content = "ANALYZING...";
        AnalysisDescriptionText.Text = "Reading your system. No settings are being changed.";
        ScoreStatusText.Text = "ANALYZING";
        CopilotStatusText.Text = "Reading your system profile.";

        try
        {
            _profile = await Task.Run(_profileCollector.Collect);
            await _profileStore.SaveAsync(_profile);
            await ApplyProfileToDashboardAsync(_profile, persisted: false);
        }
        catch (Exception ex)
        {
            ScoreStatusText.Text = "SCAN FAILED";
            ScoreDescriptionText.Text = "Farla could not complete the system scan.";
            AnalysisDescriptionText.Text = ex.Message;
            CopilotStatusText.Text = "Analysis failed. No changes were made.";
            StartAnalysisButton.IsEnabled = true;
            StartAnalysisButton.Content = "TRY AGAIN";
            return;
        }

        StartAnalysisButton.Content = "SYSTEM ANALYZED";
    }

    private async Task ApplyProfileToDashboardAsync(SystemProfile profile, bool persisted)
    {
        ScoreStatusText.Text = "ANALYZED";
        ScoreDescriptionText.Text = persisted
            ? "Farla remembered your last system analysis. Refresh it whenever your hardware or Windows setup changes."
            : "System profile captured. Farla is ready to evaluate compatibility and recommendations.";
        AnalysisDescriptionText.Text = BuildProfileSummary(profile);

        PerformanceValueText.Text = $"Performance   {profile.RamGb} GB RAM";
        StabilityValueText.Text = $"Stability        {profile.Architecture}";
        GamingValueText.Text = $"Gaming          {profile.Gpu}";
        NetworkValueText.Text = $"Network        {profile.OsVersion}";

        try
        {
            var tweaks = await _catalogLoader.LoadAsync();
            var recommendations = _recommendationEngine.Build(profile, tweaks, Array.Empty<string>());
            RecommendationEyebrowText.Text = "RECOMMENDATION ENGINE";
            RecommendationTitleText.Text = recommendations.Count == 0
                ? "No compatible recommendations yet."
                : $"{recommendations.Count} compatible recommendations are ready to review.";
            AnalysisDescriptionText.Text = $"{BuildProfileSummary(profile)} Farla found {recommendations.Count} compatible catalog entries and did not apply any of them.";
            CopilotStatusText.Text = recommendations.Count == 0
                ? "Monitoring your system. Nothing requires action yet."
                : "Profile analyzed. Recommendations are ready for review.";
        }
        catch
        {
            RecommendationEyebrowText.Text = "RECOMMENDATION ENGINE";
            RecommendationTitleText.Text = "System analyzed. Recommendation catalog unavailable.";
            CopilotStatusText.Text = "Profile captured. Recommendation engine needs attention.";
        }

        StartAnalysisButton.IsEnabled = true;
        StartAnalysisButton.Content = "REFRESH SYSTEM ANALYSIS";
    }

    private static string BuildProfileSummary(SystemProfile profile)
    {
        var cpu = string.IsNullOrWhiteSpace(profile.Cpu) ? "Unknown CPU" : profile.Cpu;
        var gpu = string.IsNullOrWhiteSpace(profile.Gpu) ? "Unknown GPU" : profile.Gpu;
        var memory = profile.RamGb > 0 ? $"{profile.RamGb} GB RAM" : "RAM unavailable";
        return $"Detected {cpu}, {gpu}, {memory}. Nothing was modified.";
    }

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
