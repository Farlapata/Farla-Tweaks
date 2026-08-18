using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FarlaTweaks.Core.Compatibility;
using FarlaTweaks.Core.Database;
using FarlaTweaks.Core.Diagnostics;
using FarlaTweaks.Core.Models;

namespace FarlaTweaks.App;

public partial class SetupWizard : Window
{
    private int _step = 1;

    public SetupWizard()
    {
        InitializeComponent();
    }

    private void NextButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_step == 1)
        {
            _step = 2;
            ShowStep();
            return;
        }

        if (_step == 2)
        {
            _step = 3;
            _ = BuildPlanAsync();
            ShowStep();
            return;
        }

        DialogResult = true;
        Close();
    }

    private void BackButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_step <= 1)
            return;

        _step--;
        ShowStep();
    }

    private void ShowStep()
    {
        Step1Panel.Visibility = _step == 1 ? Visibility.Visible : Visibility.Collapsed;
        Step2Panel.Visibility = _step == 2 ? Visibility.Visible : Visibility.Collapsed;
        Step3Panel.Visibility = _step == 3 ? Visibility.Visible : Visibility.Collapsed;

        StepLabel.Text = $"0{_step} / 03";
        BackButton.Visibility = _step == 1 ? Visibility.Collapsed : Visibility.Visible;
        NextButton.Content = _step == 3 ? "Finish Setup" : "Continue";

        (TitleText.Text, SubtitleText.Text) = _step switch
        {
            1 => ("Tell Farla how you use your PC.", "Your answers shape which optimizations Farla considers."),
            2 => ("Tell Farla what your setup depends on.", "This prevents the optimizer from breaking tools you actually use."),
            _ => ("Your personalized Farla plan.", "Nothing is applied yet. Review comes before changes.")
        };
    }

    private async Task BuildPlanAsync()
    {
        PlanSummary.Text = "Analyzing your setup...";
        PlanItems.Items.Clear();

        try
        {
            var detectedProfile = await Task.Run(() => new SystemProfileCollector().Collect());
            var userCapabilities = new List<string>
            {
                CrosshairBox.IsChecked == true ? "crosshair-x" : "game-bar-unused"
            };

            if (ObsBox.IsChecked == true)
                userCapabilities.Add("obs");
            if (DiscordBox.IsChecked == true)
                userCapabilities.Add("discord-overlay");
            if (OutplayedBox.IsChecked == true)
                userCapabilities.Add("recording");
            if (AfterburnerBox.IsChecked == true)
                userCapabilities.Add("afterburner");
            if (WallpaperEngineBox.IsChecked == true)
                userCapabilities.Add("wallpaper-engine");

            var profile = detectedProfile with
            {
                Capabilities = detectedProfile.Capabilities
                    .Concat(userCapabilities)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            };

            var catalog = await new TweakCatalogLoader().LoadAsync();
            var engine = new CompatibilityEngine();
            var selectedIds = catalog.Select(t => t.Id).ToArray();

            var compatible = catalog
                .Select(t => (Tweak: t, Result: engine.Evaluate(t, profile, selectedIds)))
                .Where(x => x.Result.IsCompatible && x.Tweak.Risk != RiskLevel.Rejected)
                .Select(x => x.Tweak)
                .ToList();

            PlanSummary.Text = $"{compatible.Count} compatible recommendations for {GameBox.Text}.";
            foreach (var tweak in compatible.Take(6))
            {
                var risk = tweak.Risk == RiskLevel.Safe ? "Safe" : "Review";
                PlanItems.Items.Add(new TextBlock
                {
                    Text = $"• {tweak.Name}  ·  {risk}",
                    Foreground = (System.Windows.Media.Brush)FindResource("FarlaText"),
                    Margin = new Thickness(0, 4, 0, 0)
                });
            }

            if (CrosshairBox.IsChecked == true)
            {
                PlanItems.Items.Add(new TextBlock
                {
                    Text = "• Game Bar stays available because Crosshair X was selected.",
                    Foreground = (System.Windows.Media.Brush)FindResource("FarlaMuted"),
                    Margin = new Thickness(0, 10, 0, 0),
                    TextWrapping = TextWrapping.Wrap
                });
            }
        }
        catch
        {
            PlanSummary.Text = "We couldn't complete the system scan yet. Nothing has been changed.";
        }
    }

    private void DragArea_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
