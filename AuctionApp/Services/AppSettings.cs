using System.IO;
using System.Text.Json;
using System.Windows;

namespace AuctionApp.Services;

/// <summary>Preferences of this PC (not of a tournament), saved as settings.json in the app's data folder.</summary>
public sealed class AppSettings
{
    public const string SystemTheme = "System";
    public const string LightTheme = "Light";
    public const string DarkTheme = "Dark";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private string _path = string.Empty;

    /// <summary>"System" (follow Windows), "Light" or "Dark".</summary>
    public string Theme { get; set; } = SystemTheme;

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
        if (settings.Theme is not (SystemTheme or LightTheme or DarkTheme))
        {
            settings.Theme = SystemTheme;
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

    /// <summary>Switches the whole app to the theme, live (WPF's built-in Fluent theme in light or dark).</summary>
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
}
