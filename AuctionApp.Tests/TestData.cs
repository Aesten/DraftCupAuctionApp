using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;

namespace AuctionApp.Tests;

internal static class TestData
{
    /// <summary>A tournament with a pool of players and one division, players in pool order (no shuffle).</summary>
    public static Tournament Tournament(int players = 12, int captains = 2, int teamSize = 5, decimal budget = 20m, bool shuffle = false)
    {
        var tournament = new Tournament { Title = "Test Cup" };
        for (var i = 1; i <= players; i++)
        {
            tournament.Players.Add(new Player { Name = $"Player {i}", Classes = [PlayerClasses.Infantry] });
        }

        AddDivision(tournament, captains, teamSize, budget, shuffle);
        return tournament;
    }

    public static Division AddDivision(Tournament tournament, int captains = 2, int teamSize = 5, decimal budget = 20m, bool shuffle = false)
    {
        var division = tournament.AddDivision();
        division.TeamSize = teamSize;
        division.ShuffleOrder = shuffle;
        for (var i = 1; i <= captains; i++)
        {
            division.Captains.Add(new Captain { Name = $"{division.Name} Captain {i}", Budget = budget });
        }

        return division;
    }

    public static AuctionEngine Start(Tournament tournament, Division? division = null)
    {
        var engine = new AuctionEngine(tournament, division ?? tournament.Divisions[0], new Random(42));
        engine.Start();
        return engine;
    }
}
