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

        if (price < 0)
        {
            return "The price can't be negative.";
        }

        if (!Money.IsWholeStep(price))
        {
            return $"Prices go in steps of {Money.Format(Money.Step)}.";
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
        var team = GetTeam(captainId);
        var pick = team.Picks.FirstOrDefault(p => p.Player.Id == playerId) ?? throw new AuctionException("That player isn't in this team.");
        team.Picks.Remove(pick);
        Session.Queue.Insert(0, pick.Player);
        Log(ActivityKind.Returned, $"{pick.Player.Name} taken back from {team.CaptainName} (refunded {Money.Format(pick.Price)})");
    }

    /// <summary>Adds pool players that became available after the auction started to the end of the queue.</summary>
    public void AddToQueue(IReadOnlyCollection<Player> players)
    {
        EnsureRunning();
        var known = Session.AllPlayers().Select(p => p.Id).ToHashSet();
        var added = players.Where(player => !known.Contains(player.Id)).Select(SessionPlayer.From).ToList();
        if (added.Count == 0)
        {
            return;
        }

        Shuffle(added);
        Session.Queue.AddRange(added);
        Log(ActivityKind.Info, $"{added.Count} new player(s) added to the queue");
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
