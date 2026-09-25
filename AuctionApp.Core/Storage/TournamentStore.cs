using System.Text.Json;
using AuctionApp.Core.Model;

namespace AuctionApp.Core.Storage;

public sealed record TournamentSummary(
    Guid Id,
    string Title,
    DateTimeOffset UpdatedAt,
    int PlayerCount,
    int DivisionCount,
    int DivisionsInProgress,
    int DivisionsFinished);

/// <summary>
/// The app's library of tournaments, kept in the user's local app data folder, one file per tournament. Writes are
/// atomic and the previous version is kept as a backup, so a crash in the middle of an auction can't corrupt it.
/// </summary>
public sealed class TournamentStore
{
    private const string BackupExtension = ".bak";

    public TournamentStore(string rootDirectory)
    {
        RootDirectory = rootDirectory;
        TournamentsDirectory = Path.Combine(rootDirectory, "Tournaments");
        DeletedDirectory = Path.Combine(rootDirectory, "Deleted");
        Directory.CreateDirectory(TournamentsDirectory);
    }

    public static string DefaultRootDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DraftCupAuction");

    public string RootDirectory { get; }

    public string TournamentsDirectory { get; }

    public string DeletedDirectory { get; }

    public IReadOnlyList<TournamentSummary> List()
    {
        var summaries = new List<TournamentSummary>();
        foreach (var path in Directory.EnumerateFiles(TournamentsDirectory, "*" + TournamentJson.FileExtension))
        {
            var tournament = TryRead(path) ?? TryRead(path + BackupExtension);
            if (tournament != null)
            {
                summaries.Add(Summarize(tournament));
            }
        }

        return summaries.OrderByDescending(summary => summary.UpdatedAt).ToList();
    }

    public Tournament Load(Guid id)
    {
        var path = PathFor(id);
        try
        {
            return TournamentJson.Deserialize(File.ReadAllText(path));
        }
        catch (Exception ex) when (ex is IOException or JsonException && File.Exists(path + BackupExtension))
        {
            // The main file is damaged or unreadable: fall back to the previous save.
            return TournamentJson.Deserialize(File.ReadAllText(path + BackupExtension));
        }
    }

    public bool Exists(Guid id) => File.Exists(PathFor(id));

    /// <summary>Writes the tournament. It keeps its own timestamps: callers update them when they change something.</summary>
    public void Save(Tournament tournament)
    {
        var json = TournamentJson.Serialize(tournament);
        var path = PathFor(tournament.Id);
        var temp = path + ".tmp";

        File.WriteAllText(temp, json);
        if (File.Exists(path))
        {
            File.Copy(path, path + BackupExtension, overwrite: true);
        }

        File.Move(temp, path, overwrite: true);
    }

    /// <summary>Moves the tournament to the "Deleted" folder rather than erasing it, just in case.</summary>
    public void Delete(Guid id)
    {
        Directory.CreateDirectory(DeletedDirectory);
        var path = PathFor(id);
        if (File.Exists(path))
        {
            File.Move(path, Path.Combine(DeletedDirectory, $"{id:N}.{Stamp()}{TournamentJson.FileExtension}"), overwrite: true);
        }

        if (File.Exists(path + BackupExtension))
        {
            File.Delete(path + BackupExtension);
        }
    }

    /// <summary>Keeps a copy of the tournament as it is now in the "Deleted" folder, before something destructive.</summary>
    public void SaveCopyAside(Tournament tournament, string reason)
    {
        Directory.CreateDirectory(DeletedDirectory);
        File.WriteAllText(
            Path.Combine(DeletedDirectory, $"{tournament.Id:N}.{Stamp()}.{reason}{TournamentJson.FileExtension}"),
            TournamentJson.Serialize(tournament));
    }

    public static TournamentSummary Summarize(Tournament tournament) => new(
        tournament.Id,
        tournament.Title,
        tournament.UpdatedAt,
        tournament.Players.Count,
        tournament.Divisions.Count,
        tournament.Divisions.Count(division => division.Status == DivisionStatus.InProgress),
        tournament.Divisions.Count(division => division.Status == DivisionStatus.Finished));

    private static string Stamp() => DateTime.Now.ToString("yyyyMMdd-HHmmss");

    private string PathFor(Guid id) => Path.Combine(TournamentsDirectory, id.ToString("N") + TournamentJson.FileExtension);

    private static Tournament? TryRead(string path)
    {
        try
        {
            return File.Exists(path) ? TournamentJson.Deserialize(File.ReadAllText(path)) : null;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
