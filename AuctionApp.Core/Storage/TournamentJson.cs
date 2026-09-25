using System.Text.Json;
using System.Text.Json.Serialization;
using AuctionApp.Core.Model;

namespace AuctionApp.Core.Storage;

public static class TournamentJson
{
    /// <summary>Extension of exported tournament files. They are plain JSON, readable in any text editor.</summary>
    public const string FileExtension = ".draftcup.json";

    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static string Serialize(Tournament tournament) => JsonSerializer.Serialize(tournament, Options);

    public static Tournament Deserialize(string json)
    {
        var tournament = JsonSerializer.Deserialize<Tournament>(json, Options) ?? throw new JsonException("The file is empty.");
        if (tournament.SchemaVersion > Tournament.CurrentSchemaVersion)
        {
            throw new JsonException("This tournament was saved by a newer version of the app. Please update the app to open it.");
        }

        tournament.SchemaVersion = Tournament.CurrentSchemaVersion;
        tournament.Normalize();
        return tournament;
    }

    public static Tournament Clone(Tournament tournament) => Deserialize(Serialize(tournament));

    public static AuctionSession CloneSession(AuctionSession session) =>
        JsonSerializer.Deserialize<AuctionSession>(JsonSerializer.Serialize(session, Options), Options)!;
}
