using System.IO;

namespace AuctionApp.Services;

/// <summary>Reading files picked or dropped by the user, with a size limit so a wrong file can't freeze the app.</summary>
public static class FileLimits
{
    /// <summary>Far more than any tournament or player list (a big tournament is well under 1 MB).</summary>
    public const long MaxBytes = 20L * 1024 * 1024;

    /// <summary>Reads a text file, or throws <see cref="InvalidDataException"/> if it's too big to be one of ours.</summary>
    public static string ReadText(string path)
    {
        if (new FileInfo(path).Length > MaxBytes)
        {
            throw new InvalidDataException("This file is too big to be a tournament or a player list.");
        }

        return File.ReadAllText(path);
    }
}
