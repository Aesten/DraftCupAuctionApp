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
/// and is saved immediately. Random Pick: only the next few players are revealed so captains can't plan too far
/// ahead. Captain Pick: the players still available are on the pick board, by tier and class, and the auctioneer puts
/// the one a captain names on the block.
/// </summary>
public sealed partial class AuctionViewModel : ObservableObject
{
    private const string DefaultPrice = "0.1";

    private readonly DivisionViewModel _owner;
    private readonly UndoHistory _history = new();
    private AuctionEngine? _engine;
    private Guid? _lastOnBlock;

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

    /// <summary>Everyone still in the queue after the player on the block, sorted by name so the order stays hidden.</summary>
    public ObservableCollection<PlayerItemViewModel> Remaining { get; } = [];

    /// <summary>Captain Pick: the players still available, by tier and class.</summary>
    public ObservableCollection<BoardTierViewModel> Board { get; } = [];

    public bool IsCaptainPick => _owner.Tournament.IsCaptainPick;

    public bool IsRandomPick => !IsCaptainPick;

    /// <summary>Captain Pick: how many players of each class are left on the board (the board's column headers).</summary>
    [ObservableProperty]
    public partial IReadOnlyList<ClassCount> BoardClassTotals { get; set; } = [];

    /// <summary>Captain Pick: how many players are on the board (not bought yet).</summary>
    [ObservableProperty]
    public partial int BoardCount { get; set; }

    /// <summary>Captain Pick, while nobody is on the block: the button that opens the pick board.</summary>
    [ObservableProperty]
    public partial bool ShowsBoardButton { get; set; }

    /// <summary>Captain Pick: the tier of the player on the block and where bidding starts ("TIER 2 · FROM 1.5").</summary>
    [ObservableProperty]
    public partial string CurrentTierText { get; set; } = string.Empty;

    /// <summary>What the second counter above the stage counts: the queue, or the pick board.</summary>
    public string RemainingLabel => IsCaptainPick ? " on the board" : " in the queue";

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

