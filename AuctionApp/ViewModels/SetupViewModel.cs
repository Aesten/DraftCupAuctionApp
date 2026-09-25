using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;
using AuctionApp.Core.Engine;
using AuctionApp.Core.Model;
using AuctionApp.Core.Storage;
using AuctionApp.Services;
using AuctionApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuctionApp.ViewModels;

/// <summary>
/// Editing a draft before the auction: general settings, captains, stages and players. The rows edit the draft
/// directly and every change is saved automatically.
/// </summary>
public sealed partial class SetupViewModel : ObservableObject
{
    private readonly DraftViewModel _owner;

    public SetupViewModel(DraftViewModel owner)
    {
        _owner = owner;

        foreach (var stage in Draft.Stages)
        {
            Stages.Add(new StageRowViewModel(stage, this));
        }

        foreach (var captain in Draft.Captains)
        {
            Captains.Add(new CaptainRowViewModel(captain, this));
        }

        foreach (var player in Draft.Players)
        {
            Players.Add(new PlayerRowViewModel(player) { Owner = this });
        }

        DefaultBudgetText = Money.Format(Draft.DefaultBudget);
        Players.CollectionChanged += OnPlayersCollectionChanged;
        PlayersView = CollectionViewSource.GetDefaultView(Players);
        PlayersView.Filter = item => item is PlayerRowViewModel row && MatchesSearch(row);
        RenumberStages();
        Validate();
    }

    private Draft Draft => _owner.Draft;

    public IReadOnlyList<int> TeamSizeOptions { get; } = Enumerable.Range(1, 20).ToList();

    public ObservableCollection<CaptainRowViewModel> Captains { get; } = [];

    public ObservableCollection<StageRowViewModel> Stages { get; } = [];

    public ObservableCollection<PlayerRowViewModel> Players { get; } = [];

    public ICollectionView PlayersView { get; }

    public ObservableCollection<ValidationIssue> Issues { get; } = [];

    public bool IsLocked => Draft.Session != null;

    public bool IsEditable => !IsLocked;

    public string Title
    {
        get => Draft.Title;
        set
        {
            if (Draft.Title == value)
            {
                return;
            }

            Draft.Title = value;
            OnPropertyChanged();
            _owner.OnTitleChanged();
            Changed();
        }
    }

    public int TeamSize
    {
        get => Draft.TeamSize;
        set
        {
            if (Draft.TeamSize == value || value < 1)
            {
                return;
            }

            Draft.TeamSize = value;
            OnPropertyChanged();
            Changed();
        }
    }

    public bool ShuffleOrder
    {
        get => Draft.ShuffleOrder;
        set
        {
            if (Draft.ShuffleOrder == value)
            {
                return;
            }

            Draft.ShuffleOrder = value;
            OnPropertyChanged();
            Changed();
        }
    }

    [ObservableProperty]
    public partial string DefaultBudgetText { get; set; }

