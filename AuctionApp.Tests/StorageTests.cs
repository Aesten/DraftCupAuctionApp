using AuctionApp.Core.Model;
using AuctionApp.Core.Storage;

namespace AuctionApp.Tests;

public sealed class StorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "auction-tests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public void SaveAndLoad_RoundTripsARunningAuction()
    {
        var store = new TournamentStore(_root);
        var tournament = TestData.Tournament();
        var engine = TestData.Start(tournament);
        engine.Sell(engine.Session.Teams[0].CaptainId, 2.5m);
        engine.Skip();

        store.Save(tournament);
        var loaded = store.Load(tournament.Id);

        var division = loaded.Divisions[0];
        Assert.Equal(DivisionStatus.InProgress, division.Status);
        Assert.Equal(2.5m, division.Session!.Teams[0].Picks[0].Price);
        Assert.Single(division.Session.Skipped);
        Assert.Equal(tournament.Players.Select(p => p.Id), loaded.Players.Select(p => p.Id));
    }

    [Fact]
    public void List_ReturnsSummariesNewestFirst()
    {
        var store = new TournamentStore(_root);
        var older = TestData.Tournament();
        older.Title = "Older";
        older.UpdatedAt = DateTimeOffset.Now.AddHours(-1);
        store.Save(older);
        var newer = TestData.Tournament();
        newer.Title = "Newer";
        store.Save(newer);

        var list = store.List();

        Assert.Equal(["Newer", "Older"], list.Select(s => s.Title));
        Assert.Equal(12, list[0].PlayerCount);
    }

    [Fact]
    public void Load_FallsBackToTheBackupWhenTheFileIsDamaged()
    {
        var store = new TournamentStore(_root);
        var tournament = TestData.Tournament();
        store.Save(tournament);
        tournament.Title = "Second save";
        store.Save(tournament);

        File.WriteAllText(Directory.GetFiles(store.TournamentsDirectory, "*.draftcup.json").Single(), "{ broken");

        Assert.Equal("Test Cup", store.Load(tournament.Id).Title);
    }

    [Fact]
    public void Delete_MovesTheTournamentAside()
    {
        var store = new TournamentStore(_root);
        var tournament = TestData.Tournament();
        store.Save(tournament);

        store.Delete(tournament.Id);

        Assert.Empty(store.List());
        Assert.Single(Directory.GetFiles(store.DeletedDirectory));
    }

    [Fact]
    public void Import_ReadsExportedTournamentsAndKeepsTheirId()
    {
        var tournament = TestData.Tournament();
        TestData.Start(tournament);

        var imported = TournamentImporter.Import(TournamentExporter.ToFile(tournament), "fallback");

        Assert.Equal(tournament.Id, imported.Id);
        Assert.Equal(tournament.Divisions[0].Session!.Queue.Count, imported.Divisions[0].Session!.Queue.Count);
    }

    [Fact]
    public void Import_RejectsAnythingButTournamentFiles()
    {
        Assert.Throws<InvalidDataException>(() => TournamentImporter.Import("""{ "hello": 1 }""", "x"));
        Assert.Throws<InvalidDataException>(() => TournamentImporter.Import("Player,INF,ARC,CAV\r\nAlice,x,,\r\n", "x"));
        Assert.Throws<InvalidDataException>(() => TournamentImporter.Import("""{ "players": [ { "name": "Alice" } ] }""", "x"));
    }

    [Fact]
    public void Merge_TakesEachDivisionFromTheCopyThatChangedItLast()
    {
        var original = TestData.Tournament(players: 20);
        var second = TestData.AddDivision(original);
        original.Divisions[0].UpdatedAt = original.Divisions[1].UpdatedAt = DateTimeOffset.Now.AddHours(-1);

        // An auctioneer runs Division 1 on another computer...
        var copy = TournamentJson.Clone(original);
        var engine = TestData.Start(copy);
        engine.Sell(engine.Session.Teams[0].CaptainId, 3m);
        copy.Divisions[0].Touch();

        // ...while the organizer renames Division 2 at home.
        second.Name = "Division B";
        second.Touch();

        var result = TournamentMerger.Merge(original, copy);

        Assert.True(result.HasChanges);
        Assert.Equal(DivisionStatus.InProgress, result.Tournament.Divisions[0].Status);
        Assert.Equal("Division B", result.Tournament.Divisions[1].Name);
        Assert.Contains(result.Changes, change => change.StartsWith("Division 1 updated"));
    }

    [Fact]
    public void Merge_TakesTheNewerPoolAndAddsUnknownDivisions()
    {
        var original = TestData.Tournament(players: 5);
        original.PoolUpdatedAt = DateTimeOffset.Now.AddHours(-1);
        var copy = TournamentJson.Clone(original);
        copy.Players.Add(new Player { Name = "Late signup" });
        copy.TouchPool();
        copy.AddDivision();

        var result = TournamentMerger.Merge(original, copy);

        Assert.Equal(6, result.Tournament.Players.Count);
        Assert.Equal(2, result.Tournament.Divisions.Count);
        Assert.Equal(2, result.Changes.Count);
    }

    [Fact]
    public void Merge_OfIdenticalCopiesChangesNothing()
    {
        var original = TestData.Tournament();

        Assert.False(TournamentMerger.Merge(original, TournamentJson.Clone(original)).HasChanges);
    }

    [Fact]
    public void ResultsCsv_QuotesNamesWithCommas()
    {
        var tournament = TestData.Tournament();
        tournament.Players[0].Name = "Smith, John";
        var engine = TestData.Start(tournament);
        engine.Sell(engine.Session.Teams[0].CaptainId, 1.5m);

        var csv = TournamentExporter.ResultsToCsv(tournament.Divisions[0]);

        Assert.Contains("Division 1 Captain 1,\"Smith, John\",INF,1.5", csv);
    }

    [Fact]
    public void CloneWithoutResults_KeepsPoolAndDivisionsButNotAuctions()
    {
        var tournament = TestData.Tournament();
        TestData.Start(tournament);

        var copy = tournament.CloneWithoutResults("Next cup");

        Assert.NotEqual(tournament.Id, copy.Id);
        Assert.Equal(tournament.Players.Count, copy.Players.Count);
        Assert.Null(copy.Divisions[0].Session);
        Assert.Equal(2, copy.Divisions[0].Captains.Count);
    }
}

