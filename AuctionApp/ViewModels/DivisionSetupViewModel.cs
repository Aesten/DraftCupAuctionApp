using System.Collections.ObjectModel;
using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;
using AuctionApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuctionApp.ViewModels;

/// <summary>A division's configuration: its captains and budgets, team size and auction options.</summary>
public sealed partial class DivisionSetupViewModel : ObservableObject
{
    private readonly DivisionViewModel _owner;

    public DivisionSetupViewModel(DivisionViewModel owner)
    {
        _owner = owner;
        foreach (var captain in Division.Captains)
        {
            Captains.Add(new CaptainRowViewModel(captain, this));
        }

        Refresh();
    }

    private Division Division => _owner.Division;

    private Tournament Tournament => _owner.Tournament;

    private IDialogService Dialogs => _owner.Dialogs;

    public IReadOnlyList<int> TeamSizeOptions { get; } = Enumerable.Range(Division.MinTeamSize, Division.MaxTeamSize - Division.MinTeamSize + 1).ToList();

    public IReadOnlyList<int> UpcomingOptions { get; } = Enumerable.Range(0, 6).ToList();

    public ObservableCollection<CaptainRowViewModel> Captains { get; } = [];

    /// <summary>Captain Pick: the tournament's minimum bids, shown for reference (changed from the menu).</summary>
    public string MinimumBidsText => string.Join("  ·  ", Tiers.All.Select(tier => $"T{tier} {Money.Format(Tournament.MinimumBid(tier))}"));

    public bool IsCaptainPick => Tournament.IsCaptainPick;

    public bool IsRandomPick => !IsCaptainPick;

    public ObservableCollection<ValidationIssue> Issues { get; } = [];

    public bool IsLocked => Division.Session != null;

    public bool IsEditable => !IsLocked;

    [ObservableProperty]
    public partial bool CanStart { get; set; }

    [ObservableProperty]
    public partial string Summary { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AvailabilityText { get; set; } = string.Empty;

    public string CaptainsHeader => $"Captains ({Captains.Count})";

    public string Name
    {
        get => Division.Name;
        set => Set(Division.Name, value, v => Division.Name = v);
    }

    public int TeamSize
    {
        get => Division.TeamSize;
        set => Set(Division.TeamSize, value, v => Division.TeamSize = v);
    }

    public int UpcomingShown
    {
        get => Division.UpcomingShown;
        set => Set(Division.UpcomingShown, value, v => Division.UpcomingShown = v);
    }

    public bool HalfBudgetCapAtStart
    {
        get => Division.HalfBudgetCapAtStart;
        set => Set(Division.HalfBudgetCapAtStart, value, v => Division.HalfBudgetCapAtStart = v);
    }

    private void Set<T>(T current, T value, Action<T> apply, [System.Runtime.CompilerServices.CallerMemberName] string? property = null)
    {
        if (EqualityComparer<T>.Default.Equals(current, value))
        {
            return;
        }

        apply(value);
        OnPropertyChanged(property);
        Changed();
    }

    /// <summary>Records a change: re-checks the division and schedules a save.</summary>
    internal void Changed()
    {
        Refresh();
        _owner.SettingsChanged();
    }

    /// <summary>
    /// A captain's name or class changed. Allowed during the auction too: the team is a slot the captain is attached
    /// to, so its card, the pool's statuses and the results follow straight away.
    /// </summary>
    internal void CaptainChanged(Captain captain)
    {
        Division.SyncCaptain(captain);
        Changed();
        if (IsLocked)
        {
            _owner.Auction.OnPoolChanged(clearUndo: false);
            _owner.Owner.Pool.RefreshStatuses();
        }
    }

    /// <summary>Re-checks the division, e.g. after the pool or another division changed.</summary>
    internal void Refresh()
    {
        OnPropertyChanged(nameof(IsLocked));
        OnPropertyChanged(nameof(IsEditable));
        OnPropertyChanged(nameof(MinimumBidsText));
        Issues.Clear();
        if (!IsLocked)
        {
            foreach (var issue in DivisionValidator.Validate(Tournament, Division).OrderByDescending(issue => issue.Severity))
            {
                Issues.Add(issue);
            }
        }

        CanStart = !IsLocked && Issues.All(issue => issue.Severity != IssueSeverity.Error);
        var spots = Division.Captains.Count * Division.TeamSize;
        Summary = $"{Division.Captains.Count} teams × {Division.TeamSize} players = {spots} spots";

        var available = TournamentRules.AvailablePlayers(Tournament, Division).Count;
        var picked = TournamentRules.PickedPlayerIds(Tournament, except: Division).Count;
        AvailabilityText = IsLocked
            ? $"Players added to the pool from now on go to this auction's {(IsCaptainPick ? "pick board" : "skipped list")}."
            : $"{available} of the {Tournament.Players.Count} players in the pool will be auctioned"
              + (picked > 0 ? $" ({picked} already bought in other divisions)." : ".");
        OnPropertyChanged(nameof(CaptainsHeader));
    }

    [RelayCommand]
    private void AddCaptain()
    {
        // Budgets are balanced per captain; start from the last one entered as a guess.
        var captain = new Captain { Budget = Division.Captains.LastOrDefault()?.Budget ?? 20m };
        Division.Captains.Add(captain);
        Captains.Add(new CaptainRowViewModel(captain, this) { FocusRequested = true });
        Changed();
    }

    [RelayCommand]
    private void RemoveCaptain(CaptainRowViewModel row)
    {
        Division.Captains.Remove(row.Model);
        Captains.Remove(row);
        Changed();
    }

    [RelayCommand]
    private void MoveCaptainUp(CaptainRowViewModel row) => MoveCaptain(row, -1);

    [RelayCommand]
    private void MoveCaptainDown(CaptainRowViewModel row) => MoveCaptain(row, +1);

    private void MoveCaptain(CaptainRowViewModel row, int offset)
    {
        var from = Captains.IndexOf(row);
        var to = from + offset;
        if (from < 0 || to < 0 || to >= Captains.Count)
        {
            return;
        }

        Captains.Move(from, to);
        Division.Captains.RemoveAt(from);
        Division.Captains.Insert(to, row.Model);
        Changed();
    }

    [RelayCommand]
    private void DeleteDivision() => _owner.Owner.RemoveDivision(_owner);

    [RelayCommand]
    private void StartAuction()
    {
        _owner.Owner.SaveNow();
        var warnings = Issues.Where(issue => issue.Severity == IssueSeverity.Warning).Select(issue => "• " + issue.Message).ToList();
        var message = "The division's settings will be locked while the auction runs.";
        if (warnings.Count > 0)
        {
            message = string.Join("\n", warnings) + "\n\n" + message;
        }

        if (Dialogs.Ask($"Start the {Division.Name} auction?", message, "Start auction") != DialogChoice.Primary)
        {
            return;
        }

        try
        {
            new AuctionEngine(Tournament, Division).Start();
        }
        catch (AuctionException ex)
        {
            Dialogs.ShowError("The auction can't start yet", ex.Message);
            return;
        }

        _owner.AuctionChanged();
        _owner.StatusChanged(reloadAuction: true);
        _owner.SelectedPage = DivisionViewModel.AuctionPage;
    }

    [RelayCommand]
    private void ResetAuction()
    {
        var answer = Dialogs.Ask(
            $"Reset the {Division.Name} auction?",
            "Every sale and skip will be undone, its players become available again, and the settings are unlocked. A copy of the tournament as it is now is kept in the app's data folder (Deleted).",
            "Reset auction");
        if (answer != DialogChoice.Primary || !_owner.Owner.TryKeepCopy("before-reset"))
        {
            return;
        }

        Division.Session = null;
        _owner.AuctionChanged();
        _owner.StatusChanged(reloadAuction: true);
        _owner.SelectedPage = DivisionViewModel.ConfigurePage;
    }
}

public sealed partial class CaptainRowViewModel : ObservableObject
{
    private readonly DivisionSetupViewModel _owner;

