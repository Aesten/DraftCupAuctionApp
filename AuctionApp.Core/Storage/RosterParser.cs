using AuctionApp.Core.Model;

namespace AuctionApp.Core.Storage;

public sealed record ParsedPlayer(string Name, List<string> Classes, int? Tier = null);

/// <summary>
/// Reads the CSV (and plain text) layouts of a <see cref="PlayerList"/>: rows in the exported layout
/// (<c>Name,x,,x</c>, one column per class), and also looser lines like <c>Name</c>, <c>Name, inf cav</c> or
/// <c>Name;archer</c>, with commas, semicolons or tabs between columns. A tier (Captain Pick) is a number from 1 to
/// <see cref="Tiers.Count"/>: the column after the class columns, or a cell like <c>3</c>, <c>t3</c> or <c>tier 3</c>.
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
            int? tier = null;
            if (rest.Count >= PlayerClasses.All.Count && rest.Take(PlayerClasses.All.Count).All(cell => cell.Length == 0 || Marks.Contains(cell)))
            {
                // Spreadsheet layout: one column per class, then the tier; anything after that is ignored.
                classes = PlayerClasses.All.Where((_, i) => Marks.Contains(rest[i])).ToList();
                tier = rest.Count > PlayerClasses.All.Count ? ParseTier(rest[PlayerClasses.All.Count]) : null;
            }
            else
            {
                classes = [];
                foreach (var cell in rest)
                {
                    if (ParseTier(cell) is { } cellTier)
                    {
                        tier ??= cellTier;
                        continue;
                    }

                    foreach (var word in cell.Split([' ', '/', '|', '+'], StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (PlayerClasses.FromName(word) is { } code)
                        {
                            classes.Add(code);
                        }
                        else if (ParseTier(word) is { } wordTier)
                        {
                            tier ??= wordTier;
                        }
                    }
                }
            }

            players.Add(new ParsedPlayer(name, PlayerClasses.Normalize(classes), tier));
        }

        return players;
    }

    /// <summary>A tier written as <c>3</c>, <c>t3</c>, <c>T 3</c> or <c>Tier 3</c>, or null.</summary>
    public static int? ParseTier(string text)
    {
        var value = text.Trim();
        if (value.StartsWith("tier", StringComparison.OrdinalIgnoreCase))
        {
            value = value[4..];
        }
        else if (value.StartsWith('t') || value.StartsWith('T'))
        {
            value = value[1..];
        }

        return int.TryParse(value.Trim(), out var tier) && Tiers.IsValid(tier) ? tier : null;
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
