using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;

namespace AuctionApp.Tests;

public class StageTests
{
    private static (Draft Draft, Stage High, Stage Low) TwoTierDraft(int? highCap = null)
    {
        var draft = TestDrafts.Create(players: 0, teamSize: 4);
        var high = draft.Stages[0];
        high.Name = "High tier";
        high.MaxPicksPerTeam = highCap;
        var low = new Stage { Name = "Low tier" };
        draft.Stages.Add(low);

        for (var i = 1; i <= 4; i++)
        {
            draft.Players.Add(new Player { Name = $"High {i}", StageId = high.Id });
        }

        for (var i = 1; i <= 4; i++)
        {
            draft.Players.Add(new Player { Name = $"Low {i}", StageId = low.Id });
        }

        return (draft, high, low);
    }

    [Fact]
    public void Start_OnlyQueuesTheFirstStage()
    {
        var (draft, _, _) = TwoTierDraft();
        var engine = TestDrafts.Started(draft);

        Assert.All(engine.Session.Queue, p => Assert.StartsWith("High", p.Name));
        Assert.Equal(4, engine.Session.Waiting.Count);
        Assert.True(engine.HasNextStage);
        Assert.Equal("High tier", engine.CurrentStage.Name);
    }

    [Fact]
    public void AdvanceStage_CarriesUnsoldPlayersIntoTheNextStage()
    {
        var (draft, _, _) = TwoTierDraft();
        var engine = TestDrafts.Started(draft);
        engine.Sell(engine.Session.Teams[0].CaptainId, 1m);
        engine.Skip();

        engine.AdvanceStage(carryUnsold: true);

        Assert.Equal("Low tier", engine.CurrentStage.Name);
        Assert.Equal(7, engine.Session.Queue.Count);
        Assert.Equal(3, engine.Session.Queue.Count(p => p.Name.StartsWith("High")));
        Assert.Empty(engine.Session.Skipped);
        Assert.Empty(engine.Session.Waiting);
        Assert.Empty(engine.Session.Unsold);
        Assert.False(engine.HasNextStage);
    }

    [Fact]
    public void AdvanceStage_CanSetUnsoldPlayersAside()
    {
        var (draft, _, _) = TwoTierDraft();
        var engine = TestDrafts.Started(draft);

        engine.AdvanceStage(carryUnsold: false);

        Assert.Equal(4, engine.Session.Queue.Count);
        Assert.Equal(4, engine.Session.Unsold.Count);
    }

    [Fact]
    public void AdvanceStage_KeepsBudgetsAndTeams()
    {
        var (draft, _, _) = TwoTierDraft();
        var engine = TestDrafts.Started(draft);
        var team = engine.Session.Teams[0];
        engine.Sell(team.CaptainId, 3m);

        engine.AdvanceStage(carryUnsold: true);

        Assert.Equal(17m, team.Remaining);
        Assert.Single(team.Picks);
    }

    [Fact]
    public void AdvanceStage_FailsOnTheLastStage()
    {
        var engine = TestDrafts.Started(TestDrafts.Create());

        Assert.Throws<AuctionException>(() => engine.AdvanceStage(true));
    }

    [Fact]
    public void StageLimit_CapsPicksPerTeamWithinTheStage()
    {
        var (draft, _, _) = TwoTierDraft(highCap: 1);
        var engine = TestDrafts.Started(draft);
        var team = engine.Session.Teams[0];
        engine.Sell(team.CaptainId, 1m);

        Assert.NotNull(engine.CheckSale(team.CaptainId, 1m));
        Assert.Equal(0, engine.PicksLeftThisStage(team));

        engine.AdvanceStage(carryUnsold: true);

        Assert.Null(engine.CheckSale(team.CaptainId, 1m));
        Assert.Equal(3, engine.PicksLeftThisStage(team));
    }

    [Fact]
    public void Finish_EarlyMarksLaterStagesUnsold()
    {
        var (draft, _, _) = TwoTierDraft();
        var engine = TestDrafts.Started(draft);

        engine.Finish();

        Assert.Equal(8, engine.Session.Unsold.Count);
    }

    [Fact]
    public void Validator_WarnsAboutEmptyStages()
    {
        var (draft, _, low) = TwoTierDraft();
        draft.Players.RemoveAll(p => p.StageId == low.Id);

        var issues = DraftValidator.Validate(draft);

        Assert.Contains(issues, issue => issue.Severity == IssueSeverity.Warning && issue.Message.Contains("Low tier"));
    }
}