    [ObservableProperty]
    public partial bool HasDefaultBudgetError { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool CanStart { get; set; }

    [ObservableProperty]
    public partial string Summary { get; set; } = string.Empty;

    public bool HasMultipleStages => Stages.Count > 1;

    public string CaptainsHeader => $"Captains ({Captains.Count})";

    public string PlayersHeader => $"Players ({Players.Count})";

    partial void OnDefaultBudgetTextChanged(string value)
    {
        HasDefaultBudgetError = !Money.TryParse(value, out var budget) || budget < 0;
        if (!HasDefaultBudgetError && Draft.DefaultBudget != budget)
        {
            Draft.DefaultBudget = budget;
            Changed();
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        EndPendingEdits();
        PlayersView.Refresh();
    }

    /// <summary>The list can't be filtered or changed while the grid is in the middle of adding or editing a row.</summary>
    private void EndPendingEdits()
    {
        if (PlayersView is IEditableCollectionView editable)
        {
            if (editable.IsAddingNew)
            {
                editable.CommitNew();
            }

            if (editable.IsEditingItem)
            {
                editable.CommitEdit();
            }
        }
    }

    /// <summary>Records a change: re-checks the setup and schedules a save.</summary>
    internal void Changed()
    {
        Validate();
        _owner.MarkDirty();
    }

    internal void RefreshLock()
    {
        OnPropertyChanged(nameof(IsLocked));
        OnPropertyChanged(nameof(IsEditable));
        Validate();
    }

    internal int StageOrder(Guid stageId) => Stages.ToList().FindIndex(stage => stage.Id == stageId);

    private void Validate()
    {
        Issues.Clear();
        if (!IsLocked)
        {
            foreach (var issue in DraftValidator.Validate(Draft).OrderByDescending(issue => issue.Severity))
            {
                Issues.Add(issue);
            }
        }

        CanStart = !IsLocked && Issues.All(issue => issue.Severity != IssueSeverity.Error);
        var spots = Draft.Captains.Count * Draft.TeamSize;
        Summary = $"{Draft.Captains.Count} teams × {Draft.TeamSize} players = {spots} spots  ·  {Draft.Players.Count} players signed up";
        OnPropertyChanged(nameof(CaptainsHeader));
        OnPropertyChanged(nameof(PlayersHeader));
        foreach (var stage in Stages)
        {
            stage.NotifyPlayerCount();
        }
    }

    private bool MatchesSearch(PlayerRowViewModel row) =>
        string.IsNullOrWhiteSpace(SearchText) || row.Name.Contains(SearchText.Trim(), StringComparison.CurrentCultureIgnoreCase);

    // Captains

    [RelayCommand]
    private void AddCaptain()
    {
        var captain = new Captain { Name = string.Empty, Budget = Draft.DefaultBudget };
        Draft.Captains.Add(captain);
        Captains.Add(new CaptainRowViewModel(captain, this) { FocusRequested = true });
        Changed();
    }

    [RelayCommand]
    private void RemoveCaptain(CaptainRowViewModel row)
    {
        Draft.Captains.Remove(row.Model);
        Captains.Remove(row);
        Changed();
    }

    [RelayCommand]
    private void ApplyBudgetToAll()
    {
        foreach (var captain in Captains)
        {
            captain.BudgetText = Money.Format(Draft.DefaultBudget);
        }
    }

    // Stages

    [RelayCommand]
    private void AddStage()
    {
        var stage = new Stage { Name = Stages.Count == 1 ? "Second auction" : $"Auction {Stages.Count + 1}" };
        if (Stages.Count == 1 && Stages[0].Name == Stage.CreateDefault().Name)
        {
            // Going from one auction to several: name them after tiers, which is what they're usually for.
            Stages[0].Name = "High tier";
            stage.Name = "Low tier";
        }

        Draft.Stages.Add(stage);
        Stages.Add(new StageRowViewModel(stage, this));
        StagesChanged();
    }

    [RelayCommand]
    private void RemoveStage(StageRowViewModel row)
    {
        if (Stages.Count <= 1)
        {
            return;
        }

        var index = Stages.IndexOf(row);
        var fallback = Stages[index == 0 ? 1 : index - 1];
        var affected = Players.Count(player => player.StageId == row.Id);
        if (affected > 0)
        {
            var answer = _owner.Dialogs.Ask(
                $"Remove \"{row.Name}\"?",
                $"{affected} player(s) are in this stage. They will be moved to \"{fallback.Name}\".",
                "Remove stage");
            if (answer != DialogChoice.Primary)
            {
                return;
            }
        }

        foreach (var player in Players.Where(player => player.StageId == row.Id))
        {
            player.StageId = fallback.Id;
        }

        Draft.Stages.Remove(row.Model);
        Stages.Remove(row);
        StagesChanged();
    }

    [RelayCommand]
    private void MoveStageUp(StageRowViewModel row) => MoveStage(row, -1);

    [RelayCommand]
    private void MoveStageDown(StageRowViewModel row) => MoveStage(row, +1);

    private void MoveStage(StageRowViewModel row, int offset)
    {
        var index = Stages.IndexOf(row);
        var target = index + offset;
        if (index < 0 || target < 0 || target >= Stages.Count)
        {
            return;
        }

        Stages.Move(index, target);
        Draft.Stages.RemoveAt(index);
        Draft.Stages.Insert(target, row.Model);
        StagesChanged();
    }

    private void StagesChanged()
    {
        RenumberStages();
        OnPropertyChanged(nameof(HasMultipleStages));
        foreach (var player in Players)
        {
            player.NotifyStageChanged();
        }

        Changed();
    }

    private void RenumberStages()
    {
        for (var i = 0; i < Stages.Count; i++)
        {
            Stages[i].Number = i + 1;
            Stages[i].IsFirst = i == 0;
            Stages[i].IsLast = i == Stages.Count - 1;
            Stages[i].CanRemove = Stages.Count > 1;
        }
    }

    // Players

    private void OnPlayersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Rows can be added and removed by the grid itself (new row placeholder, Delete key): mirror that in the draft.
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                var index = e.NewStartingIndex;
                foreach (PlayerRowViewModel row in e.NewItems!)
                {
                    row.Owner = this;
                    if (Stages.All(stage => stage.Id != row.StageId))
                    {
                        row.Model.StageId = Stages[0].Id;
                    }

                    Draft.Players.Insert(Math.Clamp(index++, 0, Draft.Players.Count), row.Model);
                }

                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (PlayerRowViewModel row in e.OldItems!)
                {
                    Draft.Players.Remove(row.Model);
                }

                break;
            case NotifyCollectionChangedAction.Reset:
                Draft.Players.Clear();
                Draft.Players.AddRange(Players.Select(row => row.Model));
                break;
        }

        Changed();
    }

