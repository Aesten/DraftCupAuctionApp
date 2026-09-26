using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;
using AuctionApp.Core.Storage;
using AuctionApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuctionApp.ViewModels;

/// <summary>
/// The window. Works like a document editor: one tournament project is open at a time. The start page and the
/// floating menu list the known projects, and let you create, import or export one.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly TournamentStore _store;
    private readonly IDialogService _dialogs;

    private readonly AppSettings _settings;

    public MainViewModel(TournamentStore store, IDialogService dialogs, AppSettings settings)
    {
        _store = store;
        _dialogs = dialogs;
        _settings = settings;
        RefreshRecent();
    }

    /// <summary>0: light, 1: dark. Starts like Windows; a choice is applied straight away and remembered on this PC.</summary>
    public int ThemeIndex
    {
        get => _settings.IsDark ? 1 : 0;
        set
        {
            var theme = value == 1 ? AppSettings.DarkTheme : AppSettings.LightTheme;
            if (theme == _settings.Theme)
            {
                return;
            }

            _settings.Theme = theme;
            _settings.Save();
            _settings.ApplyTheme();
            OnPropertyChanged();
        }
    }

    /// <summary>Windows switched between light and dark: the toggle follows while no theme has been chosen.</summary>
    internal void OnWindowsThemeChanged() => OnPropertyChanged(nameof(ThemeIndex));

    /// <summary>The tournaments saved on this computer, most recently edited first.</summary>
    public ObservableCollection<TournamentListItem> Recent { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasProject))]
    public partial TournamentViewModel? Current { get; set; }

    /// <summary>The floating menu (projects, import, export) that slides over the page.</summary>
    [ObservableProperty]
    public partial bool IsMenuOpen { get; set; }

    public bool HasProject => Current != null;

    public bool HasRecent => Recent.Count > 0;

    public string DataFolder => _store.RootDirectory;

    partial void OnCurrentChanging(TournamentViewModel? oldValue, TournamentViewModel? newValue) => oldValue?.Close();

    public void RefreshRecent()
    {
        Recent.Clear();
        foreach (var summary in _store.List())
        {
            Recent.Add(new TournamentListItem(summary) { IsOpen = summary.Id == Current?.Id });
        }

        OnPropertyChanged(nameof(HasRecent));
    }

    partial void OnIsMenuOpenChanged(bool value)
    {
        if (value)
        {
            Current?.SaveNow();
            Current?.RefreshMenu();
            RefreshRecent();
        }
    }

    [RelayCommand]
    private void ToggleMenu() => IsMenuOpen = !IsMenuOpen;

    [RelayCommand]
    private void OpenRecent(TournamentListItem item) => Open(item.Id);

    [RelayCommand]
    private void NewTournament()
    {
        var format = _dialogs.Ask(
            "New tournament",
            "How do players come up for auction?\n\n"
            + "• Random Pick: players come up one by one in a random order. Nobody bids → the player is skipped.\n\n"
            + "• Captain Pick: players are sorted in tiers. A captain names the player they want, and bidding starts at "
            + "that player's tier minimum.\n\n"
            + "You can switch from the menu until an auction starts.",
            "Random Pick",
            "Captain Pick");
        if (format == DialogChoice.Cancel)
        {
            return;
        }

        var tournament = new Tournament
        {
            Title = $"Draft Cup {DateTime.Now:MMMM yyyy}",
            Format = format == DialogChoice.Secondary ? AuctionFormat.CaptainPick : AuctionFormat.RandomPick,
        };
        tournament.AddDivision();
        if (TrySave(tournament))
        {
            Open(tournament.Id);
        }
    }

    [RelayCommand]
    private void Import()
    {
        var path = _dialogs.PickFileToOpen(
            "Import a tournament",
            $"Draft Cup tournaments (*{TournamentJson.FileExtension})|*{TournamentJson.FileExtension}|JSON files (*.json)|*.json|All files (*.*)|*.*");
        if (path != null)
        {
            ImportFile(path);
        }
    }

    [RelayCommand]
    private void CloseTournament()
    {
        Current = null;
        IsMenuOpen = false;
        RefreshRecent();
    }

    [RelayCommand]
    private void OpenDataFolder() => _dialogs.OpenFolder(_store.RootDirectory);

    /// <summary>
    /// Imports a tournament file (from the Import button, a file dropped on the window or "Open with"). A copy of a
    /// tournament that is already on this computer is merged into it, so results auctioned elsewhere come back in.
    /// </summary>
    public void ImportFile(string path)
    {
        IsMenuOpen = false;
        Tournament incoming;
        try
        {
            incoming = TournamentImporter.Import(FileLimits.ReadText(path), TitleFromFileName(path));
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or JsonException or UnauthorizedAccessException)
        {
            _dialogs.ShowError("Couldn't import this file", ex.Message);
            return;
        }

        if (!_store.Exists(incoming.Id))
        {
            incoming.UpdatedAt = DateTimeOffset.Now;
            if (TrySave(incoming))
            {
                Open(incoming.Id);
            }

            return;
        }

        Current?.SaveNow();
        var existing = _store.Load(incoming.Id);
        var merge = TournamentMerger.Merge(existing, incoming);
        var answer = merge.HasChanges
            ? _dialogs.Ask(
                $"Update \"{existing.Title}\"?",
                "This file is a copy of a tournament you already have. Whichever copy changed each part last wins:\n\n"
                + string.Join("\n", merge.Changes.Select(change => "• " + change)),
                "Update",
                "Import as a separate copy")
            : _dialogs.Ask(
                "Already up to date",
                $"\"{existing.Title}\" already contains everything in this file.",
                "Open it",
                "Import as a separate copy",
                cancel: null);

        switch (answer)
        {
            case DialogChoice.Primary when merge.HasChanges:
                _store.SaveCopyAside(existing, "before-import");
                if (TrySave(merge.Tournament))
                {
                    Open(merge.Tournament.Id, reload: true);
                    WarnAboutDoublePicks(merge.Tournament);
                }

                break;
            case DialogChoice.Primary:
                Open(existing.Id);
                break;
            case DialogChoice.Secondary:
                incoming.Id = Guid.NewGuid();
                incoming.Title += " (copy)";
                if (TrySave(incoming))
                {
                    Open(incoming.Id);
                }

                break;
        }
    }

    /// <summary>Opens a tournament; <paramref name="reload"/> re-reads it from disk even if it is already open.</summary>
    internal void Open(Guid id, bool reload = false)
    {
        IsMenuOpen = false;
        if (Current?.Id == id && !reload)
        {
            return;
        }

        Current = null;
        try
        {
            Current = new TournamentViewModel(_store.Load(id), _store, _dialogs, this);
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            _dialogs.ShowError("Couldn't open this tournament", ex.Message);
        }

        RefreshRecent();
    }

    internal void OnDeleted()
    {
        Current = null;
        RefreshRecent();
    }

    /// <summary>Called when the window closes, so the last edits are written.</summary>
    public void Shutdown() => Current?.SaveNow();

    internal bool TrySave(Tournament tournament)
    {
        try
        {
            _store.Save(tournament);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _dialogs.ShowError("Couldn't save the tournament", ex.Message);
            return false;
        }
    }

    private void WarnAboutDoublePicks(Tournament tournament)
    {
        var doubles = TournamentRules.DoublePicks(tournament);
        if (doubles.Count > 0)
        {
            _dialogs.ShowError(
                "Some players were bought twice",
                "These players were bought in more than one division (probably auctioned at the same time on different computers):\n\n"
                + string.Join("\n", doubles.Select(entry => $"• {entry.PlayerName}: {string.Join(", ", entry.Divisions.Select(d => d.Name))}"))
                + "\n\nTake them back from one of the teams to fix it.");
        }
    }

    private static string TitleFromFileName(string path)
    {
        var name = Path.GetFileName(path);
        foreach (var extension in new[] { TournamentJson.FileExtension, ".json" })
        {
            if (name.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                return name[..^extension.Length];
            }
        }

        return name;
    }
}

