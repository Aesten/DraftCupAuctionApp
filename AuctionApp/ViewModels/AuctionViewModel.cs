using System.Collections.ObjectModel;
using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;
using AuctionApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuctionApp.ViewModels;

/// <summary>The live auction screen. Every action is checked by the engine, can be undone, and is saved immediately.</summary>
public sealed partial class AuctionViewModel : ObservableObject
{
    private const string DefaultPrice = "0.1";

    private readonly DraftViewModel _owner;
    private readonly UndoHistory _history = new();
    private AuctionEngine? _engine;

    public AuctionViewModel(DraftViewModel owner)
    {
        _owner = owner;
        Reload();
    }

    private Draft Draft => _owner.Draft;

    private IDialogService Dialogs => _owner.Dialogs;

    public ObservableCollection<TeamCardViewModel> Teams { get; } = [];

    public ObservableCollection<PlayerItemViewModel> UpNext { get; } = [];

    public ObservableCollection<PlayerItemViewModel> Skipped { get; } = [];

    public ObservableCollection<ActivityItemViewModel> Activity { get; } = [];

    [ObservableProperty]
    public partial bool HasSession { get; set; }

    [ObservableProperty]
    public partial bool IsFinished { get; set; }

    [ObservableProperty]
    public partial bool IsRunning { get; set; }

    [ObservableProperty]
    public partial bool HasCurrentPlayer { get; set; }

    /// <summary>The queue is empty but the auction isn't finished: time to bring back skipped players or move on.</summary>
    [ObservableProperty]
    public partial bool IsStageDone { get; set; }

    [ObservableProperty]
    public partial string CurrentPlayerName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IReadOnlyList<string> CurrentPlayerClasses { get; set; } = [];

