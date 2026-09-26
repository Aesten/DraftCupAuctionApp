using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;
using AuctionApp.Core.Storage;

namespace AuctionApp.Tests;

public class CaptainPickTests
{
    /// <summary>A Captain Pick tournament: players "Player 1".. with tiers 1, 2, 3, 4, 5, 1, 2... and one class each.</summary>
    private static Tournament CaptainPickTournament(int players = 12, int captains = 2, int teamSize = 5, decimal budget = 20m)
    {
        var tournament = TestData.Tournament(players, captains, teamSize, budget);
        tournament.SetFormat(AuctionFormat.CaptainPick);
        for (var i = 0; i < tournament.Players.Count; i++)
        {
            tournament.Players[i].Tier = i % Tiers.Count + 1;
        }

        return tournament;
    }

    private static Guid Captain(AuctionEngine engine, int index) => engine.Session.Teams[index].CaptainId;

    private static SessionPlayer Board(AuctionEngine engine, string name) => engine.Session.Queue.Single(player => player.Name == name);

    [Fact]
    public void Start_PutsEveryoneOnTheBoardAndNobodyOnTheBlock()
    {
        var engine = TestData.Start(CaptainPickTournament(), shuffle: true);

        Assert.True(engine.Session.CaptainPick);
        Assert.Null(engine.Session.CurrentPlayer);
        Assert.Equal(12, engine.Session.Queue.Count);
        Assert.Equal("Player 1", engine.Session.Queue[0].Name);
    }

    [Fact]
    public void PutOnBlock_ThenSell_AtTheTierMinimum()
    {
        var engine = TestData.Start(CaptainPickTournament());
        var player = Board(engine, "Player 2");

        engine.PutOnBlock(player.Id);

        Assert.Same(player, engine.Session.CurrentPlayer);
        Assert.Equal(1.5m, engine.MinimumBid(player));
        engine.Sell(Captain(engine, 0), 1.5m);
        Assert.Null(engine.Session.CurrentPlayer);
        Assert.DoesNotContain(player, engine.Session.Queue);
        Assert.Equal(player, engine.Session.Teams[0].Picks.Single().Player);
    }

    [Fact]
    public void Sell_BelowTheMinimum_NeedsConfirmation()
    {
        var engine = TestData.Start(CaptainPickTournament());
        engine.PutOnBlock(Board(engine, "Player 1").Id);

        var issue = engine.CheckSale(Captain(engine, 0), 1.9m);

        Assert.NotNull(issue);
        Assert.True(issue.CanOverride);
        Assert.Contains("minimum bid", issue.Message);
        Assert.Throws<AuctionException>(() => engine.Sell(Captain(engine, 0), 1.9m));
        engine.Sell(Captain(engine, 0), 1.9m, overBudget: true);
        Assert.Contains("below the minimum", engine.Session.Activity[^1].Text);
    }

    [Fact]
    public void TierMinimums_AreSetPerDivision()
    {
        var tournament = CaptainPickTournament();
        tournament.Divisions[0].TierMinimums[0] = 3m;
        var engine = TestData.Start(tournament);

        Assert.Equal(3m, engine.MinimumBid(Board(engine, "Player 1")));
        Assert.Equal(0.1m, engine.MinimumBid(Board(engine, "Player 5")));
    }

    [Fact]
    public void PickingAnother_SendsTheFirstBackToTheBoard()
    {
        var engine = TestData.Start(CaptainPickTournament());
        engine.PutOnBlock(Board(engine, "Player 1").Id);

        engine.PutOnBlock(Board(engine, "Player 3").Id);

        Assert.Equal("Player 3", engine.Session.CurrentPlayer!.Name);
        Assert.Equal(12, engine.Session.Queue.Count);
    }

    [Fact]
    public void ReturnToBoard_ClearsTheBlock()
    {
        var engine = TestData.Start(CaptainPickTournament());
        engine.PutOnBlock(Board(engine, "Player 1").Id);

        engine.ReturnToBoard();

        Assert.Null(engine.Session.CurrentPlayer);
        Assert.Equal(12, engine.Session.Queue.Count);
    }

    [Fact]
    public void QueueAndSkippedList_DoNotExist()
    {
        var engine = TestData.Start(CaptainPickTournament());
        engine.PutOnBlock(Board(engine, "Player 1").Id);

        Assert.Throws<AuctionException>(engine.Skip);
        Assert.Throws<AuctionException>(engine.RequeueSkipped);
    }

    [Fact]
    public void ReturnPick_PutsThePlayerBackOnTheBlockOrTheBoard()
    {
        var engine = TestData.Start(CaptainPickTournament());
        var first = Board(engine, "Player 1");
        var second = Board(engine, "Player 2");
        engine.PutOnBlock(first.Id);
        engine.Sell(Captain(engine, 0), 2m);
        engine.PutOnBlock(second.Id);
        engine.Sell(Captain(engine, 1), 1.5m);

        engine.ReturnPick(Captain(engine, 0), first.Id);
        Assert.Same(first, engine.Session.CurrentPlayer);

        engine.ReturnPickToSkipped(Captain(engine, 1), second.Id);
        Assert.Same(first, engine.Session.CurrentPlayer);
        Assert.Contains(second, engine.Session.Queue);
        Assert.Empty(engine.Session.Skipped);
    }