public class DamagedFileTests
{
    [Fact]
    public void Normalize_BringsHandEditedValuesBackWithinLimits()
    {
        var tournament = TestData.Tournament();
        var engine = TestData.Start(tournament);
        engine.Sell(engine.Session.Teams[0].CaptainId, 2m);
        var json = Core.Storage.TournamentJson.Serialize(tournament)
            .Replace("\"teamSize\": 5", "\"teamSize\": 100000")
            .Replace("\"budget\": 20", "\"budget\": -5")
            .Replace("\"price\": 2", "\"price\": 9999")
            .Replace("\"name\": \"Player 3\"", "\"name\": \"" + new string('x', 500) + "\"");

        var loaded = Core.Storage.TournamentJson.Deserialize(json);
        var division = loaded.Divisions[0];

        Assert.Equal(Division.MaxTeamSize, division.TeamSize);
        Assert.All(division.Captains, captain => Assert.Equal(Core.Engine.Money.MinBudget, captain.Budget));
        Assert.Equal(Core.Engine.Money.Max, division.Session!.Teams[0].Picks.Single().Price);
        Assert.Equal(Tournament.MaxNameLength, loaded.Players[2].Name.Length);
    }

    [Fact]
    public void Normalize_DropsEmptyEntries()
    {
        var json = Core.Storage.TournamentJson.Serialize(TestData.Tournament())
            .Replace("\"players\": [", "\"players\": [ null,")
            .Replace("\"captains\": [", "\"captains\": [ null,");

        var loaded = Core.Storage.TournamentJson.Deserialize(json);

        Assert.Equal(12, loaded.Players.Count);
        Assert.Equal(2, loaded.Divisions[0].Captains.Count);
    }
}
