using System.Collections.ObjectModel;
using System.Globalization;
using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;
using AuctionApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuctionApp.ViewModels;

/// <summary>
/// A division's live auction screen, meant to be streamed. Every action is checked by the engine, can be undone,
/// and is saved immediately. Only the next few players are revealed so captains can't plan too far ahead.
/// </summary>
public sealed partial class AuctionViewModel : ObservableObject
{
    private const string DefaultPrice = "0.1";

    private readonly DivisionViewModel _owner;
    private readonly UndoHistory _history = new();
    private AuctionEngine? _engine;

    public AuctionViewModel(DivisionViewModel owner)
    {
        _owner = owner;
        Reload();
    }

    private Division Division => _owner.Division;

    private IDialogService Dialogs => _owner.Dialogs;

    public ObservableCollection<TeamCardViewModel> Teams { get; } = [];

    public ObservableCollection<PlayerItemViewModel> UpNext { get; } = [];

    public ObservableCollection<PlayerItemViewModel> Skipped { get; } = [];

    [ObservableProperty]
    public partial bool HasSession { get; set; }

    [ObservableProperty]
    public partial bool IsFinished { get; set; }

    [ObservableProperty]
    public partial bool IsRunning { get; set; }

    [ObservableProperty]
    public partial bool HasCurrentPlayer { get; set; }

    /// <summary>The queue is empty but the auction isn't finished: bring back skipped players or finish.</summary>
    [ObservableProperty]
    public partial bool IsQueueEmpty { get; set; }

    [ObservableProperty]
    public partial string CurrentPlayerName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IReadOnlyList<string> CurrentPlayerClasses { get; set; } = [];

    [ObservableProperty]
    public partial string QueueEmptyText { get; set; } = string.Empty;

    /// <summary>The label above the center stage: who is on the block, or why nobody is.</summary>
    [ObservableProperty]
    public partial string StageLabel { get; set; } = "NOW ON THE BLOCK";

    /// <summary>Shown on the center stage when nobody is on the block.</summary>
    [ObservableProperty]
    public partial string StageMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string UpNextHeader { get; set; } = "Up next";

    [ObservableProperty]
    public partial bool ShowsUpNext { get; set; }

    /// <summary>How many rows the upcoming list is laid out for, so it fills its panel evenly.</summary>
    [ObservableProperty]
    public partial int UpNextRows { get; set; } = 1;

    [ObservableProperty]
    public partial int SoldCount { get; set; }

    [ObservableProperty]
    public partial int RemainingCount { get; set; }

    [ObservableProperty]
    public partial int SkippedCount { get; set; }

    [ObservableProperty]
    public partial bool HasSkipped { get; set; }

    /// <summary>The latest thing that happened, e.g. "Alice sold to Bob for 2.5".</summary>
    [ObservableProperty]
    public partial string LastAction { get; set; } = string.Empty;

    [ObservableProperty]
    public partial TeamCardViewModel? SelectedTeam { get; set; }

    [ObservableProperty]
    public partial string PriceText { get; set; } = DefaultPrice;

    /// <summary>Why the sale can't happen right now (shown under the price), or null.</summary>
    [ObservableProperty]
    public partial string? SaleProblem { get; set; }

    [ObservableProperty]
    public partial string UndoText { get; set; } = "Nothing to undo";

    public string Title => $"{_owner.Tournament.Title} — {Division.Name}";

    public bool HalfBudgetCap
    {
        get => _engine?.Session.HalfBudgetCap ?? false;
        set
        {
            if (_engine == null || _engine.Session.HalfBudgetCap == value)
            {
                return;
            }

            Apply(value ? "turn on the half budget cap" : "turn off the half budget cap", engine => engine.SetHalfBudgetCap(value));
        }
    }

