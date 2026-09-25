using System.Globalization;
using System.Text;
using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;

namespace AuctionApp.Core.Storage;

/// <summary>Turns a draft into files or text meant for people: backups, spreadsheets and chat messages.</summary>
public static class DraftExporter
{
    /// <summary>A complete copy of the draft that can be imported again, on this computer or another one.</summary>
    public static string ToBackupJson(Draft draft) => DraftJson.Serialize(draft);

    /// <summary>The player list as a spreadsheet: name, one column per class and the stage.</summary>
    public static string PlayersToCsv(Draft draft)
    {
        var csv = new StringBuilder();
        AppendRow(csv, ["Player", .. PlayerClasses.All.Select(PlayerClasses.ShortName), "Stage"]);
        foreach (var player in draft.Players)
        {
            AppendRow(csv,
            [
                player.Name,
                .. PlayerClasses.All.Select(c => player.Classes.Contains(c) ? "x" : string.Empty),
                draft.FindStage(player.StageId)?.Name ?? string.Empty,
            ]);
        }

        return csv.ToString();
    }

    /// <summary>Every sale, team by team, as a spreadsheet.</summary>
    public static string ResultsToCsv(Draft draft)
    {
        var session = draft.Session ?? throw new InvalidOperationException("The auction hasn't started.");
        var csv = new StringBuilder();
        AppendRow(csv, ["Captain", "Player", "Classes", "Price", "Stage"]);
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
                    draft.FindStage(pick.StageId)?.Name ?? string.Empty,
                ]);
            }
        }

        foreach (var player in session.Unsold.Concat(session.Skipped).Concat(session.Queue).Concat(session.Waiting))
        {
            AppendRow(csv, [string.Empty, player.Name, string.Join(" ", player.Classes.Select(PlayerClasses.ShortName)), string.Empty, "Unsold"]);
        }

        return csv.ToString();
    }

    /// <summary>A readable summary of the teams, formatted to paste in Discord or similar.</summary>
    public static string ResultsToText(Draft draft)
    {
        var session = draft.Session ?? throw new InvalidOperationException("The auction hasn't started.");
        var text = new StringBuilder();
        text.AppendLine($"**{draft.Title}**");
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
