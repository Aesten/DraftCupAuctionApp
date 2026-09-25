using System.Text.Json;
using System.Text.Json.Nodes;
using AuctionApp.Core.Model;

namespace AuctionApp.Core.Storage;

/// <summary>
/// Reads tournament files exported by this app (<c>.draftcup.json</c>): everything, auctions in progress included.
/// Exported tournaments keep their id, so they can be merged into the original. Player lists are a separate format
/// (<see cref="PlayerList"/>), imported into a tournament's pool.
/// </summary>
public static class TournamentImporter
{
    public static Tournament Import(string json, string fallbackTitle)
    {
        JsonNode? root;
        try
        {
            root = JsonNode.Parse(json);
        }
        catch (JsonException)
        {
            throw NotATournament();
        }

        if (root is not JsonObject obj || !obj.ContainsKey("divisions") || !obj.ContainsKey("players"))
        {
            throw NotATournament();
        }

        Tournament tournament;
        try
        {
            tournament = TournamentJson.Deserialize(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException(ex.Message, ex);
        }

        if (string.IsNullOrWhiteSpace(tournament.Title))
        {
            tournament.Title = fallbackTitle;
        }

        return tournament;
    }

    private static InvalidDataException NotATournament() =>
        new("This file isn't a Draft Cup tournament (.draftcup.json). Player lists (CSV or JSON) go into a tournament's pool: use Import… in the player pool.");
}
