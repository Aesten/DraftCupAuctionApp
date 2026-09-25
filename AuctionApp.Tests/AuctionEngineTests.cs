using System.Globalization;
using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;

namespace AuctionApp.Tests;

public class AuctionEngineTests
{
    [Fact]
    public void Start_QueuesThePoolInOrder()
    {
        var tournament = TestData.Tournament(players: 5);
        var engine = TestData.Start(tournament);

        Assert.Equal(["Player 1", "Player 2", "Player 3", "Player 4", "Player 5"], engine.Session.Queue.Select(p => p.Name));
        Assert.Equal(2, engine.Session.Teams.Count);
        Assert.Equal(DivisionStatus.InProgress, tournament.Divisions[0].Status);
    }

    [Fact]
    public void Start_ShufflesWhenEnabled()
    {
        var tournament = TestData.Tournament(players: 30);
        var engine = TestData.Start(tournament, shuffle: true);

        var order = engine.Session.Queue.Select(p => p.Name).ToList();
        Assert.NotEqual(tournament.Players.Select(p => p.Name).ToList(), order);
        Assert.Equal(30, order.Distinct().Count());
    }

    [Fact]
    public void Start_RefusesAnInvalidDivision()
    {
        var tournament = TestData.Tournament(captains: 1);
        var engine = new AuctionEngine(tournament, tournament.Divisions[0]);

        Assert.Throws<AuctionException>(engine.Start);
        Assert.Null(tournament.Divisions[0].Session);
    }

    [Theory]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(10, true)]
    [InlineData(11, false)]
    public void Start_RequiresATeamSizeBetweenFiveAndTen(int size, bool allowed)
    {
        var tournament = TestData.Tournament(teamSize: size);

        var errors = DivisionValidator.Validate(tournament, tournament.Divisions[0]).Where(i => i.Severity == IssueSeverity.Error);

        Assert.Equal(allowed, !errors.Any());
    }

    [Fact]
    public void Start_UsesTheDivisionsHalfBudgetSetting()
    {
        var tournament = TestData.Tournament();
        tournament.Divisions[0].HalfBudgetCapAtStart = false;

        Assert.False(TestData.Start(tournament).Session.HalfBudgetCap);
    }

    [Fact]
    public void Sell_MovesPlayerToTeamAndChargesBudget()
    {
        var engine = TestData.Start(TestData.Tournament());
        var team = engine.Session.Teams[0];

        engine.Sell(team.CaptainId, 2.5m);

        Assert.Equal("Player 1", Assert.Single(team.Picks).Player.Name);
        Assert.Equal(17.5m, team.Remaining);
        Assert.Equal("Player 2", engine.Session.CurrentPlayer!.Name);
    }

    [Fact]
    public void Sell_RespectsHalfBudgetCap()
    {
        var engine = TestData.Start(TestData.Tournament(budget: 20.5m));
        var team = engine.Session.Teams[0];

        // Half of 20.5 rounded up to 0.1 is 10.3, so 10.2 can be spent.
        Assert.Equal(10.2m, engine.MaxBid(team));
        Assert.NotNull(engine.CheckSale(team.CaptainId, 10.3m));
        Assert.Null(engine.CheckSale(team.CaptainId, 10.2m));

        engine.SetHalfBudgetCap(false);
        Assert.Equal(20.5m, engine.MaxBid(team));
    }

    [Fact]
    public void Sell_RefusesFullTeams()
    {
        var engine = TestData.Start(TestData.Tournament(teamSize: 5));
        var team = engine.Session.Teams[0];
        for (var i = 0; i < 5; i++)
        {
            engine.Sell(team.CaptainId, 0.5m);
        }

        Assert.Contains("full", engine.CheckSale(team.CaptainId, 0.5m)!.Message);
        Assert.Equal(0m, engine.MaxBid(team));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("0.25")]
    public void Sell_RefusesInvalidPrices(string price)
    {
        var engine = TestData.Start(TestData.Tournament());

        Assert.NotNull(engine.CheckSale(engine.Session.Teams[0].CaptainId, decimal.Parse(price, CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void SkipAndBringBack_RoundTrips()
    {
        var engine = TestData.Start(TestData.Tournament());
        var first = engine.Session.CurrentPlayer!;

        engine.Skip();
        Assert.Contains(first, engine.Session.Skipped);

        engine.BringBack(first.Id);
        Assert.Equal(first, engine.Session.CurrentPlayer);
        Assert.Empty(engine.Session.Skipped);
    }

    [Fact]
    public void RequeueSkipped_AppendsToTheQueue()
    {
        var engine = TestData.Start(TestData.Tournament(players: 3));
        engine.Skip();
        engine.Skip();

        engine.RequeueSkipped();

        Assert.Equal(["Player 3", "Player 1", "Player 2"], engine.Session.Queue.Select(p => p.Name));
    }

    [Fact]
    public void ReturnPick_RefundsAndPutsPlayerBackOnTheBlock()
    {
        var engine = TestData.Start(TestData.Tournament());
        var team = engine.Session.Teams[1];
        var pick = engine.Sell(team.CaptainId, 4m);

        engine.ReturnPick(team.CaptainId, pick.Player.Id);

        Assert.Empty(team.Picks);
        Assert.Equal(20m, team.Remaining);
        Assert.Equal(pick.Player, engine.Session.CurrentPlayer);
    }

    [Fact]
    public void FinishAndReopen()
    {
        var engine = TestData.Start(TestData.Tournament(players: 4));
        engine.Sell(engine.Session.Teams[0].CaptainId, 1m);
        engine.Skip();

        engine.Finish();
        Assert.Equal(3, engine.Session.Unsold.Count);
        Assert.Throws<AuctionException>(engine.Skip);

        engine.Reopen();
        Assert.False(engine.Session.IsFinished);
        Assert.Equal(3, engine.Session.Skipped.Count);
    }

    [Fact]
    public void Sell_OverTheBudgetOnlyWhenConfirmed()
    {
        var engine = TestData.Start(TestData.Tournament(budget: 10m));
        var team = engine.Session.Teams[0];

        var capped = engine.CheckSale(team.CaptainId, 6m)!;
        Assert.True(capped.CanOverride);
        Assert.Contains("half budget cap", capped.Message);
        Assert.Throws<AuctionException>(() => engine.Sell(team.CaptainId, 6m));

        engine.Sell(team.CaptainId, 6m, overBudget: true);
        Assert.Equal(4m, team.Remaining);
        Assert.Contains("confirmed", engine.Session.Activity.Last().Text);

        var broke = engine.CheckSale(team.CaptainId, 5m)!;
        Assert.True(broke.CanOverride);
        Assert.Contains("only has 4.0", broke.Message);
        engine.Sell(team.CaptainId, 5m, overBudget: true);
        Assert.Equal(-1m, team.Remaining);
    }

    [Fact]
    public void Sell_CantOverrideAFullTeamOrABadPrice()
    {
        var engine = TestData.Start(TestData.Tournament(teamSize: 5));
        var team = engine.Session.Teams[0];

        Assert.False(engine.CheckSale(team.CaptainId, 1.25m)!.CanOverride);
        Assert.Throws<AuctionException>(() => engine.Sell(team.CaptainId, 1.25m, overBudget: true));

        for (var i = 0; i < 5; i++)
        {
            engine.Sell(team.CaptainId, 0.1m);
        }

        Assert.False(engine.CheckSale(team.CaptainId, 0.1m)!.CanOverride);
        Assert.Throws<AuctionException>(() => engine.Sell(team.CaptainId, 0.1m, overBudget: true));
    }
}
