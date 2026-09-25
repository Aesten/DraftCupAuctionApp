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
    public void Captains_AreNeverAuctioned()
    {
        var tournament = TestData.Tournament(players: 4);
        tournament.Players.Add(new Player { Name = "division 1 captain 1" });

        var engine = TestData.Start(tournament);

        Assert.Equal(4, engine.Session.Queue.Count);
        Assert.Equal(PoolStatusKind.Captain, TournamentRules.PoolStatuses(tournament)[tournament.Players[4].Id].Kind);
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
    public void LatePoolAdditions_CanBeAddedToARunningAuction()
    {
        var tournament = TestData.Tournament(players: 3);
        var engine = TestData.Start(tournament);
        var late = new Player { Name = "Late signup" };
        tournament.Players.Add(late);

        var fresh = TournamentRules.NewlyAvailable(tournament, tournament.Divisions[0]);
        Assert.Equal(late, Assert.Single(fresh));

        engine.AddToQueue(fresh);
        Assert.Equal("Late signup", engine.Session.Queue[^1].Name);
        Assert.Empty(TournamentRules.NewlyAvailable(tournament, tournament.Divisions[0]));
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
    public void RemovingAPoolPlayer_IsRefusedOnceBought()
    {
        var tournament = TestData.Tournament(players: 3);
        var engine = TestData.Start(tournament);
        engine.Sell(engine.Session.Teams[0].CaptainId, 1m);

        Assert.NotNull(TournamentRules.CanRemovePlayer(tournament, tournament.Players[0]));
        Assert.Null(TournamentRules.CanRemovePlayer(tournament, tournament.Players[1]));

        TournamentRules.RemovePlayer(tournament, tournament.Players[1]);
        Assert.Single(engine.Session.Queue);
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
