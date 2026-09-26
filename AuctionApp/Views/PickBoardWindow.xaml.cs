using System.Windows;
using System.Windows.Input;
using AuctionApp.ViewModels;

namespace AuctionApp.Views;

/// <summary>Captain Pick: the pick board, in its own window so it can sit on another screen or be shared on stream.</summary>
public partial class PickBoardWindow : Window
{
    private const long DoubleClickMilliseconds = 500;

    private Guid _lastClickedId;
    private long _lastClickAt;

    public PickBoardWindow(AuctionViewModel auction)
    {
        InitializeComponent();
        DataContext = auction;
        auction.Detached += Close;
        Closed += (_, _) => auction.Detached -= Close;
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
    /// Esc: leaves full screen, else closes the board. F11: full screen on / off.
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
        else if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Double-clicking a player picks them and goes back to the auction: the first click already put them on the
    /// block, the second one closes the board.
    /// </summary>
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);
        if (e.OriginalSource is not FrameworkElement { DataContext: BoardPlayerViewModel player })
        {
            return;
        }

        // The first click rebuilds the board (the player gets highlighted), so the second one may land on a new tile:
        // the time between the two clicks on the same player is checked too.
        var now = Environment.TickCount64;
        var isDoubleClick = e.ClickCount >= 2 || player.Id == _lastClickedId && now - _lastClickAt <= DoubleClickMilliseconds;
        _lastClickedId = player.Id;
        _lastClickAt = now;
        if (isDoubleClick)
        {
            e.Handled = true;
            Close();
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
