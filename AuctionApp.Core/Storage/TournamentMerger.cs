using AuctionApp.Core.Model;

namespace AuctionApp.Core.Storage;

public sealed record MergeResult(Tournament Tournament, IReadOnlyList<string> Changes)
{
    public bool HasChanges => Changes.Count > 0;
}

/// <summary>
/// Combines two copies of the same tournament, e.g. the organizer's and the one an auctioneer ran a division on.
/// Each part is taken from whichever copy changed it last: the title and player pool as a whole, and each division
/// on its own. Divisions only present in one copy are kept.
/// </summary>
public static class TournamentMerger
{
    public static MergeResult Merge(Tournament mine, Tournament theirs)
    {
        if (mine.Id != theirs.Id)
        {
            throw new InvalidOperationException("Only copies of the same tournament can be merged.");
        }

        var result = TournamentJson.Clone(mine);
        var incoming = TournamentJson.Clone(theirs);
        var changes = new List<string>();

        if (incoming.PoolUpdatedAt > result.PoolUpdatedAt)
        {
            if (result.Title != incoming.Title)
            {
                changes.Add($"Title changed to \"{incoming.Title}\"");
            }

            if (!SamePool(result, incoming))
            {
                changes.Add($"Player pool updated ({incoming.Players.Count} players)");
            }

            result.Title = incoming.Title;
            result.Players = incoming.Players;
            result.PoolUpdatedAt = incoming.PoolUpdatedAt;
        }

        foreach (var division in incoming.Divisions)
        {
            var index = result.Divisions.FindIndex(existing => existing.Id == division.Id);
            if (index < 0)
            {
                result.Divisions.Add(division);
                changes.Add($"{division.Name} added{Describe(division)}");
            }
            else if (division.UpdatedAt > result.Divisions[index].UpdatedAt)
            {
                result.Divisions[index] = division;
                changes.Add($"{division.Name} updated{Describe(division)}");
            }
        }

        if (changes.Count > 0)
        {
            result.UpdatedAt = DateTimeOffset.Now;
        }

        return new MergeResult(result, changes);
    }

    private static string Describe(Division division) => division.Status switch
    {
        DivisionStatus.InProgress => $" (auction in progress, {division.Session!.SoldCount} sold)",
        DivisionStatus.Finished => $" (auction finished, {division.Session!.SoldCount} sold)",
        _ => string.Empty,
    };

    private static bool SamePool(Tournament a, Tournament b) =>
        a.Players.Count == b.Players.Count
        && a.Players.Zip(b.Players).All(pair =>
            pair.First.Id == pair.Second.Id
            && pair.First.Name == pair.Second.Name
            && pair.First.Classes.SequenceEqual(pair.Second.Classes));
}
