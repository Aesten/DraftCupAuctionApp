using AuctionApp.Core.Model;
using AuctionApp.Core.Storage;

namespace AuctionApp.Tests;

public class PlayerListTests
{
    private static readonly List<Player> Players =
    [
        new() { Name = "Alice", Classes = [PlayerClasses.Infantry] },
        new() { Name = "Bob", Classes = [PlayerClasses.Archer, PlayerClasses.Cavalry] },
        new() { Name = "Smith, Carol; Jr", Classes = [] },
        new() { Name = "Émile", Classes = [PlayerClasses.Cavalry] },
    ];

    [Fact]
    public void Csv_UsesTheOldAppsLayout()
    {
        var csv = PlayerList.ToCsv(Players);

        Assert.StartsWith("Player,INF,ARC,CAV\r\nAlice,x,,\r\nBob,,x,x\r\n\"Smith, Carol; Jr\",,,\r\n", csv);
    }

    [Fact]
    public void Json_UsesTheOldAppsPlayerObjects()
    {
        var json = PlayerList.ToJson(Players);

        Assert.Contains("\"players\": [", json);
        Assert.Contains("\"name\": \"Émile\"", json);
        Assert.Contains("\"classes\": [\n        \"arc\",\n        \"cav\"\n      ]", json.Replace("\r\n", "\n"));
    }

    [Theory]
    [InlineData("csv")]
    [InlineData("json")]
    public void RoundTrips(string format)
    {
        var text = format == "csv" ? PlayerList.ToCsv(Players) : PlayerList.ToJson(Players);

        var parsed = PlayerList.Parse(text);

        Assert.Equal(Players.Select(p => p.Name), parsed.Select(p => p.Name));
        Assert.Equal(Players.Select(p => string.Join(" ", p.Classes)), parsed.Select(p => string.Join(" ", p.Classes)));
    }

    [Fact]
    public void Parse_AcceptsLooserSignUpLayouts()
    {
        Assert.Equal(["inf", "cav"], PlayerList.Parse("﻿Name,Classes\nAlice,Infantry cav").Single().Classes);
        Assert.Equal("Bob", PlayerList.Parse("""[ { "name": "Bob", "classes": ["archer"] } ]""").Single().Name);

        // The players of an auction plan of the previous version, which stored them the same way.
        var plan = """{ "type": "Auction", "title": "Plan", "players": [ { "name": "Carol", "classes": ["cav"] } ], "captains": [] }""";
        Assert.Equal("Carol", PlayerList.Parse(plan).Single().Name);
    }

    [Fact]
    public void Parse_RejectsFilesWithoutPlayers()
    {
        Assert.Throws<InvalidDataException>(() => PlayerList.Parse("\n\n"));
        Assert.Throws<InvalidDataException>(() => PlayerList.Parse("""{ "title": "Nope" }"""));
        Assert.Throws<InvalidDataException>(() => PlayerList.Parse("{ broken"));
    }
}
