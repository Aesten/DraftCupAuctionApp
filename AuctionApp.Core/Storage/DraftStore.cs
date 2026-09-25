using System.Text.Json;
using AuctionApp.Core.Model;

namespace AuctionApp.Core.Storage;

public sealed record DraftSummary(
    Guid Id,
    string Title,
    DraftStatus Status,
    DateTimeOffset UpdatedAt,
    int CaptainCount,
    int PlayerCount,
    int StageCount,
    int SoldCount);

/// <summary>
/// Keeps drafts in the user's local app data folder, one file per draft. Writes are atomic and the previous
/// version is kept as a backup, so a crash in the middle of an auction can't corrupt it.
/// </summary>
public sealed class DraftStore
{
    private const string Extension = ".draft.json";
    private const string BackupExtension = ".bak";

    public DraftStore(string rootDirectory)
    {
        RootDirectory = rootDirectory;
        DraftsDirectory = Path.Combine(rootDirectory, "Drafts");
        DeletedDirectory = Path.Combine(rootDirectory, "Deleted");
        Directory.CreateDirectory(DraftsDirectory);
    }

    public static string DefaultRootDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DraftCupAuction");

    public string RootDirectory { get; }

    public string DraftsDirectory { get; }

    public string DeletedDirectory { get; }

    public IReadOnlyList<DraftSummary> List()
    {
        var summaries = new List<DraftSummary>();
        foreach (var path in Directory.EnumerateFiles(DraftsDirectory, "*" + Extension))
        {
            var draft = TryRead(path) ?? TryRead(path + BackupExtension);
            if (draft != null)
            {
                summaries.Add(Summarize(draft));
            }
        }

        return summaries.OrderByDescending(summary => summary.UpdatedAt).ToList();
    }

    public Draft Load(Guid id)
    {
        var path = PathFor(id);
        try
        {
            return DraftJson.Deserialize(File.ReadAllText(path));
        }
        catch (Exception ex) when (ex is IOException or JsonException && File.Exists(path + BackupExtension))
        {
            // The main file is damaged or unreadable: fall back to the previous save.
            return DraftJson.Deserialize(File.ReadAllText(path + BackupExtension));
        }
    }

    public void Save(Draft draft)
    {
        draft.UpdatedAt = DateTimeOffset.Now;
        var json = DraftJson.Serialize(draft);
        var path = PathFor(draft.Id);
        var temp = path + ".tmp";

        File.WriteAllText(temp, json);
        if (File.Exists(path))
        {
            File.Copy(path, path + BackupExtension, overwrite: true);
        }

        File.Move(temp, path, overwrite: true);
    }

    /// <summary>Moves the draft to the "Deleted" folder rather than erasing it, just in case.</summary>
    public void Delete(Guid id)
    {
        Directory.CreateDirectory(DeletedDirectory);
        var path = PathFor(id);
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        if (File.Exists(path))
        {
            File.Move(path, Path.Combine(DeletedDirectory, $"{id:N}.{stamp}{Extension}"), overwrite: true);
        }

        if (File.Exists(path + BackupExtension))
        {
            File.Delete(path + BackupExtension);
        }
    }

    /// <summary>Keeps a copy of the draft as it is now in the "Deleted" folder, before something destructive like a reset.</summary>
    public void SaveCopyAside(Draft draft, string reason)
    {
        Directory.CreateDirectory(DeletedDirectory);
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        File.WriteAllText(Path.Combine(DeletedDirectory, $"{draft.Id:N}.{stamp}.{reason}{Extension}"), DraftJson.Serialize(draft));
    }

    public bool Exists(Guid id) => File.Exists(PathFor(id));

    public static DraftSummary Summarize(Draft draft) => new(
        draft.Id,
        draft.Title,
        draft.Status,
        draft.UpdatedAt,
        draft.Captains.Count,
        draft.Players.Count,
        draft.Stages.Count,
        draft.Session?.SoldCount ?? 0);

    private string PathFor(Guid id) => Path.Combine(DraftsDirectory, id.ToString("N") + Extension);

    private static Draft? TryRead(string path)
    {
        try
        {
            return File.Exists(path) ? DraftJson.Deserialize(File.ReadAllText(path)) : null;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
