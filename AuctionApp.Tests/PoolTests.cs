using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;

namespace AuctionApp.Tests;

public class PoolTests
{
    [Fact]
    public void SecondDivision_StartsWithThePoolMinusPlayersBoughtInTheFirst()
    {
        var tournament = TestData.Tournament(players: 10);
        var second = TestData.AddDivision(tournament);
        var first = TestData.Start(tournament);
        first.Sell(first.Session.Teams[0].CaptainId, 1m);
        first.Skip();
        first.Sell(first.Session.Teams[1].CaptainId, 1m);
        first.Finish();

        var engine = TestData.Start(tournament, second);

        Assert.Equal(8, engine.Session.Queue.Count);
        Assert.DoesNotContain(engine.Session.Queue, p => p.Name is "Player 1" or "Player 3");
        Assert.Contains(engine.Session.Queue, p => p.Name == "Player 2");
    }

    [Fact]
    public void WhicheverDivisionStartsFirst_GetsTheWholePool()
    {
        var tournament = TestData.Tournament(players: 10);
        var second = TestData.AddDivision(tournament);

        var engine = TestData.Start(tournament, second);

        Assert.Equal(10, engine.Session.Queue.Count);
    }

    [Fact]
    public void PoolStatuses_TellWhereEveryoneIs()
    {
        var tournament = TestData.Tournament(players: 3);
        var engine = TestData.Start(tournament);
        engine.Sell(engine.Session.Teams[0].CaptainId, 2m);

        var statuses = TournamentRules.PoolStatuses(tournament);

        Assert.Equal(PoolStatusKind.Picked, statuses[tournament.Players[0].Id].Kind);
        Assert.Equal(2m, statuses[tournament.Players[0].Id].Price);
        Assert.Equal(PoolStatusKind.InAuction, statuses[tournament.Players[1].Id].Kind);

        engine.Finish();
        Assert.Equal(PoolStatusKind.Available, TournamentRules.PoolStatuses(tournament)[tournament.Players[1].Id].Kind);
    }

    [Fact]
    public void EditingAPoolPlayer_UpdatesTheAuctions()
    {
        var tournament = TestData.Tournament(players: 2);
        var engine = TestData.Start(tournament);
        engine.Sell(engine.Session.Teams[0].CaptainId, 1m);
        var player = tournament.Players[0];

        player.Name = "Renamed";
        player.Classes = [PlayerClasses.Cavalry];
        TournamentRules.SyncPlayer(tournament, player);

        var pick = engine.Session.Teams[0].Picks[0];
        Assert.Equal("Renamed", pick.Player.Name);
        Assert.Equal([PlayerClasses.Cavalry], pick.Player.Classes);
    }

    [Fact]
    public void LatePoolAdditions_GoToTheSkippedListOfRunningAuctions()
    {
        var tournament = TestData.Tournament(players: 3);
        var engine = TestData.Start(tournament);
        tournament.Players.Add(new Player { Name = "Late signup" });
        tournament.Players.Add(new Player { Name = "" });

        var added = TournamentRules.AddNewPlayersToRunningAuctions(tournament);

        Assert.Equal(1, added);
        Assert.Equal("Late signup", Assert.Single(engine.Session.Skipped).Name);
        Assert.Equal(0, TournamentRules.AddNewPlayersToRunningAuctions(tournament));
        engine.BringBack(engine.Session.Skipped[0].Id);
        Assert.Equal("Late signup", engine.Session.CurrentPlayer!.Name);
    }

    [Fact]
    public void LatePoolAdditions_DoNotTouchFinishedOrUnstartedDivisions()
    {
        var tournament = TestData.Tournament(players: 3);
        var engine = TestData.Start(tournament);
        engine.Finish();
        TestData.AddDivision(tournament);
        tournament.Players.Add(new Player { Name = "Late signup" });

        Assert.Equal(0, TournamentRules.AddNewPlayersToRunningAuctions(tournament));
    }

    [Fact]
    public void RemovingASoldPlayer_TakesThemOffTheRosterAndRefunds()
    {
        var tournament = TestData.Tournament(players: 3);
        var engine = TestData.Start(tournament);
        var team = engine.Session.Teams[0];
        engine.Sell(team.CaptainId, 3m);
        var player = tournament.Players[0];

        Assert.Single(TournamentRules.Sales(tournament, player));
        TournamentRules.RemovePlayer(tournament, player);

        Assert.Empty(team.Picks);
        Assert.Equal(20m, team.Remaining);
        Assert.Equal(2, tournament.Players.Count);
    }

    [Fact]
    public void RemovingAQueuedPlayer_TakesThemOutOfTheAuction()
    {
        var tournament = TestData.Tournament(players: 3);
        var engine = TestData.Start(tournament);

        TournamentRules.RemovePlayer(tournament, tournament.Players[1]);

        Assert.Equal(2, engine.Session.Queue.Count);
    }

    [Fact]
    public void Validator_WarnsWhenAnotherDivisionIsStillRunning()
    {
        var tournament = TestData.Tournament(players: 12);
        var second = TestData.AddDivision(tournament);
        TestData.Start(tournament);

        var issues = DivisionValidator.Validate(tournament, second);

        Assert.Contains(issues, issue => issue.Severity == IssueSeverity.Warning && issue.Message.Contains("isn't finished"));
    }

    [Fact]
    public void PlayersBoughtElsewhere_LeaveOtherRunningAuctions()
    {
        var tournament = TestData.Tournament(players: 12);
        var second = TestData.AddDivision(tournament);
        var a = TestData.Start(tournament);
        a.Skip();
        var b = TestData.Start(tournament, second);
        b.Sell(b.Session.Teams[0].CaptainId, 1m); // Player 1, still in A's skipped list
        b.Sell(b.Session.Teams[0].CaptainId, 1m); // Player 2, on the block in A

        var dropped = TournamentRules.DropPlayersTakenElsewhere(tournament);

        Assert.Equal(["Player 2", "Player 1"], Assert.Single(dropped).PlayerNames);
        Assert.Empty(a.Session.Skipped);
        Assert.Equal("Player 3", a.Session.CurrentPlayer!.Name);
        Assert.Empty(TournamentRules.DoublePicks(tournament));
    }

    [Fact]
    public void DoublePicks_AreDetected()
    {
        var tournament = TestData.Tournament(players: 12);
        var second = TestData.AddDivision(tournament);
        // Both divisions auctioned in parallel on different computers, then merged.
        var a = TestData.Start(tournament);
        var b = TestData.Start(tournament, second);
        a.Sell(a.Session.Teams[0].CaptainId, 1m);
        b.Sell(b.Session.Teams[0].CaptainId, 1m);

        var doubles = TournamentRules.DoublePicks(tournament);

        Assert.Equal("Player 1", Assert.Single(doubles).PlayerName);
    }
}
