using System.Runtime.InteropServices;
using AuctionApp.Core.Model;

namespace AuctionApp.Core.Engine;

public sealed class AuctionException(string message) : Exception(message);

/// <summary>Why a sale can't go through, and whether the auctioneer may confirm it anyway (budget limits only).</summary>
public sealed record SaleIssue(string Message, bool CanOverride);

/// <summary>All the rules of a division's auction. Every change to a session goes through here.</summary>
public sealed class AuctionEngine
{
    private readonly Random _random;

    public AuctionEngine(Tournament tournament, Division division, Random? random = null)
    {
        Tournament = tournament;
        Division = division;
        _random = random ?? Random.Shared;
    }

    public Tournament Tournament { get; }

    public Division Division { get; }

    /// <summary>Players are always auctioned in random order; tests can keep the pool's order to know who comes up.</summary>
    public bool KeepPoolOrder { get; init; }

    public AuctionSession Session => Division.Session ?? throw new AuctionException("The auction hasn't started.");

    /// <summary>Captain Pick: the price bidding starts at for this player.</summary>
    public decimal MinimumBid(SessionPlayer player) => Session.CaptainPick ? Tournament.MinimumBid(player.Tier) : 0m;

    /// <summary>Validates the division and creates its auction from the players still available in the pool.</summary>
    public void Start()
    {
        if (Division.Session != null)
        {
            throw new AuctionException("The auction has already started.");
        }

        var errors = DivisionValidator.Validate(Tournament, Division).Where(issue => issue.Severity == IssueSeverity.Error).ToList();
        if (errors.Count > 0)
        {
            throw new AuctionException(errors[0].Message);
        }

        var players = TournamentRules.AvailablePlayers(Tournament, Division).Select(SessionPlayer.From).ToList();
        if (!Tournament.IsCaptainPick)
        {
            Shuffle(players);
        }

        Division.Session = new AuctionSession
        {
            CaptainPick = Tournament.IsCaptainPick,
            HalfBudgetCap = Division.HalfBudgetCapAtStart,
            Queue = players,
            Teams = Division.Captains.Select(captain => new SessionTeam
            {
                CaptainId = captain.Id,
                CaptainName = captain.Name.Trim(),
                InitialBudget = captain.Budget,
            }).ToList(),
        };
        Log(ActivityKind.Info, $"Auction started with {players.Count} players");
    }

    public SessionTeam GetTeam(Guid captainId) =>
        Session.Teams.FirstOrDefault(team => team.CaptainId == captainId) ?? throw new AuctionException("Unknown team.");

    public int SlotsLeft(SessionTeam team) => Math.Max(0, Division.TeamSize - team.Picks.Count);

    /// <summary>The highest price this team may pay right now, or 0 when it can't buy at all.</summary>
    public decimal MaxBid(SessionTeam team) =>
        SlotsLeft(team) > 0 ? Math.Max(0, team.Spendable(Session.HalfBudgetCap)) : 0;

    /// <summary>
    /// Returns why the current player can't be sold to this team at this price, or null if the sale is allowed.
    /// Going over the budget (or the half budget cap) can be overridden by the auctioneer; the rest can't.
    /// </summary>
    public SaleIssue? CheckSale(Guid captainId, decimal price)
    {
        if (Session.IsFinished)
        {
            return new("The auction is finished.", CanOverride: false);
        }

        if (Session.CurrentPlayer is not { } player)
        {
            return new("There is no player on the block.", CanOverride: false);
        }

        var team = Session.Teams.FirstOrDefault(t => t.CaptainId == captainId);
        if (team == null)
        {
            return new("Select the bidding team.", CanOverride: false);
        }

        if (PriceProblem(price) is { } priceProblem)
        {
            return new(priceProblem, CanOverride: false);
        }

        if (SlotsLeft(team) == 0)
        {
            return new($"{team.CaptainName}'s team is already full.", CanOverride: false);
        }

        if (price < MinimumBid(player))
        {
            return new($"The minimum bid for {player.Name} ({Tiers.Name(player.Tier!.Value).ToLowerInvariant()}) is {Money.Format(MinimumBid(player))}.", CanOverride: true);
        }

        var spendable = team.Spendable(Session.HalfBudgetCap);
        if (price > spendable)
        {
            // While the cap is on, the team cards show what can be spent before the half: the warning says the same.
            return new(
                Session.HalfBudgetCap
                    ? $"{team.CaptainName} can only spend {Money.Format(Math.Max(0, spendable))} while the half budget cap is on ({Money.Format(team.Remaining)} left in total)."
                    : $"{team.CaptainName} only has {Money.Format(team.Remaining)} left.",
                CanOverride: true);
        }

        return null;
    }