    /// <summary>Shown in a small popup when Sold! is clicked but the sale breaks a rule.</summary>
    [ObservableProperty]
    public partial string SaleWarning { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsSaleWarningOpen { get; set; }

    /// <summary>The rule broken is a budget limit, which the auctioneer may override.</summary>
    [ObservableProperty]
    public partial bool CanSellAnyway { get; set; }

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
            Remaining.Clear();
            Board.Clear();
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
        CurrentTierText = current != null && session.CaptainPick && Tiers.IsValid(current.Tier)
            ? $"{Tiers.Name(current.Tier!.Value).ToUpperInvariant()}  ·  FROM {Money.Format(_engine.MinimumBid(current))}"
            : string.Empty;
        if (session.CaptainPick)
        {
            QueueEmptyText = session.Queue.Count > 0 ? "Waiting for a captain to pick a player." : "Everyone has been picked.";
            StageLabel = IsFinished ? "ALL DONE" : IsQueueEmpty ? "NEXT PICK" : "NOW ON THE BLOCK";
        }
        else
        {
            QueueEmptyText = session.Skipped.Count > 0
                ? $"Nobody is left in the queue, but {session.Skipped.Count} skipped player(s) can get another chance."
                : "Everyone has been auctioned.";
            StageLabel = IsFinished ? "ALL DONE" : IsQueueEmpty ? "QUEUE EMPTY" : "NOW ON THE BLOCK";
        }

        StageMessage = IsFinished ? "The auction is finished." : QueueEmptyText;

        // Captain Pick: a newly picked player starts at their tier's minimum, with no winner chosen yet.
        if (session.CaptainPick && current?.Id != _lastOnBlock)
        {
            if (current != null)
            {
                PriceText = Money.Format(_engine.MinimumBid(current));
                SelectedTeam = null;
            }

            _lastOnBlock = current?.Id;
        }

        SoldCount = session.SoldCount;
        // Captain Pick: the player on the block stays on the board (highlighted) until sold.
        RemainingCount = session.CaptainPick ? session.Queue.Count : Math.Max(0, session.Queue.Count - (current != null ? 1 : 0));
        BoardCount = session.CaptainPick && IsRunning ? session.Queue.Count : 0;
        ShowsBoardButton = session.CaptainPick && IsQueueEmpty && BoardCount > 0;
        RefreshBoard(session);
        SkippedCount = session.Skipped.Count;
        HasSkipped = SkippedCount > 0;

        // Only the next few players are revealed on stream.
        var shown = Division.UpcomingShown;
        ShowsUpNext = shown > 0 && IsRunning && !session.CaptainPick;
        UpNextHeader = shown == 1 ? "NEXT UP" : $"NEXT {shown}";
        UpNextRows = Math.Max(1, shown);
        Replace(UpNext, session.Queue.Skip(1).Take(shown).Select((player, i) => new PlayerItemViewModel(player, i + 1)));
        Replace(Skipped, session.Skipped.Select((player, i) => new PlayerItemViewModel(player, i + 1)));
        Replace(Remaining, session.Queue.Skip(1)
            .OrderBy(player => player.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select((player, i) => new PlayerItemViewModel(player, i + 1)));
        LastAction = session.Activity.LastOrDefault()?.Text ?? string.Empty;

        foreach (var team in Teams)
        {
            team.Update(_engine);
        }

        OnPropertyChanged(nameof(HalfBudgetCap));
        SellCommand.NotifyCanExecuteChanged();
        UpdateUndo();
    }

    /// <summary>
    /// Captain Pick: rebuilds the pick board, one card per tier with a column per class, names sorted alphabetically.
    /// Players picked leave the board; the one on the block stays, highlighted, until they are sold.
    /// </summary>
    private void RefreshBoard(AuctionSession session)
    {
        Board.Clear();
        if (!session.CaptainPick || session.IsFinished)
        {
            return;
        }

        foreach (var tier in Tiers.All)
        {
            var players = session.Queue.Where(player => player.Tier == tier).ToList();
            var columns = PlayerClasses.All
                .Select(code => new BoardColumnViewModel(
                    code,
                    players.Where(player => player.Classes.FirstOrDefault() == code)
                        .OrderBy(player => player.Name, StringComparer.CurrentCultureIgnoreCase)
                        .Select(player => new BoardPlayerViewModel(player, player.Id == session.OnBlockId, this))
                        .ToList()))
                .ToList();
            Board.Add(new BoardTierViewModel(tier, Money.Format(_owner.Tournament.MinimumBid(tier)), columns));
        }

        // Players without a tier or class (edited in the pool mid-auction) still need to be pickable.
        var others = session.Queue.Where(player => !Tiers.IsValid(player.Tier) || !PlayerClasses.All.Contains(player.Classes.FirstOrDefault() ?? string.Empty)).ToList();
        if (others.Count > 0)
        {
            var list = others.OrderBy(player => player.Name, StringComparer.CurrentCultureIgnoreCase)
                .Select(player => new BoardPlayerViewModel(player, player.Id == session.OnBlockId, this))
                .ToList();
            Board.Add(new BoardTierViewModel(0, string.Empty, [new BoardColumnViewModel(string.Empty, list)]));
        }

        BoardClassTotals = PlayerClasses.All
            .Select(code => new ClassCount(code, Board.Sum(tier => tier.Counts.FirstOrDefault(count => count.Code == code)?.Count ?? 0)))
            .ToList();
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

        IsSaleWarningOpen = false;
        SellCommand.NotifyCanExecuteChanged();
    }

    internal void Select(TeamCardViewModel team)
    {
        if (HasCurrentPlayer)
        {
            SelectedTeam = SelectedTeam == team ? null : team;
        }
    }

    /// <summary>The price box was left: shows the price as it will be used (e.g. "2,5" becomes "2.5").</summary>
    internal void CommitPrice()
    {
        if (Money.TryParse(PriceText, out var price))
        {
            PriceText = Money.Format(price);
        }
    }

    private bool CanSell() => HasCurrentPlayer && SelectedTeam != null;

    /// <summary>Sells to the selected team, or explains in a popup why not (offering to go over a budget limit).</summary>
    [RelayCommand(CanExecute = nameof(CanSell))]
    private void Sell()
    {
        if (_engine == null || SelectedTeam is not { } team)
        {
            return;
        }

        if (!Money.TryParse(PriceText, out var price))
        {
            ShowSaleWarning("Enter a price first.", canOverride: false);
            return;
        }

        if (_engine.CheckSale(team.CaptainId, price) is { } issue)
        {
            ShowSaleWarning(issue.CanOverride ? $"{issue.Message} Sell anyway for {Money.Format(price)}?" : issue.Message, issue.CanOverride);
            return;
        }

        Complete(team, price, overBudget: false);
    }

    [RelayCommand]
    private void SellAnyway()
    {
        IsSaleWarningOpen = false;
        if (SelectedTeam is { } team && Money.TryParse(PriceText, out var price))
        {
            Complete(team, price, overBudget: true);
        }
    }

    [RelayCommand]
    private void DismissSaleWarning() => IsSaleWarningOpen = false;

    private void ShowSaleWarning(string message, bool canOverride)
    {
        SaleWarning = message;
        CanSellAnyway = canOverride;
        IsSaleWarningOpen = true;
    }

    private void Complete(TeamCardViewModel team, decimal price, bool overBudget)
    {
        if (Apply($"sell {CurrentPlayerName} to {team.Name}", engine => engine.Sell(team.CaptainId, price, overBudget)))
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
        PriceText = Money.Format(Math.Clamp(decimal.Round(current, 1) + step, 0, Money.Max));
    }

    [RelayCommand]
    private void Skip()
    {
        if (HasCurrentPlayer)
        {
            Apply($"skip {CurrentPlayerName}", engine => engine.Skip());
        }
    }

    /// <summary>Captain Pick: the player a captain named goes on the block (from the pick board).</summary>
    internal void PutOnBlock(BoardPlayerViewModel player)
    {
        if (IsRunning && !player.IsOnBlock)
        {
            Apply($"pick {player.Name}", engine => engine.PutOnBlock(player.Id));
        }
    }

    /// <summary>Captain Pick: nobody wants the player after all, back on the board.</summary>
    [RelayCommand]
    private void ReturnToBoard()
    {
        if (HasCurrentPlayer)
        {
            Apply($"put {CurrentPlayerName} back on the board", engine => engine.ReturnToBoard());
        }
    }

    /// <summary>Random Pick: a player from the queue goes on the block now (from the remaining players list).</summary>
    internal void BringToBlock(PlayerItemViewModel player) =>
        Apply($"bring {player.Name} to the block", engine => engine.BringToBlock(player.Id));

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
        Apply(IsCaptainPick ? $"put {pick.Name} back on the board" : $"send {pick.Name} to the skipped list", engine => engine.ReturnPickToSkipped(team.CaptainId, pick.PlayerId));

    /// <summary>Corrects a sale's price; going over the team's budget is allowed once the auctioneer confirms it.</summary>
    internal void ChangePickPrice(TeamCardViewModel team, PickItemViewModel pick)
    {
        if (_engine == null
            || Dialogs.AskPrice($"Change {pick.Name}'s price", $"{team.Name} paid {pick.PriceText}. The difference is refunded or charged to {team.Name}.", pick.Price) is not { } price
            || price == pick.Price)
        {
            return;
        }

        var issue = _engine.CheckPickPrice(team.CaptainId, pick.PlayerId, price);
        if (Confirm(issue, $"Change {pick.Name}'s price anyway?", "Change anyway") is { } overBudget)
        {
            Apply($"change {pick.Name}'s price", engine => engine.ChangePickPrice(team.CaptainId, pick.PlayerId, price, overBudget));
        }
    }

    /// <summary>
    /// Moves a bought player to another team, which pays the same price (going over its budget needs the
    /// auctioneer's confirmation). Once the auction is finished, the move is free.
    /// </summary>
    internal void MovePick(TeamCardViewModel team, PickItemViewModel pick, TeamCardViewModel target)
    {
        if (_engine == null)
        {
            return;
        }

        var issue = _engine.CheckMovePick(team.CaptainId, pick.PlayerId, target.CaptainId);
        if (Confirm(issue, $"Move {pick.Name} to {target.Name} anyway?", "Move anyway") is not { } overBudget)
        {
            return;
        }

        if (IsFinished && issue == null && pick.Price > 0
            && Dialogs.Ask(
                $"Move {pick.Name} to {target.Name}?",
                $"The auction is finished, so the move is free: {team.Name} gets {pick.PriceText} back and {target.Name} pays nothing.",
                "Move") != DialogChoice.Primary)
        {
            return;
        }

        Apply($"move {pick.Name} to {target.Name}", engine => engine.MovePick(team.CaptainId, pick.PlayerId, target.CaptainId, overBudget));
    }

    /// <summary>
    /// For a correction that breaks a rule: null when it can't be done (the reason is shown) or the auctioneer
    /// declines; otherwise whether the budget limit is being overridden.
    /// </summary>
    private bool? Confirm(SaleIssue? issue, string question, string confirm)
    {
        if (issue == null)
        {
            return false;
        }

        if (!issue.CanOverride)
        {
            Dialogs.ShowError("That can't be done", issue.Message);
            return null;
        }

        return Dialogs.Ask(question, issue.Message, confirm) == DialogChoice.Primary ? true : null;
    }

    internal void SwapPick(TeamCardViewModel team, PickItemViewModel pick)
    {
        if (_engine == null)
        {
            return;
        }

        var session = _engine.Session;
        var choices = session.Queue.Select(player => Choice(player, player == session.CurrentPlayer ? "On the block" : session.CaptainPick ? "On the board" : "In the queue"))
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

    /// <summary>What the team can still spend: its budget left, or, while the half budget cap is on, what's left above the half.</summary>
    [ObservableProperty]
    public partial string BudgetText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BudgetToolTip { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string InitialBudgetText { get; set; } = string.Empty;

    // The budget bar: 0 to the starting budget, filled up to what's left, the reserved half hatched while capped.

    [ObservableProperty]
    public partial double InitialBudget { get; set; }

    [ObservableProperty]
    public partial double Remaining { get; set; }

    [ObservableProperty]
    public partial double Reserved { get; set; }

    [ObservableProperty]
    public partial bool IsCapped { get; set; }

    [ObservableProperty]
    public partial string SlotsText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IReadOnlyList<ClassCount> Composition { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<string> CaptainClasses { get; set; } = [];

    [ObservableProperty]
    public partial bool CanBuy { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    internal void Update(AuctionEngine engine)
    {
        var team = engine.GetTeam(CaptainId);
        Name = team.CaptainName;
        var slotsLeft = engine.SlotsLeft(team);
        CanBuy = !engine.Session.IsFinished && slotsLeft > 0;
        SlotsText = $"{team.Picks.Count}/{engine.Division.TeamSize}";

        // Like the previous version of the app: while the cap is on, the big number is what can be spent before the half.
        IsCapped = engine.Session.HalfBudgetCap && !engine.Session.IsFinished;
        var spendable = IsCapped && team.Remaining >= 0 ? Math.Max(0, team.Spendable(halfBudgetCap: true)) : team.Remaining;
        BudgetText = Money.Format(spendable);
        BudgetToolTip = IsCapped
            ? $"{Money.Format(spendable)} can be spent before the half budget cap ({Money.Format(team.Remaining)} left in total)"
            : $"{Money.Format(team.Remaining)} left";
        InitialBudgetText = Money.Format(team.InitialBudget);
        InitialBudget = (double)team.InitialBudget;
        Remaining = (double)team.Remaining;
        Reserved = (double)team.HalfBudgetReserve;
        CaptainClasses = AuctionViewModel.CaptainClasses(engine.Division, CaptainId);
        Composition = AuctionViewModel.Composition(engine.Division.CaptainClass(CaptainId), team.Picks);

        Picks.Clear();
        foreach (var pick in team.Picks)
        {
            Picks.Add(new PickItemViewModel(pick, !engine.Session.IsFinished, engine.Session.CaptainPick, this, owner));
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
public sealed partial class PickItemViewModel(Pick pick, bool isRunning, bool captainPick, TeamCardViewModel team, AuctionViewModel auction)
{
    /// <summary>The second way to take a player back: to the skipped list, or to the pick board in Captain Pick.</summary>
    public string ReturnToListText => captainPick ? "Refund and put back on the board" : "Refund and send to the skipped list";

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

/// <summary>Captain Pick: one tier of the pick board (tier 0: players missing a tier or a class).</summary>
public sealed class BoardTierViewModel(int tier, string minimumText, IReadOnlyList<BoardColumnViewModel> columns)
{
    public int Tier => tier;

    public string Title => tier > 0 ? Tiers.Name(tier) : "No tier or class";

    public string MinimumText => tier > 0 ? $"from {minimumText}" : string.Empty;

    public IReadOnlyList<BoardColumnViewModel> Columns => columns;

    public int Count => columns.Sum(column => column.Players.Count);

    /// <summary>How many players of each class are left in this tier (for the summary on the auction screen).</summary>
    public IReadOnlyList<ClassCount> Counts { get; } = columns.Where(column => column.Code.Length > 0).Select(column => new ClassCount(column.Code, column.Players.Count)).ToList();
}

/// <summary>Captain Pick: the players of one class in a tier of the pick board.</summary>
public sealed class BoardColumnViewModel(string code, IReadOnlyList<BoardPlayerViewModel> players)
{
    public string Code => code;

    public IReadOnlyList<BoardPlayerViewModel> Players => players;

    public int Count => players.Count;
}

/// <summary>Captain Pick: a player on the pick board. Clicking them puts them on the block.</summary>
public sealed partial class BoardPlayerViewModel(SessionPlayer player, bool isOnBlock, AuctionViewModel auction)
{
    public Guid Id => player.Id;

    public string Name => player.Name;

    public bool IsOnBlock => isOnBlock;

    [RelayCommand]
    private void Pick() => auction.PutOnBlock(this);
}
