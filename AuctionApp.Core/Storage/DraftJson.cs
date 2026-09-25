using System.Text.Json;
using System.Text.Json.Serialization;
using AuctionApp.Core.Model;

namespace AuctionApp.Core.Storage;

public static class DraftJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static string Serialize(Draft draft) => JsonSerializer.Serialize(draft, Options);

    public static Draft Deserialize(string json)
    {
        var draft = JsonSerializer.Deserialize<Draft>(json, Options) ?? throw new JsonException("The file is empty.");
        if (draft.SchemaVersion > Draft.CurrentSchemaVersion)
        {
            throw new JsonException("This draft was saved by a newer version of the app. Please update the app to open it.");
        }

        draft.Normalize();
        return draft;
    }

    public static AuctionSession CloneSession(AuctionSession session) =>
        JsonSerializer.Deserialize<AuctionSession>(JsonSerializer.Serialize(session, Options), Options)!;
}