    /// <summary>Rebuilds everything from the division (after the auction starts, is reset, or is replaced).</summary>
    public void Reload()
    {
        _history.Clear();
        _engine = Division.Session != null ? new AuctionEngine(_owner.Tournament, Division) : null;
        Teams.Clear();
        SelectedTeam = null;
        if (_engine != null)
        {
            foreach (var team in _engine.Session.Teams)
            {
                Teams.Add(new TeamCardViewModel(team.CaptainId, this));
            }
        }

        Refresh();
    }

    private void Refresh()
    {
        HasSession = _engine != null;
        OnPropertyChanged(nameof(Title));
        if (_engine == null)
        {
            IsRunning = IsFinished = HasCurrentPlayer = IsQueueEmpty = false;
            UpNext.Clear();
            Skipped.Clear();
            LastAction = string.Empty;
            UpdateUndo();
            return;
        }

        var session = _engine.Session;
        IsFinished = session.IsFinished;
        IsRunning = !session.IsFinished;
        var current = session.CurrentPlayer;
        HasCurrentPlayer = IsRunning && current != null;
        IsQueueEmpty = IsRunning && current == null;
        CurrentPlayerName = current?.Name ?? string.Empty;
        CurrentPlayerClasses = current != null ? KnownClasses(current.Classes) : [];
        QueueEmptyText = session.Skipped.Count > 0
            ? $"Nobody is left in the queue, but {session.Skipped.Count} skipped player(s) can get another chance."
            : "Everyone has been auctioned.";
        StageLabel = IsFinished ? "ALL DONE" : IsQueueEmpty ? "QUEUE EMPTY" : "NOW ON THE BLOCK";
        StageMessage = IsFinished ? "The auction is finished." : QueueEmptyText;

        SoldCount = session.SoldCount;
        RemainingCount = session.Queue.Count;
        SkippedCount = session.Skipped.Count;
        HasSkipped = SkippedCount > 0;

        // Only the next few players are revealed on stream.
        var shown = Division.UpcomingShown;
        ShowsUpNext = shown > 0 && IsRunning;
        UpNextHeader = shown == 1 ? "NEXT UP" : $"NEXT {shown}";
        UpNextRows = Math.Max(1, shown);
        Replace(UpNext, session.Queue.Skip(1).Take(shown).Select((player, i) => new PlayerItemViewModel(player, i + 1)));
        Replace(Skipped, session.Skipped.Select((player, i) => new PlayerItemViewModel(player, i + 1)));
        LastAction = session.Activity.LastOrDefault()?.Text ?? string.Empty;

        foreach (var team in Teams)
        {
            team.Update(_engine);
        }

        OnPropertyChanged(nameof(HalfBudgetCap));
        CheckSale();
        UpdateUndo();
    }

    /// <summary>
    /// The pool changed (players added, edited or removed): show the result. Undo points are dropped when players
    /// were removed, since going back to them would bring removed players back into the auction.
    /// </summary>
    internal void OnPoolChanged(bool clearUndo)
    {
        if (clearUndo)
        {
            _history.Clear();
        }

        if (_engine == null && Division.Session != null || _engine != null && Division.Session == null)
        {
            Reload();
        }
        else
        {
            Refresh();
        }
    }

    /// <summary>
    /// How many players of each class a team has, captain included, zeros too (e.g. INF 3 · ARC 0 · CAV 2).
    /// A player with several classes counts in each of them.
    /// </summary>
    internal static IReadOnlyList<ClassCount> Composition(string captainClass, IEnumerable<Pick> picks)
    {
        var classes = picks.Select(pick => pick.Player.Classes).Append(captainClass.Length > 0 ? [captainClass] : []).ToList();
        return PlayerClasses.All.Select(code => new ClassCount(code, classes.Count(list => list.Contains(code)))).ToList();
    }

    internal static IReadOnlyList<string> CaptainClasses(Division division, Guid captainId) =>
        KnownClasses([division.CaptainClass(captainId)]);

