using AuctionApp.Core.Model;

namespace AuctionApp.Core.Engine;

public enum IssueSeverity
{
    Warning,
    Error,
}

public sealed record ValidationIssue(IssueSeverity Severity, string Message);

/// <summary>Checks that a division is ready for its auction.</summary>
public static class DivisionValidator
{
    public static IReadOnlyList<ValidationIssue> Validate(Tournament tournament, Division division)
    {
        var issues = new List<ValidationIssue>();

        void Error(string message) => issues.Add(new ValidationIssue(IssueSeverity.Error, message));
        void Warning(string message) => issues.Add(new ValidationIssue(IssueSeverity.Warning, message));

        if (division.TeamSize is < Division.MinTeamSize or > Division.MaxTeamSize)
        {
            Error($"Teams must have between {Division.MinTeamSize} and {Division.MaxTeamSize} players besides the captain.");
        }

        if (division.Captains.Count < 2)
        {
            Error("Add at least two captains.");
        }

        if (division.Captains.Any(captain => string.IsNullOrWhiteSpace(captain.Name)))
        {
            Error("Every captain needs a name.");
        }

        foreach (var name in Duplicates(division.Captains.Select(captain => captain.Name)))
        {
            Error($"Captain \"{name}\" is listed more than once.");
        }

        if (division.Captains.Any(captain => captain.Budget < 0))
        {
            Error("Budgets can't be negative.");
        }

        var available = TournamentRules.AvailablePlayers(tournament, division);
        var needed = division.Captains.Count * division.TeamSize;
        if (tournament.Players.Count == 0)
        {
            Error("The player pool is empty.");
        }
        else if (available.Count == 0)
        {
            Error("Every player of the pool has already been bought in another division.");
        }
        else if (available.Count < needed)
        {
            Warning($"Only {available.Count} players are available for {needed} spots, so some teams won't be full.");
        }

        var unnamed = tournament.Players.Count(player => string.IsNullOrWhiteSpace(player.Name));
        if (unnamed > 0)
        {
            Warning($"{unnamed} player(s) in the pool have no name and won't be auctioned.");
        }

        foreach (var other in tournament.Divisions.Where(other => other != division && other.Status == DivisionStatus.InProgress))
        {
            Warning($"The auction of {other.Name} isn't finished. Players it hasn't sold yet are also available here.");
        }

        return issues;
    }

    private static IEnumerable<string> Duplicates(IEnumerable<string> names) =>
        names.Where(name => !string.IsNullOrWhiteSpace(name))
            .GroupBy(name => name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);
}
