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

    public PlayerListImportDialog()
    {
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
        ExampleText.Text = IsJson ? JsonExample : CsvExample;
        FormatNote.Text = IsJson ? JsonNote : CsvNote;
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
