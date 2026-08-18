using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FarlaTweaks.Core.Compatibility;
using FarlaTweaks.Core.Models;
using FarlaTweaks.Core.System;

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
            BuildPlan();
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

    private void BuildPlan()
    {
        try
        {
            var profile = new WindowsSystemProfileScanner().Scan();
            var tweaks = new[]
            {
                new TweakDefinition
                {
                    Id = "gaming.disable-game-dvr",
                    Name = "Disable unused Game DVR capture",
                    Category = "Gaming",
                    Description = "Stops background Game DVR capture when it is not needed.",
                    Purpose = "Reduce unnecessary background capture activity.",
                    Risk = RiskLevel.Safe,
                    Dependencies = CrosshairBox.IsChecked == true ? new[] { "game-bar-unused" } : Array.Empty<string>()
                },
                new TweakDefinition
                {
                    Id = "gaming.optimize-background-capture",
                    Name = "Reduce background capture activity",
                    Category = "Gaming",
                    Description = "Targets background capture overhead without disabling Game Bar dependencies.",
                    Purpose = "Reduce background activity while preserving overlay tools.",
                    Risk = RiskLevel.Moderate
                },
                new TweakDefinition
                {
                    Id = "system.safe-startup-review",
                    Name = "Review unnecessary startup applications",
                    Category = "Windows",
                    Description = "Identifies startup programs for review instead of blindly disabling them.",
                    Purpose = "Reduce unnecessary startup work.",
                    Risk = RiskLevel.Safe
                }
            };

            var engine = new CompatibilityEngine();
            var selectedIds = tweaks.Select(t => t.Id).ToArray();
            var compatible = tweaks
                .Select(t => (Tweak: t, Result: engine.Evaluate(t, profile, selectedIds)))
                .Where(x => x.Result.IsCompatible)
                .Select(x => x.Tweak)
                .ToList();

            PlanSummary.Text = $"{compatible.Count} recommendations for {GameBox.Text}.";
            PlanItems.Items.Clear();
            foreach (var tweak in compatible)
            {
                PlanItems.Items.Add(new TextBlock
                {
                    Text = $"• {tweak.Name}",
                    Foreground = (System.Windows.Media.Brush)FindResource("FarlaText"),
                    Margin = new Thickness(0, 4, 0, 0)
                });
            }
        }
        catch
        {
            PlanSummary.Text = "We couldn't read the system yet. The plan can be generated after setup.";
            PlanItems.Items.Clear();
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