    [RelayCommand]
    private void PastePlayers()
    {
        EndPendingEdits();
        var result = _owner.Dialogs.AskForRoster(Stages.Select(stage => new StageChoice(stage.Id, stage.Name)).ToList());
        if (result is not { } roster)
        {
            return;
        }

        var existing = Players.Select(player => player.Name.Trim()).ToHashSet(StringComparer.CurrentCultureIgnoreCase);
        var skipped = 0;
        foreach (var parsed in roster.Players)
        {
            if (!existing.Add(parsed.Name))
            {
                skipped++;
                continue;
            }

            var stage = Stages.FirstOrDefault(s => string.Equals(s.Name, parsed.StageName, StringComparison.CurrentCultureIgnoreCase));
            Players.Add(new PlayerRowViewModel(new Player
            {
                Name = parsed.Name,
                Classes = parsed.Classes,
                StageId = stage?.Id ?? roster.StageId,
            }));
        }

        if (skipped > 0)
        {
            _owner.Dialogs.Ask("Some players were already listed", $"{skipped} name(s) were already in the list and were not added twice.", "OK", cancel: null);
        }
    }

    [RelayCommand]
    private void RemovePlayers(IList? selected)
    {
        var rows = selected?.OfType<PlayerRowViewModel>().ToList() ?? [];
        EndPendingEdits();
        foreach (var row in rows)
        {
            Players.Remove(row);
        }
    }

    [RelayCommand]
    private void ExportPlayers()
    {
        var path = _owner.Dialogs.PickFileToSave("Export the player list", "CSV spreadsheet (*.csv)|*.csv", $"{Draft.Title} players.csv");
        if (path != null)
        {
            _owner.Dialogs.TryWriteFile(path, DraftExporter.PlayersToCsv(Draft));
        }
    }

    // Auction

    [RelayCommand]
    private void StartAuction()
    {
        _owner.SaveNow();
        var warnings = Issues.Where(issue => issue.Severity == IssueSeverity.Warning).Select(issue => "• " + issue.Message).ToList();
        var message = "The setup will be locked while the auction runs.";
        if (warnings.Count > 0)
        {
            message = string.Join("\n", warnings) + "\n\n" + message;
        }

        if (_owner.Dialogs.Ask("Start the auction?", message, "Start auction") != DialogChoice.Primary)
        {
            return;
        }

        try
        {
            new AuctionEngine(Draft).Start();
        }
        catch (AuctionException ex)
        {
            _owner.Dialogs.ShowError("The auction can't start yet", ex.Message);
            return;
        }

        _owner.Commit();
        _owner.OnSessionReplaced();
        _owner.SelectedTab = DraftViewModel.AuctionTab;
    }

    [RelayCommand]
    private void ResetAuction()
    {
        var answer = _owner.Dialogs.Ask(
            "Reset the auction?",
            "Every sale and skip will be undone and the setup unlocked. A copy of the auction as it is now is kept in the app's data folder (Deleted).",
            "Reset auction");
        if (answer != DialogChoice.Primary)
        {
            return;
        }

        try
        {
            _owner.Store.SaveCopyAside(Draft, "before-reset");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            if (_owner.Dialogs.Ask("Couldn't keep a copy", ex.Message + "\n\nReset anyway?", "Reset anyway") != DialogChoice.Primary)
            {
                return;
            }
        }

        Draft.Session = null;
        _owner.Commit();
        _owner.OnSessionReplaced();
        _owner.SelectedTab = DraftViewModel.SetupTab;
    }
}

