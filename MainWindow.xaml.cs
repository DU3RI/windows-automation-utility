using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace windows_automation_utility;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string selectedApp;
    private ManagementEventWatcher watcher;
    private HttpClient httpClient = new HttpClient();
    private string configFile = "config.json";
    private AppConfig config;

    public class AppConfig
    {
        public string SelectedApp { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }
        public string ApiKey { get; set; }
        public string Headers { get; set; }
        public string Body { get; set; }
        public bool AutoStartApp { get; set; }
        public bool AutoStartMonitoring { get; set; }
    }

    public MainWindow()
    {
        InitializeComponent();
        LoadConfig();
        UpdateAutoStart();
        RefreshAppList();
        if (config.AutoStartMonitoring && !string.IsNullOrEmpty(config.SelectedApp))
        {
            selectedApp = config.SelectedApp;
            LoadApiConfig();
            StartMonitoring();
        }
    }

    private void LoadConfig()
    {
        if (File.Exists(configFile))
        {
            string json = File.ReadAllText(configFile);
            config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        else
        {
            config = new AppConfig();
        }
    }

    private void SaveConfig()
    {
        config.SelectedApp = selectedApp;
        config.Url = UrlTextBox.Text;
        config.Method = ((ComboBoxItem)MethodComboBox.SelectedItem)?.Content.ToString();
        config.ApiKey = ApiKeyTextBox.Text;
        config.Headers = HeadersTextBox.Text;
        config.Body = BodyTextBox.Text;
        string json = JsonSerializer.Serialize(config);
        File.WriteAllText(configFile, json);
    }

    private void LoadApiConfig()
    {
        UrlTextBox.Text = config.Url ?? "https://unifi-controller.local:8443/api/";
        if (config.Method != null)
        {
            foreach (ComboBoxItem item in MethodComboBox.Items)
            {
                if (item.Content.ToString() == config.Method)
                {
                    MethodComboBox.SelectedItem = item;
                    break;
                }
            }
        }
        ApiKeyTextBox.Text = config.ApiKey ?? "";
        HeadersTextBox.Text = config.Headers ?? "Content-Type: application/json";
        BodyTextBox.Text = config.Body ?? "{\"event\": \"app_started\", \"app\": \"{app}\", \"timestamp\": \"{timestamp}\"}";
    }

    private void RefreshAppList()
    {
        AppListBox.Items.Clear();
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                string name = process.ProcessName;
                string title = string.IsNullOrEmpty(process.MainWindowTitle) ? name : $"{name} - {process.MainWindowTitle}";
                AppListBox.Items.Add(title);
            }
            catch
            {
                // Skip processes that can't be accessed
            }
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshAppList();
    }

    private void AppListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AppListBox.SelectedItem != null)
        {
            string selected = AppListBox.SelectedItem.ToString();
            // Extract process name (before " - " if title present)
            int dashIndex = selected.IndexOf(" - ");
            selectedApp = dashIndex > 0 ? selected.Substring(0, dashIndex) : selected;
            CustomExeTextBox.Text = ""; // Clear custom
            StatusTextBlock.Text = $"Selected: {selectedApp}";
        }
    }

    private void CustomExeTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        selectedApp = CustomExeTextBox.Text;
        if (!string.IsNullOrEmpty(selectedApp))
        {
            AppListBox.SelectedIndex = -1; // Deselect list
            StatusTextBlock.Text = $"Selected: {selectedApp}";
        }
    }

    private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow(config.AutoStartApp, config.AutoStartMonitoring);
        if (settingsWindow.ShowDialog() == true)
        {
            config.AutoStartApp = settingsWindow.AutoStartApp;
            config.AutoStartMonitoring = settingsWindow.AutoStartMonitoring;
            SaveConfig();
            UpdateAutoStart();
        }
    }

    private async void TestApiButton_Click(object sender, RoutedEventArgs e)
    {
        await SendApiRequest();
    }

    private async void StartMonitoringButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(selectedApp))
        {
            MessageBox.Show("Please select an application first.");
            return;
        }

        SaveConfig();

        StartMonitoring();
    }

    private void StartMonitoring()
    {
        string exeName = selectedApp.EndsWith(".exe") ? selectedApp : $"{selectedApp}.exe";
        string query = $"SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process' AND TargetInstance.Name = '{exeName}'";
        watcher = new ManagementEventWatcher(new WqlEventQuery(query));
        watcher.EventArrived += OnProcessStarted;
        watcher.Start();

        StartMonitoringButton.IsEnabled = false;
        StopMonitoringButton.IsEnabled = true;
        StatusTextBlock.Text = $"Monitoring started for {selectedApp}";
    }

    private void StopMonitoringButton_Click(object sender, RoutedEventArgs e)
    {
        if (watcher != null)
        {
            watcher.Stop();
            watcher.Dispose();
            watcher = null;
        }

        StartMonitoringButton.IsEnabled = true;
        StopMonitoringButton.IsEnabled = false;
        StatusTextBlock.Text = "Monitoring stopped";
    }

    private async void OnProcessStarted(object sender, EventArrivedEventArgs e)
    {
        Dispatcher.Invoke(async () =>
        {
            StatusTextBlock.Text = $"{selectedApp} started. Sending API request...";
            await SendApiRequest();
            StatusTextBlock.Text = "API request sent.";
        });
    }

    private async Task SendApiRequest()
    {
        try
        {
            string url = UrlTextBox.Text;
            string method = ((ComboBoxItem)MethodComboBox.SelectedItem).Content.ToString();
            string apiKey = ApiKeyTextBox.Text;
            string headers = HeadersTextBox.Text;
            string body = BodyTextBox.Text;

            // Replace placeholders in body
            body = body.Replace("{app}", selectedApp ?? "unknown");
            body = body.Replace("{timestamp}", DateTime.Now.ToString("o"));

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), url);

            // Add API key if provided
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.Headers.Add("Authorization", $"Bearer {apiKey}");
            }

            // Add headers
            foreach (string line in headers.Split('\n'))
            {
                string trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    int colonIndex = trimmed.IndexOf(':');
                    if (colonIndex > 0)
                    {
                        string key = trimmed.Substring(0, colonIndex).Trim();
                        string value = trimmed.Substring(colonIndex + 1).Trim();
                        request.Headers.Add(key, value);
                    }
                }
            }

            if (!string.IsNullOrEmpty(body) && method != "GET")
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            HttpResponseMessage response = await httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            MessageBox.Show($"Response: {response.StatusCode}\n{responseContent}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
        }
    }

    private void UpdateAutoStart()
    {
        string appName = "WindowsAutomationUtility";
        string appPath = Path.Combine(AppContext.BaseDirectory, "windows-automation-utility.exe");

        using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
        {
            if (config.AutoStartApp)
            {
                key.SetValue(appName, appPath);
            }
            else
            {
                key.DeleteValue(appName, false);
            }
        }
    }
}