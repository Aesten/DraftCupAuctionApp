using System.IO;
using System.Text.Json;
using AuctionApp.Core.Storage;
using AuctionApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AuctionApp.ViewModels;

/// <summary>Switches between the list of drafts and an open draft.</summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly DraftStore _store;
    private readonly IDialogService _dialogs;

    public MainViewModel(DraftStore store, IDialogService dialogs)
    {
        _store = store;
        _dialogs = dialogs;
        Home = new HomeViewModel(store, dialogs, OpenDraft);
        Home.Refresh();
        CurrentPage = Home;
    }

    public HomeViewModel Home { get; }

    [ObservableProperty]
    public partial object CurrentPage { get; set; }

    public void OpenDraft(Guid id)
    {
        try
        {
            var draft = _store.Load(id);
            CurrentPage = new DraftViewModel(draft, _store, _dialogs, GoHome);
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            _dialogs.ShowError("Couldn't open this draft", ex.Message);
            Home.Refresh();
        }
    }

    public void GoHome()
    {
        if (CurrentPage is DraftViewModel draft)
        {
            draft.SaveNow();
        }

        Home.Refresh();
        CurrentPage = Home;
    }

    /// <summary>Called when the window closes, so the last edits are written.</summary>
    public void Shutdown()
    {
        if (CurrentPage is DraftViewModel draft)
        {
            draft.SaveNow();
        }
    }
}
