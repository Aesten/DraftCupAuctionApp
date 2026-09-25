using AuctionApp.Core.Model;
using AuctionApp.Core.Storage;

namespace AuctionApp.Core.Engine;

/// <summary>Snapshots of an auction session taken before each action, so mistakes can be undone.</summary>
public sealed class UndoHistory(int capacity = 100)
{
    private readonly LinkedList<(string Description, AuctionSession Snapshot)> _entries = new();

    public bool CanUndo => _entries.Count > 0;

    /// <summary>What the next undo would revert, for display.</summary>
    public string? NextDescription => _entries.Last?.Value.Description;

    public void Record(string description, AuctionSession session)
    {
        _entries.AddLast((description, TournamentJson.CloneSession(session)));
        while (_entries.Count > capacity)
        {
            _entries.RemoveFirst();
        }
    }

    /// <summary>Removes the latest snapshot without applying it (when the recorded action didn't go through).</summary>
    public void Discard()
    {
        if (_entries.Count > 0)
        {
            _entries.RemoveLast();
        }
    }

    public AuctionSession? Pop()
    {
        if (_entries.Last is not { } last)
        {
            return null;
        }

        _entries.RemoveLast();
        return last.Value.Snapshot;
    }

    public void Clear() => _entries.Clear();
}
