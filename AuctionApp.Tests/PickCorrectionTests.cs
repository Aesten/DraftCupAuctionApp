using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;

namespace AuctionApp.Tests;

/// <summary>Fixing mistakes after a sale: refunds, price changes, moving and swapping bought players.</summary>
public class PickCorrectionTests
{
    private static (AuctionEngine Engine, SessionTeam First, SessionTeam Second, SessionPlayer Sold) SellFirstPlayer(decimal price = 3m)
    {
        var engine = TestData.Start(TestData.Tournament());
        var first = engine.Session.Teams[0];
        var second = engine.Session.Teams[1];
        var sold = engine.Session.CurrentPlayer!;
        engine.Sell(first.CaptainId, price);
        return (engine, first, second, sold);
    }

    [Fact]
    public void ReturnPickToSkipped_RefundsAndSkips()
    {
        var (engine, first, _, sold) = SellFirstPlayer();

        engine.ReturnPickToSkipped(first.CaptainId, sold.Id);

        Assert.Empty(first.Picks);
        Assert.Equal(20m, first.Remaining);
        Assert.Equal(sold.Id, engine.Session.Skipped.Single().Id);
        Assert.Equal("Player 2", engine.Session.CurrentPlayer!.Name);
    }

    [Fact]
    public void ReturnPick_PutsThePlayerBackOnTheBlockBeforeTheCurrentOne()
    {
        var (engine, first, _, sold) = SellFirstPlayer();

        engine.ReturnPick(first.CaptainId, sold.Id);

        Assert.Equal(["Player 1", "Player 2"], engine.Session.Queue.Take(2).Select(p => p.Name));
        Assert.Equal(20m, first.Remaining);
    }

    [Fact]
    public void ChangePickPrice_IgnoresTheHalfBudgetCapButNotTheBudget()
    {
        var (engine, first, _, sold) = SellFirstPlayer();

        engine.ChangePickPrice(first.CaptainId, sold.Id, 15m);
        Assert.Equal(5m, first.Remaining);

        Assert.Throws<AuctionException>(() => engine.ChangePickPrice(first.CaptainId, sold.Id, 20.1m));
        Assert.Throws<AuctionException>(() => engine.ChangePickPrice(first.CaptainId, sold.Id, 1.25m));
        Assert.Equal(15m, first.Picks.Single().Price);
    }

    [Fact]
    public void MovePick_ChargesTheOtherTeam()
    {
        var (engine, first, second, sold) = SellFirstPlayer(price: 4m);

        engine.MovePick(first.CaptainId, sold.Id, second.CaptainId);

        Assert.Empty(first.Picks);
        Assert.Equal(20m, first.Remaining);
        Assert.Equal(sold.Id, second.Picks.Single().Player.Id);
        Assert.Equal(16m, second.Remaining);
    }

    [Fact]
    public void MovePick_RefusesAFullTeam()
    {
        var engine = TestData.Start(TestData.Tournament(players: 12, teamSize: 5));
        var first = engine.Session.Teams[0];
        var second = engine.Session.Teams[1];
        for (var i = 0; i < 5; i++)
        {
            engine.Sell(second.CaptainId, 0.1m);
        }

        var sold = engine.Session.CurrentPlayer!;
        engine.Sell(first.CaptainId, 1m);

        Assert.Throws<AuctionException>(() => engine.MovePick(first.CaptainId, sold.Id, second.CaptainId));
        Assert.Single(first.Picks);
    }

    [Fact]
    public void SwapPick_ExchangesPlaces()
    {
        var (engine, first, _, sold) = SellFirstPlayer(price: 2m);
        engine.Skip(); // Player 2 goes to the skipped list
        var skipped = engine.Session.Skipped.Single();
        var queued = engine.Session.Queue[3];

        engine.SwapPick(first.CaptainId, sold.Id, skipped.Id);
        Assert.Equal(skipped.Id, first.Picks.Single().Player.Id);
        Assert.Equal(sold.Id, engine.Session.Skipped.Single().Id);
        Assert.Equal(2m, first.Picks.Single().Price);

        engine.SwapPick(first.CaptainId, skipped.Id, queued.Id);
        Assert.Equal(queued.Id, first.Picks.Single().Player.Id);
        Assert.Equal(skipped.Id, engine.Session.Queue[3].Id);
        Assert.Equal(12, engine.Session.AllPlayers().Select(p => p.Id).Distinct().Count());
    }

    [Fact]
    public void SwapPick_RefusesPlayersBoughtElsewhere()
    {
        var (engine, first, second, sold) = SellFirstPlayer();
        var other = engine.Session.CurrentPlayer!;
        engine.Sell(second.CaptainId, 1m);

        Assert.Throws<AuctionException>(() => engine.SwapPick(first.CaptainId, sold.Id, other.Id));
    }

    [Fact]
    public void SyncCaptain_RenamesTheTeamDuringTheAuction()
    {
        var tournament = TestData.Tournament();
        var engine = TestData.Start(tournament);
        var captain = tournament.Divisions[0].Captains[0];

        captain.Name = "  Renamed ";
        captain.Class = PlayerClasses.Archer;
        tournament.Divisions[0].SyncCaptain(captain);

        Assert.Equal("Renamed", engine.GetTeam(captain.Id).CaptainName);
        Assert.Equal(PlayerClasses.Archer, tournament.Divisions[0].CaptainClass(captain.Id));
    }
}
