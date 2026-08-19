using System.Windows;
using System.Windows.Input;

namespace FarlaTweaks.App;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void OpenWizardButton_OnClick(object sender, RoutedEventArgs e)
    {
        var wizard = new SetupWizard { Owner = this };
        if (wizard.ShowDialog() == true)
            DialogResult = true;
    }

    private void OpenDiagnosticsButton_OnClick(object sender, RoutedEventArgs e)
    {
        var diagnostics = new DiagnosticsWindow { Owner = this };
        diagnostics.ShowDialog();
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