    internal static IReadOnlyList<string> KnownClasses(IEnumerable<string> classes) => classes.Where(PlayerClasses.All.Contains).ToList();

    private static void Replace<T>(ObservableCollection<T> collection, IEnumerable<T> items)
    {
        collection.Clear();
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }

    /// <summary>Runs an action with an undo point, saves, and refreshes the screen. Errors are shown, not thrown.</summary>
    private bool Apply(string description, Action<AuctionEngine> action)
    {
        if (_engine == null)
        {
            return false;
        }

        _history.Record(description, _engine.Session);
        try
        {
            action(_engine);
        }
        catch (AuctionException ex)
        {
            _history.Discard();
            Dialogs.ShowError("That didn't work", ex.Message);
            Refresh();
            return false;
        }

        _owner.AuctionChanged();
        Refresh();
        return true;
    }

    private void UpdateUndo()
    {
        UndoText = _history.NextDescription is { } next ? $"Undo: {next}" : "Nothing to undo";
        UndoCommand.NotifyCanExecuteChanged();
    }

    // Selling

    partial void OnSelectedTeamChanged(TeamCardViewModel? value)
    {
        foreach (var team in Teams)
        {
            team.IsSelected = team == value;
        }

        CheckSale();
    }

    partial void OnPriceTextChanged(string value) => CheckSale();

    internal void Select(TeamCardViewModel team)
    {
        if (HasCurrentPlayer)
        {
            SelectedTeam = SelectedTeam == team ? null : team;
        }
    }

    private void CheckSale()
    {
        if (_engine == null || !HasCurrentPlayer)
        {
            SaleProblem = null;
        }
        else if (!Money.TryParse(PriceText, out var price))
        {
            SaleProblem = "Enter a price.";
        }
        else if (SelectedTeam == null)
        {
            // The team placeholder already says to click the winning team.
            SaleProblem = null;
        }
        else
        {
            SaleProblem = _engine.CheckSale(SelectedTeam.CaptainId, price);
        }

        SellCommand.NotifyCanExecuteChanged();
    }

    private bool CanSell() => HasCurrentPlayer && SelectedTeam != null && SaleProblem == null;

    [RelayCommand(CanExecute = nameof(CanSell))]
    private void Sell()
    {
        if (SelectedTeam is not { } team || !Money.TryParse(PriceText, out var price))
        {
            return;
        }

        if (Apply($"sell {CurrentPlayerName} to {team.Name}", engine => engine.Sell(team.CaptainId, price)))
        {
            PriceText = DefaultPrice;
            SelectedTeam = null;
        }
    }

    [RelayCommand]
    private void ChangePrice(string delta)
    {
        if (!decimal.TryParse(delta, NumberStyles.Number, CultureInfo.InvariantCulture, out var step))
        {
            return;
        }

        var current = Money.TryParse(PriceText, out var price) ? price : 0m;
        PriceText = Money.Format(Math.Max(0, decimal.Round(current, 1) + step));
    }

    [RelayCommand]
    private void Skip()
    {
        if (HasCurrentPlayer)
        {
            Apply($"skip {CurrentPlayerName}", engine => engine.Skip());
        }
    }

    [RelayCommand]
    private void BringBack(PlayerItemViewModel player) =>
        Apply($"bring back {player.Name}", engine => engine.BringBack(player.Id));

    [RelayCommand]
    private void RequeueSkipped() =>
        Apply("send skipped players back to the queue", engine => engine.RequeueSkipped());

    // Fixing a sale: from the menu of a bought player.

    internal IEnumerable<TeamCardViewModel> OtherTeams(TeamCardViewModel team) => Teams.Where(other => other != team);

    internal void ReturnToBlock(TeamCardViewModel team, PickItemViewModel pick) =>
        Apply($"take {pick.Name} back from {team.Name}", engine => engine.ReturnPick(team.CaptainId, pick.PlayerId));