public sealed partial class TournamentListItem(TournamentSummary summary) : ObservableObject
{
    public Guid Id { get; } = summary.Id;

    public string Title { get; } = string.IsNullOrWhiteSpace(summary.Title) ? "Untitled tournament" : summary.Title;

    public string Details { get; } = Describe(summary);

    public string EditedText { get; } = "Edited " + Relative(summary.UpdatedAt);

    public bool IsLive { get; } = summary.DivisionsInProgress > 0;

    [ObservableProperty]
    public partial bool IsOpen { get; set; }

    private static string Describe(TournamentSummary summary)
    {
        var divisions = summary.DivisionCount == 1 ? "1 division" : $"{summary.DivisionCount} divisions";
        var format = summary.Format == AuctionFormat.CaptainPick ? "Captain Pick · " : string.Empty;
        var state = summary.DivisionsInProgress > 0
            ? " · auction in progress"
            : summary.DivisionsFinished == summary.DivisionCount && summary.DivisionCount > 0 ? " · done" : string.Empty;
        return $"{format}{summary.PlayerCount} players · {divisions}{state}";
    }

    private static string Relative(DateTimeOffset time)
    {
        var elapsed = DateTimeOffset.Now - time;
        return elapsed.TotalMinutes switch
        {
            < 1 => "just now",
            < 60 => $"{(int)elapsed.TotalMinutes} min ago",
            < 60 * 24 => $"{(int)elapsed.TotalHours} h ago",
            < 60 * 24 * 7 => time.LocalDateTime.ToString("dddd HH:mm"),
            _ => time.LocalDateTime.ToString("d MMM yyyy"),
        };
    }
}