    /// <summary>Sells the player on the block. <paramref name="overBudget"/>: the auctioneer allowed going over the budget.</summary>
    public Pick Sell(Guid captainId, decimal price, bool overBudget = false)
    {
        var issue = CheckSale(captainId, price);
        if (issue != null && !(overBudget && issue.CanOverride))
        {
            throw new AuctionException(issue.Message);
        }

        var team = GetTeam(captainId);
        var player = Session.CurrentPlayer!;
        Session.Queue.Remove(player);
        Session.OnBlockId = null;
        var pick = new Pick { Player = player, Price = price };
        team.Picks.Add(pick);
        Log(ActivityKind.Sold, $"{player.Name} sold to {team.CaptainName} for {Money.Format(price)}" + (issue == null ? string.Empty : price < MinimumBid(player) ? " (below the minimum, confirmed)" : " (over the limit, confirmed)"));
        return pick;
    }

    /// <summary>Captain Pick: puts the player a captain picked on the block (a player already there goes back to the board).</summary>
    public void PutOnBlock(Guid playerId)
    {
        EnsureRunning();
        EnsureCaptainPick();
        var player = Session.Queue.FirstOrDefault(p => p.Id == playerId) ?? throw new AuctionException("That player can't be picked.");
        if (Session.OnBlockId == playerId)
        {
            return;
        }

        Session.OnBlockId = playerId;
        Log(ActivityKind.Info, $"{player.Name} picked" + (Tiers.IsValid(player.Tier) ? $" ({Tiers.Name(player.Tier!.Value).ToLowerInvariant()}, from {Money.Format(MinimumBid(player))})" : string.Empty));
    }

    /// <summary>Captain Pick: takes the player off the block, back to the board, without a sale.</summary>
    public void ReturnToBoard()
    {
        EnsureRunning();
        EnsureCaptainPick();
        var player = Session.CurrentPlayer ?? throw new AuctionException("There is no player on the block.");
        Session.OnBlockId = null;
        Log(ActivityKind.Returned, $"{player.Name} back on the board");
    }

    /// <summary>Moves the player on the block to the skipped list.</summary>
    public void Skip()
    {
        EnsureRunning();
        EnsureRandomPick();
        var player = Session.CurrentPlayer ?? throw new AuctionException("There is no player on the block.");
        Session.Queue.RemoveAt(0);
        Session.Skipped.Add(player);
        Log(ActivityKind.Skipped, $"{player.Name} skipped");
    }

    /// <summary>Puts a skipped player back on the block.</summary>
    public void BringBack(Guid playerId)
    {
        EnsureRunning();
        EnsureRandomPick();
        var player = Session.Skipped.FirstOrDefault(p => p.Id == playerId) ?? throw new AuctionException("That player isn't in the skipped list.");
        Session.Skipped.Remove(player);
        Session.Queue.Insert(0, player);
        Log(ActivityKind.Returned, $"{player.Name} is back on the block");
    }

    /// <summary>
    /// Random Pick: puts a player waiting in the queue on the block now. The player who was on the block comes up
    /// right after.
    /// </summary>
    public void BringToBlock(Guid playerId)
    {
        EnsureRunning();
        EnsureRandomPick();
        var index = Session.Queue.FindIndex(p => p.Id == playerId);
        if (index < 0)
        {
            throw new AuctionException("That player isn't in the queue.");
        }

        if (index == 0)
        {
            return;
        }

        var player = Session.Queue[index];
        Session.Queue.RemoveAt(index);
        Session.Queue.Insert(0, player);
        Log(ActivityKind.Returned, $"{player.Name} brought to the block");
    }

    /// <summary>Sends every skipped player to the end of the queue for another round.</summary>
    public void RequeueSkipped()
    {
        EnsureRunning();
        EnsureRandomPick();
        if (Session.Skipped.Count == 0)
        {
            return;
        }

        var players = Session.Skipped.ToList();
        Shuffle(players);
        Session.Queue.AddRange(players);
        Session.Skipped.Clear();
        Log(ActivityKind.Returned, $"{players.Count} skipped player(s) sent back to the queue");
    }

