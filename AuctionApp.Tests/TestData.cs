using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;

namespace AuctionApp.Tests;

internal static class TestData
{
    /// <summary>A tournament with a pool of players ("Player 1", "Player 2"...) and one division.</summary>
    public static Tournament Tournament(int players = 12, int captains = 2, int teamSize = 5, decimal budget = 20m)
    {
        var tournament = new Tournament { Title = "Test Cup" };
        for (var i = 1; i <= players; i++)
        {
            tournament.Players.Add(new Player { Name = $"Player {i}", Classes = [PlayerClasses.Infantry] });
        }

        AddDivision(tournament, captains, teamSize, budget);
        return tournament;
    }

    public static Division AddDivision(Tournament tournament, int captains = 2, int teamSize = 5, decimal budget = 20m)
    {
        var division = tournament.AddDivision();
        division.TeamSize = teamSize;
        for (var i = 1; i <= captains; i++)
        {
            division.Captains.Add(new Captain { Name = $"{division.Name} Captain {i}", Budget = budget });
        }

        return division;
    }

    /// <summary>Starts an auction, in pool order unless <paramref name="shuffle"/>, so tests know who comes up.</summary>
    public static AuctionEngine Start(Tournament tournament, Division? division = null, bool shuffle = false)
    {
        var engine = new AuctionEngine(tournament, division ?? tournament.Divisions[0], new Random(42)) { KeepPoolOrder = !shuffle };
        engine.Start();
        return engine;
    }
}
