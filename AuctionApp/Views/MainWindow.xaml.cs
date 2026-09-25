using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using AuctionApp.ViewModels;

namespace AuctionApp.Views;

public partial class MainWindow : Window
{
    private WindowState _stateBeforeFullScreen = WindowState.Normal;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        (DataContext as MainViewModel)?.Shutdown();
        base.OnClosing(e);
    }

    /// <summary>F11 toggles full screen, handy when the auction is shown on a stream or projector.</summary>
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (e.Key != Key.F11)
        {
            return;
        }

        if (WindowStyle == WindowStyle.None)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = _stateBeforeFullScreen;
        }
        else
        {
            _stateBeforeFullScreen = WindowState;
            WindowStyle = WindowStyle.None;
            // Going through Normal makes Windows recompute the maximized bounds without the title bar.
            WindowState = WindowState.Normal;
            WindowState = WindowState.Maximized;
        }

        e.Handled = true;
    }
}
