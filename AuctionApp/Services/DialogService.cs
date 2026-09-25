using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using AuctionApp.Core.Storage;
using AuctionApp.Views;
using Microsoft.Win32;

namespace AuctionApp.Services;

public enum DialogChoice
{
    Primary,
    Secondary,
    Cancel,
}

/// <summary>A player offered in <see cref="IDialogService.PickPlayer"/>; <paramref name="Where"/> says where they are now.</summary>
/// <summary>What a file is imported as: a whole tournament (or a new one from a player list), or players for the pool.</summary>
public enum ImportKind
{
    Tournament,
    Players,
}

public sealed record PlayerChoice(Guid Id, string Name, IReadOnlyList<string> Classes, string Where);

/// <summary>Everything that talks to the user outside of the main window: dialogs, file pickers, clipboard.</summary>
public interface IDialogService
{
    DialogChoice Ask(string title, string message, string primary, string? secondary = null, string? cancel = "Cancel");

    void ShowError(string title, string message);

    /// <summary>Asks for a price (steps of 0.1). Returns null when cancelled.</summary>
    decimal? AskPrice(string title, string message, decimal initial);

    /// <summary>Lets the user pick one player from a searchable list. Returns null when cancelled.</summary>
    Guid? PickPlayer(string title, string message, IReadOnlyList<PlayerChoice> players);

    string? PickFileToOpen(string title, string filter);

    string? PickFileToSave(string title, string filter, string suggestedName);

    /// <summary>Shows the expected format, then the file picker. Returns the chosen file, or null.</summary>
    string? PickImportFile(ImportKind kind);

    bool CopyToClipboard(string text);

    void OpenFolder(string path);

    /// <summary>Writes a text file (UTF-8 with BOM so Excel reads accents correctly). Returns false and tells the user on failure.</summary>
    bool TryWriteFile(string path, string contents);
}

public sealed class DialogService : IDialogService
{
    private static Window? Owner =>
        Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
        ?? (Application.Current?.MainWindow is { IsVisible: true } main ? main : null);

    public DialogChoice Ask(string title, string message, string primary, string? secondary = null, string? cancel = "Cancel")
    {
        var dialog = new MessageDialog(title, message, primary, secondary, cancel) { Owner = Owner };
        dialog.ShowDialog();
        return dialog.Choice;
    }

    public void ShowError(string title, string message) => Ask(title, message, "OK", cancel: null);

    public decimal? AskPrice(string title, string message, decimal initial)
    {
        var dialog = new PriceDialog(title, message, initial) { Owner = Owner };
        return dialog.ShowDialog() == true ? dialog.Price : null;
    }

    public Guid? PickPlayer(string title, string message, IReadOnlyList<PlayerChoice> players)
    {
        var dialog = new PlayerPickerDialog(title, message, players) { Owner = Owner };
        return dialog.ShowDialog() == true ? dialog.Picked?.Id : null;
    }

    public string? PickFileToOpen(string title, string filter)
    {
        var dialog = new OpenFileDialog { Title = title, Filter = filter, CheckFileExists = true };
        return dialog.ShowDialog(Owner) == true ? dialog.FileName : null;
    }

    public string? PickFileToSave(string title, string filter, string suggestedName)
    {
        var dialog = new SaveFileDialog { Title = title, Filter = filter, FileName = SafeFileName(suggestedName), AddExtension = true };
        return dialog.ShowDialog(Owner) == true ? dialog.FileName : null;
    }

    public string? PickImportFile(ImportKind kind)
    {
        if (new ImportDialog(kind) { Owner = Owner }.ShowDialog() != true)
        {
            return null;
        }

        return kind == ImportKind.Players
            ? PickFileToOpen("Import players", "Player lists (*.csv;*.txt)|*.csv;*.txt|Tournaments (*.json)|*.json|All files (*.*)|*.*")
            : PickFileToOpen("Import a tournament", "Tournaments and player lists (*.json;*.csv;*.txt)|*.json;*.csv;*.txt|All files (*.*)|*.*");
    }

    public bool CopyToClipboard(string text)
    {
        // The clipboard can be briefly locked by another application.
        for (var attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                Clipboard.SetText(text);
                return true;
            }
            catch (COMException)
            {
                Thread.Sleep(50);
            }
        }

        ShowError("Couldn't copy", "Another application is using the clipboard. Please try again.");
        return false;
    }

    public void OpenFolder(string path) => Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });

    public bool TryWriteFile(string path, string contents)
    {
        try
        {
            File.WriteAllText(path, contents, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ShowError("Couldn't save the file", ex.Message);
            return false;
        }
    }

    private static string SafeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
        return cleaned.Length > 0 ? cleaned : "draft";
    }
}
