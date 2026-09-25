using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;

namespace AuctionApp.Tests;

internal static class TestDrafts
{
    public static Draft Create(int captains = 2, int players = 6, int teamSize = 3, decimal budget = 20m, bool shuffle = false)
    {
        var draft = new Draft { Title = "Test Cup", TeamSize = teamSize, ShuffleOrder = shuffle };
        for (var i = 1; i <= captains; i++)
        {
            draft.Captains.Add(new Captain { Name = $"Captain {i}", Budget = budget });
        }

        for (var i = 1; i <= players; i++)
        {
            draft.Players.Add(new Player { Name = $"Player {i}", Classes = [PlayerClasses.Infantry], StageId = draft.Stages[0].Id });
        }

        return draft;
    }

    public static AuctionEngine Started(Draft draft)
    {
        var engine = new AuctionEngine(draft, new Random(42));
        engine.Start();
        return engine;
    }
}
