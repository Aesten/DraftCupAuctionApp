using System.Windows;
using System.Windows.Controls;

namespace AuctionApp.Views;

/// <summary>
/// Explains the player list format (CSV or JSON, see <see cref="Core.Storage.PlayerList"/>), then lets the user pick
/// the file (the caller shows the file picker). The same formats the pool's Export writes.
/// </summary>
public partial class PlayerListImportDialog : Window
{
    private const string CsvExample = "Player,INF,ARC,CAV\nAlice,x,,\nBob,,x,x\nCarol,,,x";

    private const string CsvNote =
        "One player per line, with a column per class marked x (the layout the previous version of the app exported). "
        + "From Excel: File › Save As › CSV. A column of classes written out (\"inf cav\") works too.";

    private const string JsonExample = """
        {
          "players": [
            { "name": "Alice", "classes": ["inf"] },
            { "name": "Bob", "classes": ["arc", "cav"] },
            { "name": "Carol", "classes": ["cav"] }
          ]
        }
        """;

    private const string JsonNote = "Players the way the previous version of the app stored them. Classes are inf, arc and cav.";

    // Captain Pick: one class per player, and a tier from 1 to 5.
    private const string TieredCsvExample = "Player,INF,ARC,CAV,Tier\nAlice,x,,,1\nBob,,x,,3\nCarol,,,x,5";

    private const string TieredCsvNote =
        "One player per line: a column per class marked x (one class per player), then the tier, 1 to 5. "
        + "From Excel: File › Save As › CSV. Lines like \"Alice, inf, 3\" work too.";

    private const string TieredJsonExample = """
        {
          "players": [
            { "name": "Alice", "classes": ["inf"], "tier": 1 },
            { "name": "Bob", "classes": ["arc"], "tier": 3 },
            { "name": "Carol", "classes": ["cav"], "tier": 5 }
          ]
        }
        """;

    private const string TieredJsonNote = "Classes are inf, arc and cav (one per player); tiers go from 1 to 5.";

    private readonly bool _withTiers;

    /// <param name="withTiers">Captain Pick: the examples show a tier and a single class per player.</param>
    public PlayerListImportDialog(bool withTiers = false)
    {
        _withTiers = withTiers;
        InitializeComponent();
        Title = "Import a player list";
        HeadingText.Text = Title;
        IntroText.Text = "Adds the players of a list, for example from a sign-up sheet, to the pool. Names already in the pool are skipped. "
            + "The pool's Export… saves the list in the same formats.";
        ShowFormat();
    }

    private bool IsJson => FormatList.SelectedIndex == 1;

    private void FormatList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded)
        {
            ShowFormat();
        }
    }

    private void ShowFormat()
    {
        ExampleText.Text = (IsJson, _withTiers) switch
        {
            (true, true) => TieredJsonExample,
            (true, false) => JsonExample,
            (false, true) => TieredCsvExample,
            _ => CsvExample,
        };
        FormatNote.Text = (IsJson, _withTiers) switch
        {
            (true, true) => TieredJsonNote,
            (true, false) => JsonNote,
            (false, true) => TieredCsvNote,
            _ => CsvNote,
        };
        CopyLabel.Text = "Copy";
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Clipboard.SetText(ExampleText.Text.Replace("\n", Environment.NewLine));
            CopyLabel.Text = "Copied";
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            // The clipboard is busy; the example can still be selected and copied by hand.
        }
    }

    private void Open_Click(object sender, RoutedEventArgs e) => DialogResult = true;
}
