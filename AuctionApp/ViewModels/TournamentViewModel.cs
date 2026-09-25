using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Threading;
using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;
using AuctionApp.Core.Storage;
using AuctionApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuctionApp.ViewModels;

/// <summary>
/// An open tournament: the player pool tab and one tab per division. Takes care of saving automatically and of
/// keeping the time stamps that let copies from other computers be merged.
/// </summary>
public sealed partial class TournamentViewModel : ObservableObject
{
    private readonly MainViewModel _main;
    private readonly DispatcherTimer _saveTimer;
    private bool _dirty;
    private bool _closed;

    public TournamentViewModel(Tournament tournament, TournamentStore store, IDialogService dialogs, MainViewModel main)
    {
        Tournament = tournament;
        Store = store;
        Dialogs = dialogs;
        _main = main;
        _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
        _saveTimer.Tick += (_, _) => SaveNow();

        // A merged copy may have sold a player another auction here still has in its queue.
        if (TournamentRules.DropPlayersTakenElsewhere(tournament).Count > 0)
        {
            _dirty = true;
        }

        Pool = new PoolViewModel(this);
        Tabs.Add(Pool);
        foreach (var division in tournament.Divisions)
        {
            var page = new DivisionViewModel(division, this);
            Divisions.Add(page);
            Tabs.Add(page);
        }

        // Open where the action is: a running auction, else the first division that isn't done, else the pool.
        SelectedTab = Divisions.FirstOrDefault(d => d.Division.Status == DivisionStatus.InProgress)
            ?? (object?)Divisions.FirstOrDefault(d => d.Division.Status == DivisionStatus.NotStarted && tournament.Players.Count > 0)
            ?? Pool;
    }

    public Guid Id => Tournament.Id;

    public Tournament Tournament { get; }

    public TournamentStore Store { get; }

    public IDialogService Dialogs { get; }

    public MainViewModel Main => _main;

    public PoolViewModel Pool { get; }

    public ObservableCollection<DivisionViewModel> Divisions { get; } = [];

    /// <summary>The pool followed by the divisions, as shown in the tab strip.</summary>
    public ObservableCollection<object> Tabs { get; } = [];

    [ObservableProperty]
    public partial object? SelectedTab { get; set; }

    /// <summary>Set when the last save failed (disk full, folder not writable...). Shown as a banner.</summary>
    [ObservableProperty]
    public partial string? SaveError { get; set; }

    public string Title
    {
        get => Tournament.Title;
        set
        {
            if (Tournament.Title == value)
            {
                return;
            }

            Tournament.Title = value;
            OnPropertyChanged();
            PoolChanged();
        }
    }

    public string Summary
    {
        get
        {
            var divisions = Tournament.Divisions.Count == 1 ? "1 division" : $"{Tournament.Divisions.Count} divisions";
            return $"{Tournament.Players.Count} players in the pool · {divisions}";
        }
    }

    // Change tracking

    /// <summary>The title or the pool changed.</summary>
    internal void PoolChanged()
    {
        Tournament.TouchPool();
        MarkDirty();
        foreach (var division in Divisions)
        {
            division.OnPoolChanged();
        }

        OnPropertyChanged(nameof(Summary));
    }

    /// <summary>A division's settings changed (saved shortly after, to not write on every key press).</summary>
    internal void DivisionChanged(Division division)
    {
        division.Touch();
        Tournament.UpdatedAt = division.UpdatedAt;
        MarkDirty();
    }

    /// <summary>
    /// A division's auction changed. Saved right away so nothing is lost if the PC crashes, and the pool and the
    /// other divisions are told, since who is still available may have changed.
    /// </summary>
    internal void AuctionChanged(Division division)
    {
        division.Touch();
        Tournament.UpdatedAt = division.UpdatedAt;
        DropPlayersTakenElsewhere();
        _dirty = true;
        SaveNow();
        Pool.RefreshStatuses();
        foreach (var other in Divisions)
        {
            other.OnPoolChanged();
        }
    }

    /// <summary>Keeps running auctions from selling a player another division already bought.</summary>
    private void DropPlayersTakenElsewhere()
    {
        foreach (var (division, _) in TournamentRules.DropPlayersTakenElsewhere(Tournament))
        {
            division.Touch();
            Divisions.FirstOrDefault(page => page.Division == division)?.Auction.Reload();
        }
    }