    [ObservableProperty]
    public partial string StageName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StageProgress { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasMultipleStages { get; set; }

    [ObservableProperty]
    public partial bool HasNextStage { get; set; }

    [ObservableProperty]
    public partial string NextStageText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StageDoneText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int SoldCount { get; set; }

    [ObservableProperty]
    public partial int QueueCount { get; set; }

    [ObservableProperty]
    public partial int SkippedCount { get; set; }

    [ObservableProperty]
    public partial bool HasSkipped { get; set; }

    [ObservableProperty]
    public partial int TeamColumns { get; set; } = 4;

    [ObservableProperty]
    public partial TeamCardViewModel? SelectedTeam { get; set; }

    [ObservableProperty]
    public partial string PriceText { get; set; } = DefaultPrice;

    /// <summary>Why the sale can't happen right now (shown under the price), or null.</summary>
    [ObservableProperty]
    public partial string? SaleProblem { get; set; }

    [ObservableProperty]
    public partial string UndoText { get; set; } = "Nothing to undo";

    public bool HalfBudgetCap
    {
        get => _engine?.Session.HalfBudgetCap ?? false;
        set
        {
            if (_engine == null || _engine.Session.HalfBudgetCap == value)
            {
                return;
            }

            Apply(value ? "Turn on the half budget cap" : "Turn off the half budget cap", engine => engine.SetHalfBudgetCap(value));
        }
    }

    /// <summary>Rebuilds everything from the draft (after the auction starts, is reset, or a different state is loaded).</summary>
    public void Reload()
    {
        _history.Clear();
        _engine = Draft.Session != null ? new AuctionEngine(Draft) : null;
        Teams.Clear();
        SelectedTeam = null;
        if (_engine != null)
        {
            foreach (var team in _engine.Session.Teams)
            {
                Teams.Add(new TeamCardViewModel(team.CaptainId, this));
            }
        }

        TeamColumns = Teams.Count switch
        {
            <= 4 => Math.Max(1, Teams.Count),
            <= 6 => 3,
            <= 8 => 4,
            <= 10 => 5,
            _ => 6,
        };
        Refresh();
    }

    private void Refresh()
    {
        HasSession = _engine != null;
        if (_engine == null)
        {
            IsRunning = IsFinished = HasCurrentPlayer = IsStageDone = false;
            UpNext.Clear();
            Skipped.Clear();
            Activity.Clear();
            UpdateUndo();
            return;
        }

        var session = _engine.Session;
        IsFinished = session.IsFinished;
        IsRunning = !session.IsFinished;
        var current = session.CurrentPlayer;
        HasCurrentPlayer = IsRunning && current != null;
        IsStageDone = IsRunning && current == null;
        CurrentPlayerName = current?.Name ?? string.Empty;
        CurrentPlayerClasses = current != null ? KnownClasses(current.Classes) : [];

        var stage = _engine.CurrentStage;
        StageName = stage.Name;
        HasMultipleStages = Draft.Stages.Count > 1;
        StageProgress = $"Stage {session.StageIndex + 1} of {Draft.Stages.Count}";
        HasNextStage = _engine.HasNextStage;
        NextStageText = _engine.NextStage is { } next ? $"Start {next.Name}" : string.Empty;
        StageDoneText = session.Skipped.Count > 0
            ? $"Nobody is left in the queue, but {session.Skipped.Count} skipped player(s) can get another chance."
            : _engine.HasNextStage
                ? $"Everyone in {stage.Name} has been auctioned."
                : "Everyone has been auctioned.";

        SoldCount = session.SoldCount;
        QueueCount = session.Queue.Count;
        SkippedCount = session.Skipped.Count;
        HasSkipped = SkippedCount > 0;

        Replace(UpNext, session.Queue.Skip(1).Select((player, i) => new PlayerItemViewModel(player, i + 1)));
        Replace(Skipped, session.Skipped.Select((player, i) => new PlayerItemViewModel(player, i + 1)));
        Replace(Activity, session.Activity.AsEnumerable().Reverse().Take(100).Select(entry => new ActivityItemViewModel(entry)));

        foreach (var team in Teams)
        {
            team.Update(_engine);
        }

        OnPropertyChanged(nameof(HalfBudgetCap));
        CheckSale();
        UpdateUndo();
    }

    /// <summary>How many players of each class a team has, e.g. "3 INF · 2 CAV".</summary>
    internal static string Composition(IEnumerable<Pick> picks) =>
        string.Join("  ·  ", PlayerClasses.All
            .Select(code => (code, count: picks.Count(pick => pick.Player.Classes.Contains(code))))
            .Where(entry => entry.count > 0)
            .Select(entry => $"{entry.count} {PlayerClasses.ShortName(entry.code)}"));

    internal static IReadOnlyList<string> KnownClasses(IEnumerable<string> classes) => classes.Where(PlayerClasses.All.Contains).ToList();

    private static void Replace<T>(ObservableCollection<T> collection, IEnumerable<T> items)
    {
        collection.Clear();
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }

    /// <summary>Runs an action with an undo point, saves, and refreshes the screen. Errors are shown, not thrown.</summary>
    private bool Apply(string description, Action<AuctionEngine> action)
    {
        if (_engine == null)
        {
            return false;
        }

        _history.Record(description, _engine.Session);
        try
        {
            action(_engine);
        }
        catch (AuctionException ex)
        {
            _history.Discard();
            Dialogs.ShowError("That didn't work", ex.Message);
            Refresh();
            return false;
        }

        _owner.Commit();
        Refresh();
        return true;
    }

    private void UpdateUndo()
    {
        UndoText = _history.NextDescription is { } next ? $"Undo: {next}" : "Nothing to undo";
        UndoCommand.NotifyCanExecuteChanged();
    }

    // Selling

    partial void OnSelectedTeamChanged(TeamCardViewModel? value)
    {
        foreach (var team in Teams)
        {
            team.IsSelected = team == value;
        }

        CheckSale();
    }

    partial void OnPriceTextChanged(string value) => CheckSale();

    internal void Select(TeamCardViewModel team)
    {
        if (HasCurrentPlayer)
        {
            SelectedTeam = SelectedTeam == team ? null : team;
        }
    }

    private void CheckSale()
    {
        if (_engine == null || !HasCurrentPlayer)
        {
            SaleProblem = null;
        }
        else if (!Money.TryParse(PriceText, out var price))
        {
            SaleProblem = "Enter a price.";
        }
        else if (SelectedTeam == null)
        {
            SaleProblem = "Click the team that won the bid.";
        }
        else
        {
            SaleProblem = _engine.CheckSale(SelectedTeam.CaptainId, price);
        }

        SellCommand.NotifyCanExecuteChanged();
    }

    private bool CanSell() => HasCurrentPlayer && SaleProblem == null;

    [RelayCommand(CanExecute = nameof(CanSell))]
    private void Sell()
    {
        if (SelectedTeam is not { } team || !Money.TryParse(PriceText, out var price))
        {
            return;
        }

        if (Apply($"sell {CurrentPlayerName} to {team.Name}", engine => engine.Sell(team.CaptainId, price)))
        {
            PriceText = DefaultPrice;
            SelectedTeam = null;
        }
    }

    [RelayCommand]
    private void ChangePrice(string delta)
    {
        if (!decimal.TryParse(delta, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var step))
        {
            return;
        }

        var current = Money.TryParse(PriceText, out var price) ? price : 0m;
        PriceText = Money.Format(Math.Max(0, decimal.Round(current, 1) + step));
    }

    [RelayCommand]
    private void MaxPrice()
    {
        if (_engine != null && SelectedTeam != null)
        {
            PriceText = Money.Format(_engine.MaxBid(_engine.GetTeam(SelectedTeam.CaptainId)));
        }
    }

    [RelayCommand]
    private void Skip()
    {
        if (HasCurrentPlayer)
        {
            Apply($"skip {CurrentPlayerName}", engine => engine.Skip());
        }
    }

    [RelayCommand]
    private void BringBack(PlayerItemViewModel player) =>
        Apply($"bring back {player.Name}", engine => engine.BringBack(player.Id));

    [RelayCommand]
    private void RequeueSkipped() =>
        Apply("send skipped players back to the queue", engine => engine.RequeueSkipped());

    internal void ReturnPick(TeamCardViewModel team, PickItemViewModel pick)
    {
        var answer = Dialogs.Ask(
            $"Take {pick.Name} back from {team.Name}?",
            $"{team.Name} gets {pick.PriceText} back and {pick.Name} goes back on the block.",
            "Take back");
        if (answer == DialogChoice.Primary)
        {
            Apply($"take {pick.Name} back from {team.Name}", engine => engine.ReturnPick(team.CaptainId, pick.PlayerId));
        }
    }

    // Stages and end of the auction

    [RelayCommand]
    private void NextStage()
    {
        if (_engine?.NextStage is not { } next)
        {
            return;
        }

        var current = _engine.CurrentStage;
        var leftovers = _engine.UnsoldInStage;
        bool carry;
        if (leftovers == 0)
        {
            if (Dialogs.Ask($"Start {next.Name}?", $"{current.Name} is over. Teams keep their players and remaining budget.", $"Start {next.Name}") != DialogChoice.Primary)
            {
                return;
            }

            carry = false;
        }
        else
        {
            var answer = Dialogs.Ask(
                $"Start {next.Name}?",
                $"{leftovers} player(s) from {current.Name} haven't been sold. They can be auctioned again in {next.Name}, or set aside as unsold.\n\nTeams keep their players and remaining budget.",
                $"Move them to {next.Name}",
                "Set them aside");
            if (answer == DialogChoice.Cancel)
            {
                return;
            }

            carry = answer == DialogChoice.Primary;
        }

        Apply($"start {next.Name}", engine => engine.AdvanceStage(carry));
    }

    [RelayCommand]
    private void Finish()
    {
        if (_engine == null)
        {
            return;
        }

        var remaining = _engine.UnsoldInStage + _engine.Session.Waiting.Count;
        var message = remaining > 0
            ? $"{remaining} player(s) haven't been sold and will be listed as unsold. You can reopen the auction later if needed."
            : "Every player has been auctioned. You can reopen the auction later if needed.";
        if (Dialogs.Ask("Finish the auction?", message, "Finish auction") != DialogChoice.Primary)
        {
            return;
        }

        if (Apply("finish the auction", engine => engine.Finish()))
        {
            _owner.OnSessionStatusChanged();
            _owner.SelectedTab = DraftViewModel.ResultsTab;
        }
    }

    [RelayCommand]
    private void Reopen()
    {
        if (Apply("reopen the auction", engine => engine.Reopen()))
        {
            _owner.OnSessionStatusChanged();
            _owner.SelectedTab = DraftViewModel.AuctionTab;
        }
    }

    [RelayCommand]
    private void ShowResults() => _owner.SelectedTab = DraftViewModel.ResultsTab;

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        if (_history.Pop() is not { } snapshot)
        {
            return;
        }

        var wasFinished = Draft.Session?.IsFinished;
        Draft.Session = snapshot;
        _engine = new AuctionEngine(Draft);
        _owner.Commit();
        Refresh();
        if (wasFinished != snapshot.IsFinished)
        {
            _owner.OnSessionStatusChanged();
        }
    }

