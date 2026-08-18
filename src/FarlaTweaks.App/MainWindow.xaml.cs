using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FarlaTweaks.Core.Diagnostics;

namespace FarlaTweaks.App;

public partial class MainWindow : Window
{
    private readonly SystemProfileCollector _profileCollector = new();

    public MainWindow()
    {
        InitializeComponent();
        UpdateGreeting();
        Loaded += MainWindow_OnLoaded;
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        await RefreshSystemSummaryAsync();
    }

    private async Task RefreshSystemSummaryAsync()
    {
        try
        {
            var profile = await Task.Run(_profileCollector.Collect);
            var cpu = string.IsNullOrWhiteSpace(profile.Cpu) ? "Unknown CPU" : profile.Cpu;
            var gpu = string.IsNullOrWhiteSpace(profile.Gpu) ? "Unknown GPU" : profile.Gpu;
            var memory = profile.RamGb > 0 ? $"{profile.RamGb} GB RAM" : "RAM unavailable";

            SystemSummaryText.Text = $"{cpu}  ·  {gpu}  ·  {memory}  ·  {profile.OsVersion}";
        }
        catch
        {
            SystemSummaryText.Text = "System profile could not be read yet.";
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

    private void StartWizardButton_OnClick(object sender, RoutedEventArgs e)
    {
        var wizard = new SetupWizard
        {
            Owner = this
        };
        wizard.ShowDialog();
    }
}