    internal void ReturnToSkipped(TeamCardViewModel team, PickItemViewModel pick) =>
        Apply($"send {pick.Name} to the skipped list", engine => engine.ReturnPickToSkipped(team.CaptainId, pick.PlayerId));

    internal void ChangePickPrice(TeamCardViewModel team, PickItemViewModel pick)
    {
        if (Dialogs.AskPrice($"Change {pick.Name}'s price", $"{team.Name} paid {pick.PriceText}. The difference is refunded or charged to {team.Name}.", pick.Price) is { } price
            && price != pick.Price)
        {
            Apply($"change {pick.Name}'s price", engine => engine.ChangePickPrice(team.CaptainId, pick.PlayerId, price));
        }
    }

    internal void MovePick(TeamCardViewModel team, PickItemViewModel pick, TeamCardViewModel target) =>
        Apply($"move {pick.Name} to {target.Name}", engine => engine.MovePick(team.CaptainId, pick.PlayerId, target.CaptainId));

    internal void SwapPick(TeamCardViewModel team, PickItemViewModel pick)
    {
        if (_engine == null)
        {
            return;
        }

        var session = _engine.Session;
        var choices = session.Queue.Select((player, i) => Choice(player, i == 0 ? "On the block" : "In the queue"))
            .Concat(session.Skipped.Select(player => Choice(player, "Skipped")))
            .Concat(session.Unsold.Select(player => Choice(player, "Not sold")))
            .ToList();
        if (choices.Count == 0)
        {
            Dialogs.ShowError("Nobody to swap with", "Every player of this auction has been bought.");
            return;
        }

        var message = $"The player you pick joins {team.Name} for {pick.PriceText}, and {pick.Name} takes their place.";
        if (Dialogs.PickPlayer($"Swap {pick.Name} with…", message, choices) is { } otherId)
        {
            var other = choices.First(choice => choice.Id == otherId).Name;
            Apply($"swap {pick.Name} with {other}", engine => engine.SwapPick(team.CaptainId, pick.PlayerId, otherId));
        }

        static PlayerChoice Choice(SessionPlayer player, string where) => new(player.Id, player.Name, KnownClasses(player.Classes), where);
    }

    // End of the auction

    [RelayCommand]
    private void Finish()
    {
        if (_engine == null)
        {
            return;
        }

        var remaining = _engine.Session.Queue.Count + _engine.Session.Skipped.Count;
        var message = remaining > 0
            ? $"{remaining} player(s) haven't been sold. They stay available for the other divisions. You can reopen the auction later if needed."
            : "Every player has been auctioned. You can reopen the auction later if needed.";
        if (Dialogs.Ask($"Finish the {Division.Name} auction?", message, "Finish auction") != DialogChoice.Primary)
        {
            return;
        }

        if (Apply("finish the auction", engine => engine.Finish()))
        {
            _owner.StatusChanged(reloadAuction: false);
            _owner.SelectedPage = DivisionViewModel.TeamsPage;
        }
    }

    [RelayCommand]
    private void Reopen()
    {
        if (Apply("reopen the auction", engine => engine.Reopen()))
        {
            _owner.StatusChanged(reloadAuction: false);
            _owner.SelectedPage = DivisionViewModel.AuctionPage;
        }
    }

    [RelayCommand]
    private void ShowTeams() => _owner.SelectedPage = DivisionViewModel.TeamsPage;

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        if (_history.Pop() is not { } snapshot)
        {
            return;
        }

        var wasFinished = Division.Session?.IsFinished;
        Division.Session = snapshot;
        _engine = new AuctionEngine(_owner.Tournament, Division);
        _owner.AuctionChanged();
        Refresh();
        if (wasFinished != snapshot.IsFinished)
        {
            _owner.StatusChanged(reloadAuction: false);
        }
    }

    private bool CanUndo() => _history.CanUndo;
}

