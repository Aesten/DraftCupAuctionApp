using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using AuctionApp.Core.Model;

namespace AuctionApp.Core.Storage;

/// <summary>
/// The player list format: the link between a sign-up sheet and the app. Only players (name and classes), nothing
/// about tournaments or auctions. Two layouts, both from the previous version of the app:
/// <list type="bullet">
/// <item>CSV, as it exported its player list: <c>Player,INF,ARC,CAV</c>, one column per class marked <c>x</c>.</item>
/// <item>JSON, as its files stored players: <c>{ "players": [ { "name": "Alice", "classes": ["inf"] } ] }</c>.</item>
/// </list>
/// Captain Pick adds each player's tier: a <c>Tier</c> column after the classes, or <c>"tier": 3</c>.
/// </summary>
public static class PlayerList
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        // Names stay readable (accents, symbols) for people editing the file by hand.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>The CSV layout; <paramref name="withTiers"/> adds the Tier column (Captain Pick).</summary>
    public static string ToCsv(IEnumerable<Player> players, bool withTiers = false)
    {
        var csv = new StringBuilder();
        string[] header = ["Player", .. PlayerClasses.All.Select(PlayerClasses.ShortName)];
        AppendRow(csv, withTiers ? [.. header, "Tier"] : header);
        foreach (var player in players)
        {
            string[] row = [player.Name, .. PlayerClasses.All.Select(code => player.Classes.Contains(code) ? "x" : string.Empty)];
            AppendRow(csv, withTiers ? [.. row, player.Tier?.ToString() ?? string.Empty] : row);
        }

        return csv.ToString();
    }

    /// <summary>The JSON layout; <paramref name="withTiers"/> adds each player's tier (Captain Pick).</summary>
    public static string ToJson(IEnumerable<Player> players, bool withTiers = false) =>
        JsonSerializer.Serialize(new PlayerListFile(players.Select(p => new PlayerEntry(p.Name, [.. p.Classes], withTiers ? p.Tier : null)).ToList()), JsonOptions);

    /// <summary>
    /// Reads a player list: JSON (an object with <c>players</c>, or just the array), or CSV and other text layouts
    /// (see <see cref="RosterParser"/>). Throws <see cref="InvalidDataException"/> when nothing usable is found.
    /// </summary>
    public static List<ParsedPlayer> Parse(string text)
    {
        var trimmed = text.TrimStart('﻿', ' ', '\t', '\r', '\n');
        var players = trimmed.StartsWith('{') || trimmed.StartsWith('[') ? ParseJson(trimmed) : RosterParser.Parse(text);
        if (players.Count == 0)
        {
            throw new InvalidDataException("No players were found in this file.");
        }

        return players;
    }

    private static List<ParsedPlayer> ParseJson(string json)
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

        var entries = root switch
        {
            JsonArray array => array,
            JsonObject obj when obj["players"] is JsonArray array => array,
            _ => throw new InvalidDataException("This file isn't a player list: it needs a \"players\" list."),
        };

        return entries.OfType<JsonObject>()
            .Select(entry => new ParsedPlayer(
                (entry["name"]?.GetValueKind() == JsonValueKind.String ? entry["name"]!.GetValue<string>() : string.Empty).Trim(),
                PlayerClasses.Normalize((entry["classes"] as JsonArray ?? [])
                    .Where(value => value?.GetValueKind() == JsonValueKind.String)
                    .Select(value => value!.GetValue<string>())),
                ReadTier(entry["tier"])))
            .Where(player => player.Name.Length > 0)
            .ToList();
    }

    private static int? ReadTier(JsonNode? node) => node?.GetValueKind() switch
    {
        JsonValueKind.Number when node.AsValue().TryGetValue(out int tier) && Tiers.IsValid(tier) => tier,
        JsonValueKind.String => RosterParser.ParseTier(node.GetValue<string>()),
        _ => null,
    };

    private static void AppendRow(StringBuilder csv, IEnumerable<string> cells)
    {
        csv.AppendJoin(',', cells.Select(Escape));
        csv.Append("\r\n");
    }

    private static string Escape(string value) =>
        value.IndexOfAny([',', ';', '"', '\n', '\r']) >= 0 ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;

    private sealed record PlayerListFile(List<PlayerEntry> Players);

    private sealed record PlayerEntry(string Name, List<string> Classes, int? Tier);
}
