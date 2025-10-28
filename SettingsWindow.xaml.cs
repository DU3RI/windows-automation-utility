using System.Windows;

namespace windows_automation_utility;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
    public bool AutoStartApp { get; set; }
    public bool AutoStartMonitoring { get; set; }

    public SettingsWindow(bool autoStartApp, bool autoStartMonitoring)
    {
        InitializeComponent();
        AutoStartAppCheckBox.IsChecked = autoStartApp;
        AutoStartMonitoringCheckBox.IsChecked = autoStartMonitoring;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        AutoStartApp = AutoStartAppCheckBox.IsChecked ?? false;
        AutoStartMonitoring = AutoStartMonitoringCheckBox.IsChecked ?? false;
        DialogResult = true;
        Close();
    }
}