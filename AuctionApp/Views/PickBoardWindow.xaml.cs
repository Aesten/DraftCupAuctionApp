using System.Windows;
using System.Windows.Input;
using AuctionApp.ViewModels;

namespace AuctionApp.Views;

/// <summary>Captain Pick: the pick board, in its own window so it can sit on another screen or be shared on stream.</summary>
public partial class PickBoardWindow : Window
{
    public PickBoardWindow(AuctionViewModel auction)
    {
        InitializeComponent();
        DataContext = auction;
        Title = $"Pick board — {auction.Title}";
    }

    /// <summary>
    /// Hands the focus back to the app before closing: otherwise Windows activates whatever window was used before,
    /// and the app ends up behind it.
    /// </summary>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        base.OnClosing(e);
        if (!e.Cancel && IsActive)
        {
            Owner?.Activate();
        }
    }

    /// <summary>
    /// Enter: back to the auction once a player is on the block (the board closes). Esc: leaves full screen, else
    /// closes the board. F11: full screen on / off.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.F11)
        {
            SetFullScreen(WindowStyle != WindowStyle.None);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && WindowStyle == WindowStyle.None)
        {
            SetFullScreen(false);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape || e.Key == Key.Enter && DataContext is AuctionViewModel { HasCurrentPlayer: true })
        {
            Close();
            e.Handled = true;
        }
    }

    private void SetFullScreen(bool on)
    {
        // Leaving the maximized state first lets the window cover the taskbar when it becomes borderless.
        WindowState = WindowState.Normal;
        WindowStyle = on ? WindowStyle.None : WindowStyle.SingleBorderWindow;
        ResizeMode = on ? ResizeMode.NoResize : ResizeMode.CanResize;
        WindowState = on ? WindowState.Maximized : WindowState.Normal;
    }
}
