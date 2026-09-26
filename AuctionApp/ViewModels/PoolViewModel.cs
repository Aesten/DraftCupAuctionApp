using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.ComponentModel;
using System.Windows.Data;
using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;
using AuctionApp.Core.Storage;
using AuctionApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuctionApp.ViewModels;

/// <summary>
/// The tournament's player pool, shared by every division, sorted by name or by date added (and by tier in Captain
/// Pick, where each player also has a tier and a single class). Rows edit the tournament directly, running auctions
/// follow, and every change is saved.
/// </summary>
public sealed partial class PoolViewModel : ObservableObject
{
    private readonly TournamentViewModel _owner;
    private bool _syncing;

    public PoolViewModel(TournamentViewModel owner)
    {
        _owner = owner;
        foreach (var player in Tournament.Players)
        {
            Players.Add(new PoolPlayerRowViewModel(player) { Owner = this });
        }

        Players.CollectionChanged += OnPlayersCollectionChanged;
        PlayersView = CollectionViewSource.GetDefaultView(Players);
        PlayersView.Filter = item => item is PoolPlayerRowViewModel row && MatchesFilters(row);
        ApplySort();
        RefreshStatuses();
    }

    private Tournament Tournament => _owner.Tournament;

    /// <summary>Captain Pick: players have a tier and one class.</summary>
    public bool IsCaptainPick => Tournament.IsCaptainPick;

    public string ClassesHeader => IsCaptainPick ? "CLASS" : "CLASSES";

    private IDialogService Dialogs => _owner.Dialogs;

    public string Header => "Player pool";

    public ObservableCollection<PoolPlayerRowViewModel> Players { get; } = [];

