using System.Text.Json.Serialization;

namespace AuctionApp.Core.Model;

/// <summary>
/// The live state of a division's auction. Teams are created from the division's captains when it starts, and
/// players are copied from the pool, so the division's settings are locked while it exists.
/// </summary>
public sealed class AuctionSession
{
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? FinishedAt { get; set; }

    [JsonIgnore]
    public bool IsFinished => FinishedAt.HasValue;

    /// <summary>When on, a team may only spend down to half of its initial budget (the other half stays reserved).</summary>
    public bool HalfBudgetCap { get; set; } = true;

    /// <summary>
    /// Captain Pick: captains name the player they want instead of players coming up in order. The queue is then the
    /// board of players still available, and <see cref="OnBlockId"/> the one being bid on.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool CaptainPick { get; set; }

    /// <summary>Captain Pick: the player picked and being bid on, or null while waiting for a pick.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? OnBlockId { get; set; }

    /// <summary>
    /// Players still to be auctioned. Random Pick: in order, the first one being on the block. Captain Pick: the
    /// players captains can still pick.
    /// </summary>
    public List<SessionPlayer> Queue { get; set; } = [];

    /// <summary>Players nobody bought (yet).</summary>
    public List<SessionPlayer> Skipped { get; set; } = [];

    /// <summary>Players left unsold when the auction finished. They stay available to later divisions.</summary>
    public List<SessionPlayer> Unsold { get; set; } = [];

    public List<SessionTeam> Teams { get; set; } = [];

    /// <summary>What happened, newest last.</summary>
    public List<ActivityEntry> Activity { get; set; } = [];

    [JsonIgnore]
    public SessionPlayer? CurrentPlayer => CaptainPick
        ? Queue.FirstOrDefault(player => player.Id == OnBlockId)
        : Queue.Count > 0 ? Queue[0] : null;

    [JsonIgnore]
    public int SoldCount => Teams.Sum(team => team.Picks.Count);

    /// <summary>Every player this auction knows about, sold or not.</summary>
    public IEnumerable<SessionPlayer> AllPlayers() =>
        Queue.Concat(Skipped).Concat(Unsold).Concat(Teams.SelectMany(team => team.Picks).Select(pick => pick.Player));

    public void Normalize()
    {
        Queue ??= [];
        Skipped ??= [];
        Unsold ??= [];
        Teams ??= [];
        Activity ??= [];
        foreach (var team in Teams)
        {
            team.Picks ??= [];
            team.CaptainName ??= string.Empty;
        }

        foreach (var player in AllPlayers())
        {
            player.Name ??= string.Empty;
            player.Classes ??= [];
        }

        if (OnBlockId is { } id && Queue.All(player => player.Id != id))
        {
            OnBlockId = null;
        }
    }
}

/// <summary>A player as the auction sees them; the id links back to the pool.</summary>
public sealed class SessionPlayer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public List<string> Classes { get; set; } = [];

    /// <summary>Captain Pick: the player's tier (see <see cref="Tiers"/>).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Tier { get; set; }

    public static SessionPlayer From(Player player) => new()
    {
        Id = player.Id,
        Name = player.Name,
        Classes = [.. player.Classes],
        Tier = player.Tier,
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
}

public sealed class Pick
{
    public SessionPlayer Player { get; set; } = new();

    public decimal Price { get; set; }

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
}
