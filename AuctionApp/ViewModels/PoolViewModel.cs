using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
/// The tournament's player pool, shared by every division. Its order matters for divisions that don't shuffle, so
/// it can be rearranged by drag and drop. The rows edit the tournament directly and every change is saved.
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
        PlayersView.Filter = item => item is PoolPlayerRowViewModel row && MatchesSearch(row);
        RefreshStatuses();
    }

    private Tournament Tournament => _owner.Tournament;

    private IDialogService Dialogs => _owner.Dialogs;

    public string Header => "Player pool";

    public ObservableCollection<PoolPlayerRowViewModel> Players { get; } = [];

    public ICollectionView PlayersView { get; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Summary { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsFiltered { get; set; }

    partial void OnSearchTextChanged(string value)
    {
        EndPendingEdits();
        IsFiltered = !string.IsNullOrWhiteSpace(value);
        PlayersView.Refresh();
    }

    private bool MatchesSearch(PoolPlayerRowViewModel row) =>
        string.IsNullOrWhiteSpace(SearchText) || row.Name.Contains(SearchText.Trim(), StringComparison.CurrentCultureIgnoreCase);

    /// <summary>Updates each player's status (available, bought, captain...) and the summary line.</summary>
    internal void RefreshStatuses()
    {
        var statuses = TournamentRules.PoolStatuses(Tournament);
        for (var i = 0; i < Players.Count; i++)
        {
            var row = Players[i];
            row.Position = i + 1;
            if (statuses.TryGetValue(row.Model.Id, out var status))
            {
                row.SetStatus(status);
            }
        }

        var available = statuses.Values.Count(status => status.Kind == PoolStatusKind.Available);
        var picked = statuses.Values.Count(status => status.Kind == PoolStatusKind.Picked);
        var captains = statuses.Values.Count(status => status.Kind == PoolStatusKind.Captain);
        var parts = new List<string> { $"{Players.Count} players", $"{available} available" };
        if (picked > 0)
        {
            parts.Add($"{picked} bought");
        }

        if (captains > 0)
        {
            parts.Add($"{captains} {(captains == 1 ? "is a captain" : "are captains")}");
        }

        Summary = string.Join("  ·  ", parts);
    }

    /// <summary>A player's name or classes changed.</summary>
    internal void PlayerEdited(PoolPlayerRowViewModel row)
    {
        TournamentRules.SyncPlayer(Tournament, row.Model);
        Changed();
    }

    private void Changed()
    {
        if (_syncing)
        {
            return;
        }

        RefreshStatuses();
        _owner.PoolChanged();
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

                break;
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

    /// <summary>Checked before the grid deletes rows with the Delete key: bought players can't be removed.</summary>
    public bool ConfirmRemoval(IEnumerable<PoolPlayerRowViewModel> rows)
    {
        var problems = rows.Select(row => TournamentRules.CanRemovePlayer(Tournament, row.Model)).OfType<string>().ToList();
        if (problems.Count > 0)
        {
            Dialogs.ShowError("Can't remove these players", string.Join("\n", problems));
            return false;
        }

        return true;
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
        foreach (var row in rows)
        {
            Players.Remove(row);
        }
    }

    [RelayCommand]
    private void PastePlayers()
    {
        EndPendingEdits();
        var parsed = Dialogs.AskForRoster();
        if (parsed == null)
        {
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

                Players.Add(new PoolPlayerRowViewModel(new Player { Name = player.Name, Classes = player.Classes }));
            }
        }
        finally
        {
            _syncing = false;
        }

        Changed();
        if (skipped > 0)
        {
            Dialogs.Ask("Some players were already in the pool", $"{skipped} name(s) were already listed and were not added twice.", "OK", cancel: null);
        }
    }

    /// <summary>Moves a player before or after another one (drag and drop).</summary>
    public void Move(PoolPlayerRowViewModel row, PoolPlayerRowViewModel? target, bool after)
    {
        EndPendingEdits();
        var from = Players.IndexOf(row);
        if (from < 0)
        {
            return;
        }

        int to;
        if (target == null)
        {
            to = Players.Count - 1;
        }
        else
        {
            to = Players.IndexOf(target) + (after ? 1 : 0);
            if (to > from)
            {
                to--;
            }
        }

        to = Math.Clamp(to, 0, Players.Count - 1);
        if (to != from)
        {
            Players.Move(from, to);
        }
    }

    public void MoveBy(PoolPlayerRowViewModel row, int offset)
    {
        var from = Players.IndexOf(row);
        var to = Math.Clamp(from + offset, 0, Players.Count - 1);
        if (from >= 0 && to != from)
        {
            EndPendingEdits();
            Players.Move(from, to);
        }
    }

    [RelayCommand]
    private void SortByName()
    {
        if (Dialogs.Ask("Sort the pool by name?", "This replaces the current order of the pool, which divisions that don't shuffle use as their auction order.", "Sort") != DialogChoice.Primary)
        {
            return;
        }

        EndPendingEdits();
        var sorted = Players.OrderBy(row => row.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        _syncing = true;
        try
        {
            for (var i = 0; i < sorted.Count; i++)
            {
                var from = Players.IndexOf(sorted[i]);
                if (from != i)
                {
                    Players.Move(from, i);
                }
            }
        }
        finally
        {
            _syncing = false;
        }

        Changed();
    }

    [RelayCommand]
    private void ExportPlayers()
    {
        var path = Dialogs.PickFileToSave("Export the player pool", "CSV spreadsheet (*.csv)|*.csv", $"{Tournament.Title} players.csv");
        if (path != null)
        {
            Dialogs.TryWriteFile(path, TournamentExporter.PlayersToCsv(Tournament));
        }
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
    public partial int Position { get; set; }

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

    internal void SetStatus(PoolStatus status)
    {
        StatusKind = status.Kind;
        StatusText = status.Kind switch
        {
            PoolStatusKind.Picked => $"{status.Division!.Name} · {status.CaptainName} · {Money.Format(status.Price)}",
            PoolStatusKind.InAuction => $"In the {status.Division!.Name} auction",
            PoolStatusKind.Captain => $"Captain in {status.Division!.Name}",
            _ => "Available",
        };
    }

    private bool HasClass(string code) => Model.Classes.Contains(code);

    private void SetClass(string code, bool value, [System.Runtime.CompilerServices.CallerMemberName] string? property = null)
    {
        if (HasClass(code) == value)
        {
            return;
        }

        var classes = value ? Model.Classes.Append(code) : Model.Classes.Where(c => c != code);
        Model.Classes = PlayerClasses.Normalize(classes);
        OnPropertyChanged(property);
        Owner?.PlayerEdited(this);
    }
}