public sealed partial class CaptainRowViewModel : ObservableObject
{
    private readonly SetupViewModel _owner;

    public CaptainRowViewModel(Captain model, SetupViewModel owner)
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
            _owner.Changed();
        }
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

public sealed partial class StageRowViewModel : ObservableObject
{
    private readonly SetupViewModel _owner;

    public StageRowViewModel(Stage model, SetupViewModel owner)
    {
        Model = model;
        _owner = owner;
        MaxPicksText = model.MaxPicksPerTeam?.ToString() ?? string.Empty;
    }

    public Stage Model { get; }

    public Guid Id => Model.Id;

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
            _owner.Changed();
        }
    }

    /// <summary>Empty means no limit besides the team size.</summary>
    [ObservableProperty]
    public partial string MaxPicksText { get; set; }

    [ObservableProperty]
    public partial bool HasMaxPicksError { get; set; }

    [ObservableProperty]
    public partial int Number { get; set; }

    [ObservableProperty]
    public partial bool IsFirst { get; set; }

    [ObservableProperty]
    public partial bool IsLast { get; set; }

    [ObservableProperty]
    public partial bool CanRemove { get; set; }

    public string PlayerCountText => _owner.Players.Count(player => player.StageId == Id) switch
    {
        1 => "1 player",
        var count => $"{count} players",
    };

    internal void NotifyPlayerCount() => OnPropertyChanged(nameof(PlayerCountText));

    partial void OnMaxPicksTextChanged(string value)
    {
        int? max = null;
        HasMaxPicksError = false;
        if (!string.IsNullOrWhiteSpace(value))
        {
            if (int.TryParse(value.Trim(), out var parsed) && parsed > 0)
            {
                max = parsed;
            }
            else
            {
                HasMaxPicksError = true;
                return;
            }
        }

        if (Model.MaxPicksPerTeam != max)
        {
            Model.MaxPicksPerTeam = max;
            _owner?.Changed();
        }
    }
}

public sealed partial class PlayerRowViewModel : ObservableObject
{
    /// <summary>Used by the grid when the user types in its empty last row.</summary>
    public PlayerRowViewModel()
        : this(new Player())
    {
    }

    public PlayerRowViewModel(Player model) => Model = model;

    public Player Model { get; }

    internal SetupViewModel? Owner { get; set; }

    public string Name
    {
        get => Model.Name;
        set
        {
            var trimmed = value?.Trim() ?? string.Empty;
            if (Model.Name == trimmed)
            {
                return;
            }

            Model.Name = trimmed;
            OnPropertyChanged();
            Owner?.Changed();
        }
    }

    public bool Infantry
    {
        get => HasClass(PlayerClasses.Infantry);
        set => SetClass(PlayerClasses.Infantry, value);
    }

    public bool Archer
    {
        get => HasClass(PlayerClasses.Archer);
        set => SetClass(PlayerClasses.Archer, value);
    }

    public bool Cavalry
    {
        get => HasClass(PlayerClasses.Cavalry);
        set => SetClass(PlayerClasses.Cavalry, value);
    }

    public Guid StageId
    {
        get => Model.StageId;
        set
        {
            if (Model.StageId == value || value == Guid.Empty)
            {
                return;
            }

            Model.StageId = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StageOrder));
            Owner?.Changed();
        }
    }

    /// <summary>Lets the grid sort players by stage in auction order.</summary>
    public int StageOrder => Owner?.StageOrder(StageId) ?? 0;

    internal void NotifyStageChanged()
    {
        OnPropertyChanged(nameof(StageId));
        OnPropertyChanged(nameof(StageOrder));
    }

    private bool HasClass(string code) => Model.Classes.Contains(code);

    private void SetClass(string code, bool value, [System.Runtime.CompilerServices.CallerMemberName] string? property = null)
    {
        if (HasClass(code) == value)
        {
            return;
        }

        var classes = value ? Model.Classes.Append(code) : Model.Classes.Where(c => c != code);
        Model.Classes = PlayerClasses.Normalize(classes);
        OnPropertyChanged(property);
        Owner?.Changed();
    }
}
