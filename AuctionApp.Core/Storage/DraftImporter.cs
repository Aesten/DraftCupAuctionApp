using System.Text.Json;
using System.Text.Json.Nodes;
using AuctionApp.Core.Model;

namespace AuctionApp.Core.Storage;

/// <summary>
/// Reads drafts exported by this app as well as the JSON files of the previous (WinForms) version:
/// auction plans (<c>"type": "Auction"</c>) and auction states (<c>"type": "AuctionState"</c>).
/// Imported drafts always get a new id so they never overwrite an existing one.
/// </summary>
public static class DraftImporter
{
    public static Draft Import(string json, string fallbackTitle)
    {
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
            throw new InvalidDataException("This file doesn't contain a draft.");
        }

        var legacyType = obj["type"]?.GetValue<string>();
        var draft = legacyType switch
        {
            "Auction" => FromLegacyPlan(obj.Deserialize<LegacyAuction>(DraftJson.Options)!),
            "AuctionState" => FromLegacyState(obj.Deserialize<LegacyAuctionState>(DraftJson.Options)!),
            null when obj.ContainsKey("captains") || obj.ContainsKey("players") => DraftJson.Deserialize(json),
            _ => throw new InvalidDataException("This file isn't a draft cup auction file."),
        };

        draft.Id = Guid.NewGuid();
        if (string.IsNullOrWhiteSpace(draft.Title))
        {
            draft.Title = fallbackTitle;
        }

        draft.Normalize();
        return draft;
    }

    private static Draft FromLegacyPlan(LegacyAuction legacy)
    {
        var draft = new Draft
        {
            Title = legacy.Title ?? string.Empty,
            TeamSize = legacy.TeamSize > 0 ? legacy.TeamSize : 6,
        };

        var stage = draft.Stages[0];
        draft.Captains = (legacy.Captains ?? []).Select(c => new Captain { Name = c.Name ?? string.Empty, Budget = c.Budget }).ToList();
        if (draft.Captains.Count > 0)
        {
            draft.DefaultBudget = draft.Captains[0].Budget;
        }

        draft.Players = (legacy.Players ?? []).Select(p => new Player
        {
            Name = p.Name ?? string.Empty,
            Classes = PlayerClasses.Normalize(p.Classes ?? []),
            StageId = stage.Id,
        }).ToList();
        return draft;
    }

    private static Draft FromLegacyState(LegacyAuctionState legacy)
    {
        var draft = new Draft
        {
            Title = legacy.Title ?? string.Empty,
            TeamSize = legacy.TeamSize > 0 ? legacy.TeamSize : 6,
        };
        var stage = draft.Stages[0];

        SessionPlayer ToSessionPlayer(LegacyPlayer p) => new()
        {
            Name = p.Name ?? string.Empty,
            Classes = PlayerClasses.Normalize(p.Classes ?? []),
            StageId = stage.Id,
        };

        var session = new AuctionSession
        {
            HalfBudgetCap = legacy.HalfBudgetDisplay,
            Queue = (legacy.PlayerQueue ?? []).Select(ToSessionPlayer).ToList(),
            Skipped = (legacy.Skipped ?? []).Select(ToSessionPlayer).ToList(),
        };

        foreach (var legacyTeam in legacy.Teams ?? [])
        {
            var captain = new Captain { Name = legacyTeam.Captain ?? string.Empty, Budget = legacyTeam.InitialBudget };
            draft.Captains.Add(captain);
            session.Teams.Add(new SessionTeam
            {
                CaptainId = captain.Id,
                CaptainName = captain.Name,
                InitialBudget = captain.Budget,
                Picks = (legacyTeam.Members ?? []).Select(member => new Pick
                {
                    Player = ToSessionPlayer(new LegacyPlayer { Name = member.Name, Classes = member.Classes }),
                    Price = member.Cost,
                    StageId = stage.Id,
                }).ToList(),
            });
        }

        if (draft.Captains.Count > 0)
        {
            draft.DefaultBudget = draft.Captains[0].Budget;
        }

        // The setup lists everyone who took part, sold or not.
        var everyone = session.Teams.SelectMany(team => team.Picks).Select(pick => pick.Player)
            .Concat(session.Queue)
            .Concat(session.Skipped);
        draft.Players = everyone.Select(p => new Player { Id = p.Id, Name = p.Name, Classes = [.. p.Classes], StageId = stage.Id }).ToList();

        session.Activity.Add(new ActivityEntry { Kind = ActivityKind.Info, Text = "Imported from an auction state file" });
        draft.Session = session;
        return draft;
    }

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
        public decimal Budget { get; set; }
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