    private bool CanUndo() => _history.CanUndo;
}

public sealed partial class TeamCardViewModel(Guid captainId, AuctionViewModel owner) : ObservableObject
{
    public Guid CaptainId { get; } = captainId;

    public ObservableCollection<PickItemViewModel> Picks { get; } = [];

    /// <summary>One placeholder per empty spot, so the cards show how many players are still missing.</summary>
    public ObservableCollection<int> EmptySlots { get; } = [];

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RemainingText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MaxBidText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SlotsText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CompositionText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double BudgetLeftRatio { get; set; }

    [ObservableProperty]
    public partial bool CanBuy { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    internal void Update(AuctionEngine engine)
    {
        var team = engine.GetTeam(CaptainId);
        Name = team.CaptainName;
        RemainingText = Money.Format(team.Remaining);
        BudgetLeftRatio = team.InitialBudget > 0 ? (double)Math.Clamp(team.Remaining / team.InitialBudget, 0, 1) : 0;
        CanBuy = !engine.Session.IsFinished && engine.PicksLeftThisStage(team) > 0;
        var slotsLeft = engine.SlotsLeft(team);
        SlotsText = $"{team.Picks.Count}/{engine.Draft.TeamSize}";
        MaxBidText = slotsLeft == 0
            ? "Team complete"
            : !CanBuy
                ? engine.Session.IsFinished ? $"{slotsLeft} spot(s) left" : "Stage limit reached"
                : $"Max bid {Money.Format(engine.MaxBid(team))}";

        CompositionText = AuctionViewModel.Composition(team.Picks);

        Picks.Clear();
        foreach (var pick in team.Picks)
        {
            Picks.Add(new PickItemViewModel(pick, engine.Draft.FindStage(pick.StageId)?.Name, !engine.Session.IsFinished, this));
        }

        EmptySlots.Clear();
        for (var i = 0; i < slotsLeft; i++)
        {
            EmptySlots.Add(i);
        }
    }

    [RelayCommand]
    private void Select() => owner.Select(this);

    internal void ReturnPick(PickItemViewModel pick) => owner.ReturnPick(this, pick);
}

public sealed partial class PickItemViewModel(Pick pick, string? stageName, bool canReturn, TeamCardViewModel team)
{
    public bool CanReturn => canReturn;

    public Guid PlayerId => pick.Player.Id;

    public string Name => pick.Player.Name;

    public IReadOnlyList<string> Classes { get; } = AuctionViewModel.KnownClasses(pick.Player.Classes);

    public string PriceText => Money.Format(pick.Price);

    public string Details => stageName != null ? $"Bought in {stageName} for {PriceText}" : $"Bought for {PriceText}";

    [RelayCommand]
    private void Return() => team.ReturnPick(this);
}

public sealed class PlayerItemViewModel(SessionPlayer player, int position)
{
    public Guid Id => player.Id;

    public string Name => player.Name;

    public int Position => position;

    public IReadOnlyList<string> Classes { get; } = AuctionViewModel.KnownClasses(player.Classes);
}

public sealed class ActivityItemViewModel(ActivityEntry entry)
{
    public string Time => entry.At.LocalDateTime.ToString("HH:mm");

    public string Text => entry.Text;

    public ActivityKind Kind => entry.Kind;

    public string Glyph => entry.Kind switch
    {
        ActivityKind.Sold => "",
        ActivityKind.Skipped => "",
        ActivityKind.Returned => "",
        ActivityKind.Stage => "",
        _ => "",
    };
}
