using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FarlaTweaks.Core.Models;
using FarlaTweaks.Core.Persistence;

namespace FarlaTweaks.App;

public partial class GamesWindow : Window
{
    private readonly UserPreferencesStore _preferencesStore = new();

    public GamesWindow()
    {
        InitializeComponent();
        Loaded += GamesWindow_OnLoaded;
    }

    private async void GamesWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= GamesWindow_OnLoaded;
        await RefreshAsync();
    }

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
            GameStatusText.Text = "RUNNING · SESSION DETECTED";
            GameStatusText.Foreground = (Brush)FindResource("FarlaSuccess");
            StatusDot.Fill = (Brush)FindResource("FarlaSuccess");
        }
        else
        {
            GameStatusText.Text = "NOT RUNNING";
            GameStatusText.Foreground = (Brush)FindResource("FarlaMuted");
            StatusDot.Fill = (Brush)FindResource("FarlaMuted");
        }
    }

    private async void RefreshButton_OnClick(object sender, RoutedEventArgs e) => await RefreshAsync();

    private void DragArea_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e) => Close();
}
