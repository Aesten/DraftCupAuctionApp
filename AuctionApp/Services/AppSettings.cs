using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using Microsoft.Win32;

namespace AuctionApp.Services;

/// <summary>Preferences of this PC (not of a tournament), saved as settings.json in the app's data folder.</summary>
public sealed class AppSettings
{
    public const string LightTheme = "Light";
    public const string DarkTheme = "Dark";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private string _path = string.Empty;

    /// <summary>"Light" or "Dark" once chosen; empty until then, meaning: like Windows.</summary>
    public string Theme { get; set; } = string.Empty;

    /// <summary>The theme in use: the one chosen, or the one Windows uses for apps.</summary>
    [JsonIgnore]
    public bool IsDark => Theme == DarkTheme || Theme.Length == 0 && !WindowsUsesLightTheme();

    public static AppSettings Load(string folder)
    {
        var path = Path.Combine(folder, "settings.json");
        AppSettings settings;
        try
        {
            settings = File.Exists(path) ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), JsonOptions) ?? new() : new();
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            settings = new();
        }

        settings._path = path;
        if (settings.Theme is not (LightTheme or DarkTheme))
        {
            settings.Theme = string.Empty;
        }

        return settings;
    }

    public void Save()
    {
        try
        {
            File.WriteAllText(_path, JsonSerializer.Serialize(this, JsonOptions));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // A preference that couldn't be saved only means choosing it again next time.
        }
    }

    /// <summary>
    /// Switches the whole app to the theme, live (WPF's built-in Fluent theme in light or dark). Until a theme is
    /// chosen, the app follows Windows, including when Windows switches while it's open.
    /// </summary>
    public void ApplyTheme()
    {
        if (Application.Current is not { } app)
        {
            return;
        }

#pragma warning disable WPF0001 // Setting ThemeMode from code is marked experimental in .NET 10; it's the supported way to switch light/dark.
        app.ThemeMode = Theme switch
        {
            LightTheme => ThemeMode.Light,
            DarkTheme => ThemeMode.Dark,
            _ => ThemeMode.System,
        };
#pragma warning restore WPF0001
    }

    /// <summary>
    /// Windows' "app mode" (Settings › Personalization › Colors), read the same way WPF does for ThemeMode.System:
    /// the apps setting, else the system one, else dark.
    /// </summary>
    public static bool WindowsUsesLightTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme") as int? ?? key?.GetValue("SystemUsesLightTheme") as int?;
            return value is not null and not 0;
        }
        catch (Exception ex) when (ex is System.Security.SecurityException or IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