    public ICollectionView PlayersView { get; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Summary { get; set; } = string.Empty;

    /// <summary>The "add a player" row: a name, optionally followed by classes ("Alice, inf cav") and a tier ("Alice, inf 3").</summary>
    [ObservableProperty]
    public partial string NewPlayerText { get; set; } = string.Empty;

    /// <summary>Classes ticked in the "add a player" row, added to whatever was typed.</summary>
    [ObservableProperty]
    public partial bool NewInfantry { get; set; }

    [ObservableProperty]
    public partial bool NewArcher { get; set; }

    [ObservableProperty]
    public partial bool NewCavalry { get; set; }

    /// <summary>Captain Pick: the tier picked in the "add a player" row.</summary>
    [ObservableProperty]
    public partial int? NewTier { get; set; }

    // Captain Pick: one class per player, so ticking a class in the add row unticks the others.
    partial void OnNewInfantryChanged(bool value) => KeepOneNewClass(value, nameof(NewInfantry));

    partial void OnNewArcherChanged(bool value) => KeepOneNewClass(value, nameof(NewArcher));

    partial void OnNewCavalryChanged(bool value) => KeepOneNewClass(value, nameof(NewCavalry));

    private void KeepOneNewClass(bool value, string ticked)
    {
        if (!value || !IsCaptainPick)
        {
            return;
        }

        NewInfantry = ticked == nameof(NewInfantry);
        NewArcher = ticked == nameof(NewArcher);
        NewCavalry = ticked == nameof(NewCavalry);
    }

    /// <summary>Raised after a player is added, so the view can select and show them.</summary>
    public event Action<PoolPlayerRowViewModel>? PlayerAdded;

    [ObservableProperty]
    public partial bool IsFiltered { get; set; }

    /// <summary>0: sorted by name. 1: in the order the players were added (the pool's own order). 2: by tier, then name (Captain Pick).</summary>
    [ObservableProperty]
    public partial int SortIndex { get; set; }

    partial void OnSortIndexChanged(int value) => ApplySort();

    private void ApplySort()
    {
        EndPendingEdits();
        PlayersView.SortDescriptions.Clear();
        if (SortIndex == 2)
        {
            PlayersView.SortDescriptions.Add(new SortDescription(nameof(PoolPlayerRowViewModel.Tier), ListSortDirection.Ascending));
        }

        if (SortIndex != 1)
        {
            PlayersView.SortDescriptions.Add(new SortDescription(nameof(PoolPlayerRowViewModel.Name), ListSortDirection.Ascending));
        }
    }

    /// <summary>Captain Pick: sets the tier of the selected players (keys 1 to 5 in the list).</summary>
    public void SetTier(IEnumerable<PoolPlayerRowViewModel> rows, int tier)
    {
        foreach (var row in rows.ToList())
        {
            row.Tier = tier;
        }

        // Keeps the list in tier order when it is sorted by tier.
        if (SortIndex == 2)
        {
            PlayersView.Refresh();
        }
    }

    /// <summary>The pool tab was opened: statuses may have changed in the meantime.</summary>
    internal void OnShown() => RefreshStatuses();

    // Filters: search by name, by class, and by availability.

    [ObservableProperty]
    public partial bool FilterInfantry { get; set; }

    [ObservableProperty]
    public partial bool FilterArcher { get; set; }

    [ObservableProperty]
    public partial bool FilterCavalry { get; set; }

    /// <summary>0: everyone, 1: available (not bought yet), 2: unavailable (bought).</summary>
    [ObservableProperty]
    public partial int AvailabilityFilter { get; set; }

    [ObservableProperty]
    public partial string ShownCount { get; set; } = string.Empty;

    partial void OnSearchTextChanged(string value) => ApplyFilters();

    partial void OnFilterInfantryChanged(bool value) => ApplyFilters();

    partial void OnFilterArcherChanged(bool value) => ApplyFilters();

    partial void OnFilterCavalryChanged(bool value) => ApplyFilters();

    partial void OnAvailabilityFilterChanged(int value) => ApplyFilters();

    private void ApplyFilters()
    {
        EndPendingEdits();
        IsFiltered = !string.IsNullOrWhiteSpace(SearchText) || FilterInfantry || FilterArcher || FilterCavalry || AvailabilityFilter != 0;
        PlayersView.Refresh();
        UpdateShownCount();
    }

    private void UpdateShownCount() =>
        ShownCount = IsFiltered ? $"Showing {PlayersView.Cast<object>().Count()} of {Players.Count}" : string.Empty;

    /// <summary>A player shows when their name contains the search, they have one of the selected classes, and they match the availability.</summary>
    private bool MatchesFilters(PoolPlayerRowViewModel row)
    {
        if (!string.IsNullOrWhiteSpace(SearchText) && !row.Name.Contains(SearchText.Trim(), StringComparison.CurrentCultureIgnoreCase))
        {
            return false;
        }

        if ((FilterInfantry || FilterArcher || FilterCavalry)
            && !(FilterInfantry && row.Infantry || FilterArcher && row.Archer || FilterCavalry && row.Cavalry))
        {
            return false;
        }

        return AvailabilityFilter switch
        {
            1 => row.StatusKind != PoolStatusKind.Picked,
            2 => row.StatusKind == PoolStatusKind.Picked,
            _ => true,
        };
    }

    /// <summary>Updates each player's status (available, bought, captain...) and the summary line.</summary>
    internal void RefreshStatuses()
    {
        var statuses = TournamentRules.PoolStatuses(Tournament);
        foreach (var row in Players)
        {
            if (statuses.TryGetValue(row.Model.Id, out var status))
            {
                row.SetStatus(status);
            }
        }

        // "Available" means not bought yet, matching the availability filter; players in a running auction count.
        var picked = statuses.Values.Count(status => status.Kind == PoolStatusKind.Picked);
        var parts = new List<string> { $"{Players.Count} players" };
        if (picked > 0)
        {
            parts.Add($"{picked} bought");
        }

        parts.Add($"{Players.Count - picked} available");
        if (IsCaptainPick && Players.Count(row => row.Tier == null && !string.IsNullOrWhiteSpace(row.Name)) is > 0 and var noTier)
        {
            parts.Add($"{noTier} without a tier");
        }

        Summary = string.Join("  ·  ", parts);
        if (IsFiltered && AvailabilityFilter != 0)
        {
            PlayersView.Refresh();
        }

        UpdateShownCount();
    }

    /// <summary>A player's name or classes changed.</summary>
    internal void PlayerEdited(PoolPlayerRowViewModel row)
    {
        TournamentRules.SyncPlayer(Tournament, row.Model);
        Changed();
    }

    private void Changed(bool playersRemoved = false)
    {
        if (_syncing)
        {
            return;
        }

        // The tournament first (running auctions pick up new players), then the statuses shown here.
        _owner.PoolChanged(playersRemoved);
        RefreshStatuses();
    }

    private void OnPlayersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Rows can be added and removed by the grid itself (new row placeholder, Delete key): mirror that in the pool.
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                var index = e.NewStartingIndex;
                foreach (PoolPlayerRowViewModel row in e.NewItems!)
                {
                    row.Owner = this;
                    Tournament.Players.Insert(Math.Clamp(index++, 0, Tournament.Players.Count), row.Model);
                }

                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (PoolPlayerRowViewModel row in e.OldItems!)
                {
                    TournamentRules.RemovePlayer(Tournament, row.Model);
                }

                Changed(playersRemoved: true);
                return;
            case NotifyCollectionChangedAction.Move:
                Tournament.Players.RemoveAt(e.OldStartingIndex);
                Tournament.Players.Insert(e.NewStartingIndex, ((PoolPlayerRowViewModel)e.NewItems![0]!).Model);
                break;
        }

