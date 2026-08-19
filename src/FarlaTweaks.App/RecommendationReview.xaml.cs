using System.Windows;
using System.Windows.Controls;
using FarlaTweaks.Core.Database;
using FarlaTweaks.Core.Execution;
using FarlaTweaks.Core.Models;
using FarlaTweaks.Core.Persistence;
using FarlaTweaks.Core.Recommendations;

namespace FarlaTweaks.App;

public partial class RecommendationReview : Window
{
    private readonly ProfileStore _profileStore = new();
    private readonly UserPreferencesStore _preferencesStore = new();
    private readonly SnapshotStore _snapshotStore = new();
    private readonly TweakCatalogLoader _catalogLoader = new();
    private readonly RecommendationEngine _recommendationEngine = new();
    private readonly ITweakExecutor _executor = new WindowsRegistryTweakExecutor();
    private readonly List<CheckBox> _selectionBoxes = new();
    private readonly Dictionary<string, TweakDefinition> _tweaksById = new(StringComparer.OrdinalIgnoreCase);

    public RecommendationReview()
    {
        InitializeComponent();
        Loaded += RecommendationReview_OnLoaded;
    }

    private async void RecommendationReview_OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= RecommendationReview_OnLoaded;
        try
        {
            var profile = await _profileStore.LoadAsync();
            var preferences = await _preferencesStore.LoadAsync() ?? new UserPreferences();
            if (profile is null)
            {
                ProfileStatusText.Text = "Run a system analysis first.";
                return;
            }

            var selectedDependencies = preferences.Dependencies.Count == 0
                ? new[] { "game-bar-unused" }
                : preferences.Dependencies.ToArray();
            GameBarConfirmationBox.IsChecked = selectedDependencies.Contains("game-bar-unused", StringComparer.OrdinalIgnoreCase);

            var effectiveProfile = profile with
            {
                Capabilities = profile.Capabilities
                    .Concat(selectedDependencies)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            };

            var tweaks = await _catalogLoader.LoadAsync();
            var recommendations = _recommendationEngine.Build(effectiveProfile, tweaks, selectedDependencies);
            _tweaksById.Clear();
            foreach (var tweak in tweaks)
                _tweaksById[tweak.Id] = tweak;

            RecommendedCountText.Text = recommendations.Count.ToString();
            RestartCountText.Text = recommendations.Count(r => r.RequiresRestart).ToString();
            ProfileStatusText.Text = preferences.Dependencies.Contains("crosshair-x", StringComparer.OrdinalIgnoreCase)
                ? "Crosshair X was detected in your setup profile. Farla will not disable Game Bar itself."
                : "Based on your saved system profile and setup dependencies.";

            foreach (var recommendation in recommendations)
                AddRecommendationCard(recommendation);

            UpdateApplyState();
        }
        catch (Exception ex)
        {
            ProfileStatusText.Text = $"Unable to load recommendations: {ex.Message}";
        }
    }

    private void AddRecommendationCard(Recommendation recommendation)
    {
        var check = new CheckBox
        {
            IsChecked = recommendation.Risk == RiskLevel.Safe,
            Margin = new Thickness(0),
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
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 0, 0, 10),
            Child = check
        };

        RecommendationList.Children.Add(card);
        _selectionBoxes.Add(check);
    }

    private void SelectionChanged(object sender, RoutedEventArgs e) => UpdateApplyState();

    private void UpdateApplyState()
    {
        var selected = _selectionBoxes.Any(x => x.IsChecked == true);
        ApplySelectedButton.IsEnabled = selected && GameBarConfirmationBox.IsChecked == true;
    }

    private async void ApplySelectedButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (GameBarConfirmationBox.IsChecked != true)
            return;

        var selected = _selectionBoxes
            .Where(x => x.IsChecked == true)
            .Select(x => x.DataContext as Recommendation)
            .Where(x => x is not null)
            .Cast<Recommendation>()
            .ToArray();

        if (selected.Length == 0)
            return;

        ApplySelectedButton.IsEnabled = false;
        ProfileStatusText.Text = "Applying selected audited changes...";

        var applied = 0;
        var failures = new List<string>();
        foreach (var recommendation in selected)
        {
            if (!_tweaksById.TryGetValue(recommendation.TweakId, out var tweak))
                continue;

            try
            {
                var snapshot = await _executor.ApplyAsync(tweak);
                await _snapshotStore.SaveAsync(snapshot);
                applied++;
            }
            catch (Exception ex)
            {
                failures.Add($"{tweak.Name}: {ex.Message}");
            }
        }

        ProfileStatusText.Text = failures.Count == 0
            ? $"Applied {applied} audited change{(applied == 1 ? "" : "s")}. Rollback snapshots were saved locally."
            : $"Applied {applied}; {failures.Count} failed. Failed changes were rolled back transactionally.";

        MessageBox.Show(
            failures.Count == 0
                ? $"Farla applied {applied} audited change{(applied == 1 ? "" : "s")} and saved rollback snapshots."
                : string.Join(Environment.NewLine, failures),
            failures.Count == 0 ? "Farla optimization complete" : "Farla optimization errors",
            MessageBoxButton.OK,
            failures.Count == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);

        UpdateApplyState();
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e) => Close();
}
