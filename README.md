# Windows Automation Utility

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Windows](https://img.shields.io/badge/platform-Windows-lightgrey.svg)](https://www.microsoft.com/windows)

A modern WPF application for Windows that monitors application startup events and automatically sends configurable API requests. Perfect for integrating Windows events with external services like home automation systems, monitoring tools, or webhooks.

## Features

- **Application Monitoring**: Select from running processes or specify custom EXE paths
- **API Integration**: Configure HTTP requests with custom URLs, methods, headers, and bodies
- **Authentication Support**: Built-in API key/token support for authenticated endpoints
- **Flexible Configuration**: Dynamic placeholders for app name and timestamp in requests
- **Modern UI**: Clean, intuitive WPF interface with dark/light theme support
- **Auto-Start**: Configure automatic startup with Windows and monitoring on launch
- **Persistent Settings**: Saves configuration to JSON for consistent behavior
- **API Testing**: Built-in test functionality to validate configurations

## Requirements

- **OS**: Windows 10/11
- **Framework**: .NET 8.0 or later
- **Permissions**: Administrative privileges recommended for WMI monitoring

## Installation & Setup

### Option 1: Download Release
1. Download the latest release from the [Releases](https://github.com/DU3RI/windows-automation-utility/releases) page
2. Extract and run `windows-automation-utility.exe`

### Option 2: Build from Source
```bash
# Clone the repository
git clone https://github.com/yourusername/windows-automation-utility.git
cd windows-automation-utility

# Restore dependencies
dotnet restore

# Build the project
dotnet build --configuration Release

# Run the application
dotnet run
```

## Usage

### Basic Setup
1. **Launch** the application
2. **Select Application**: Choose from running processes or enter a custom EXE path
3. **Configure API**:
   - **API URL**: Enter your endpoint (e.g., `https://api.example.com/webhook`)
   - **HTTP Method**: Select GET, POST, PUT, or DELETE
   - **API Key**: Enter authentication token (optional)
   - **Headers**: Add custom headers (Content-Type pre-filled)
   - **Request Body**: JSON payload with placeholders

### Placeholders
Use these placeholders in your request body:
- `{app}` - Application name that started
- `{timestamp}` - ISO 8601 timestamp of the event

Example JSON body:
```json
{
  "event": "app_started",
  "application": "{app}",
  "timestamp": "{timestamp}",
  "source": "windows-automation-utility"
}
```

### Monitoring
- Click **"Test API"** to validate your configuration
- Click **"Start Monitoring"** to begin watching
- The app will automatically send API requests when the selected application starts

### Settings
Access the **Settings** menu to configure:
- Auto-start the utility with Windows
- Auto-start monitoring when the app launches

## Configuration

Settings are automatically saved to `config.json` in the application directory. The file includes:
- Selected application/process
- API configuration (URL, method, headers, body)
- Auto-start preferences

## Architecture

- **UI Framework**: WPF with XAML
- **Monitoring**: Windows Management Instrumentation (WMI)
- **HTTP Client**: System.Net.Http with async support
- **Serialization**: System.Text.Json
- **Registry Integration**: Microsoft.Win32 for auto-start

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Important Notes

- **Administrative Privileges**: WMI monitoring may require admin rights
- **Process Monitoring**: Monitors for new process instances by name
- **Security**: API keys are stored locally - ensure proper file permissions
- **Compatibility**: Designed for Windows 10/11 with .NET 8.0+

## Troubleshooting

**Monitoring not working?**
- Ensure you're running as administrator
- Check Windows Event Viewer for WMI errors
- Verify the process name matches exactly

**API requests failing?**
- Test with "Test API" button first
- Check firewall settings
- Verify API endpoint is accessible

**Application won't start?**
- Ensure .NET 8.0 runtime is installed
- Check antivirus exclusions
- Run as administrator