    private void MarkDirty()
    {
        if (_closed)
        {
            return;
        }

        _dirty = true;
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    public void SaveNow()
    {
        _saveTimer.Stop();
        if (!_dirty || _closed)
        {
            return;
        }

        try
        {
            Store.Save(Tournament);
            _dirty = false;
            SaveError = null;
            _main.UpdateListItem(Tournament);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            SaveError = $"Your changes couldn't be saved: {ex.Message}";
        }
    }

    /// <summary>Saves pending changes and stops saving (the tournament was closed or replaced).</summary>
    internal void Close()
    {
        SaveNow();
        _closed = true;
    }

    // Divisions

    [RelayCommand]
    private void AddDivision()
    {
        var division = Tournament.AddDivision();
        var previous = Tournament.Divisions.Count > 1 ? Tournament.Divisions[^2] : null;
        if (previous != null)
        {
            division.TeamSize = previous.TeamSize;
            division.ShuffleOrder = previous.ShuffleOrder;
            division.UpcomingShown = previous.UpcomingShown;
            division.HalfBudgetCapAtStart = previous.HalfBudgetCapAtStart;
        }

        var page = new DivisionViewModel(division, this);
        Divisions.Add(page);
        Tabs.Add(page);
        SelectedTab = page;
        DivisionChanged(division);
        OnPropertyChanged(nameof(Summary));
    }

    internal void RemoveDivision(DivisionViewModel page)
    {
        var hasResults = page.Division.Session != null;
        var message = hasResults
            ? $"Its auction and every sale in it will be removed, and its players become available to the other divisions again. A copy of the tournament as it is now is kept in the app's data folder (Deleted)."
            : "Its captains and settings will be removed.";
        if (Dialogs.Ask($"Delete {page.Division.Name}?", message, "Delete division") != DialogChoice.Primary)
        {
            return;
        }

        if (hasResults)
        {
            TryKeepCopy("before-delete-division");
        }

        var index = Tabs.IndexOf(page);
        Tournament.Divisions.Remove(page.Division);
        Divisions.Remove(page);
        Tabs.Remove(page);
        SelectedTab = Tabs[Math.Clamp(index - 1, 0, Tabs.Count - 1)];
        Tournament.UpdatedAt = DateTimeOffset.Now;
        _dirty = true;
        SaveNow();
        Pool.RefreshStatuses();
        foreach (var division in Divisions)
        {
            division.OnPoolChanged();
        }

        OnPropertyChanged(nameof(Summary));
    }

    internal void ShowPool() => SelectedTab = Pool;

    /// <summary>Keeps a copy of the tournament in the Deleted folder; asks whether to go on if that fails.</summary>
    internal bool TryKeepCopy(string reason)
    {
        try
        {
            Store.SaveCopyAside(Tournament, reason);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Dialogs.Ask("Couldn't keep a copy", ex.Message + "\n\nContinue anyway?", "Continue") == DialogChoice.Primary;
        }
    }

    // Tournament actions

    [RelayCommand]
    private void Export()
    {
        SaveNow();
        var path = Dialogs.PickFileToSave(
            "Export the tournament",
            $"Tournament file (*{TournamentJson.FileExtension})|*{TournamentJson.FileExtension}",
            Tournament.Title + TournamentJson.FileExtension);
        if (path == null)
        {
            return;
        }

        if (!path.EndsWith(TournamentJson.FileExtension, StringComparison.OrdinalIgnoreCase))
        {
            path = Path.ChangeExtension(path, null) + TournamentJson.FileExtension;
        }

        if (Dialogs.TryWriteFile(path, TournamentExporter.ToFile(Tournament)))
        {
            Dialogs.Ask(
                "Tournament exported",
                "Send this file to whoever runs the next auction. When they send it back, import it here: the results are merged into this tournament.",
                "OK",
                cancel: null);
        }
    }

    [RelayCommand]
    private void Duplicate()
    {
        var copy = Tournament.CloneWithoutResults($"{Tournament.Title} (copy)");
        if (_main.TrySave(copy))
        {
            _main.Open(copy.Id);
        }
    }

    [RelayCommand]
    private void Delete()
    {
        var answer = Dialogs.Ask(
            $"Delete \"{Tournament.Title}\"?",
            "It will be removed from the list. A copy is kept in the app's data folder (Deleted) in case you change your mind.",
            "Delete");
        if (answer != DialogChoice.Primary)
        {
            return;
        }

        Close();
        Store.Delete(Tournament.Id);
        _main.OnDeleted();
    }
}
