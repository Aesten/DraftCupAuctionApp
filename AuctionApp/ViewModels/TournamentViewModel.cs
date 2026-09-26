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
        if (TournamentRules.DropPlayersTakenElsewhere(tournament).Count > 0 | TournamentRules.AddNewPlayersToRunningAuctions(tournament) > 0)
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
        OnSelectedTabChanged(SelectedTab);
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
    [NotifyPropertyChangedFor(nameof(SelectedDivision))]
    public partial object? SelectedTab { get; set; }

    /// <summary>The selected division, if a division tab is selected (its page switch shows in the top bar).</summary>
    public DivisionViewModel? SelectedDivision => SelectedTab as DivisionViewModel;

    partial void OnSelectedTabChanged(object? value)
    {
        // The pool is shown sorted by name by default, so opening it mid-auction doesn't reveal the upcoming order.
        if (value is PoolViewModel pool)
        {
            pool.OnShown();
        }
    }

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
            return $"{FormatName} · {Tournament.Players.Count} players · {divisions}";
        }
    }

    public bool IsCaptainPick => Tournament.IsCaptainPick;

    public string FormatName => IsCaptainPick ? "Captain Pick" : "Random Pick";

    /// <summary>The format can be switched until an auction starts.</summary>
    public bool CanChangeFormat => Tournament.CanChangeFormat;

    public string SwitchFormatText => IsCaptainPick ? "Switch to Random Pick…" : "Switch to Captain Pick…";

    /// <summary>The menu opened: what it shows about the tournament may have changed (an auction started...).</summary>
    internal void RefreshMenu()
    {
        OnPropertyChanged(nameof(CanChangeFormat));
        OnPropertyChanged(nameof(Summary));
    }

    [RelayCommand]
    private void SwitchFormat()
    {
        if (!CanChangeFormat)
        {
            return;
        }

        var toCaptainPick = !IsCaptainPick;
        var multiClass = Tournament.Players.Count(player => player.Classes.Count > 1);
        var message = toCaptainPick
            ? "Captains will name the player they want, and bidding starts at the minimum of the player's tier. "
              + "Give every player a tier (1 to 5) in the player pool."
              + (multiClass > 0 ? $"\n\nIn Captain Pick a player has one class: {multiClass} player(s) with several classes keep only the first one." : string.Empty)
            : "Players will come up one by one in a random order. Their tiers are kept in case you switch back.";
        var answer = Dialogs.Ask(
            toCaptainPick ? "Switch to Captain Pick?" : "Switch to Random Pick?",
            message,
            toCaptainPick ? "Switch to Captain Pick" : "Switch to Random Pick");
        if (answer != DialogChoice.Primary)
        {
            return;
        }

        _main.IsMenuOpen = false;
        Tournament.SetFormat(toCaptainPick ? AuctionFormat.CaptainPick : AuctionFormat.RandomPick);
        _dirty = true;
        SaveNow();

        // Every page shows the format differently: reopen the tournament.
        _main.Open(Id, reload: true);
    }

    // Change tracking

    /// <summary>
    /// The title or the pool changed. Running auctions follow along: new players land in their skipped list,
    /// edits show up on the team cards, removed players leave (refunded if they were sold).
    /// </summary>
    internal void PoolChanged(bool playersRemoved = false)
    {
        Tournament.TouchPool();
        var affectsAuctions = TournamentRules.AddNewPlayersToRunningAuctions(Tournament) > 0 || playersRemoved;
        if (affectsAuctions)
        {
            // Saved right away: it may change a running auction.
            _dirty = true;
            SaveNow();
        }
        else
        {
            MarkDirty();
        }

        foreach (var division in Divisions)
        {
            division.OnPoolChanged(clearUndo: playersRemoved);
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
        TournamentRules.AddNewPlayersToRunningAuctions(Tournament);
        _dirty = true;
        SaveNow();
        Pool.RefreshStatuses();
        foreach (var other in Divisions.Where(other => other.Division != division))
        {
            other.OnPoolChanged(clearUndo: false);
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
            division.UpcomingShown = previous.UpcomingShown;
            division.HalfBudgetCapAtStart = previous.HalfBudgetCapAtStart;
            division.TierMinimums = [.. previous.TierMinimums];
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
        TournamentRules.AddNewPlayersToRunningAuctions(Tournament);
        SaveNow();
        Pool.RefreshStatuses();
        foreach (var division in Divisions)
        {
            division.OnPoolChanged(clearUndo: false);
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
        _main.IsMenuOpen = false;
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
