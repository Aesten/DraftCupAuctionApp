using System.Text.Json.Serialization;

namespace AuctionApp.Core.Model;

/// <summary>
/// A draft cup: its setup (captains, players, stages) and, once started, its live auction session.
/// One draft is stored as one file; the user never has to deal with that file directly.
/// </summary>
public sealed class Draft
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = "New Draft Cup";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

    /// <summary>Number of players each captain buys (the captain excluded).</summary>
    public int TeamSize { get; set; } = 6;

    /// <summary>Budget given to newly added captains.</summary>
    public decimal DefaultBudget { get; set; } = 20m;

    /// <summary>Shuffle each stage's players when it starts. When off, players come up in list order.</summary>
    public bool ShuffleOrder { get; set; } = true;

    public List<Captain> Captains { get; set; } = [];

    public List<Player> Players { get; set; } = [];

    /// <summary>The auctions run one after the other (for example a high tier and a low tier). Always at least one.</summary>
    public List<Stage> Stages { get; set; } = [Stage.CreateDefault()];

    /// <summary>The live auction, or null while the draft is still being set up.</summary>
    public AuctionSession? Session { get; set; }

    [JsonIgnore]
    public DraftStatus Status => Session switch
    {
        null => DraftStatus.Setup,
        { IsFinished: true } => DraftStatus.Finished,
        _ => DraftStatus.InProgress,
    };

    /// <summary>Makes sure invariants hold after loading a file that may have been edited by hand or written by an older version.</summary>
    public void Normalize()
    {
        Title ??= string.Empty;
        Captains ??= [];
        Players ??= [];
        Stages ??= [];
        if (Stages.Count == 0)
        {
            Stages.Add(Stage.CreateDefault());
        }

        foreach (var player in Players)
        {
            player.Name ??= string.Empty;
            player.Classes ??= [];
            if (Stages.All(stage => stage.Id != player.StageId))
            {
                player.StageId = Stages[0].Id;
            }
        }

        foreach (var captain in Captains)
        {
            captain.Name ??= string.Empty;
        }

        Session?.Normalize();
    }

    public Stage? FindStage(Guid id) => Stages.FirstOrDefault(stage => stage.Id == id);

    /// <summary>Copies the setup into a brand new draft (no auction session), e.g. to reuse a roster for the next cup.</summary>
    public Draft CloneSetup(string title)
    {
        var stageMap = Stages.ToDictionary(stage => stage.Id, stage => stage with { Id = Guid.NewGuid() });
        return new Draft
        {
            Title = title,
            TeamSize = TeamSize,
            DefaultBudget = DefaultBudget,
            ShuffleOrder = ShuffleOrder,
            Captains = Captains.Select(captain => captain with { Id = Guid.NewGuid() }).ToList(),
            Stages = Stages.Select(stage => stageMap[stage.Id]).ToList(),
            Players = Players.Select(player => new Player
            {
                Name = player.Name,
                Classes = [.. player.Classes],
                StageId = stageMap.TryGetValue(player.StageId, out var stage) ? stage.Id : stageMap.Values.First().Id,
            }).ToList(),
        };
    }
}

public enum DraftStatus
{
    Setup,
    InProgress,
    Finished,
}

public sealed record Captain
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public decimal Budget { get; set; } = 20m;
}

public sealed class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    /// <summary>Class codes, see <see cref="PlayerClasses"/>.</summary>
    public List<string> Classes { get; set; } = [];

    /// <summary>The stage (tier) this player is auctioned in.</summary>
    public Guid StageId { get; set; }
}

public sealed record Stage
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = "Main auction";

    /// <summary>Maximum number of players a team may buy during this stage. Null means only the team size limits it.</summary>
    public int? MaxPicksPerTeam { get; set; }

    public static Stage CreateDefault() => new();
}

public static class PlayerClasses
{
    public const string Infantry = "inf";
    public const string Archer = "arc";
    public const string Cavalry = "cav";

    public static IReadOnlyList<string> All { get; } = [Infantry, Archer, Cavalry];

    public static string DisplayName(string code) => code switch
    {
        Infantry => "Infantry",
        Archer => "Archer",
        Cavalry => "Cavalry",
        _ => code,
    };

    public static string ShortName(string code) => code.ToUpperInvariant();

    /// <summary>Keeps known classes in canonical order and drops duplicates.</summary>
    public static List<string> Normalize(IEnumerable<string> classes)
    {
        var set = classes.Select(c => c.Trim().ToLowerInvariant()).Where(c => c.Length > 0).ToHashSet();
        var ordered = All.Where(set.Contains).ToList();
        ordered.AddRange(set.Where(c => !All.Contains(c)).Order(StringComparer.Ordinal));
        return ordered;
    }
}
