using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using AuctionApp.Core.Model;
using AuctionApp.Core.Storage;
using AuctionApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AuctionApp.ViewModels;

/// <summary>The start page: every saved draft, and ways to create or import one.</summary>
public sealed partial class HomeViewModel(DraftStore store, IDialogService dialogs, Action<Guid> open) : ObservableObject
{
    private const string JsonFilter = "Draft files (*.json)|*.json|All files (*.*)|*.*";

    public ObservableCollection<DraftCardViewModel> Drafts { get; } = [];

    [ObservableProperty]
    public partial bool IsEmpty { get; set; }

    public string DataFolder => store.RootDirectory;

    public void Refresh()
    {
        Drafts.Clear();
        foreach (var summary in store.List())
        {
            Drafts.Add(new DraftCardViewModel(summary));
        }

        IsEmpty = Drafts.Count == 0;
    }

    [RelayCommand]
    private void NewDraft()
    {
        var draft = new Draft { Title = $"Draft Cup {DateTime.Now:MMMM yyyy}" };
        if (TrySave(draft))
        {
            open(draft.Id);
        }
    }

    [RelayCommand]
    private void Open(DraftCardViewModel card) => open(card.Id);

    [RelayCommand]
    private void Duplicate(DraftCardViewModel card)
    {
        var copy = store.Load(card.Id).CloneSetup($"{card.Title} (copy)");
        if (TrySave(copy))
        {
            Refresh();
        }
    }

    [RelayCommand]
    private void Delete(DraftCardViewModel card)
    {
        var answer = dialogs.Ask(
            "Delete this draft?",
            $"\"{card.Title}\" will be removed from the list. A copy is kept in the app's data folder (Deleted) in case you change your mind.",
            "Delete");
        if (answer != DialogChoice.Primary)
        {
            return;
        }

        store.Delete(card.Id);
        Refresh();
    }

    [RelayCommand]
    private void Export(DraftCardViewModel card)
    {
        var path = dialogs.PickFileToSave("Export a backup of this draft", JsonFilter, card.Title + ".json");
        if (path != null)
        {
            dialogs.TryWriteFile(path, DraftExporter.ToBackupJson(store.Load(card.Id)));
        }
    }

    [RelayCommand]
    private void Import()
    {
        var path = dialogs.PickFileToOpen("Import a draft (backup or file from the previous version of the app)", JsonFilter);
        if (path == null)
        {
            return;
        }

        Draft draft;
        try
        {
            draft = DraftImporter.Import(File.ReadAllText(path), Path.GetFileNameWithoutExtension(path));
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or JsonException or UnauthorizedAccessException)
        {
            dialogs.ShowError("Couldn't import this file", ex.Message);
            return;
        }

        if (TrySave(draft))
        {
            open(draft.Id);
        }
    }

    [RelayCommand]
    private void OpenDataFolder() => dialogs.OpenFolder(store.RootDirectory);

    private bool TrySave(Draft draft)
    {
        try
        {
            store.Save(draft);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            dialogs.ShowError("Couldn't save the draft", ex.Message);
            return false;
        }
    }
}

public sealed class DraftCardViewModel(DraftSummary summary)
{
    public Guid Id => summary.Id;

    public string Title => string.IsNullOrWhiteSpace(summary.Title) ? "Untitled draft" : summary.Title;

    public DraftStatus Status => summary.Status;

    public string StatusText => summary.Status switch
    {
        DraftStatus.Setup => "Setting up",
        DraftStatus.InProgress => "Auction in progress",
        _ => "Finished",
    };

    public string Details
    {
        get
        {
            var parts = new List<string>
            {
                Plural(summary.CaptainCount, "captain"),
                Plural(summary.PlayerCount, "player"),
            };
            if (summary.StageCount > 1)
            {
                parts.Add(Plural(summary.StageCount, "stage"));
            }

            if (summary.Status != DraftStatus.Setup)
            {
                parts.Add($"{summary.SoldCount} sold");
            }

            return string.Join("  ·  ", parts);
        }
    }

    public string UpdatedText => "Edited " + Relative(summary.UpdatedAt);

    private static string Plural(int count, string noun) => count == 1 ? $"1 {noun}" : $"{count} {noun}s";

    private static string Relative(DateTimeOffset time)
    {
        var elapsed = DateTimeOffset.Now - time;
        return elapsed.TotalMinutes switch
        {
            < 1 => "just now",
            < 60 => $"{(int)elapsed.TotalMinutes} min ago",
            < 60 * 24 => $"{(int)elapsed.TotalHours} h ago",
            < 60 * 24 * 7 => time.LocalDateTime.ToString("dddd HH:mm"),
            _ => time.LocalDateTime.ToString("d MMM yyyy"),
        };
    }
}
