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

    /// <summary>How players come up for auction. Part of the pool: it decides whether players have a tier and one class.</summary>
    public AuctionFormat Format { get; set; } = AuctionFormat.RandomPick;

    [JsonIgnore]
    public bool IsCaptainPick => Format == AuctionFormat.CaptainPick;

    /// <summary>Captain Pick: the minimum bid for each tier (index 0 is tier 1). Part of the pool, like the tiers.</summary>
    public List<decimal> TierMinimums { get; set; } = [.. Tiers.DefaultMinimums];

    /// <summary>Captain Pick: the price bidding starts at for a player of this tier (0 without a tier).</summary>
    public decimal MinimumBid(int? tier) => Tiers.IsValid(tier) ? TierMinimums[tier!.Value - 1] : 0m;

    /// <summary>The format can change until a division's auction starts.</summary>
    [JsonIgnore]
    public bool CanChangeFormat => Divisions.All(division => division.Session == null);

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
        TierMinimums ??= [];
        TierMinimums = TierMinimums.Take(Tiers.Count).Select(minimum => Math.Max(0m, decimal.Round(minimum, 1))).ToList();
        TierMinimums.AddRange(Tiers.DefaultMinimums.Skip(TierMinimums.Count));
        foreach (var player in Players)
        {
            player.Name ??= string.Empty;
            player.Classes = PlayerClasses.Normalize(player.Classes ?? []);
            player.Tier = Tiers.IsValid(player.Tier) ? player.Tier : null;
            if (IsCaptainPick && player.Classes.Count > 1)
            {
                player.Classes = [player.Classes[0]];
            }
        }

        foreach (var division in Divisions)
        {
            division.Normalize();
        }
    }

    /// <summary>
    /// Switches between Random Pick and Captain Pick. In Captain Pick a player plays one class, so players with
    /// several keep only the first one. Returns how many players lost a class.
    /// </summary>
    public int SetFormat(AuctionFormat format)
    {
        if (format == Format)
        {
            return 0;
        }

        if (!CanChangeFormat)
        {
            throw new InvalidOperationException("The format can't change once an auction has started.");
        }

        Format = format;
        var trimmed = 0;
        if (IsCaptainPick)
        {
            foreach (var player in Players.Where(player => player.Classes.Count > 1))
            {
                player.Classes = [player.Classes[0]];
                trimmed++;
            }
        }

        TouchPool();
        return trimmed;
    }

    /// <summary>A fresh copy with the same pool and division settings but no auction results, for the next event.</summary>
    public Tournament CloneWithoutResults(string title) => new()
    {
        Title = title,
        Format = Format,
        TierMinimums = [.. TierMinimums],
        Players = Players.Select(player => new Player { Name = player.Name, Classes = [.. player.Classes], Tier = player.Tier }).ToList(),
        Divisions = Divisions.Select(division => new Division
        {
            Name = division.Name,
            TeamSize = division.TeamSize,
            UpcomingShown = division.UpcomingShown,
            HalfBudgetCapAtStart = division.HalfBudgetCapAtStart,
            Captains = division.Captains.Select(captain => new Captain { Name = captain.Name, Budget = captain.Budget, Class = captain.Class }).ToList(),
        }).ToList(),
    };
}

public sealed class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    /// <summary>Class codes, see <see cref="PlayerClasses"/>. In Captain Pick, a single class.</summary>
    public List<string> Classes { get; set; } = [];

    /// <summary>Captain Pick only: the player's tier, 1 (best) to <see cref="Tiers.Count"/>, which sets their minimum bid.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Tier { get; set; }
}

public enum AuctionFormat
{
    /// <summary>Players come up in a random order; nobody bid on them → skipped.</summary>
    RandomPick,

    /// <summary>Captains name the player they want; bidding starts at the minimum of the player's tier.</summary>
    CaptainPick,
}

/// <summary>Captain Pick tiers: 1 is the best. Each tier has a minimum bid, set for the whole tournament.</summary>
public static class Tiers
{
    public const int Count = 5;

    public static IReadOnlyList<decimal> DefaultMinimums { get; } = [2.0m, 1.5m, 1.0m, 0.5m, 0.1m];

    public static IEnumerable<int> All => Enumerable.Range(1, Count);

    public static bool IsValid(int? tier) => tier is >= 1 and <= Count;

    public static string Name(int tier) => $"Tier {tier}";
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

    /// <summary>
    /// A team is a slot led by a captain; the captain's name (and class) can change during the auction, and the team
    /// follows. Budgets and the list of captains stay locked while the auction exists.
    /// </summary>
    public void SyncCaptain(Captain captain)
    {
        if (Session?.Teams.FirstOrDefault(team => team.CaptainId == captain.Id) is { } team)
        {
            team.CaptainName = captain.Name.Trim();
        }
    }

    /// <summary>The class of the captain leading a team, or empty.</summary>
    public string CaptainClass(Guid captainId) => Captains.FirstOrDefault(captain => captain.Id == captainId)?.Class ?? string.Empty;

    public void Normalize()
    {
        Name ??= string.Empty;
        Captains ??= [];
        foreach (var captain in Captains)
        {
            captain.Name ??= string.Empty;
            captain.Class = PlayerClasses.FromName(captain.Class ?? string.Empty) ?? string.Empty;
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

    /// <summary>The single class the captain signed up with (a <see cref="PlayerClasses"/> code), or empty.</summary>
    public string Class { get; set; } = string.Empty;
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

    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["inf"] = Infantry,
        ["infantry"] = Infantry,
        ["arc"] = Archer,
        ["archer"] = Archer,
        ["archers"] = Archer,
        ["ranged"] = Archer,
        ["cav"] = Cavalry,
        ["cavalry"] = Cavalry,
    };

    /// <summary>The class code for a name like "inf", "Infantry" or "archer", or null if it isn't a class.</summary>
    public static string? FromName(string name) => Aliases.GetValueOrDefault(name.Trim());

    /// <summary>Keeps known classes in canonical order and drops duplicates. Names like "Infantry" become codes.</summary>
    public static List<string> Normalize(IEnumerable<string> classes)
    {
        var set = classes.Select(c => FromName(c) ?? c.Trim().ToLowerInvariant()).Where(c => c.Length > 0).ToHashSet();
        var ordered = All.Where(set.Contains).ToList();
        ordered.AddRange(set.Where(c => !All.Contains(c)).Order(StringComparer.Ordinal));
        return ordered;
    }
}
