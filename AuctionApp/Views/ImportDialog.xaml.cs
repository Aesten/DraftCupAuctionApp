using System.Windows;
using AuctionApp.Services;

namespace AuctionApp.Views;

/// <summary>Explains the file format, then lets the user pick the file (the caller shows the file picker).</summary>
public partial class ImportDialog : Window
{
    public const string Example = "Name,Classes\nAlice,inf\nBob,arc cav\nCarol,cavalry";

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
                + "You can also start a new tournament from a list of players, for example one kept in Excel."),
        };
        HeadingText.Text = Title;
        ExampleText.Text = Example;
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Clipboard.SetText(Example.Replace("\n", Environment.NewLine));
            CopyLabel.Text = "Copied";
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            // The clipboard is busy; the example can still be selected and copied by hand.
        }
    }

    private void Open_Click(object sender, RoutedEventArgs e) => DialogResult = true;
}
