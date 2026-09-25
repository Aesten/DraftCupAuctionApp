using AuctionApp.Core.Model;
using AuctionApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuctionApp.ViewModels;

/// <summary>A division's tab: its configuration, its live auction and its teams.</summary>
public sealed partial class DivisionViewModel : ObservableObject
{
    public const int ConfigurePage = 0;
    public const int AuctionPage = 1;
    public const int TeamsPage = 2;

    public DivisionViewModel(Division division, TournamentViewModel owner)
    {
        Division = division;
        Owner = owner;
        Setup = new DivisionSetupViewModel(this);
        Auction = new AuctionViewModel(this);
        Results = new ResultsViewModel(this);
        SelectedPage = division.Status switch
        {
            DivisionStatus.NotStarted => ConfigurePage,
            DivisionStatus.InProgress => AuctionPage,
            _ => TeamsPage,
        };
    }

    public Division Division { get; }

    public TournamentViewModel Owner { get; }

    public Tournament Tournament => Owner.Tournament;

    public IDialogService Dialogs => Owner.Dialogs;

    public DivisionSetupViewModel Setup { get; }

    public AuctionViewModel Auction { get; }

    public ResultsViewModel Results { get; }

    [ObservableProperty]
    public partial int SelectedPage { get; set; }

    public string Header => string.IsNullOrWhiteSpace(Division.Name) ? "Division" : Division.Name;

    public DivisionStatus Status => Division.Status;

    public string StatusText => Division.Status switch
    {
        DivisionStatus.NotStarted => "Not started",
        DivisionStatus.InProgress => "Auction in progress",
        _ => "Auction finished",
    };

    public string Details
    {
        get
        {
            var status = Division.Status switch
            {
                DivisionStatus.NotStarted => "not started",
                DivisionStatus.InProgress => $"live · {Division.Session!.SoldCount} sold",
                _ => $"finished · {Division.Session!.SoldCount} sold",
            };
            return $"{Division.Captains.Count} teams · {Division.TeamSize} players + captain · {status}";
        }
    }

    partial void OnSelectedPageChanged(int value)
    {
        if (value == TeamsPage)
        {
            Results.Refresh();
        }
    }

    /// <summary>Settings changed: saved shortly after.</summary>
    internal void SettingsChanged()
    {
        Owner.DivisionChanged(Division);
        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(Details));
    }

    /// <summary>The auction changed: saved right away, and the rest of the tournament is refreshed.</summary>
    internal void AuctionChanged()
    {
        Owner.AuctionChanged(Division);
        OnPropertyChanged(nameof(Details));
    }

    /// <summary>The auction was started, reset, finished or reopened.</summary>
    internal void StatusChanged(bool reloadAuction)
    {
        if (reloadAuction)
        {
            Auction.Reload();
        }

        Setup.Refresh();
        Results.Refresh();
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(Details));
    }

    /// <summary>The pool or another division changed, so the players available here may have too.</summary>
    internal void OnPoolChanged()
    {
        Setup.Refresh();
        Auction.RefreshNewPlayers();
    }

    [RelayCommand]
    private void Delete() => Owner.RemoveDivision(this);

    [RelayCommand]
    private void ShowPool() => Owner.ShowPool();
}
