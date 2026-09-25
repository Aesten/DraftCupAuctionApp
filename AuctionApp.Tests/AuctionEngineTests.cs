using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;

namespace AuctionApp.Tests;

public class AuctionEngineTests
{
    [Fact]
    public void Start_QueuesAllPlayersOfTheFirstStage()
    {
        var draft = TestDrafts.Create(players: 5);
        var engine = TestDrafts.Started(draft);

        Assert.Equal(5, engine.Session.Queue.Count);
        Assert.Equal("Player 1", engine.Session.CurrentPlayer!.Name);
        Assert.Equal(2, engine.Session.Teams.Count);
        Assert.Equal(DraftStatus.InProgress, draft.Status);
    }

    [Fact]
    public void Start_RefusesAnInvalidSetup()
    {
        var draft = TestDrafts.Create(captains: 1);
        var engine = new AuctionEngine(draft);

        Assert.Throws<AuctionException>(engine.Start);
        Assert.Null(draft.Session);
    }

    [Fact]
    public void Start_ShufflesWhenEnabled()
    {
        var draft = TestDrafts.Create(players: 30, teamSize: 15, shuffle: true);
        var engine = TestDrafts.Started(draft);

        var order = engine.Session.Queue.Select(p => p.Name).ToList();
        Assert.NotEqual(draft.Players.Select(p => p.Name).ToList(), order);
        Assert.Equal(30, order.Distinct().Count());
    }

    [Fact]
    public void Sell_MovesPlayerToTeamAndChargesBudget()
    {
        var engine = TestDrafts.Started(TestDrafts.Create());
        var team = engine.Session.Teams[0];

        engine.Sell(team.CaptainId, 2.5m);

        Assert.Single(team.Picks);
        Assert.Equal("Player 1", team.Picks[0].Player.Name);
        Assert.Equal(17.5m, team.Remaining);
        Assert.Equal("Player 2", engine.Session.CurrentPlayer!.Name);
    }

    [Fact]
    public void Sell_RespectsHalfBudgetCap()
    {
        var engine = TestDrafts.Started(TestDrafts.Create(budget: 20.5m));
        var team = engine.Session.Teams[0];

        // Half of 20.5 rounded up to 0.1 is 10.3, so 10.2 can be spent.
        Assert.Equal(10.2m, engine.MaxBid(team));
        Assert.NotNull(engine.CheckSale(team.CaptainId, 10.3m));
        Assert.Null(engine.CheckSale(team.CaptainId, 10.2m));

        engine.SetHalfBudgetCap(false);
        Assert.Equal(20.5m, engine.MaxBid(team));
        Assert.Null(engine.CheckSale(team.CaptainId, 15m));
    }

    [Fact]
    public void Sell_RefusesFullTeams()
    {
        var engine = TestDrafts.Started(TestDrafts.Create(teamSize: 1));
        var team = engine.Session.Teams[0];
        engine.Sell(team.CaptainId, 1m);

        Assert.Contains("full", engine.CheckSale(team.CaptainId, 1m));
        Assert.Equal(0m, engine.MaxBid(team));
        Assert.Throws<AuctionException>(() => engine.Sell(team.CaptainId, 1m));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("0.25")]
    public void Sell_RefusesInvalidPrices(string price)
    {
        var engine = TestDrafts.Started(TestDrafts.Create());

        Assert.NotNull(engine.CheckSale(engine.Session.Teams[0].CaptainId, decimal.Parse(price, System.Globalization.CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void Sell_AllowsFreePlayers()
    {
        var engine = TestDrafts.Started(TestDrafts.Create());

        engine.Sell(engine.Session.Teams[0].CaptainId, 0m);

        Assert.Equal(20m, engine.Session.Teams[0].Remaining);
    }

    [Fact]
    public void SkipAndBringBack_RoundTrips()
    {
        var engine = TestDrafts.Started(TestDrafts.Create());
        var first = engine.Session.CurrentPlayer!;

        engine.Skip();
        Assert.Equal("Player 2", engine.Session.CurrentPlayer!.Name);
        Assert.Contains(first, engine.Session.Skipped);

        engine.BringBack(first.Id);
        Assert.Equal(first, engine.Session.CurrentPlayer);
        Assert.Empty(engine.Session.Skipped);
    }

    [Fact]
    public void RequeueSkipped_AppendsToTheQueue()
    {
        var engine = TestDrafts.Started(TestDrafts.Create(players: 3));
        engine.Skip();
        engine.Skip();

        engine.RequeueSkipped();

        Assert.Equal(["Player 3", "Player 1", "Player 2"], engine.Session.Queue.Select(p => p.Name));
        Assert.Empty(engine.Session.Skipped);
    }

    [Fact]
    public void ReturnPick_RefundsAndPutsPlayerBackOnTheBlock()
    {
        var engine = TestDrafts.Started(TestDrafts.Create());
        var team = engine.Session.Teams[1];
        var pick = engine.Sell(team.CaptainId, 4m);

        engine.ReturnPick(team.CaptainId, pick.Player.Id);

        Assert.Empty(team.Picks);
        Assert.Equal(20m, team.Remaining);
        Assert.Equal(pick.Player, engine.Session.CurrentPlayer);
    }

    [Fact]
    public void Finish_ListsLeftoversAsUnsold()
    {
        var engine = TestDrafts.Started(TestDrafts.Create(players: 4));
        engine.Sell(engine.Session.Teams[0].CaptainId, 1m);
        engine.Skip();

        engine.Finish();

        Assert.True(engine.Session.IsFinished);
        Assert.Equal(3, engine.Session.Unsold.Count);
        Assert.Empty(engine.Session.Queue);
        Assert.Throws<AuctionException>(engine.Skip);
        Assert.NotNull(engine.CheckSale(engine.Session.Teams[0].CaptainId, 1m));
    }

    [Fact]
    public void Reopen_PutsUnsoldPlayersInTheSkippedList()
    {
        var engine = TestDrafts.Started(TestDrafts.Create(players: 4));
        engine.Finish();

        engine.Reopen();

        Assert.False(engine.Session.IsFinished);
        Assert.Equal(4, engine.Session.Skipped.Count);
        Assert.Empty(engine.Session.Unsold);
    }
}
