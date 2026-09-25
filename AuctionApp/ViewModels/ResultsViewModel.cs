using System.Collections.ObjectModel;
using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;
using AuctionApp.Core.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuctionApp.ViewModels;

/// <summary>The final (or current) teams, with ways to share them.</summary>
public sealed partial class ResultsViewModel(DraftViewModel owner) : ObservableObject
{
    private Draft Draft => owner.Draft;

    public ObservableCollection<TeamResultViewModel> Teams { get; } = [];

    public ObservableCollection<PlayerItemViewModel> Unsold { get; } = [];

    [ObservableProperty]
    public partial bool HasSession { get; set; }

    [ObservableProperty]
    public partial bool HasUnsold { get; set; }

    [ObservableProperty]
    public partial int Columns { get; set; } = 4;

    [ObservableProperty]
    public partial string Headline { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? CopyConfirmation { get; set; }

    public void Refresh()
    {
        Teams.Clear();
        Unsold.Clear();
        CopyConfirmation = null;
        var session = Draft.Session;
        HasSession = session != null;
        if (session == null)
        {
            HasUnsold = false;
            return;
        }

        foreach (var team in session.Teams)
        {
            Teams.Add(new TeamResultViewModel(team, Draft));
        }

        var unsold = session.IsFinished ? session.Unsold : session.Unsold.Concat(session.Skipped).ToList();
        foreach (var player in unsold)
        {
            Unsold.Add(new PlayerItemViewModel(player, Unsold.Count + 1));
        }

        HasUnsold = Unsold.Count > 0;
        Columns = Math.Clamp(Teams.Count, 1, 4);
        Headline = session.IsFinished
            ? $"Final teams · {session.SoldCount} players sold"
            : $"Teams so far · {session.SoldCount} players sold (the auction is still running)";
    }

    [RelayCommand]
    private void CopyText()
    {
        if (Draft.Session != null && owner.Dialogs.CopyToClipboard(DraftExporter.ResultsToText(Draft)))
        {
            CopyConfirmation = "Copied! Paste it in Discord or anywhere else.";
        }
    }

    [RelayCommand]
    private void ExportCsv()
    {
        if (Draft.Session == null)
        {
            return;
        }

        var path = owner.Dialogs.PickFileToSave("Export the results", "CSV spreadsheet (*.csv)|*.csv", $"{Draft.Title} results.csv");
        if (path != null)
        {
            owner.Dialogs.TryWriteFile(path, DraftExporter.ResultsToCsv(Draft));
        }
    }

    [RelayCommand]
    private void ExportBackup()
    {
        owner.SaveNow();
        var path = owner.Dialogs.PickFileToSave("Export a backup of this draft", "Draft files (*.json)|*.json", $"{Draft.Title}.json");
        if (path != null)
        {
            owner.Dialogs.TryWriteFile(path, DraftExporter.ToBackupJson(Draft));
        }
    }
}

public sealed class TeamResultViewModel
{
    public TeamResultViewModel(SessionTeam team, Draft draft)
    {
        Name = team.CaptainName;
        SpentText = $"Spent {Money.Format(team.Spent)} of {Money.Format(team.InitialBudget)}";
        SlotsText = $"{team.Picks.Count}/{draft.TeamSize}";
        Picks = team.Picks.Select(pick => new ResultPickViewModel(
            pick.Player.Name,
            AuctionViewModel.KnownClasses(pick.Player.Classes),
            Money.Format(pick.Price),
            draft.Stages.Count > 1 ? draft.FindStage(pick.StageId)?.Name : null)).ToList();
        CompositionText = AuctionViewModel.Composition(team.Picks);
    }

    public string Name { get; }

    public string SpentText { get; }

    public string SlotsText { get; }

    public string CompositionText { get; }

    public IReadOnlyList<ResultPickViewModel> Picks { get; }
}

public sealed record ResultPickViewModel(string Name, IReadOnlyList<string> Classes, string PriceText, string? StageName);