    [Fact]
    public void SwapPick_WithThePlayerOnTheBlock_PutsTheOtherOneOnTheBlock()
    {
        var engine = TestData.Start(CaptainPickTournament());
        var sold = Board(engine, "Player 1");
        engine.PutOnBlock(sold.Id);
        engine.Sell(Captain(engine, 0), 2m);
        var onBlock = Board(engine, "Player 2");
        engine.PutOnBlock(onBlock.Id);

        engine.SwapPick(Captain(engine, 0), sold.Id, onBlock.Id);

        Assert.Same(sold, engine.Session.CurrentPlayer);
        Assert.Same(onBlock, engine.Session.Teams[0].Picks.Single().Player);
    }

    [Fact]
    public void FinishAndReopen_KeepsTheUnsoldOnTheBoard()
    {
        var engine = TestData.Start(CaptainPickTournament());
        engine.PutOnBlock(Board(engine, "Player 1").Id);

        engine.Finish();
        Assert.Equal(12, engine.Session.Unsold.Count);
        Assert.Null(engine.Session.OnBlockId);

        engine.Reopen();
        Assert.Equal(12, engine.Session.Queue.Count);
        Assert.Empty(engine.Session.Skipped);
    }

    [Fact]
    public void LateSignUps_JoinTheBoard()
    {
        var tournament = CaptainPickTournament();
        var engine = TestData.Start(tournament);
        tournament.Players.Add(new Player { Name = "Late", Classes = [PlayerClasses.Archer], Tier = 3 });

        TournamentRules.AddNewPlayersToRunningAuctions(tournament);

        Assert.Equal(3, Board(engine, "Late").Tier);
        Assert.Empty(engine.Session.Skipped);
    }

    [Fact]
    public void EditingATier_ReachesTheAuction()
    {
        var tournament = CaptainPickTournament();
        var engine = TestData.Start(tournament);
        var player = tournament.Players[0];

        player.Tier = 4;
        TournamentRules.SyncPlayer(tournament, player);

        Assert.Equal(0.5m, engine.MinimumBid(Board(engine, "Player 1")));
    }

    [Fact]
    public void Validator_RequiresATierAndAClass()
    {
        var tournament = CaptainPickTournament();
        tournament.Players[0].Tier = null;
        tournament.Players[1].Classes = [];

        var errors = DivisionValidator.Validate(tournament, tournament.Divisions[0])
            .Where(issue => issue.Severity == IssueSeverity.Error)
            .Select(issue => issue.Message)
            .ToList();

        Assert.Contains(errors, message => message.Contains("no tier") && message.Contains("Player 1"));
        Assert.Contains(errors, message => message.Contains("no class") && message.Contains("Player 2"));
    }

    [Fact]
    public void SetFormat_KeepsOneClassPerPlayer_AndIsLockedOnceAnAuctionStarted()
    {
        var tournament = TestData.Tournament();
        tournament.Players[0].Classes = [PlayerClasses.Archer, PlayerClasses.Cavalry];

        Assert.Equal(1, tournament.SetFormat(AuctionFormat.CaptainPick));
        Assert.Equal([PlayerClasses.Archer], tournament.Players[0].Classes);

        foreach (var player in tournament.Players)
        {
            player.Tier = 1;
        }

        TestData.Start(tournament);
        Assert.Throws<InvalidOperationException>(() => tournament.SetFormat(AuctionFormat.RandomPick));
    }

    [Fact]
    public void Json_RoundTripsFormatTiersAndTheBlock()
    {
        var tournament = CaptainPickTournament();
        var engine = TestData.Start(tournament);
        engine.PutOnBlock(Board(engine, "Player 3").Id);

        var copy = TournamentJson.Clone(tournament);

        Assert.True(copy.IsCaptainPick);
        Assert.Equal(3, copy.Players[2].Tier);
        Assert.Equal("Player 3", copy.Divisions[0].Session!.CurrentPlayer!.Name);
        Assert.Equal(Tiers.DefaultMinimums, copy.Divisions[0].TierMinimums);
    }

    [Fact]
    public void RandomPickFiles_HaveNoCaptainPickFields()
    {
        var tournament = TestData.Tournament();
        TestData.Start(tournament);

        var json = TournamentJson.Serialize(tournament);

        Assert.DoesNotContain("\"tier\"", json);
        Assert.DoesNotContain("captainPick", json);
        Assert.DoesNotContain("onBlockId", json);
    }

    [Fact]
    public void PlayerLists_CarryTiers()
    {
        List<Player> players =
        [
            new() { Name = "Alice", Classes = [PlayerClasses.Infantry], Tier = 1 },
            new() { Name = "Bob", Classes = [PlayerClasses.Cavalry], Tier = 5 },
        ];

        var csv = PlayerList.ToCsv(players, withTiers: true);
        Assert.StartsWith("Player,INF,ARC,CAV,Tier\r\nAlice,x,,,1\r\nBob,,,x,5\r\n", csv);
        Assert.Equal([1, 5], PlayerList.Parse(csv).Select(player => player.Tier));
        Assert.Equal([1, 5], PlayerList.Parse(PlayerList.ToJson(players, withTiers: true)).Select(player => player.Tier));
        Assert.DoesNotContain("tier", PlayerList.ToJson(players));
    }

    [Theory]
    [InlineData("Alice, inf, 3")]
    [InlineData("Alice, inf 3")]
    [InlineData("Alice;Infantry;Tier 3")]
    [InlineData("Alice\tt3\tinf")]
    public void PlayerLists_ReadLooseTiers(string line)
    {
        var player = PlayerList.Parse(line).Single();

        Assert.Equal("Alice", player.Name);
        Assert.Equal([PlayerClasses.Infantry], player.Classes);
        Assert.Equal(3, player.Tier);
    }
}
