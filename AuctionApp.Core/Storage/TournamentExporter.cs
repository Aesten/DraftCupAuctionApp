using System.Globalization;
using System.Text;
using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;

namespace AuctionApp.Core.Storage;

/// <summary>Turns a tournament into files or text meant for people: tournament files, spreadsheets and chat messages.</summary>
public static class TournamentExporter
{
    /// <summary>The whole tournament, to hand over to another computer and merge back later.</summary>
    public static string ToFile(Tournament tournament) => TournamentJson.Serialize(tournament);

    /// <summary>The player pool as a spreadsheet: name, one column per class, and where each player ended up.</summary>
    public static string PlayersToCsv(Tournament tournament)
    {
        var statuses = TournamentRules.PoolStatuses(tournament);
        var csv = new StringBuilder();
        AppendRow(csv, ["Player", .. PlayerClasses.All.Select(PlayerClasses.ShortName), "Division", "Team", "Price"]);
        foreach (var player in tournament.Players)
        {
            var status = statuses[player.Id];
            var picked = status.Kind == PoolStatusKind.Picked;
            AppendRow(csv,
            [
                player.Name,
                .. PlayerClasses.All.Select(c => player.Classes.Contains(c) ? "x" : string.Empty),
                picked ? status.Division!.Name : string.Empty,
                picked ? status.CaptainName! : string.Empty,
                picked ? status.Price.ToString("0.0", CultureInfo.InvariantCulture) : string.Empty,
            ]);
        }

        return csv.ToString();
    }

    /// <summary>Every sale of a division, team by team, as a spreadsheet.</summary>
    public static string ResultsToCsv(Division division)
    {
        var session = division.Session ?? throw new InvalidOperationException("The auction hasn't started.");
        var csv = new StringBuilder();
        AppendRow(csv, ["Captain", "Player", "Classes", "Price"]);
        foreach (var team in session.Teams)
        {
            foreach (var pick in team.Picks)
            {
                AppendRow(csv,
                [
                    team.CaptainName,
                    pick.Player.Name,
                    string.Join(" ", pick.Player.Classes.Select(PlayerClasses.ShortName)),
                    pick.Price.ToString("0.0", CultureInfo.InvariantCulture),
                ]);
            }
        }

        return csv.ToString();
    }

    /// <summary>A readable summary of a division's teams, formatted to paste in Discord or similar.</summary>
    public static string ResultsToText(Tournament tournament, Division division)
    {
        var session = division.Session ?? throw new InvalidOperationException("The auction hasn't started.");
        var text = new StringBuilder();
        text.AppendLine($"**{tournament.Title} — {division.Name}**");
        foreach (var team in session.Teams)
        {
            text.AppendLine();
            text.AppendLine($"**{team.CaptainName}** — spent {Money.Format(team.Spent, CultureInfo.InvariantCulture)} / {Money.Format(team.InitialBudget, CultureInfo.InvariantCulture)}");
            foreach (var pick in team.Picks)
            {
                var classes = pick.Player.Classes.Count > 0 ? $" ({string.Join("/", pick.Player.Classes.Select(PlayerClasses.ShortName))})" : string.Empty;
                text.AppendLine($"- {pick.Player.Name}{classes} — {Money.Format(pick.Price, CultureInfo.InvariantCulture)}");
            }
        }

        return text.ToString().TrimEnd() + Environment.NewLine;
    }

    private static void AppendRow(StringBuilder csv, IEnumerable<string> cells)
    {
        csv.AppendJoin(',', cells.Select(Escape));
        csv.Append("\r\n");
    }

    private static string Escape(string value) =>
        value.IndexOfAny([',', '"', '\n', '\r']) >= 0 ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
}
