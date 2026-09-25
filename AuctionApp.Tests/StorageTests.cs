using AuctionApp.Core.Engine;
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
        var store = new DraftStore(_root);
        var draft = TestDrafts.Create();
        var engine = TestDrafts.Started(draft);
        engine.Sell(engine.Session.Teams[0].CaptainId, 2.5m);
        engine.Skip();

        store.Save(draft);
        var loaded = store.Load(draft.Id);

        Assert.Equal(draft.Title, loaded.Title);
        Assert.Equal(DraftStatus.InProgress, loaded.Status);
        Assert.Equal(2.5m, loaded.Session!.Teams[0].Picks[0].Price);
        Assert.Single(loaded.Session.Skipped);
        Assert.Equal(draft.Session!.Queue.Select(p => p.Id), loaded.Session.Queue.Select(p => p.Id));
    }

    [Fact]
    public void List_ReturnsSummariesNewestFirst()
    {
        var store = new DraftStore(_root);
        var older = TestDrafts.Create();
        older.Title = "Older";
        store.Save(older);
        Thread.Sleep(20);
        var newer = TestDrafts.Create();
        newer.Title = "Newer";
        store.Save(newer);

        var list = store.List();

        Assert.Equal(["Newer", "Older"], list.Select(s => s.Title));
        Assert.Equal(6, list[0].PlayerCount);
    }

    [Fact]
    public void Load_FallsBackToTheBackupWhenTheFileIsDamaged()
    {
        var store = new DraftStore(_root);
        var draft = TestDrafts.Create();
        store.Save(draft);
        draft.Title = "Second save";
        store.Save(draft);

        var path = Directory.GetFiles(store.DraftsDirectory, "*.draft.json").Single();
        File.WriteAllText(path, "{ broken");

        Assert.Equal("Test Cup", store.Load(draft.Id).Title);
        Assert.Single(store.List());
    }

    [Fact]
    public void Delete_MovesTheDraftAside()
    {
        var store = new DraftStore(_root);
        var draft = TestDrafts.Create();
        store.Save(draft);

        store.Delete(draft.Id);

        Assert.Empty(store.List());
        Assert.Single(Directory.GetFiles(store.DeletedDirectory));
    }

    [Fact]
    public void Import_ReadsLegacyAuctionPlans()
    {
        const string json = """
            {
              "type": "Auction",
              "title": "Winter Cup",
              "teamSize": 5,
              "players": [ { "name": "Alice", "classes": ["cav", "inf"] }, { "name": "Bob", "classes": [] } ],
              "captains": [ { "name": "Cap A", "budget": 20.0 }, { "name": "Cap B", "budget": 21.5 } ]
            }
            """;

        var draft = DraftImporter.Import(json, "fallback");

        Assert.Equal("Winter Cup", draft.Title);
        Assert.Equal(5, draft.TeamSize);
        Assert.Null(draft.Session);
        Assert.Equal(["inf", "cav"], draft.Players[0].Classes);
        Assert.Equal(21.5m, draft.Captains[1].Budget);
        Assert.All(draft.Players, p => Assert.Equal(draft.Stages[0].Id, p.StageId));
    }

    [Fact]
    public void Import_ReadsLegacyAuctionStatesAndCanContinueThem()
    {
        const string json = """
            {
              "type": "AuctionState",
              "title": "Winter Cup",
              "teamSize": 2,
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

        var draft = DraftImporter.Import(json, "fallback");
        var engine = new AuctionEngine(draft);

        Assert.Equal(DraftStatus.InProgress, draft.Status);
        Assert.False(engine.Session.HalfBudgetCap);
        Assert.Equal(16.5m, engine.Session.Teams[0].Remaining);
        Assert.Equal("Carol", engine.Session.CurrentPlayer!.Name);
        Assert.Equal(3, draft.Players.Count);

        engine.Sell(engine.Session.Teams[1].CaptainId, 4m);
        Assert.Equal(16m, engine.Session.Teams[1].Remaining);
    }

    [Fact]
    public void Import_ReadsBackupsWithANewId()
    {
        var draft = TestDrafts.Create();
        TestDrafts.Started(draft);

        var imported = DraftImporter.Import(DraftExporter.ToBackupJson(draft), "fallback");

        Assert.NotEqual(draft.Id, imported.Id);
        Assert.Equal(draft.Session!.Queue.Count, imported.Session!.Queue.Count);
    }

    [Fact]
    public void Import_RejectsUnrelatedJson()
    {
        Assert.Throws<InvalidDataException>(() => DraftImporter.Import("""{ "hello": 1 }""", "x"));
        Assert.Throws<InvalidDataException>(() => DraftImporter.Import("not json", "x"));
    }

    [Fact]
    public void ResultsCsv_QuotesNamesWithCommas()
    {
        var draft = TestDrafts.Create();
        draft.Players[0].Name = "Smith, John";
        var engine = TestDrafts.Started(draft);
        engine.Sell(engine.Session.Teams[0].CaptainId, 1.5m);

        var csv = DraftExporter.ResultsToCsv(draft);

        Assert.Contains("Captain 1,\"Smith, John\",INF,1.5,Main auction", csv);
    }

    [Fact]
    public void CloneSetup_CopiesRosterWithoutTheSession()
    {
        var draft = TestDrafts.Create();
        draft.Stages.Add(new Stage { Name = "Low" });
        draft.Players[0].StageId = draft.Stages[1].Id;
        TestDrafts.Started(draft);

        var copy = draft.CloneSetup("Copy");

        Assert.Null(copy.Session);
        Assert.Equal(draft.Players.Count, copy.Players.Count);
        Assert.Equal(copy.Stages[1].Id, copy.Players[0].StageId);
        Assert.DoesNotContain(copy.Stages, stage => draft.Stages.Any(s => s.Id == stage.Id));
    }
}
