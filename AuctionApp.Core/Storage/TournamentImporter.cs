using System.Text.Json;
using System.Text.Json.Nodes;
using AuctionApp.Core.Model;

namespace AuctionApp.Core.Storage;

/// <summary>
/// Reads tournament files exported by this app, player lists (CSV or text, e.g. saved from Excel), and the JSON files
/// of the previous (WinForms) version:
/// auction plans (<c>"type": "Auction"</c>) and auction states (<c>"type": "AuctionState"</c>), which become a
/// tournament with a single division. Exported tournaments keep their id, so they can be merged into the original.
/// </summary>
public static class TournamentImporter
{
    public static Tournament Import(string json, string fallbackTitle)
    {
        // Anything that isn't JSON is read as a player list (a CSV file saved from Excel, a text file...).
        if (!json.TrimStart('\uFEFF', ' ', '\t', '\r', '\n').StartsWith('{'))
        {
            return FromPlayerList(json, fallbackTitle);
        }

        JsonNode? root;
        try
        {
            root = JsonNode.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("This file isn't valid JSON: " + ex.Message, ex);
        }

        if (root is not JsonObject obj)
        {
            throw new InvalidDataException("This file doesn't contain a tournament.");
        }

        var legacyType = obj["type"]?.GetValueKind() == JsonValueKind.String ? obj["type"]!.GetValue<string>() : null;
        var tournament = legacyType switch
        {
            "Auction" => FromLegacyPlan(obj.Deserialize<LegacyAuction>(TournamentJson.Options)!),
            "AuctionState" => FromLegacyState(obj.Deserialize<LegacyAuctionState>(TournamentJson.Options)!),
            null when obj.ContainsKey("players") => Deserialize(json),
            _ => throw new InvalidDataException("This file isn't a draft cup tournament."),
        };

        if (string.IsNullOrWhiteSpace(tournament.Title))
        {
            tournament.Title = fallbackTitle;
        }

        if (tournament.Divisions.Count == 0)
        {
            tournament.AddDivision();
        }

        tournament.Normalize();
        return tournament;
    }

    /// <summary>
    /// A new tournament from a list of players: the old app's CSV export (<c>Player,INF,ARC,CAV</c> with x marks),
    /// or any list read by <see cref="RosterParser"/>.
    /// </summary>
    private static Tournament FromPlayerList(string text, string title)
    {
        var players = RosterParser.Parse(text);
        if (players.Count == 0)
        {
            throw new InvalidDataException("No players were found in this file. Use one player per line, with a column per class marked x (Player,INF,ARC,CAV).");
        }

        var tournament = new Tournament { Title = title };
        tournament.Players.AddRange(players.Select(player => new Player { Name = player.Name, Classes = player.Classes }));
        tournament.AddDivision();
        return tournament;
    }

    private static Tournament Deserialize(string json)
    {
        try
        {
            return TournamentJson.Deserialize(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException(ex.Message, ex);
        }
    }

    private static Tournament FromLegacyPlan(LegacyAuction legacy)
    {
        var tournament = new Tournament { Title = legacy.Title ?? string.Empty };
        tournament.Players = (legacy.Players ?? []).Select(p => new Player
        {
            Name = p.Name ?? string.Empty,
            Classes = PlayerClasses.Normalize(p.Classes ?? []),
        }).ToList();

        var division = tournament.AddDivision();
        division.TeamSize = ClampTeamSize(legacy.TeamSize);
        division.Captains = (legacy.Captains ?? []).Select(c => new Captain
        {
            Name = c.Name ?? string.Empty,
            Budget = c.Budget,
            Class = PlayerClasses.FromName(c.Class ?? string.Empty) ?? string.Empty,
        }).ToList();
        return tournament;
    }

    private static Tournament FromLegacyState(LegacyAuctionState legacy)
    {
        var tournament = new Tournament { Title = legacy.Title ?? string.Empty };
        var division = tournament.AddDivision();
        division.TeamSize = ClampTeamSize(legacy.TeamSize);
        division.HalfBudgetCapAtStart = legacy.HalfBudgetDisplay;

        static SessionPlayer ToSessionPlayer(string? name, List<string>? classes) => new()
        {
            Name = name ?? string.Empty,
            Classes = PlayerClasses.Normalize(classes ?? []),
        };

        var session = new AuctionSession
        {
            HalfBudgetCap = legacy.HalfBudgetDisplay,
            Queue = (legacy.PlayerQueue ?? []).Select(p => ToSessionPlayer(p.Name, p.Classes)).ToList(),
            Skipped = (legacy.Skipped ?? []).Select(p => ToSessionPlayer(p.Name, p.Classes)).ToList(),
        };

        foreach (var legacyTeam in legacy.Teams ?? [])
        {
            var captain = new Captain { Name = legacyTeam.Captain ?? string.Empty, Budget = legacyTeam.InitialBudget };
            division.Captains.Add(captain);
            session.Teams.Add(new SessionTeam
            {
                CaptainId = captain.Id,
                CaptainName = captain.Name,
                InitialBudget = captain.Budget,
                Picks = (legacyTeam.Members ?? []).Select(member => new Pick
                {
                    Player = ToSessionPlayer(member.Name, member.Classes),
                    Price = member.Cost,
                }).ToList(),
            });
        }

        // The pool lists everyone who took part, sold or not, with the same ids as in the auction.
        tournament.Players = session.AllPlayers()
            .Select(p => new Player { Id = p.Id, Name = p.Name, Classes = [.. p.Classes] })
            .ToList();

        session.Activity.Add(new ActivityEntry { Kind = ActivityKind.Info, Text = "Imported from an auction state file" });
        division.Session = session;
        return tournament;
    }

    private static int ClampTeamSize(int size) => size <= 0 ? 6 : Math.Clamp(size, Division.MinTeamSize, Division.MaxTeamSize);

    private sealed class LegacyAuction
    {
        public string? Title { get; set; }
        public int TeamSize { get; set; }
        public List<LegacyPlayer>? Players { get; set; }
        public List<LegacyCaptain>? Captains { get; set; }
    }

    private sealed class LegacyCaptain
    {
        public string? Name { get; set; }

        /// <summary>The old app's default budget.</summary>
        public decimal Budget { get; set; } = 20m;

        /// <summary>Not in the old app's files; accepted so captains' classes can be given in the same format.</summary>
        public string? Class { get; set; }
    }

    private sealed class LegacyPlayer
    {
        public string? Name { get; set; }
        public List<string>? Classes { get; set; }
    }

    private sealed class LegacyAuctionState
    {
        public string? Title { get; set; }
        public int TeamSize { get; set; }
        public bool HalfBudgetDisplay { get; set; } = true;
        public List<LegacyPlayer>? PlayerQueue { get; set; }
        public List<LegacyPlayer>? Skipped { get; set; }
        public List<LegacyTeam>? Teams { get; set; }
    }

    private sealed class LegacyTeam
    {
        public string? Captain { get; set; }
        public decimal InitialBudget { get; set; }
        public List<LegacyMember>? Members { get; set; }
    }

    private sealed class LegacyMember
    {
        public string? Name { get; set; }
        public decimal Cost { get; set; }
        public List<string>? Classes { get; set; }
    }
}
