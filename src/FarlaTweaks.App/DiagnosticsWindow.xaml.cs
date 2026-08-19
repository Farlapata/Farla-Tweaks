using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FarlaTweaks.Core.Diagnostics;

namespace FarlaTweaks.App;

public partial class DiagnosticsWindow : Window
{
    private readonly SystemHealthService _healthService = new();

    public DiagnosticsWindow()
    {
        InitializeComponent();
    }

    private async void RunButton_OnClick(object sender, RoutedEventArgs e)
    {
        RunButton.IsEnabled = false;
        RunButton.Content = "CHECKING...";
        StatusText.Text = "Running safe Windows checks...";
        ResultList.Children.Clear();

        try
        {
            var results = await _healthService.RunSafeChecksAsync();
            foreach (var result in results)
            {
                var card = new Border
                {
                    Background = (Brush)FindResource("FarlaSurface"),
                    BorderBrush = (Brush)FindResource("FarlaBorder"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(16),
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var stack = new StackPanel();
                var top = new Grid();
                top.ColumnDefinitions.Add(new ColumnDefinition());
                top.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                top.Children.Add(new TextBlock
                {
                    Text = result.Check,
                    Foreground = (Brush)FindResource("FarlaText"),
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold
                });
                var status = new TextBlock
                {
                    Text = result.Success ? "PASS" : "REVIEW",
                    Foreground = result.Success ? (Brush)FindResource("FarlaSuccess") : (Brush)FindResource("FarlaWarning"),
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold
                };
                Grid.SetColumn(status, 1);
                top.Children.Add(status);
                stack.Children.Add(top);
                stack.Children.Add(new TextBlock
                {
                    Text = $"{result.Summary}  ·  {result.Duration.TotalSeconds:0.0}s",
                    Foreground = (Brush)FindResource("FarlaMuted"),
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 5, 0, 0)
                });
                card.Child = stack;
                ResultList.Children.Add(card);
            }

            StatusText.Text = $"Completed {results.Count} safe checks.";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Diagnostics failed: {ex.Message}";
        }
        finally
        {
            RunButton.IsEnabled = true;
            RunButton.Content = "RUN CHECKS";
        }
    }

    private void DragArea_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e) => Close();
}