    /// <summary>Takes a player back from a team (refunding the price) and puts them back on the block.</summary>
    public void ReturnPick(Guid captainId, Guid playerId)
    {
        EnsureRunning();
        var (team, pick) = FindPick(captainId, playerId);
        team.Picks.Remove(pick);
        if (Session.CaptainPick)
        {
            Session.Queue.Add(pick.Player);
            Session.OnBlockId = pick.Player.Id;
        }
        else
        {
            Session.Queue.Insert(0, pick.Player);
        }

        Log(ActivityKind.Returned, $"{pick.Player.Name} taken back from {team.CaptainName} (refunded {Money.Format(pick.Price)}), back on the block");
    }

    /// <summary>
    /// Takes a player back from a team (refunding the price) and puts them in the skipped list, or back on the board
    /// in Captain Pick.
    /// </summary>
    public void ReturnPickToSkipped(Guid captainId, Guid playerId)
    {
        EnsureRunning();
        var (team, pick) = FindPick(captainId, playerId);
        team.Picks.Remove(pick);
        if (Session.CaptainPick)
        {
            Session.Queue.Add(pick.Player);
            Log(ActivityKind.Returned, $"{pick.Player.Name} taken back from {team.CaptainName} (refunded {Money.Format(pick.Price)}), back on the board");
            return;
        }

        Session.Skipped.Add(pick.Player);
        Log(ActivityKind.Returned, $"{pick.Player.Name} taken back from {team.CaptainName} (refunded {Money.Format(pick.Price)}), sent to the skipped list");
    }

    /// <summary>
    /// Why this price correction needs the auctioneer's confirmation (the team would go over its budget), or null.
    /// The half budget cap applies to bids, not to fixing a typo afterwards. A price out of range can't be used at all.
    /// </summary>
    public SaleIssue? CheckPickPrice(Guid captainId, Guid playerId, decimal price)
    {
        var (team, pick) = FindPick(captainId, playerId);
        if (PriceProblem(price) is { } problem)
        {
            return new(problem, CanOverride: false);
        }

        var available = team.Remaining + pick.Price;
        return price > available
            ? new($"{team.CaptainName} would go over their budget: {Money.Format(price)} for {Money.Format(available)} available.", CanOverride: true)
            : null;
    }

    /// <summary>Corrects the price of a sale. <paramref name="overBudget"/>: the auctioneer allowed going over the budget.</summary>
    public void ChangePickPrice(Guid captainId, Guid playerId, decimal price, bool overBudget = false)
    {
        var issue = CheckPickPrice(captainId, playerId, price);
        if (issue != null && !(overBudget && issue.CanOverride))
        {
            throw new AuctionException(issue.Message);
        }

        var (_, pick) = FindPick(captainId, playerId);
        var old = pick.Price;
        pick.Price = price;
        Log(ActivityKind.Info, $"{pick.Player.Name}'s price changed from {Money.Format(old)} to {Money.Format(price)}" + (issue != null ? " (over the budget, confirmed)" : string.Empty));
    }

    /// <summary>
    /// The price a moved player costs the other team: the same, or nothing once the auction is finished (a trade
    /// between teams, not a sale).
    /// </summary>
    public decimal MovePrice(Pick pick) => Session.IsFinished ? 0m : pick.Price;

    /// <summary>
    /// Why moving this player to the other team can't be done (a full team), or needs the auctioneer's confirmation
    /// (the other team can't afford them), or null.
    /// </summary>
    public SaleIssue? CheckMovePick(Guid captainId, Guid playerId, Guid toCaptainId)
    {
        var (team, pick) = FindPick(captainId, playerId);
        var target = GetTeam(toCaptainId);
        if (target == team)
        {
            return null;
        }

        if (SlotsLeft(target) == 0)
        {
            return new($"{target.CaptainName}'s team is already full.", CanOverride: false);
        }

        var price = MovePrice(pick);
        return price > target.Remaining
            ? new($"{target.CaptainName} would go over their budget: {Money.Format(price)} for {Money.Format(target.Remaining)} left.", CanOverride: true)
            : null;
    }

