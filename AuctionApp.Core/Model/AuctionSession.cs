using System.Text.Json.Serialization;

namespace AuctionApp.Core.Model;

/// <summary>
/// The live state of an auction. It holds its own copies of players and captains, so the setup is locked while it exists.
/// </summary>
public sealed class AuctionSession
{
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? FinishedAt { get; set; }

    [JsonIgnore]
    public bool IsFinished => FinishedAt.HasValue;

    /// <summary>Index into <see cref="Draft.Stages"/> of the stage currently running.</summary>
    public int StageIndex { get; set; }

    /// <summary>When on, a team may only spend down to half of its initial budget (the other half stays reserved).</summary>
    public bool HalfBudgetCap { get; set; } = true;

    /// <summary>Players still to be auctioned in the current stage, the first one being on the block.</summary>
    public List<SessionPlayer> Queue { get; set; } = [];

    /// <summary>Players of the current stage nobody bought (yet).</summary>
    public List<SessionPlayer> Skipped { get; set; } = [];

    /// <summary>Players of later stages that were not auctioned yet.</summary>
    public List<SessionPlayer> Waiting { get; set; } = [];

    /// <summary>Players left unsold when the auction finished.</summary>
    public List<SessionPlayer> Unsold { get; set; } = [];

    public List<SessionTeam> Teams { get; set; } = [];

    /// <summary>What happened, newest last.</summary>
    public List<ActivityEntry> Activity { get; set; } = [];

    [JsonIgnore]
    public SessionPlayer? CurrentPlayer => Queue.Count > 0 ? Queue[0] : null;

    [JsonIgnore]
    public int SoldCount => Teams.Sum(team => team.Picks.Count);

    public void Normalize()
    {
        Queue ??= [];
        Skipped ??= [];
        Waiting ??= [];
        Unsold ??= [];
        Teams ??= [];
        Activity ??= [];
        foreach (var team in Teams)
        {
            team.Picks ??= [];
            team.CaptainName ??= string.Empty;
        }

        foreach (var player in Queue.Concat(Skipped).Concat(Waiting).Concat(Unsold).Concat(Teams.SelectMany(t => t.Picks).Select(p => p.Player)))
        {
            player.Name ??= string.Empty;
            player.Classes ??= [];
        }
    }
}

public sealed record SessionPlayer
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public List<string> Classes { get; set; } = [];

    public Guid StageId { get; set; }

    public static SessionPlayer From(Player player) => new()
    {
        Id = player.Id,
        Name = player.Name,
        Classes = [.. player.Classes],
        StageId = player.StageId,
    };
}

public sealed class SessionTeam
{
    public Guid CaptainId { get; set; }

    public string CaptainName { get; set; } = string.Empty;

    public decimal InitialBudget { get; set; }

    public List<Pick> Picks { get; set; } = [];

    [JsonIgnore]
    public decimal Spent => Picks.Sum(pick => pick.Price);

    [JsonIgnore]
    public decimal Remaining => InitialBudget - Spent;

    /// <summary>The amount the half budget rule keeps in reserve (half of the initial budget, rounded up to 0.1).</summary>
    [JsonIgnore]
    public decimal HalfBudgetReserve => Math.Ceiling(InitialBudget * 10m / 2m) / 10m;

    /// <summary>What the team can still spend, taking the half budget rule into account when it's on.</summary>
    public decimal Spendable(bool halfBudgetCap) => halfBudgetCap ? Remaining - HalfBudgetReserve : Remaining;

    public int PicksInStage(Guid stageId) => Picks.Count(pick => pick.StageId == stageId);
}

public sealed class Pick
{
    public SessionPlayer Player { get; set; } = new();

    public decimal Price { get; set; }

    /// <summary>The stage during which the player was bought.</summary>
    public Guid StageId { get; set; }

    public DateTimeOffset At { get; set; } = DateTimeOffset.Now;
}

public sealed class ActivityEntry
{
    public DateTimeOffset At { get; set; } = DateTimeOffset.Now;

    public ActivityKind Kind { get; set; }

    public string Text { get; set; } = string.Empty;
}

public enum ActivityKind
{
    Info,
    Sold,
    Skipped,
    Returned,
    Stage,
}
