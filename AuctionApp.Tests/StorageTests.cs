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
    public void Import_ReadsLegacyAuctionPlans()
    {
        const string json = """
            {
              "type": "Auction",
              "title": "Winter Cup",
              "teamSize": 6,
              "players": [ { "name": "Alice", "classes": ["cav", "inf"] }, { "name": "Bob", "classes": [] } ],
              "captains": [ { "name": "Cap A", "budget": 20.0 }, { "name": "Cap B", "budget": 21.5 } ]
            }
            """;

        var tournament = TournamentImporter.Import(json, "fallback");

        Assert.Equal("Winter Cup", tournament.Title);
        Assert.Equal(["inf", "cav"], tournament.Players[0].Classes);
        var division = Assert.Single(tournament.Divisions);
        Assert.Equal(6, division.TeamSize);
        Assert.Equal(21.5m, division.Captains[1].Budget);
        Assert.Null(division.Session);
    }

    [Fact]
    public void Import_ReadsLegacyAuctionStatesAndCanContinueThem()
    {
        const string json = """
            {
              "type": "AuctionState",
              "title": "Winter Cup",
              "teamSize": 6,
              "halfBudgetDisplay": false,
              "initialNumber": 3,
              "playerQueue": [ { "name": "Carol", "classes": ["arc"] } ],
              "skipped": [ { "name": "Dave", "classes": [] } ],
              "teams": [
                { "captain": "Cap A", "initialBudget": 20.0, "members": [ { "name": "Alice", "cost": 3.5, "classes": ["inf"] } ] },
                { "captain": "Cap B", "initialBudget": 20.0, "members": [] }
              ]
            }
            """;

        var tournament = TournamentImporter.Import(json, "fallback");
        var engine = new AuctionApp.Core.Engine.AuctionEngine(tournament, tournament.Divisions[0]);

        Assert.Equal(DivisionStatus.InProgress, tournament.Divisions[0].Status);
        Assert.False(engine.Session.HalfBudgetCap);
        Assert.Equal(16.5m, engine.Session.Teams[0].Remaining);
        Assert.Equal("Carol", engine.Session.CurrentPlayer!.Name);
        Assert.Equal(3, tournament.Players.Count);
        Assert.Empty(AuctionApp.Core.Engine.TournamentRules.NewlyAvailable(tournament, tournament.Divisions[0]));

        engine.Sell(engine.Session.Teams[1].CaptainId, 4m);
        Assert.Equal(16m, engine.Session.Teams[1].Remaining);
    }

    [Fact]
    public void Import_RejectsUnrelatedJson()
    {
        Assert.Throws<InvalidDataException>(() => TournamentImporter.Import("""{ "hello": 1 }""", "x"));
    }

    [Fact]
    public void Import_ReadsTheOldAppsCsvExport()
    {
        var tournament = TournamentImporter.Import("Player,INF,ARC,CAV\r\nAlice,x,,\r\nBob,,x,x\r\n", "Spring Cup");

        Assert.Equal(["Alice", "Bob"], tournament.Players.Select(p => p.Name));
        Assert.Equal([PlayerClasses.Infantry], tournament.Players[0].Classes);
        Assert.Equal([PlayerClasses.Archer, PlayerClasses.Cavalry], tournament.Players[1].Classes);
    }

    [Fact]
    public void Import_ReadsCaptainClassesInOldAuctionPlans()
    {
        const string json = """
            { "type": "Auction", "title": "Plan", "players": [], "captains": [ { "name": "Bob", "class": "cav" }, { "name": "Eve", "budget": 18 } ] }
            """;

        var captains = TournamentImporter.Import(json, "x").Divisions[0].Captains;

        Assert.Equal(PlayerClasses.Cavalry, captains[0].Class);
        Assert.Equal(20m, captains[0].Budget);
        Assert.Equal(string.Empty, captains[1].Class);
        Assert.Equal(18m, captains[1].Budget);
    }

    [Fact]
    public void Import_MakesATournamentFromAPlayerList()
    {
        var tournament = TournamentImporter.Import("\uFEFFName,Classes\r\nAlice,inf\r\nBob,arc cav\r\n\"Smith, Carol\",Cavalry\r\n", "Spring Cup");

        Assert.Equal("Spring Cup", tournament.Title);
        Assert.Equal(["Alice", "Bob", "Smith, Carol"], tournament.Players.Select(p => p.Name));
        Assert.Equal([PlayerClasses.Archer, PlayerClasses.Cavalry], tournament.Players[1].Classes);
        Assert.Equal([PlayerClasses.Cavalry], tournament.Players[2].Classes);
        Assert.Single(tournament.Divisions);
    }

    [Fact]
    public void Import_ReadsThePoolExportBack()
    {
        var original = TestData.Tournament(players: 3);
        original.Players[0].Classes = [PlayerClasses.Infantry, PlayerClasses.Cavalry];

        var imported = TournamentImporter.Import(TournamentExporter.PlayersToCsv(original), "Copy");

        Assert.Equal(original.Players.Select(p => p.Name), imported.Players.Select(p => p.Name));
        Assert.Equal([PlayerClasses.Infantry, PlayerClasses.Cavalry], imported.Players[0].Classes);
    }

    [Fact]
    public void Import_AcceptsAMinimalHandWrittenFile()
    {
        const string json = """
            {
              "title": "Winter Cup",
              "players": [ { "name": "Alice", "classes": ["Infantry", "arc"] } ],
              "divisions": [ { "name": "Main", "teamSize": 5, "captains": [ { "name": "Bob", "class": "cavalry", "budget": 18.5 } ] } ]
            }
            """;

        var tournament = TournamentImporter.Import(json, "fallback");

        Assert.Equal("Winter Cup", tournament.Title);
        Assert.NotEqual(Guid.Empty, tournament.Id);
        Assert.Equal([PlayerClasses.Infantry, PlayerClasses.Archer], tournament.Players[0].Classes);
        Assert.Equal(PlayerClasses.Cavalry, tournament.Divisions[0].Captains[0].Class);
        Assert.Equal(18.5m, tournament.Divisions[0].Captains[0].Budget);
    }

    [Fact]
    public void Import_RejectsFilesWithoutPlayers()
    {
        Assert.Throws<InvalidDataException>(() => TournamentImporter.Import("\n\n", "x"));
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
