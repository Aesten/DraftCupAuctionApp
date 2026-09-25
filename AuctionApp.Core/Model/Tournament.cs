using System.Text.Json.Serialization;

namespace AuctionApp.Core.Model;

/// <summary>
/// A tournament: one shared player pool and any number of divisions, each with its own auction.
/// A tournament is the unit that gets saved, exported and moved between computers.
/// </summary>
public sealed class Tournament
{
    public const int CurrentSchemaVersion = 2;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    /// <summary>Stays the same across computers, so an exported copy can be merged back into the original.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = "New tournament";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    /// <summary>Last change anywhere in the tournament.</summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

    /// <summary>Last change to the title or the player pool (used when merging copies).</summary>
    public DateTimeOffset PoolUpdatedAt { get; set; } = DateTimeOffset.Now;

    /// <summary>The player pool, in its manual order (used by divisions that don't shuffle).</summary>
    public List<Player> Players { get; set; } = [];

    public List<Division> Divisions { get; set; } = [];

    public Division? FindDivision(Guid id) => Divisions.FirstOrDefault(division => division.Id == id);

    public Player? FindPlayer(Guid id) => Players.FirstOrDefault(player => player.Id == id);

    public void TouchPool()
    {
        PoolUpdatedAt = DateTimeOffset.Now;
        UpdatedAt = PoolUpdatedAt;
    }

    public Division AddDivision()
    {
        var number = Divisions.Count + 1;
        while (Divisions.Any(division => division.Name == $"Division {number}"))
        {
            number++;
        }

        var division = new Division { Name = $"Division {number}" };
        Divisions.Add(division);
        UpdatedAt = DateTimeOffset.Now;
        return division;
    }

    /// <summary>Makes sure invariants hold after loading a file that may be older or edited by hand.</summary>
    public void Normalize()
    {
        Title ??= string.Empty;
        Players ??= [];
        Divisions ??= [];
        foreach (var player in Players)
        {
            player.Name ??= string.Empty;
            player.Classes ??= [];
        }

        foreach (var division in Divisions)
        {
            division.Normalize();
        }
    }

    /// <summary>A fresh copy with the same pool and division settings but no auction results, for the next event.</summary>
    public Tournament CloneWithoutResults(string title) => new()
    {
        Title = title,
        Players = Players.Select(player => new Player { Name = player.Name, Classes = [.. player.Classes] }).ToList(),
        Divisions = Divisions.Select(division => new Division
        {
            Name = division.Name,
            TeamSize = division.TeamSize,
            ShuffleOrder = division.ShuffleOrder,
            UpcomingShown = division.UpcomingShown,
            HalfBudgetCapAtStart = division.HalfBudgetCapAtStart,
            Captains = division.Captains.Select(captain => new Captain { Name = captain.Name, Budget = captain.Budget }).ToList(),
        }).ToList(),
    };
}

public sealed class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    /// <summary>Class codes, see <see cref="PlayerClasses"/>.</summary>
    public List<string> Classes { get; set; } = [];
}

/// <summary>One auction of the tournament: its captains, rules and (once started) its live session.</summary>
public sealed class Division
{
    public const int MinTeamSize = 5;
    public const int MaxTeamSize = 10;

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "Division 1";

    /// <summary>Last change to this division, its settings or its auction (used when merging copies).</summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

    /// <summary>Players each captain buys (the captain not included).</summary>
    public int TeamSize { get; set; } = 6;

    public List<Captain> Captains { get; set; } = [];

    /// <summary>Shuffle the players when the auction starts. When off, they come up in the pool's order.</summary>
    public bool ShuffleOrder { get; set; } = true;

    /// <summary>How many upcoming players the auction screen reveals (the rest of the queue stays hidden).</summary>
    public int UpcomingShown { get; set; } = 3;

    public bool HalfBudgetCapAtStart { get; set; } = true;

    public AuctionSession? Session { get; set; }

    [JsonIgnore]
    public DivisionStatus Status => Session switch
    {
        null => DivisionStatus.NotStarted,
        { IsFinished: true } => DivisionStatus.Finished,
        _ => DivisionStatus.InProgress,
    };

    public void Touch() => UpdatedAt = DateTimeOffset.Now;

    public void Normalize()
    {
        Name ??= string.Empty;
        Captains ??= [];
        foreach (var captain in Captains)
        {
            captain.Name ??= string.Empty;
        }

        UpcomingShown = Math.Clamp(UpcomingShown, 0, 10);
        Session?.Normalize();
    }
}

public enum DivisionStatus
{
    NotStarted,
    InProgress,
    Finished,
}

public sealed class Captain
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    /// <summary>Each captain has their own budget, balanced against their skill.</summary>
    public decimal Budget { get; set; } = 20m;
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
