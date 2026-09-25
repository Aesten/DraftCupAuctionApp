using AuctionApp.Core.Model;

namespace AuctionApp.Core.Engine;

public enum IssueSeverity
{
    Warning,
    Error,
}

public sealed record ValidationIssue(IssueSeverity Severity, string Message);

/// <summary>Checks that a draft's setup is complete enough to start an auction.</summary>
public static class DraftValidator
{
    public static IReadOnlyList<ValidationIssue> Validate(Draft draft)
    {
        var issues = new List<ValidationIssue>();

        void Error(string message) => issues.Add(new ValidationIssue(IssueSeverity.Error, message));
        void Warning(string message) => issues.Add(new ValidationIssue(IssueSeverity.Warning, message));

        if (draft.TeamSize < 1)
        {
            Error("Team size must be at least 1.");
        }

        if (draft.Captains.Count < 2)
        {
            Error("Add at least two captains.");
        }

        if (draft.Captains.Any(captain => string.IsNullOrWhiteSpace(captain.Name)))
        {
            Error("Every captain needs a name.");
        }

        foreach (var name in Duplicates(draft.Captains.Select(captain => captain.Name)))
        {
            Error($"Captain \"{name}\" is listed more than once.");
        }

        if (draft.Captains.Any(captain => captain.Budget < 0))
        {
            Error("Budgets can't be negative.");
        }

        if (draft.Players.Count == 0)
        {
            Error("Add some players to auction.");
        }

        if (draft.Players.Any(player => string.IsNullOrWhiteSpace(player.Name)))
        {
            Error("Every player needs a name.");
        }

        foreach (var name in Duplicates(draft.Players.Select(player => player.Name)))
        {
            Warning($"Player \"{name}\" is listed more than once.");
        }

        var needed = draft.Captains.Count * draft.TeamSize;
        if (draft.Players.Count > 0 && draft.Players.Count < needed)
        {
            Warning($"Not enough players to fill every team ({draft.Players.Count} players for {needed} spots).");
        }

        if (draft.Stages.Count > 1)
        {
            var firstStage = draft.Stages[0];
            if (draft.Players.All(player => player.StageId != firstStage.Id) && draft.Players.Count > 0)
            {
                Warning($"No players are assigned to the first stage \"{firstStage.Name}\".");
            }

            foreach (var stage in draft.Stages.Skip(1).Where(stage => draft.Players.All(player => player.StageId != stage.Id)))
            {
                Warning($"No players are assigned to \"{stage.Name}\" (it will only contain unsold players carried over).");
            }
        }

        return issues;
    }

    private static IEnumerable<string> Duplicates(IEnumerable<string> names) =>
        names.Where(name => !string.IsNullOrWhiteSpace(name))
            .GroupBy(name => name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);
}
