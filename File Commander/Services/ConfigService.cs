using System.Text.Json;
using File_Commander.Models;

namespace File_Commander.Services;

/// <summary>
/// Service for managing application configuration
/// </summary>
public class ConfigService
{
    private readonly string _configFilePath;
    private AppSettings _settings;

    public event EventHandler<AppSettings>? SettingsChanged;

    public AppSettings Settings => _settings;

    public ConfigService()
    {
        // Store config in user's home directory
        var configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fcom");
        Directory.CreateDirectory(configDir);
        _configFilePath = Path.Combine(configDir, "config.json");

        _settings = LoadSettings();
    }

    /// <summary>
    /// Loads settings from disk or creates defaults
    /// </summary>
    private AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                var json = File.ReadAllText(_configFilePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings: {ex.Message}");
        }

        return new AppSettings();
    }

    /// <summary>
    /// Saves current settings to disk
    /// </summary>
    public void SaveSettings()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(_settings, options);
            File.WriteAllText(_configFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates settings and notifies listeners
    /// </summary>
    public void UpdateSettings(AppSettings newSettings)
    {
        _settings = newSettings;
        SaveSettings();
        SettingsChanged?.Invoke(this, _settings);
    }
}

