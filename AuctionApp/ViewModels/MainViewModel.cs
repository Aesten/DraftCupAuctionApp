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

/// <summary>The window: the library of tournaments in the sidebar and the tournament that is open.</summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly TournamentStore _store;
    private readonly IDialogService _dialogs;

    public MainViewModel(TournamentStore store, IDialogService dialogs)
    {
        _store = store;
        _dialogs = dialogs;
        RefreshLibrary();
        SelectedItem = Library.FirstOrDefault();
    }

    public ObservableCollection<TournamentListItem> Library { get; } = [];

    [ObservableProperty]
    public partial TournamentListItem? SelectedItem { get; set; }

    [ObservableProperty]
    public partial TournamentViewModel? Current { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowSidebar), nameof(ShowSidebarButton))]
    public partial bool IsSidebarOpen { get; set; } = true;

    /// <summary>Full screen with only the page content visible (no sidebar, no tabs), for streams and projectors.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowSidebar), nameof(ShowSidebarButton))]
    public partial bool IsStreamMode { get; set; }

    public bool ShowSidebar => IsSidebarOpen && !IsStreamMode;

    public bool ShowSidebarButton => !IsSidebarOpen && !IsStreamMode;

    public bool IsLibraryEmpty => Library.Count == 0;

    public string DataFolder => _store.RootDirectory;

    partial void OnSelectedItemChanged(TournamentListItem? value)
    {
        if (Current?.Id == value?.Id)
        {
            return;
        }

        Current = null;
        if (value != null)
        {
            Load(value);
        }
    }

    partial void OnCurrentChanging(TournamentViewModel? oldValue, TournamentViewModel? newValue) => oldValue?.Close();

    private void Load(TournamentListItem item)
    {
        try
        {
            Current = new TournamentViewModel(_store.Load(item.Id), _store, _dialogs, this);
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            _dialogs.ShowError("Couldn't open this tournament", ex.Message);
        }
    }

    /// <summary>Re-reads the library, updating the existing items in place so the selection is kept.</summary>
    public void RefreshLibrary()
    {
        var summaries = _store.List();
        foreach (var gone in Library.Where(item => summaries.All(summary => summary.Id != item.Id)).ToList())
        {
            Library.Remove(gone);
        }

        for (var i = 0; i < summaries.Count; i++)
        {
            var existing = Library.FirstOrDefault(item => item.Id == summaries[i].Id);
            if (existing == null)
            {
                Library.Insert(i, new TournamentListItem(summaries[i]));
                continue;
            }

            existing.Update(summaries[i]);
            var index = Library.IndexOf(existing);
            if (index != i)
            {
                Library.Move(index, i);
            }
        }

        OnPropertyChanged(nameof(IsLibraryEmpty));
    }

    /// <summary>Called by the open tournament when its title or progress changes, to update the sidebar.</summary>
    internal void UpdateListItem(Tournament tournament)
    {
        var item = Library.FirstOrDefault(entry => entry.Id == tournament.Id);
        item?.Update(TournamentStore.Summarize(tournament));
    }

    [RelayCommand]
    private void NewTournament()
    {
        var tournament = new Tournament { Title = $"Draft Cup {DateTime.Now:MMMM yyyy}" };
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
            "Tournament files (*.json)|*.json|All files (*.*)|*.*");
        if (path != null)
        {
            ImportFile(path);
        }
    }

    /// <summary>
    /// Imports a tournament file (from the Import button, a file dropped on the window or "Open with"). A copy of a
    /// tournament that is already in the library is merged into it, so results auctioned elsewhere come back in.
    /// </summary>
    public void ImportFile(string path)
    {
        Tournament incoming;
        try
        {
            incoming = TournamentImporter.Import(File.ReadAllText(path), TitleFromFileName(path));
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

        if (Current?.Id == incoming.Id)
        {
            Current.SaveNow();
        }

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
                "OK",
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

    [RelayCommand]
    private void OpenDataFolder() => _dialogs.OpenFolder(_store.RootDirectory);

    [RelayCommand]
    private void ToggleSidebar() => IsSidebarOpen = !IsSidebarOpen;

    /// <summary>Selects a tournament in the sidebar and opens it; <paramref name="reload"/> re-reads it from disk.</summary>
    internal void Open(Guid id, bool reload = false)
    {
        RefreshLibrary();
        var item = Library.FirstOrDefault(entry => entry.Id == id);
        if (item == null)
        {
            return;
        }

        if (SelectedItem == item)
        {
            if (reload || Current == null)
            {
                Current = null;
                Load(item);
            }
        }
        else
        {
            SelectedItem = item;
        }
    }

    internal void OnDeleted()
    {
        Current = null;
        SelectedItem = null;
        RefreshLibrary();
        SelectedItem = Library.FirstOrDefault();
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

    [ObservableProperty]
    public partial string Title { get; set; } = Display(summary.Title);

    [ObservableProperty]
    public partial string Details { get; set; } = Describe(summary);

    internal void Update(TournamentSummary updated)
    {
        Title = Display(updated.Title);
        Details = Describe(updated);
    }

    private static string Display(string title) => string.IsNullOrWhiteSpace(title) ? "Untitled tournament" : title;

    private static string Describe(TournamentSummary summary)
    {
        var divisions = summary.DivisionCount == 1 ? "1 division" : $"{summary.DivisionCount} divisions";
        var state = summary.DivisionsInProgress > 0 ? " · live" : summary.DivisionsFinished == summary.DivisionCount && summary.DivisionCount > 0 ? " · done" : string.Empty;
        return $"{summary.PlayerCount} players · {divisions}{state}";
    }
}