    /// <summary>
    /// Gives a bought player to another team, which pays the same price (the first team is refunded). Once the auction
    /// is finished, the player moves for free. <paramref name="overBudget"/>: the auctioneer allowed going over the budget.
    /// </summary>
    public void MovePick(Guid captainId, Guid playerId, Guid toCaptainId, bool overBudget = false)
    {
        var (team, pick) = FindPick(captainId, playerId);
        var target = GetTeam(toCaptainId);
        if (target == team)
        {
            return;
        }

        var issue = CheckMovePick(captainId, playerId, toCaptainId);
        if (issue != null && !(overBudget && issue.CanOverride))
        {
            throw new AuctionException(issue.Message);
        }

        pick.Price = MovePrice(pick);
        team.Picks.Remove(pick);
        target.Picks.Add(pick);
        Log(ActivityKind.Info, $"{pick.Player.Name} moved from {team.CaptainName} to {target.CaptainName} ({Money.Format(pick.Price)})" + (issue != null ? " (over the budget, confirmed)" : string.Empty));
    }

    /// <summary>
    /// Replaces a bought player with a player not bought yet, at the same price. The replaced player takes the other
    /// one's place (on the block, in the queue, in the skipped list or among the unsold).
    /// </summary>
    public void SwapPick(Guid captainId, Guid playerId, Guid otherPlayerId)
    {
        var (team, pick) = FindPick(captainId, playerId);
        var list = new[] { Session.Queue, Session.Skipped, Session.Unsold }.FirstOrDefault(l => l.Any(p => p.Id == otherPlayerId))
            ?? throw new AuctionException("That player isn't available in this auction.");
        var index = list.FindIndex(p => p.Id == otherPlayerId);
        var other = list[index];
        list[index] = pick.Player;
        var old = pick.Player;
        if (Session.OnBlockId == other.Id)
        {
            Session.OnBlockId = old.Id;
        }

        pick.Player = other;
        Log(ActivityKind.Info, $"{old.Name} swapped with {other.Name} in {team.CaptainName}'s team ({Money.Format(pick.Price)})");
    }

    /// <summary>Closes the auction. Whoever wasn't sold is listed as unsold and stays available to other divisions.</summary>
    public void Finish()
    {
        EnsureRunning();
        Session.Unsold.AddRange(Session.Queue);
        Session.Unsold.AddRange(Session.Skipped);
        Session.Queue.Clear();
        Session.Skipped.Clear();
        Session.OnBlockId = null;
        Session.FinishedAt = DateTimeOffset.Now;
        Log(ActivityKind.Info, "Auction finished");
    }

    /// <summary>Reopens a finished auction; the unsold players go to the skipped list (back on the board in Captain Pick).</summary>
    public void Reopen()
    {
        if (!Session.IsFinished)
        {
            return;
        }

        Session.FinishedAt = null;
        (Session.CaptainPick ? Session.Queue : Session.Skipped).AddRange(Session.Unsold);
        Session.Unsold.Clear();
        Log(ActivityKind.Info, "Auction reopened");
    }

    public void SetHalfBudgetCap(bool enabled)
    {
        if (Session.HalfBudgetCap == enabled)
        {
            return;
        }

        Session.HalfBudgetCap = enabled;
        Log(ActivityKind.Info, enabled ? "Half budget cap turned on" : "Half budget cap turned off");
    }

    private static string? PriceProblem(decimal price) =>
        price < 0 ? "The price can't be negative."
        : price > Money.Max ? $"Prices go up to {Money.Format(Money.Max)}."
        : !Money.IsWholeStep(price) ? $"Prices go in steps of {Money.Format(Money.Step)}."
        : null;

    private (SessionTeam Team, Pick Pick) FindPick(Guid captainId, Guid playerId)
    {
        var team = GetTeam(captainId);
        var pick = team.Picks.FirstOrDefault(p => p.Player.Id == playerId) ?? throw new AuctionException("That player isn't in this team.");
        return (team, pick);
    }

    private void Shuffle(List<SessionPlayer> players)
    {
        if (!KeepPoolOrder)
        {
            _random.Shuffle(CollectionsMarshal.AsSpan(players));
        }
    }

    private void EnsureCaptainPick()
    {
        if (!Session.CaptainPick)
        {
            throw new AuctionException("Players are picked by captains only in Captain Pick.");
        }
    }

    private void EnsureRandomPick()
    {
        if (Session.CaptainPick)
        {
            throw new AuctionException("Captain Pick has no queue or skipped list.");
        }
    }

    private void EnsureRunning()
    {
        if (Session.IsFinished)
        {
            throw new AuctionException("The auction is finished.");
        }
    }

    private void Log(ActivityKind kind, string text) =>
        Session.Activity.Add(new ActivityEntry { Kind = kind, Text = text, At = DateTimeOffset.Now });
}