        Changed();
    }

    /// <summary>The grid can't be filtered or changed while it is in the middle of adding or editing a row.</summary>
    internal void EndPendingEdits()
    {
        if (PlayersView is IEditableCollectionView editable)
        {
            if (editable.IsAddingNew)
            {
                editable.CommitNew();
            }

            if (editable.IsEditingItem)
            {
                editable.CommitEdit();
            }
        }
    }

    /// <summary>Asked before removing players; players already bought are taken off their team, which gets its money back.</summary>
    public bool ConfirmRemoval(IReadOnlyCollection<PoolPlayerRowViewModel> rows)
    {
        var sales = rows
            .SelectMany(row => TournamentRules.Sales(Tournament, row.Model).Select(sale => $"• {row.Name}: bought by {sale}"))
            .ToList();
        if (sales.Count == 0)
        {
            return true;
        }

        return Dialogs.Ask(
            rows.Count == 1 ? $"Remove {rows.First().Name}?" : $"Remove {rows.Count} players?",
            string.Join("\n", sales) + "\n\nThey will be taken off their team, and the team gets the money back.",
            "Remove") == DialogChoice.Primary;
    }

    [RelayCommand]
    private void RemovePlayers(IList? selected)
    {
        var rows = selected?.OfType<PoolPlayerRowViewModel>().ToList() ?? [];
        if (rows.Count == 0 || !ConfirmRemoval(rows))
        {
            return;
        }

        EndPendingEdits();
        _syncing = true;
        try
        {
            foreach (var row in rows)
            {
                Players.Remove(row);
            }
        }
        finally
        {
            _syncing = false;
        }

        Changed(playersRemoved: true);
    }

    [RelayCommand]
    private void AddPlayer()
    {
        if (RosterParser.Parse(NewPlayerText).FirstOrDefault() is not { } parsed)
        {
            return;
        }

        var existing = Players.FirstOrDefault(row => string.Equals(row.Name.Trim(), parsed.Name, StringComparison.CurrentCultureIgnoreCase));
        if (existing != null
            && Dialogs.Ask($"{parsed.Name} is already in the pool", "Add another player with the same name?", "Add anyway") != DialogChoice.Primary)
        {
            PlayerAdded?.Invoke(existing);
            return;
        }

        EndPendingEdits();
        var ticked = new[] { (NewInfantry, PlayerClasses.Infantry), (NewArcher, PlayerClasses.Archer), (NewCavalry, PlayerClasses.Cavalry) }
            .Where(entry => entry.Item1)
            .Select(entry => entry.Item2);
        var classes = PlayerClasses.Normalize(ticked.Concat(parsed.Classes));
        var player = new Player { Name = parsed.Name, Classes = IsCaptainPick ? classes.Take(1).ToList() : classes };
        if (IsCaptainPick)
        {
            player.Tier = NewTier ?? parsed.Tier;
        }

        var row = new PoolPlayerRowViewModel(player);
        Players.Add(row);
        NewPlayerText = string.Empty;
        NewInfantry = NewArcher = NewCavalry = false;
        NewTier = null;
        PlayerAdded?.Invoke(row);
    }

    /// <summary>Adds the players of a player list file (CSV or JSON, e.g. from a sign-up sheet). Names already in the pool are skipped.</summary>
    [RelayCommand]
    private void ImportPlayers()
    {
        EndPendingEdits();
        if (Dialogs.PickPlayerListToImport(IsCaptainPick) is not { } path)
        {
            return;
        }

        List<ParsedPlayer> parsed;
        try
        {
            parsed = PlayerList.Parse(File.ReadAllText(path));
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            Dialogs.ShowError("Couldn't import this file", ex.Message);
            return;
        }

        var existing = Players.Select(player => player.Name.Trim()).ToHashSet(StringComparer.CurrentCultureIgnoreCase);
        var skipped = 0;
        _syncing = true;
        try
        {
            foreach (var player in parsed)
            {
                if (!existing.Add(player.Name))
                {
                    skipped++;
                    continue;
                }

                Players.Add(new PoolPlayerRowViewModel(new Player
                {
                    Name = player.Name,
                    Classes = IsCaptainPick ? player.Classes.Take(1).ToList() : player.Classes,
                    Tier = IsCaptainPick ? player.Tier : null,
                }));
            }
        }
        finally
        {
            _syncing = false;
        }

        Changed();
        var added = parsed.Count - skipped;
        var message = $"{added} player(s) added to the pool."
            + (skipped > 0 ? $" {skipped} name(s) were already listed and were not added twice." : string.Empty)
            + (IsCaptainPick && parsed.Any(player => player.Classes.Count > 1) ? " Players listed with several classes kept only the first one." : string.Empty)
            + (IsCaptainPick && added > 0 && parsed.All(player => player.Tier == null) ? " The file had no tiers: select players and press 1 to 5 to set them." : string.Empty);
        Dialogs.Ask("Players imported", message, "OK", cancel: null);
    }

    /// <summary>Saves the pool as a player list (names and classes, and tiers in Captain Pick). Format: "csv" or "json".</summary>
    [RelayCommand]
    private void ExportPlayers(string format)
    {
        EndPendingEdits();
        var isJson = format == "json";
        var path = Dialogs.PickFileToSave(
            "Export the player list",
            isJson ? "JSON player list (*.json)|*.json" : "CSV spreadsheet (*.csv)|*.csv",
            $"{Tournament.Title} players{(isJson ? ".json" : ".csv")}");
        if (path == null)
        {
            return;
        }

        var players = Tournament.Players.Where(player => !string.IsNullOrWhiteSpace(player.Name));
        Dialogs.TryWriteFile(path, isJson ? PlayerList.ToJson(players, IsCaptainPick) : PlayerList.ToCsv(players, IsCaptainPick));
    }
}

