using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FarlaTweaks.Core.Diagnostics;
using FarlaTweaks.Core.Models;
using FarlaTweaks.Core.Persistence;

namespace FarlaTweaks.App;

public partial class MainWindow : Window
{
    private readonly SystemProfileCollector _profileCollector = new();
    private readonly ProfileStore _profileStore = new();
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
            {
                ApplyProfileToDashboard(_profile, persisted: true);
            }
        }
        catch
        {
            // A missing or unreadable profile must never prevent Farla from starting.
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

        try
        {
            _profile = await Task.Run(_profileCollector.Collect);
            await _profileStore.SaveAsync(_profile);
            ApplyProfileToDashboard(_profile, persisted: false);
        }
        catch (Exception ex)
        {
            ScoreStatusText.Text = "SCAN FAILED";
            ScoreDescriptionText.Text = "Farla could not complete the system scan.";
            AnalysisDescriptionText.Text = ex.Message;
            StartAnalysisButton.IsEnabled = true;
            StartAnalysisButton.Content = "TRY AGAIN";
            return;
        }

        StartAnalysisButton.Content = "SYSTEM ANALYZED";
    }

    private void ApplyProfileToDashboard(SystemProfile profile, bool persisted)
    {
        ScoreStatusText.Text = "ANALYZED";
        ScoreDescriptionText.Text = persisted
            ? "Farla remembered your last system analysis. Run it again to refresh the profile."
            : "System profile captured. Farla is ready to evaluate compatibility and recommendations.";
        AnalysisDescriptionText.Text = BuildProfileSummary(profile);

        PerformanceValueText.Text = $"Performance   {profile.RamGb} GB RAM";
        StabilityValueText.Text = $"Stability        {profile.Architecture}";
        GamingValueText.Text = $"Gaming          {profile.Gpu}";
        NetworkValueText.Text = $"Network        {profile.OsVersion}";

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
