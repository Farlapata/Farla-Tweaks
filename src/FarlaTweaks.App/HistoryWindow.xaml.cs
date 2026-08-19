using System.Windows;
using FarlaTweaks.Core.Execution;
using FarlaTweaks.Core.Persistence;
using FarlaTweaks.Core.State;

namespace FarlaTweaks.App;

public partial class HistoryWindow : Window
{
    private readonly SnapshotStore _store = new();
    private readonly ITweakExecutor _executor = new WindowsRegistryTweakExecutor();

    public HistoryWindow()
    {
        InitializeComponent();
        Loaded += HistoryWindow_OnLoaded;
    }

    private async void HistoryWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= HistoryWindow_OnLoaded;
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        SnapshotList.Children.Clear();
        var snapshots = await _store.LoadAllAsync();
        StatusText.Text = snapshots.Count == 0
            ? "No changes have been applied yet."
            : $"{snapshots.Count} rollback snapshot{(snapshots.Count == 1 ? "" : "s")} saved locally.";

        foreach (var snapshot in snapshots)
        {
            var card = new System.Windows.Controls.Border
            {
                Background = (System.Windows.Media.Brush)FindResource("FarlaSurface"),
                BorderBrush = (System.Windows.Media.Brush)FindResource("FarlaBorder"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var grid = new System.Windows.Controls.Grid();
            grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
            grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = GridLength.Auto });

            var stack = new System.Windows.Controls.StackPanel();
            stack.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = snapshot.Label,
                Foreground = (System.Windows.Media.Brush)FindResource("FarlaText"),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold
            });
            stack.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = $"{snapshot.CreatedAt.LocalDateTime:g}  ·  {snapshot.RegistryValues.Count} registry values saved",
                Foreground = (System.Windows.Media.Brush)FindResource("FarlaMuted"),
                FontSize = 12,
                Margin = new Thickness(0, 4, 0, 0)
            });

            var revert = new System.Windows.Controls.Button
            {
                Content = "REVERT",
                Style = (System.Windows.Style)FindResource("PrimaryButtonStyle"),
                Margin = new Thickness(18, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Tag = snapshot
            };
            revert.Click += RevertButton_OnClick;

            System.Windows.Controls.Grid.SetColumn(stack, 0);
            System.Windows.Controls.Grid.SetColumn(revert, 1);
            grid.Children.Add(stack);
            grid.Children.Add(revert);
            card.Child = grid;
            SnapshotList.Children.Add(card);
        }
    }

    private async void RevertButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button button || button.Tag is not StateSnapshot snapshot)
            return;

        var result = MessageBox.Show(
            $"Revert '{snapshot.Label}'? This restores the registry values captured before Farla applied it.",
            "Confirm rollback",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            await _executor.RevertAsync(snapshot);
            StatusText.Text = $"Reverted '{snapshot.Label}'.";
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Rollback failed: {ex.Message}";
        }
    }

    private async void RevertAllButton_OnClick(object sender, RoutedEventArgs e)
    {
        var snapshots = await _store.LoadAllAsync();
        if (snapshots.Count == 0)
        {
            StatusText.Text = "There are no Farla changes to revert.";
            return;
        }

        var result = MessageBox.Show(
            $"Revert all {snapshots.Count} Farla snapshots? This restores the recorded state before Farla's changes.",
            "Confirm full rollback",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            var service = new RollbackService(_store, _executor);
            var reverted = await service.RevertAllAsync();
            StatusText.Text = $"Reverted {reverted} Farla snapshot{(reverted == 1 ? "" : "s")}.";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Full rollback failed: {ex.Message}";
        }
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e) => Close();
}
