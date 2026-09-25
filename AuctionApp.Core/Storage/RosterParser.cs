using AuctionApp.Core.Model;

namespace AuctionApp.Core.Storage;

public sealed record ParsedPlayer(string Name, List<string> Classes);

/// <summary>
/// Reads the CSV (and plain text) layouts of a <see cref="PlayerList"/>: rows in the exported layout
/// (<c>Name,x,,x</c>, one column per class), and also looser lines like <c>Name</c>, <c>Name, inf cav</c> or
/// <c>Name;archer</c>, with commas, semicolons or tabs between columns.
/// </summary>
public static class RosterParser
{
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
            if (rest.Count >= PlayerClasses.All.Count && rest.Take(PlayerClasses.All.Count).All(cell => cell.Length == 0 || Marks.Contains(cell)))
            {
                // Spreadsheet layout: one column per class; anything after that is ignored.
                classes = PlayerClasses.All.Where((_, i) => Marks.Contains(rest[i])).ToList();
            }
            else
            {
                classes = rest.SelectMany(cell => cell.Split([' ', '/', '|', '+'], StringSplitOptions.RemoveEmptyEntries))
                    .Select(PlayerClasses.FromName)
                    .OfType<string>()
                    .ToList();
            }

            players.Add(new ParsedPlayer(name, PlayerClasses.Normalize(classes)));
        }

        return players;
    }

    private static bool IsHeader(string firstCell, int parsedSoFar) =>
        parsedSoFar == 0 && (firstCell.Equals("player", StringComparison.OrdinalIgnoreCase) || firstCell.Equals("name", StringComparison.OrdinalIgnoreCase));

    /// <summary>The first comma or semicolon outside quotes (a quoted name may contain either).</summary>
    private static char UnquotedSeparator(string line)
    {
        var quoted = false;
        foreach (var c in line)
        {
            if (c == '"')
            {
                quoted = !quoted;
            }
            else if (!quoted && c is ',' or ';')
            {
                return c;
            }
        }

        return ',';
    }

    private static List<string> SplitCells(string line)
    {
        if (line.Contains('\t'))
        {
            return [.. line.Split('\t')];
        }

        var separator = UnquotedSeparator(line);
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
