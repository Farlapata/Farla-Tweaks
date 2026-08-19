using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using FarlaTweaks.Core.Models;
using FarlaTweaks.Core.Persistence;

namespace FarlaTweaks.App;

public partial class GamesWindow : Window
{
    private readonly UserPreferencesStore _preferencesStore = new();
    private readonly GameSessionStore _sessionStore = new();
    private readonly DispatcherTimer _timer;
    private DateTimeOffset? _sessionStarted;
    private bool _wasRunning;

    public GamesWindow()
    {
        InitializeComponent();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _timer.Tick += Timer_OnTick;
        Loaded += GamesWindow_OnLoaded;
        Closed += GamesWindow_OnClosed;
    }

    private async void GamesWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= GamesWindow_OnLoaded;
        await RefreshAsync();
        _timer.Start();
    }

    private async void Timer_OnTick(object? sender, EventArgs e) => await RefreshAsync();

    private async Task RefreshAsync()
    {
        var preferences = await _preferencesStore.LoadAsync() ?? new UserPreferences();
        var primaryGame = string.IsNullOrWhiteSpace(preferences.PrimaryGame) ? "Fortnite" : preferences.PrimaryGame;
        PrimaryGameText.Text = primaryGame.ToUpperInvariant();
        DependencyText.Text = preferences.Dependencies.Count == 0
            ? "Dependency profile: no optional software selected."
            : $"Dependency profile: {string.Join(", ", preferences.Dependencies)}";

        var running = primaryGame.Equals("Fortnite", StringComparison.OrdinalIgnoreCase)
                      && Process.GetProcessesByName("FortniteClient-Win64-Shipping").Length > 0;

        if (running)
        {
            if (!_wasRunning)
                _sessionStarted = DateTimeOffset.Now;

            GameStatusText.Text = _sessionStarted.HasValue
                ? $"RUNNING · {FormatDuration(DateTimeOffset.Now - _sessionStarted.Value)}"
                : "RUNNING · SESSION DETECTED";
            GameStatusText.Foreground = (Brush)FindResource("FarlaSuccess");
            StatusDot.Fill = (Brush)FindResource("FarlaSuccess");
        }
        else
        {
            if (_wasRunning && _sessionStarted.HasValue)
            {
                var started = _sessionStarted.Value;
                var ended = DateTimeOffset.Now;
                await _sessionStore.AddAsync(new GameSession(primaryGame, started, ended, ended - started));
                _sessionStarted = null;
            }

            GameStatusText.Text = "NOT RUNNING";
            GameStatusText.Foreground = (Brush)FindResource("FarlaMuted");
            StatusDot.Fill = (Brush)FindResource("FarlaMuted");
        }

        _wasRunning = running;
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours}h {duration.Minutes:00}m";
        return $"{duration.Minutes:00}:{duration.Seconds:00}";
    }

    private async void RefreshButton_OnClick(object sender, RoutedEventArgs e) => await RefreshAsync();

    private void GamesWindow_OnClosed(object? sender, EventArgs e) => _timer.Stop();

    private void DragArea_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e) => Close();
}
