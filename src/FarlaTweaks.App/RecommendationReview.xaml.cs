using System.Windows;
using System.Windows.Controls;
using FarlaTweaks.Core.Database;
using FarlaTweaks.Core.Diagnostics;
using FarlaTweaks.Core.Models;
using FarlaTweaks.Core.Persistence;
using FarlaTweaks.Core.Recommendations;

namespace FarlaTweaks.App;

public partial class RecommendationReview : Window
{
    private readonly ProfileStore _profileStore = new();
    private readonly TweakCatalogLoader _catalogLoader = new();
    private readonly RecommendationEngine _recommendationEngine = new();
    private readonly List<CheckBox> _selectionBoxes = new();

    public RecommendationReview()
    {
        InitializeComponent();
        Loaded += RecommendationReview_OnLoaded;
    }

    private async void RecommendationReview_OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= RecommendationReview_OnLoaded;
        var profile = await _profileStore.LoadAsync();
        if (profile is null)
        {
            ProfileStatusText.Text = "Run a system analysis first.";
            return;
        }

        var tweaks = await _catalogLoader.LoadAsync();
        var recommendations = _recommendationEngine.Build(profile, tweaks, Array.Empty<string>());
        RecommendedCountText.Text = recommendations.Count.ToString();
        RestartCountText.Text = recommendations.Count(r => r.RequiresRestart).ToString();
        ProfileStatusText.Text = "Based on your saved system profile.";

        foreach (var recommendation in recommendations)
        {
            var check = new CheckBox
            {
                IsChecked = recommendation.Risk == RiskLevel.Safe,
                Margin = new Thickness(0, 0, 0, 12),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            var title = new TextBlock
            {
                Text = recommendation.Title,
                Foreground = (System.Windows.Media.Brush)FindResource("FarlaText"),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold
            };

            var reason = new TextBlock
            {
                Text = recommendation.Reason,
                Foreground = (System.Windows.Media.Brush)FindResource("FarlaMuted"),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 0)
            };

            var risk = new TextBlock
            {
                Text = recommendation.RequiresRestart
                    ? $"{recommendation.Risk.ToString().ToUpperInvariant()}  ·  RESTART"
                    : recommendation.Risk.ToString().ToUpperInvariant(),
                Foreground = recommendation.Risk == RiskLevel.Safe
                    ? (System.Windows.Media.Brush)FindResource("FarlaSuccess")
                    : (System.Windows.Media.Brush)FindResource("FarlaWarning"),
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 8, 0, 0)
            };

            var content = new StackPanel();
            content.Children.Add(title);
            content.Children.Add(reason);
            content.Children.Add(risk);
            check.Content = content;
            check.DataContext = recommendation;
            check.Checked += SelectionChanged;
            check.Unchecked += SelectionChanged;

            var card = new Border
            {
                Background = (System.Windows.Media.Brush)FindResource("FarlaSurface"),
                BorderBrush = (System.Windows.Media.Brush)FindResource("FarlaBorder"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(16),
                Child = check
            };

            RecommendationList.Children.Add(card);
            _selectionBoxes.Add(check);
        }

        ApplySelectedButton.IsEnabled = _selectionBoxes.Any(x => x.IsChecked == true);
    }

    private void SelectionChanged(object sender, RoutedEventArgs e)
    {
        ApplySelectedButton.IsEnabled = _selectionBoxes.Any(x => x.IsChecked == true);
    }

    private void ApplySelectedButton_OnClick(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "The review flow is ready. Safe apply and rollback execution will be enabled once each selected tweak has an executable, verified change definition.",
            "Farla Tweaks",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