public sealed partial class PoolPlayerRowViewModel : ObservableObject
{
    /// <summary>Used by the grid when the user types in its empty last row.</summary>
    public PoolPlayerRowViewModel()
        : this(new Player())
    {
    }

    public PoolPlayerRowViewModel(Player model) => Model = model;

    public Player Model { get; }

    internal PoolViewModel? Owner { get; set; }

    [ObservableProperty]
    public partial string StatusText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial PoolStatusKind StatusKind { get; set; }

    public string Name
    {
        get => Model.Name;
        set
        {
            var trimmed = value?.Trim() ?? string.Empty;
            if (Model.Name == trimmed)
            {
                return;
            }

            Model.Name = trimmed;
            OnPropertyChanged();
            Owner?.PlayerEdited(this);
        }
    }

    public bool Infantry
    {
        get => HasClass(PlayerClasses.Infantry);
        set => SetClass(PlayerClasses.Infantry, value);
    }

    public bool Archer
    {
        get => HasClass(PlayerClasses.Archer);
        set => SetClass(PlayerClasses.Archer, value);
    }

    public bool Cavalry
    {
        get => HasClass(PlayerClasses.Cavalry);
        set => SetClass(PlayerClasses.Cavalry, value);
    }

    /// <summary>Captain Pick: the player's tier, 1 to 5, or null while not set.</summary>
    public int? Tier
    {
        get => Model.Tier;
        set
        {
            var tier = Tiers.IsValid(value) ? value : null;
            if (Model.Tier == tier)
            {
                return;
            }

            Model.Tier = tier;
            OnPropertyChanged();
            Owner?.PlayerEdited(this);
        }
    }

    internal void SetStatus(PoolStatus status)
    {
        StatusKind = status.Kind;
        StatusText = status.Kind switch
        {
            PoolStatusKind.Picked => $"{status.Division!.Name} · {status.CaptainName} · {Money.Format(status.Price)}",
            PoolStatusKind.InAuction => $"In the {status.Division!.Name} auction",
            _ => "Available",
        };
    }

    private bool HasClass(string code) => Model.Classes.Contains(code);

    private void SetClass(string code, bool value)
    {
        if (HasClass(code) == value)
        {
            return;
        }

        // Captain Pick: a player plays one class, so ticking one replaces the other.
        IEnumerable<string> classes = value && Owner?.IsCaptainPick == true ? [code]
            : value ? Model.Classes.Append(code)
            : Model.Classes.Where(c => c != code);
        Model.Classes = PlayerClasses.Normalize(classes);
        OnPropertyChanged(nameof(Infantry));
        OnPropertyChanged(nameof(Archer));
        OnPropertyChanged(nameof(Cavalry));
        Owner?.PlayerEdited(this);
    }
}