    public CaptainRowViewModel(Captain model, DivisionSetupViewModel owner)
    {
        Model = model;
        _owner = owner;
        BudgetText = Money.Format(model.Budget);
    }

    public Captain Model { get; }

    /// <summary>Set for rows the user just added, so the view can put the cursor in the name box.</summary>
    public bool FocusRequested { get; set; }

    public string Name
    {
        get => Model.Name;
        set
        {
            if (Model.Name == value)
            {
                return;
            }

            Model.Name = value;
            OnPropertyChanged();
            _owner.CaptainChanged(Model);
        }
    }

    /// <summary>Captains sign up with a single class; clicking the selected one clears it. Editable during the auction too.</summary>
    public bool IsInfantry
    {
        get => Model.Class == PlayerClasses.Infantry;
        set => SetClass(PlayerClasses.Infantry, value);
    }

    public bool IsArcher
    {
        get => Model.Class == PlayerClasses.Archer;
        set => SetClass(PlayerClasses.Archer, value);
    }

    public bool IsCavalry
    {
        get => Model.Class == PlayerClasses.Cavalry;
        set => SetClass(PlayerClasses.Cavalry, value);
    }

    private void SetClass(string code, bool on)
    {
        var updated = on ? code : Model.Class == code ? string.Empty : Model.Class;
        if (updated == Model.Class)
        {
            return;
        }

        Model.Class = updated;
        OnPropertyChanged(nameof(IsInfantry));
        OnPropertyChanged(nameof(IsArcher));
        OnPropertyChanged(nameof(IsCavalry));
        _owner.CaptainChanged(Model);
    }

    [ObservableProperty]
    public partial string BudgetText { get; set; }

    [ObservableProperty]
    public partial bool HasBudgetError { get; set; }

    partial void OnBudgetTextChanged(string value)
    {
        HasBudgetError = !Money.TryParse(value, out var budget) || budget < 0 || !Money.IsWholeStep(budget);
        if (!HasBudgetError && Model.Budget != budget)
        {
            Model.Budget = budget;
            _owner?.Changed();
        }
    }
}
