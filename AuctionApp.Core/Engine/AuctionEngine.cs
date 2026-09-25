using AuctionApp.Core.Model;

namespace AuctionApp.Core.Engine;

public sealed class AuctionException(string message) : Exception(message);

/// <summary>All the rules of a running auction. Every change to a session goes through here.</summary>
public sealed class AuctionEngine
{
    private readonly Random _random;

    public AuctionEngine(Draft draft, Random? random = null)
    {
        Draft = draft;
        _random = random ?? Random.Shared;
    }

    public Draft Draft { get; }

    public AuctionSession Session => Draft.Session ?? throw new AuctionException("The auction hasn't started.");

    public Stage CurrentStage => Draft.Stages[Math.Clamp(Session.StageIndex, 0, Draft.Stages.Count - 1)];

    public bool HasNextStage => Session.StageIndex < Draft.Stages.Count - 1;

    public Stage? NextStage => HasNextStage ? Draft.Stages[Session.StageIndex + 1] : null;

    /// <summary>Players of the current stage that are neither sold nor waiting: the queue and the skipped list.</summary>
    public int UnsoldInStage => Session.Queue.Count + Session.Skipped.Count;

    /// <summary>Validates the setup and creates the auction session.</summary>
    public void Start()
    {
        if (Draft.Session != null)
        {
            throw new AuctionException("The auction has already started.");
        }

        var errors = DraftValidator.Validate(Draft).Where(issue => issue.Severity == IssueSeverity.Error).ToList();
        if (errors.Count > 0)
        {
            throw new AuctionException(errors[0].Message);
        }

        Draft.Normalize();
        Draft.Session = new AuctionSession
        {
            Teams = Draft.Captains.Select(captain => new SessionTeam
            {
                CaptainId = captain.Id,
                CaptainName = captain.Name.Trim(),
                InitialBudget = captain.Budget,
            }).ToList(),
            Waiting = Draft.Players.Select(SessionPlayer.From).ToList(),
        };

        Log(ActivityKind.Info, "Auction started");
        LoadStage(0, []);
    }

    public SessionTeam GetTeam(Guid captainId) =>
        Session.Teams.FirstOrDefault(team => team.CaptainId == captainId) ?? throw new AuctionException("Unknown team.");

    public int SlotsLeft(SessionTeam team) => Math.Max(0, Draft.TeamSize - team.Picks.Count);

    /// <summary>How many more players the team may buy in the current stage (team size and stage limit combined).</summary>
    public int PicksLeftThisStage(SessionTeam team)
    {
        var left = SlotsLeft(team);
        if (CurrentStage.MaxPicksPerTeam is { } max)
        {
            left = Math.Min(left, Math.Max(0, max - team.PicksInStage(CurrentStage.Id)));
        }

        return left;
    }

    /// <summary>The highest price this team may pay right now, or 0 when it can't buy at all.</summary>
    public decimal MaxBid(SessionTeam team) =>
        PicksLeftThisStage(team) > 0 ? Math.Max(0, team.Spendable(Session.HalfBudgetCap)) : 0;

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

        if (PicksLeftThisStage(team) == 0)
        {
            return $"{team.CaptainName} already bought {CurrentStage.MaxPicksPerTeam} player(s) in {CurrentStage.Name}, the most allowed.";
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
        var pick = new Pick { Player = player, Price = price, StageId = CurrentStage.Id };
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
        if (Draft.ShuffleOrder)
        {
            _random.Shuffle(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(players));
        }

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

    /// <summary>
    /// Ends the current stage and starts the next one. Players of the current stage that weren't sold are either
    /// carried into the next stage or set aside as unsold.
    /// </summary>
    public void AdvanceStage(bool carryUnsold)
    {
        EnsureRunning();
        if (!HasNextStage)
        {
            throw new AuctionException("This is the last stage.");
        }

        var leftovers = Session.Queue.Concat(Session.Skipped).ToList();
        Session.Queue.Clear();
        Session.Skipped.Clear();
        if (!carryUnsold)
        {
            Session.Unsold.AddRange(leftovers);
        }

        LoadStage(Session.StageIndex + 1, carryUnsold ? leftovers : []);
    }

    /// <summary>Closes the auction. Whoever wasn't sold is listed as unsold.</summary>
    public void Finish()
    {
        EnsureRunning();
        Session.Unsold.AddRange(Session.Queue);
        Session.Unsold.AddRange(Session.Skipped);
        Session.Unsold.AddRange(Session.Waiting);
        Session.Queue.Clear();
        Session.Skipped.Clear();
        Session.Waiting.Clear();
        Session.FinishedAt = DateTimeOffset.Now;
        Log(ActivityKind.Info, "Auction finished");
    }

    /// <summary>Reopens a finished auction on its last stage; the unsold players go to the skipped list.</summary>
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

    private void LoadStage(int index, List<SessionPlayer> carried)
    {
        var stage = Draft.Stages[index];
        Session.StageIndex = index;

        var incoming = Session.Waiting.Where(player => player.StageId == stage.Id).ToList();
        Session.Waiting.RemoveAll(player => player.StageId == stage.Id);

        var pool = incoming.Concat(carried).ToList();
        if (Draft.ShuffleOrder)
        {
            _random.Shuffle(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(pool));
        }

        Session.Queue.AddRange(pool);

        var detail = carried.Count > 0 ? $"{incoming.Count} player(s) + {carried.Count} carried over" : $"{incoming.Count} player(s)";
        Log(ActivityKind.Stage, $"{stage.Name} started ({detail})");
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
