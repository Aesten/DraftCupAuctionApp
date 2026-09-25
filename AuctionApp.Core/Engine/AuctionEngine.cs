using System.Runtime.InteropServices;
using AuctionApp.Core.Model;

namespace AuctionApp.Core.Engine;

public sealed class AuctionException(string message) : Exception(message);

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

    public AuctionSession Session => Division.Session ?? throw new AuctionException("The auction hasn't started.");

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
        Shuffle(players);
        Division.Session = new AuctionSession
        {
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

    /// <summary>Returns why the current player can't be sold to this team at this price, or null if the sale is allowed.</summary>
    public string? CheckSale(Guid captainId, decimal price)
    {
        if (Session.IsFinished)
        {
            return "The auction is finished.";
        }

        if (Session.CurrentPlayer == null)
        {
            return "There is no player on the block.";
        }

        var team = Session.Teams.FirstOrDefault(t => t.CaptainId == captainId);
        if (team == null)
        {
            return "Select the team that won the bid.";
        }

        if (PriceProblem(price) is { } priceProblem)
        {
            return priceProblem;
        }

        if (SlotsLeft(team) == 0)
        {
            return $"{team.CaptainName}'s team is already full.";
        }

        var spendable = team.Spendable(Session.HalfBudgetCap);
        if (price > spendable)
        {
            return Session.HalfBudgetCap
                ? $"{team.CaptainName} can spend at most {Money.Format(Math.Max(0, spendable))} while the half budget cap is on."
                : $"{team.CaptainName} can't afford this ({Money.Format(team.Remaining)} left).";
        }

        return null;
    }

    public Pick Sell(Guid captainId, decimal price)
    {
        var problem = CheckSale(captainId, price);
        if (problem != null)
        {
            throw new AuctionException(problem);
        }

        var team = GetTeam(captainId);
        var player = Session.Queue[0];
        Session.Queue.RemoveAt(0);
        var pick = new Pick { Player = player, Price = price };
        team.Picks.Add(pick);
        Log(ActivityKind.Sold, $"{player.Name} sold to {team.CaptainName} for {Money.Format(price)}");
        return pick;
    }

    /// <summary>Moves the player on the block to the skipped list.</summary>
    public void Skip()
    {
        EnsureRunning();
        var player = Session.CurrentPlayer ?? throw new AuctionException("There is no player on the block.");
        Session.Queue.RemoveAt(0);
        Session.Skipped.Add(player);
        Log(ActivityKind.Skipped, $"{player.Name} skipped");
    }

    /// <summary>Puts a skipped player back on the block.</summary>
    public void BringBack(Guid playerId)
    {
        EnsureRunning();
        var player = Session.Skipped.FirstOrDefault(p => p.Id == playerId) ?? throw new AuctionException("That player isn't in the skipped list.");
        Session.Skipped.Remove(player);
        Session.Queue.Insert(0, player);
        Log(ActivityKind.Returned, $"{player.Name} is back on the block");
    }

    /// <summary>Sends every skipped player to the end of the queue for another round.</summary>
    public void RequeueSkipped()
    {
        EnsureRunning();
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
        Session.Queue.Insert(0, pick.Player);
        Log(ActivityKind.Returned, $"{pick.Player.Name} taken back from {team.CaptainName} (refunded {Money.Format(pick.Price)}), back on the block");
    }

    /// <summary>Takes a player back from a team (refunding the price) and puts them in the skipped list.</summary>
    public void ReturnPickToSkipped(Guid captainId, Guid playerId)
    {
        EnsureRunning();
        var (team, pick) = FindPick(captainId, playerId);
        team.Picks.Remove(pick);
        Session.Skipped.Add(pick.Player);
        Log(ActivityKind.Returned, $"{pick.Player.Name} taken back from {team.CaptainName} (refunded {Money.Format(pick.Price)}), sent to the skipped list");
    }

    /// <summary>
    /// Corrects the price of a sale. Corrections only check that the team doesn't go over its budget: the half budget
    /// cap applies to bids, not to fixing a typo afterwards.
    /// </summary>
    public void ChangePickPrice(Guid captainId, Guid playerId, decimal price)
    {
        var (team, pick) = FindPick(captainId, playerId);
        CheckPrice(price);
        if (price > team.Remaining + pick.Price)
        {
            throw new AuctionException($"{team.CaptainName} can't afford {Money.Format(price)} ({Money.Format(team.Remaining + pick.Price)} available).");
        }

        var old = pick.Price;
        pick.Price = price;
        Log(ActivityKind.Info, $"{pick.Player.Name}'s price changed from {Money.Format(old)} to {Money.Format(price)}");
    }

    /// <summary>Gives a bought player to another team at the same price (the first team is refunded).</summary>
    public void MovePick(Guid captainId, Guid playerId, Guid toCaptainId)
    {
        var (team, pick) = FindPick(captainId, playerId);
        var target = GetTeam(toCaptainId);
        if (target == team)
        {
            return;
        }

        if (SlotsLeft(target) == 0)
        {
            throw new AuctionException($"{target.CaptainName}'s team is already full.");
        }

        if (pick.Price > target.Remaining)
        {
            throw new AuctionException($"{target.CaptainName} can't afford {Money.Format(pick.Price)} ({Money.Format(target.Remaining)} left).");
        }

        team.Picks.Remove(pick);
        target.Picks.Add(pick);
        Log(ActivityKind.Info, $"{pick.Player.Name} moved from {team.CaptainName} to {target.CaptainName} ({Money.Format(pick.Price)})");
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
        Session.FinishedAt = DateTimeOffset.Now;
        Log(ActivityKind.Info, "Auction finished");
    }

    /// <summary>Reopens a finished auction; the unsold players go to the skipped list.</summary>
    public void Reopen()
    {
        if (!Session.IsFinished)
        {
            return;
        }

        Session.FinishedAt = null;
        Session.Skipped.AddRange(Session.Unsold);
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
        : !Money.IsWholeStep(price) ? $"Prices go in steps of {Money.Format(Money.Step)}."
        : null;

    private static void CheckPrice(decimal price)
    {
        if (PriceProblem(price) is { } problem)
        {
            throw new AuctionException(problem);
        }
    }

    private (SessionTeam Team, Pick Pick) FindPick(Guid captainId, Guid playerId)
    {
        var team = GetTeam(captainId);
        var pick = team.Picks.FirstOrDefault(p => p.Player.Id == playerId) ?? throw new AuctionException("That player isn't in this team.");
        return (team, pick);
    }

    private void Shuffle(List<SessionPlayer> players)
    {
        if (Division.ShuffleOrder)
        {
            _random.Shuffle(CollectionsMarshal.AsSpan(players));
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
