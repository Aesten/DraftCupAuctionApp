using AuctionApp.Core.Model;

namespace AuctionApp.Core.Storage;

public sealed record ParsedPlayer(string Name, List<string> Classes, string? StageName);

/// <summary>
/// Reads a list of players pasted from a spreadsheet, a CSV file or a chat message. Accepted lines look like
/// <c>Name</c>, <c>Name, inf cav</c>, <c>Name;archer</c> or spreadsheet rows in the exported layout
/// (<c>Name | x |  | x | Stage</c>).
/// </summary>
public static class RosterParser
{
    private static readonly Dictionary<string, string> ClassAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["inf"] = PlayerClasses.Infantry,
        ["infantry"] = PlayerClasses.Infantry,
        ["arc"] = PlayerClasses.Archer,
        ["archer"] = PlayerClasses.Archer,
        ["archers"] = PlayerClasses.Archer,
        ["ranged"] = PlayerClasses.Archer,
        ["cav"] = PlayerClasses.Cavalry,
        ["cavalry"] = PlayerClasses.Cavalry,
    };

    private static readonly HashSet<string> Marks = new(StringComparer.OrdinalIgnoreCase) { "x", "1", "yes", "y", "true", "✓", "✔" };

    public static List<ParsedPlayer> Parse(string text)
    {
        var players = new List<ParsedPlayer>();
        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.Trim('\r', ' ', '﻿');
            if (line.Length == 0)
            {
                continue;
            }

            var cells = SplitCells(line);
            var name = cells[0].Trim();
            if (name.Length == 0 || IsHeader(name, players.Count))
            {
                continue;
            }

            var rest = cells.Skip(1).Select(cell => cell.Trim()).ToList();
            List<string> classes;
            string? stage = null;
            if (rest.Count >= PlayerClasses.All.Count && rest.Take(PlayerClasses.All.Count).All(cell => cell.Length == 0 || Marks.Contains(cell)))
            {
                // Spreadsheet layout: one column per class, then optionally the stage.
                classes = PlayerClasses.All.Where((_, i) => Marks.Contains(rest[i])).ToList();
                stage = rest.Skip(PlayerClasses.All.Count).FirstOrDefault(cell => cell.Length > 0);
            }
            else
            {
                classes = rest.SelectMany(cell => cell.Split([' ', '/', '|', '+'], StringSplitOptions.RemoveEmptyEntries))
                    .Select(token => ClassAliases.GetValueOrDefault(token.Trim()))
                    .OfType<string>()
                    .ToList();
            }

            players.Add(new ParsedPlayer(name, PlayerClasses.Normalize(classes), stage));
        }

        return players;
    }

    private static bool IsHeader(string firstCell, int parsedSoFar) =>
        parsedSoFar == 0 && (firstCell.Equals("player", StringComparison.OrdinalIgnoreCase) || firstCell.Equals("name", StringComparison.OrdinalIgnoreCase));

    private static List<string> SplitCells(string line)
    {
        if (line.Contains('\t'))
        {
            return [.. line.Split('\t')];
        }

        var separator = line.Contains(';') ? ';' : ',';
        var cells = new List<string>();
        var current = new System.Text.StringBuilder();
        var quoted = false;
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                if (quoted && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    quoted = !quoted;
                }
            }
            else if (c == separator && !quoted)
            {
                cells.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        cells.Add(current.ToString());
        return cells;
    }
}
