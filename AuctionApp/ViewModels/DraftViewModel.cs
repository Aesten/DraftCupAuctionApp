using System.IO;
using System.Windows.Threading;
using AuctionApp.Core.Model;
using AuctionApp.Core.Storage;
using AuctionApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuctionApp.ViewModels;

/// <summary>An open draft with its three pages (setup, auction, results). Takes care of saving it automatically.</summary>
public sealed partial class DraftViewModel : ObservableObject
{
    public const int SetupTab = 0;
    public const int AuctionTab = 1;
    public const int ResultsTab = 2;

    private readonly Action _goHome;
    private readonly DispatcherTimer _saveTimer;
    private bool _dirty;

    public DraftViewModel(Draft draft, DraftStore store, IDialogService dialogs, Action goHome)
    {
        Draft = draft;
        Store = store;
        Dialogs = dialogs;
        _goHome = goHome;
        _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
        _saveTimer.Tick += (_, _) => SaveNow();

        Setup = new SetupViewModel(this);
        Auction = new AuctionViewModel(this);
        Results = new ResultsViewModel(this);
        SelectedTab = draft.Status switch
        {
            DraftStatus.Setup => SetupTab,
            DraftStatus.InProgress => AuctionTab,
            _ => ResultsTab,
        };
    }

    public Draft Draft { get; }

    public DraftStore Store { get; }

    public IDialogService Dialogs { get; }

    public SetupViewModel Setup { get; }

    public AuctionViewModel Auction { get; }

    public ResultsViewModel Results { get; }

    [ObservableProperty]
    public partial int SelectedTab { get; set; }

    /// <summary>Set when the last save failed (disk full, folder not writable...). Shown as a banner.</summary>
    [ObservableProperty]
    public partial string? SaveError { get; set; }

    public string Title => string.IsNullOrWhiteSpace(Draft.Title) ? "Untitled draft" : Draft.Title;

    public string StatusText => Draft.Status switch
    {
        DraftStatus.Setup => "Setting up",
        DraftStatus.InProgress => "Auction in progress",
        _ => "Auction finished",
    };

    public bool HasSession => Draft.Session != null;

    /// <summary>Saves shortly after the last change, so typing doesn't write the file on every key press.</summary>
    public void MarkDirty()
    {
        _dirty = true;
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    /// <summary>Saves right away. Used after every auction action so nothing is lost if the PC crashes.</summary>
    public void Commit()
    {
        _dirty = true;
        SaveNow();
    }

    public void SaveNow()
    {
        _saveTimer.Stop();
        if (!_dirty)
        {
            return;
        }

        try
        {
            Store.Save(Draft);
            _dirty = false;
            SaveError = null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            SaveError = $"Your changes couldn't be saved: {ex.Message}";
        }
    }

    partial void OnSelectedTabChanged(int value)
    {
        if (value == ResultsTab)
        {
            Results.Refresh();
        }
    }

    public void OnTitleChanged() => OnPropertyChanged(nameof(Title));

    /// <summary>Called when the auction starts or is reset, so every page shows the new state.</summary>
    public void OnSessionReplaced()
    {
        Auction.Reload();
        OnSessionStatusChanged();
    }

    /// <summary>Called when the auction finishes or is reopened (the auction page updates itself).</summary>
    public void OnSessionStatusChanged()
    {
        Setup.RefreshLock();
        Results.Refresh();
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(HasSession));
    }

    [RelayCommand]
    private void GoBack() => _goHome();
}
