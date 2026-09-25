using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;
using AuctionApp.Core.Storage;

namespace AuctionApp.Tests;

public class ParsingTests
{
    [Theory]
    [InlineData("2.5", 2.5)]
    [InlineData("2,5", 2.5)]
    [InlineData(" 10 ", 10)]
    [InlineData("0", 0)]
    public void Money_ParsesBothDecimalSeparators(string text, decimal expected)
    {
        Assert.True(Money.TryParse(text, out var value));
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1.2.3")]
    public void Money_RejectsGarbage(string text) => Assert.False(Money.TryParse(text, out _));

    [Fact]
    public void Roster_ParsesPlainNames()
    {
        var players = RosterParser.Parse("Alice\r\nBob\n\n  Carol  \n");

        Assert.Equal(["Alice", "Bob", "Carol"], players.Select(p => p.Name));
        Assert.All(players, p => Assert.Empty(p.Classes));
    }

    [Fact]
    public void Roster_ParsesClassTokens()
    {
        var players = RosterParser.Parse("Alice, cav inf\nBob;archer\nCarol\tInfantry/Cavalry");

        Assert.Equal([PlayerClasses.Infantry, PlayerClasses.Cavalry], players[0].Classes);
        Assert.Equal([PlayerClasses.Archer], players[1].Classes);
        Assert.Equal([PlayerClasses.Infantry, PlayerClasses.Cavalry], players[2].Classes);
    }

    [Fact]
    public void Roster_ParsesTheExportedSpreadsheetLayout()
    {
        var draft = new Draft();
        draft.Stages[0].Name = "Tier 1";
        draft.Players.Add(new Player { Name = "Smith, John", Classes = ["arc"], StageId = draft.Stages[0].Id });
        draft.Players.Add(new Player { Name = "Eve", Classes = ["inf", "cav"], StageId = draft.Stages[0].Id });

        var players = RosterParser.Parse(DraftExporter.PlayersToCsv(draft));

        Assert.Equal(2, players.Count);
        Assert.Equal("Smith, John", players[0].Name);
        Assert.Equal(["arc"], players[0].Classes);
        Assert.Equal(["inf", "cav"], players[1].Classes);
        Assert.Equal("Tier 1", players[1].StageName);
    }

    [Fact]
    public void Roster_ParsesTabSeparatedSpreadsheetRows()
    {
        var players = RosterParser.Parse("Player\tINF\tARC\tCAV\nAlice\tx\t\tx\nBob\t\tx\t");

        Assert.Equal(2, players.Count);
        Assert.Equal(["inf", "cav"], players[0].Classes);
        Assert.Equal(["arc"], players[1].Classes);
    }
}
