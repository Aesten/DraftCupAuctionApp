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

        Assert.True(engine.CheckPickPrice(first.CaptainId, sold.Id, 20.1m)!.CanOverride);
        Assert.Throws<AuctionException>(() => engine.ChangePickPrice(first.CaptainId, sold.Id, 20.1m));
        Assert.Throws<AuctionException>(() => engine.ChangePickPrice(first.CaptainId, sold.Id, 1.25m, overBudget: true));
        Assert.Throws<AuctionException>(() => engine.ChangePickPrice(first.CaptainId, sold.Id, 30.1m, overBudget: true));
        Assert.Equal(15m, first.Picks.Single().Price);

        engine.ChangePickPrice(first.CaptainId, sold.Id, 20.1m, overBudget: true);
        Assert.Equal(-0.1m, first.Remaining);
    }

    [Fact]
    public void MovePick_OverTheOtherTeamsBudget_NeedsConfirmation()
    {
        var (engine, first, second, sold) = SellFirstPlayer(price: 4m);
        engine.Sell(second.CaptainId, 17m, overBudget: true);

        Assert.True(engine.CheckMovePick(first.CaptainId, sold.Id, second.CaptainId)!.CanOverride);
        Assert.Throws<AuctionException>(() => engine.MovePick(first.CaptainId, sold.Id, second.CaptainId));

        engine.MovePick(first.CaptainId, sold.Id, second.CaptainId, overBudget: true);
        Assert.Equal(-1m, second.Remaining);
    }

    [Fact]
    public void MovePick_AfterTheAuction_IsFree()
    {
        var (engine, first, second, sold) = SellFirstPlayer(price: 4m);
        engine.Finish();

        engine.MovePick(first.CaptainId, sold.Id, second.CaptainId);

        Assert.Equal(20m, first.Remaining);
        Assert.Equal(20m, second.Remaining);
        Assert.Equal(0m, second.Picks.Single().Price);
    }

    [Fact]
    public void BringToBlock_PutsAQueuedPlayerFirst()
    {
        var engine = TestData.Start(TestData.Tournament());
        var wanted = engine.Session.Queue[5];

        engine.BringToBlock(wanted.Id);

        Assert.Same(wanted, engine.Session.CurrentPlayer);
        Assert.Equal("Player 1", engine.Session.Queue[1].Name);
        Assert.Equal(12, engine.Session.Queue.Count);
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

public class UnlockedSettingsTests
{
    [Fact]
    public void SyncSession_FollowsBudgetsOrderAndNewCaptains()
    {
        var tournament = TestData.Tournament(captains: 2);
        var division = tournament.Divisions[0];
        var engine = TestData.Start(tournament);

        division.Captains[0].Budget = 25m;
        division.Captains.Reverse();
        division.Captains.Add(new Captain { Name = "Newcomer", Budget = 18m });
        var changes = division.SyncSession();

        Assert.Equal(division.Captains.Select(c => c.Id), engine.Session.Teams.Select(t => t.CaptainId));
        Assert.Equal(25m, engine.GetTeam(division.Captains[1].Id).InitialBudget);
        Assert.Equal(18m, engine.Session.Teams[2].InitialBudget);
        Assert.Contains(changes, change => change.Contains("Newcomer"));
    }

    [Fact]
    public void SyncSession_RemovedCaptain_ReleasesTheirPlayers()
    {
        var tournament = TestData.Tournament(captains: 2);
        var division = tournament.Divisions[0];
        var engine = TestData.Start(tournament);
        var leaving = division.Captains[1];
        var sold = engine.Session.CurrentPlayer!;
        engine.Sell(leaving.Id, 1m);

        division.Captains.Remove(leaving);
        division.SyncSession();

        Assert.Single(engine.Session.Teams);
        Assert.Contains(sold, engine.Session.Skipped);
        Assert.Contains("left the auction", engine.Session.Activity[^1].Text);
    }
}

public class SaleLimitTests
{
    [Fact]
    public void HalfBudgetCap_WarnsAboutTheCapEvenPastTheWholeBudget()
    {
        var engine = TestData.Start(TestData.Tournament(budget: 1m));
        var team = engine.Session.Teams[0];

        var issue = engine.CheckSale(team.CaptainId, 1.1m)!;

        Assert.Contains("half budget cap", issue.Message);
        Assert.Contains("0.5", issue.Message.Replace(',', '.'));
    }

    [Fact]
    public void Prices_GoUpTo30()
    {
        var engine = TestData.Start(TestData.Tournament(budget: 30m));
        var team = engine.Session.Teams[0];
        engine.SetHalfBudgetCap(false);

        Assert.Null(engine.CheckSale(team.CaptainId, 30m));
        Assert.False(engine.CheckSale(team.CaptainId, 30.1m)!.CanOverride);
    }

    [Fact]
    public void Validator_ChecksBudgetsAndWarnsAboutCaptainsWithoutAClass()
    {
        var tournament = TestData.Tournament();
        var division = tournament.Divisions[0];
        division.Captains[0].Budget = 30.5m;

        var issues = DivisionValidator.Validate(tournament, division);

        Assert.Contains(issues, issue => issue.Severity == IssueSeverity.Error && issue.Message.Contains("Budgets go from"));
        Assert.Contains(issues, issue => issue.Severity == IssueSeverity.Warning && issue.Message.Contains("no class"));
    }
}

public class RunningAuctionWarningTests
{
    [Fact]
    public void ValidateRunning_WarnsAboutUnnamedAndDuplicateCaptains()
    {
        var tournament = TestData.Tournament(captains: 3);
        var division = tournament.Divisions[0];
        TestData.Start(tournament);
        division.Captains[0].Name = string.Empty;
        division.Captains[2].Name = division.Captains[1].Name;

        var issues = DivisionValidator.ValidateRunning(division);

        Assert.All(issues, issue => Assert.Equal(IssueSeverity.Warning, issue.Severity));
        Assert.Contains(issues, issue => issue.Message.StartsWith("Captain 1 has no name"));
        Assert.Contains(issues, issue => issue.Message.Contains("more than once"));
    }
}
