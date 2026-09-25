using System.Windows;
using System.Windows.Controls;
using AuctionApp.Services;

namespace AuctionApp.Views;

/// <summary>
/// Explains the file formats, then lets the user pick the file (the caller shows the file picker). The formats are
/// the previous version of the app's: its player list CSV export and its auction plan JSON files.
/// </summary>
public partial class ImportDialog : Window
{
    private const string CsvExample = "Player,INF,ARC,CAV\nAlice,x,,\nBob,,x,x\nCarol,,,x";

    private const string CsvNote =
        "One player per line, with a column per class marked x: the player list the previous version of the app exported. "
        + "From Excel: File › Save As › CSV. The pool's Export file has the same columns and can be imported as it is.";

    private const string JsonExample = """
        {
          "type": "Auction",
          "title": "Spring Cup",
          "teamSize": 6,
          "players": [
            { "name": "Alice", "classes": ["inf"] },
            { "name": "Bob", "classes": ["arc", "cav"] }
          ],
          "captains": [
            { "name": "Dave", "budget": 20, "class": "cav" }
          ]
        }
        """;

    private const string JsonNote =
        "An auction plan of the previous version of the app. Classes are inf, arc and cav. "
        + "Captains' \"class\" is optional (the previous version didn't have it).";

    public ImportDialog(ImportKind kind)
    {
        InitializeComponent();
        (Title, IntroText.Text) = kind switch
        {
            ImportKind.Players => (
                "Import players",
                "Adds the players listed in a file to the pool. Players already in the pool are skipped."),
            _ => (
                "Import a tournament",
                "Open a tournament exported from this app (.draftcup.json). If you already have a copy of it, the two are merged.\n\n"
                + "You can also start a new tournament from a player list or an auction plan, for example one kept in Excel."),
        };
        HeadingText.Text = Title;
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