public sealed partial class TeamCardViewModel(Guid captainId, AuctionViewModel owner) : ObservableObject
{
    public Guid CaptainId { get; } = captainId;

    public ObservableCollection<PickItemViewModel> Picks { get; } = [];

    /// <summary>One placeholder per empty spot, so the cards show how many players are still missing.</summary>
    public ObservableCollection<int> EmptySlots { get; } = [];

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RemainingText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MaxBidText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SlotsText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IReadOnlyList<ClassCount> Composition { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<string> CaptainClasses { get; set; } = [];

    [ObservableProperty]
    public partial double BudgetLeftRatio { get; set; }

    [ObservableProperty]
    public partial bool CanBuy { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    internal void Update(AuctionEngine engine)
    {
        var team = engine.GetTeam(CaptainId);
        Name = team.CaptainName;
        RemainingText = Money.Format(team.Remaining);
        BudgetLeftRatio = team.InitialBudget > 0 ? (double)Math.Clamp(team.Remaining / team.InitialBudget, 0, 1) : 0;
        var slotsLeft = engine.SlotsLeft(team);
        CanBuy = !engine.Session.IsFinished && slotsLeft > 0;
        SlotsText = $"{team.Picks.Count}/{engine.Division.TeamSize}";
        MaxBidText = slotsLeft == 0
            ? "Team complete"
            : engine.Session.IsFinished
                ? $"{slotsLeft} spot(s) left"
                : $"Max bid {Money.Format(engine.MaxBid(team))}";
        CaptainClasses = AuctionViewModel.CaptainClasses(engine.Division, CaptainId);
        Composition = AuctionViewModel.Composition(engine.Division.CaptainClass(CaptainId), team.Picks);

        Picks.Clear();
        foreach (var pick in team.Picks)
        {
            Picks.Add(new PickItemViewModel(pick, !engine.Session.IsFinished, this, owner));
        }

        EmptySlots.Clear();
        for (var i = 0; i < slotsLeft; i++)
        {
            EmptySlots.Add(i);
        }
    }

    [RelayCommand]
    private void Select() => owner.Select(this);
}

/// <summary>A bought player on a team card. Clicking it opens a menu to fix the sale.</summary>
public sealed partial class PickItemViewModel(Pick pick, bool isRunning, TeamCardViewModel team, AuctionViewModel auction)
{
    public Guid PlayerId => pick.Player.Id;

    public string Name => pick.Player.Name;

    public decimal Price => pick.Price;

    /// <summary>Taking a player back needs a running auction; the other fixes also work once it's finished.</summary>
    public bool CanReturn => isRunning;

    public IReadOnlyList<string> Classes { get; } = AuctionViewModel.KnownClasses(pick.Player.Classes);

    public string PriceText => Money.Format(pick.Price);

    public string Header => $"{Name} — {team.Name}, {PriceText}";

    /// <summary>The teams this player can be moved to.</summary>
    public IReadOnlyList<TeamCardViewModel> OtherTeams => auction.OtherTeams(team).ToList();

    [RelayCommand]
    private void ReturnToBlock() => auction.ReturnToBlock(team, this);

    [RelayCommand]
    private void ReturnToSkipped() => auction.ReturnToSkipped(team, this);

    [RelayCommand]
    private void ChangePrice() => auction.ChangePickPrice(team, this);

    [RelayCommand]
    private void MoveTo(TeamCardViewModel target) => auction.MovePick(team, this, target);

    [RelayCommand]
    private void Swap() => auction.SwapPick(team, this);
}

/// <summary>How many players of one class a team has.</summary>
public sealed record ClassCount(string Code, int Count)
{
    public bool IsZero => Count == 0;
}

public sealed class PlayerItemViewModel(SessionPlayer player, int position)
{
    public Guid Id => player.Id;

    public string Name => player.Name;

    public int Position => position;

    public IReadOnlyList<string> Classes { get; } = AuctionViewModel.KnownClasses(player.Classes);
}
