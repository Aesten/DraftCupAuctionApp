using System.Collections.ObjectModel;
using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;
using AuctionApp.Core.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuctionApp.ViewModels;

/// <summary>A division's teams (final or so far), with ways to share them.</summary>
public sealed partial class ResultsViewModel(DivisionViewModel owner) : ObservableObject
{
    private Division Division => owner.Division;

    public ObservableCollection<TeamResultViewModel> Teams { get; } = [];

    public ObservableCollection<PlayerItemViewModel> Unsold { get; } = [];

    [ObservableProperty]
    public partial bool HasSession { get; set; }

    [ObservableProperty]
    public partial bool HasUnsold { get; set; }

    [ObservableProperty]
    public partial string Headline { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? CopyConfirmation { get; set; }

    public void Refresh()
    {
        Teams.Clear();
        Unsold.Clear();
        CopyConfirmation = null;
        var session = Division.Session;
        HasSession = session != null;
        if (session == null)
        {
            HasUnsold = false;
            return;
        }

        foreach (var team in session.Teams)
        {
            Teams.Add(new TeamResultViewModel(team, Division));
        }

        var unsold = session.IsFinished ? session.Unsold : session.Unsold.Concat(session.Skipped).ToList();
        foreach (var player in unsold)
        {
            Unsold.Add(new PlayerItemViewModel(player, Unsold.Count + 1));
        }

        HasUnsold = Unsold.Count > 0;
        Headline = session.IsFinished
            ? $"Team compositions · {session.SoldCount} players sold"
            : $"Team compositions so far · {session.SoldCount} players sold (the auction is still running)";
    }

    [RelayCommand]
    private void CopyText()
    {
        if (Division.Session != null && owner.Dialogs.CopyToClipboard(TournamentExporter.ResultsToText(owner.Tournament, Division)))
        {
            CopyConfirmation = "Copied! Paste it in Discord or anywhere else.";
        }
    }

    [RelayCommand]
    private void ExportCsv()
    {
        if (Division.Session == null)
        {
            return;
        }

        var path = owner.Dialogs.PickFileToSave("Export the teams", "CSV spreadsheet (*.csv)|*.csv", $"{owner.Tournament.Title} {Division.Name}.csv");
        if (path != null)
        {
            owner.Dialogs.TryWriteFile(path, TournamentExporter.ResultsToCsv(Division));
        }
    }
}

public sealed class TeamResultViewModel
{
    public TeamResultViewModel(SessionTeam team, Division division)
    {
        Name = team.CaptainName;
        SpentText = $"Spent {Money.Format(team.Spent)} of {Money.Format(team.InitialBudget)}";
        SlotsText = $"{team.Picks.Count}/{division.TeamSize}";
        Picks = team.Picks.Select(pick => new ResultPickViewModel(
            pick.Player.Name,
            AuctionViewModel.KnownClasses(pick.Player.Classes),
            Money.Format(pick.Price))).ToList();
        CaptainClasses = AuctionViewModel.CaptainClasses(division, team.CaptainId);
        Composition = AuctionViewModel.Composition(division.CaptainClass(team.CaptainId), team.Picks);
    }

    public string Name { get; }

    public string SpentText { get; }

    public string SlotsText { get; }

    public IReadOnlyList<string> CaptainClasses { get; }

    public IReadOnlyList<ClassCount> Composition { get; }

    public IReadOnlyList<ResultPickViewModel> Picks { get; }
}

public sealed record ResultPickViewModel(string Name, IReadOnlyList<string> Classes, string PriceText);
