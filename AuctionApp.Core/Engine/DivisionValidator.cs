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

        if (division.Captains.Any(captain => !Money.IsValidBudget(captain.Budget)))
        {
            Error($"Budgets go from {Money.Format(Money.MinBudget)} to {Money.Format(Money.Max)}, in steps of {Money.Format(Money.Step)}.");
        }

        var classless = division.Captains.Where(captain => captain.Class.Length == 0 && !string.IsNullOrWhiteSpace(captain.Name)).Select(captain => captain.Name.Trim()).ToList();
        if (classless.Count > 0)
        {
            Warning($"{(classless.Count == 1 ? "Captain" : "Captains")} {string.Join(", ", classless)} {(classless.Count == 1 ? "has" : "have")} no class: the team's class counts won't include them.");
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

        if (tournament.IsCaptainPick)
        {
            var noTier = available.Where(player => !Tiers.IsValid(player.Tier)).Select(player => player.Name).ToList();
            if (noTier.Count > 0)
            {
                Error($"{Count(noTier.Count)} {(noTier.Count == 1 ? "has" : "have")} no tier yet: {Names(noTier)}.");
            }

            var noClass = available.Where(player => player.Classes.Count == 0).Select(player => player.Name).ToList();
            if (noClass.Count > 0)
            {
                Error($"{Count(noClass.Count)} {(noClass.Count == 1 ? "has" : "have")} no class yet: {Names(noClass)}.");
            }
        }

        foreach (var name in Duplicates(available.Select(player => player.Name)))
        {
            Warning($"\"{name}\" is in the pool more than once: viewers won't be able to tell them apart.");
        }

        var unnamed = tournament.Players.Count(player => string.IsNullOrWhiteSpace(player.Name));
        if (unnamed > 0)
        {
            Warning($"{unnamed} player(s) in the pool have no name and won't be auctioned.");
        }

        // One auction at a time: two running at once would offer the same players.
        foreach (var other in tournament.Divisions.Where(other => other != division && other.Status == DivisionStatus.InProgress))
        {
            Error($"The {other.Name} auction is still running. Finish it first: only one auction can run at a time.");
        }

        return issues;
    }

    /// <summary>
    /// While the auction runs, only the captains can still change (names always, the rest once unlocked): warnings
    /// about what would look wrong on stream.
    /// </summary>
    public static IReadOnlyList<ValidationIssue> ValidateRunning(Division division)
    {
        var issues = new List<ValidationIssue>();
        var unnamed = division.Captains
            .Select((captain, index) => (captain, number: index + 1))
            .Where(entry => string.IsNullOrWhiteSpace(entry.captain.Name))
            .Select(entry => entry.number.ToString())
            .ToList();
        if (unnamed.Count > 0)
        {
            issues.Add(new ValidationIssue(
                IssueSeverity.Warning,
                $"{(unnamed.Count == 1 ? "Captain" : "Captains")} {string.Join(", ", unnamed)} {(unnamed.Count == 1 ? "has" : "have")} no name: {(unnamed.Count == 1 ? "their team shows" : "their teams show")} without one."));
        }

        foreach (var name in Duplicates(division.Captains.Select(captain => captain.Name)))
        {
            issues.Add(new ValidationIssue(IssueSeverity.Warning, $"Captain \"{name}\" is listed more than once: viewers won't be able to tell the teams apart."));
        }

        return issues;
    }

    private static string Count(int players) => players == 1 ? "1 player" : $"{players} players";

    private static string Names(List<string> names) =>
        string.Join(", ", names.Take(5)) + (names.Count > 5 ? $" and {names.Count - 5} more" : string.Empty);

    private static IEnumerable<string> Duplicates(IEnumerable<string> names) =>
        names.Where(name => !string.IsNullOrWhiteSpace(name))
            .GroupBy(name => name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);
}
