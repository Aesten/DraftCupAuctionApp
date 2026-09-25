using AuctionApp.Core.Model;

namespace AuctionApp.Core.Engine;

public enum PoolStatusKind
{
    /// <summary>Not bought anywhere: will be auctioned by the next division that starts.</summary>
    Available,

    /// <summary>Has the same name as a captain, so is never auctioned.</summary>
    Captain,

    /// <summary>Waiting in the queue or skipped list of a division whose auction is running.</summary>
    InAuction,

    /// <summary>Bought by a team.</summary>
    Picked,
}

public sealed record PoolStatus(PoolStatusKind Kind, Division? Division = null, string? CaptainName = null, decimal Price = 0);

/// <summary>How the shared player pool is split between divisions.</summary>
public static class TournamentRules
{
    /// <summary>
    /// The players a division can auction: the pool, in its order, minus the players bought in other divisions and
    /// the players who are captains somewhere in the tournament. Whichever division starts first gets the whole pool.
    /// </summary>
    public static List<Player> AvailablePlayers(Tournament tournament, Division division)
    {
        var taken = PickedPlayerIds(tournament, except: division);
        var captains = CaptainNames(tournament);
        return tournament.Players
            .Where(player => !taken.Contains(player.Id) && !captains.Contains(player.Name.Trim()))
            .ToList();
    }

    /// <summary>
    /// Pool players available to a running division that it doesn't know about yet: added to the pool after it
    /// started, or released by another division (e.g. an auction that was reset).
    /// </summary>
    public static List<Player> NewlyAvailable(Tournament tournament, Division division)
    {
        if (division.Session is not { } session)
        {
            return [];
        }

        var known = session.AllPlayers().Select(player => player.Id).ToHashSet();
        return AvailablePlayers(tournament, division).Where(player => !known.Contains(player.Id)).ToList();
    }

    public static HashSet<string> CaptainNames(Tournament tournament) =>
        tournament.Divisions
            .SelectMany(division => division.Captains)
            .Select(captain => captain.Name.Trim())
            .Where(name => name.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public static HashSet<Guid> PickedPlayerIds(Tournament tournament, Division? except = null) =>
        tournament.Divisions
            .Where(division => division != except && division.Session != null)
            .SelectMany(division => division.Session!.Teams)
            .SelectMany(team => team.Picks)
            .Select(pick => pick.Player.Id)
            .ToHashSet();

    /// <summary>Where every pool player currently stands.</summary>
    public static Dictionary<Guid, PoolStatus> PoolStatuses(Tournament tournament)
    {
        var statuses = new Dictionary<Guid, PoolStatus>();
        foreach (var division in tournament.Divisions)
        {
            if (division.Session is not { } session)
            {
                continue;
            }

            foreach (var team in session.Teams)
            {
                foreach (var pick in team.Picks)
                {
                    statuses.TryAdd(pick.Player.Id, new PoolStatus(PoolStatusKind.Picked, division, team.CaptainName, pick.Price));
                }
            }

            if (!session.IsFinished)
            {
                foreach (var player in session.Queue.Concat(session.Skipped))
                {
                    statuses.TryAdd(player.Id, new PoolStatus(PoolStatusKind.InAuction, division));
                }
            }
        }

        var captains = tournament.Divisions
            .SelectMany(division => division.Captains.Select(captain => (Name: captain.Name.Trim(), Division: division)))
            .Where(entry => entry.Name.Length > 0)
            .GroupBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().Division, StringComparer.OrdinalIgnoreCase);

        foreach (var player in tournament.Players)
        {
            if (!statuses.ContainsKey(player.Id))
            {
                statuses[player.Id] = captains.TryGetValue(player.Name.Trim(), out var division)
                    ? new PoolStatus(PoolStatusKind.Captain, division)
                    : new PoolStatus(PoolStatusKind.Available);
            }
        }

        return statuses;
    }

    /// <summary>Players bought in more than one division (possible when copies auctioned on different computers are merged).</summary>
    public static List<(string PlayerName, List<Division> Divisions)> DoublePicks(Tournament tournament) =>
        tournament.Divisions
            .Where(division => division.Session != null)
            .SelectMany(division => division.Session!.Teams.SelectMany(team => team.Picks).Select(pick => (pick.Player, Division: division)))
            .GroupBy(entry => entry.Player.Id)
            .Where(group => group.Select(entry => entry.Division).Distinct().Count() > 1)
            .Select(group => (group.First().Player.Name, group.Select(entry => entry.Division).Distinct().ToList()))
            .ToList();

    /// <summary>
    /// Takes players bought in one division out of the queue and skipped list of the other running auctions, so
    /// nobody can be sold twice (two auctions running at once, or a copy auctioned on another computer merged in).
    /// Returns what was removed, for each division.
    /// </summary>
    public static List<(Division Division, List<string> PlayerNames)> DropPlayersTakenElsewhere(Tournament tournament)
    {
        var dropped = new List<(Division, List<string>)>();
        foreach (var division in tournament.Divisions.Where(division => division.Status == DivisionStatus.InProgress))
        {
            var session = division.Session!;
            var taken = PickedPlayerIds(tournament, except: division);
            var removed = session.Queue.Concat(session.Skipped).Where(player => taken.Contains(player.Id)).ToList();
            if (removed.Count == 0)
            {
                continue;
            }

            session.Queue.RemoveAll(player => taken.Contains(player.Id));
            session.Skipped.RemoveAll(player => taken.Contains(player.Id));
            foreach (var player in removed)
            {
                var buyer = tournament.Divisions.First(other => other != division && other.Session?.Teams.Any(team => team.Picks.Any(pick => pick.Player.Id == player.Id)) == true);
                session.Activity.Add(new ActivityEntry { Kind = ActivityKind.Info, Text = $"{player.Name} was bought in {buyer.Name} and left this auction" });
            }

            dropped.Add((division, removed.Select(player => player.Name).ToList()));
        }

        return dropped;
    }

    /// <summary>Copies a pool player's edited name and classes into every auction that holds them.</summary>
    public static void SyncPlayer(Tournament tournament, Player player)
    {
        foreach (var sessionPlayer in tournament.Divisions
                     .Where(division => division.Session != null)
                     .SelectMany(division => division.Session!.AllPlayers())
                     .Where(sessionPlayer => sessionPlayer.Id == player.Id))
        {
            sessionPlayer.Name = player.Name;
            sessionPlayer.Classes = [.. player.Classes];
        }
    }

    /// <summary>
    /// Removing a pool player also takes them out of running auctions' queues. Returns why they can't be removed
    /// (already bought), or null.
    /// </summary>
    public static string? CanRemovePlayer(Tournament tournament, Player player)
    {
        var picked = tournament.Divisions.FirstOrDefault(division =>
            division.Session?.Teams.Any(team => team.Picks.Any(pick => pick.Player.Id == player.Id)) == true);
        return picked != null ? $"{player.Name} was bought in {picked.Name}. Take them back from their team first." : null;
    }

    public static void RemovePlayer(Tournament tournament, Player player)
    {
        tournament.Players.Remove(player);
        foreach (var session in tournament.Divisions.Select(division => division.Session).OfType<AuctionSession>())
        {
            session.Queue.RemoveAll(p => p.Id == player.Id);
            session.Skipped.RemoveAll(p => p.Id == player.Id);
            session.Unsold.RemoveAll(p => p.Id == player.Id);
        }
    }
}
