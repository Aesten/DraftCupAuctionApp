using System.Windows;

namespace AuctionApp.Services;

/// <summary>Where a window was and how it was shown, remembered on this PC (in settings.json) for the next time it opens.</summary>
public sealed class WindowPlacement
{
    public double Left { get; set; }

    public double Top { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public bool Maximized { get; set; }

    public bool FullScreen { get; set; }

    /// <summary>
    /// The window's placement as it closes. When it's maximized or full screen, its normal size and position (where it
    /// goes back to) are kept, and it opens maximized again on the same screen.
    /// </summary>
    public static WindowPlacement Capture(Window window)
    {
        var bounds = window.WindowState == WindowState.Normal
            ? new Rect(window.Left, window.Top, window.ActualWidth, window.ActualHeight)
            : window.RestoreBounds;
        return new WindowPlacement
        {
            Left = bounds.Left,
            Top = bounds.Top,
            Width = bounds.Width,
            Height = bounds.Height,
            Maximized = window.WindowState == WindowState.Maximized,
            FullScreen = window.WindowStyle == WindowStyle.None,
        };
    }

    /// <summary>
    /// Puts a window back where it was, before it's shown. Nothing happens if the saved place isn't on a screen any
    /// more (a monitor was unplugged): the window then opens at its default place.
    /// </summary>
    public void ApplyTo(Window window)
    {
        if (!IsOnScreen())
        {
            return;
        }

        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Left = Left;
        window.Top = Top;
        window.Width = Math.Max(Width, window.MinWidth);
        window.Height = Math.Max(Height, window.MinHeight);
    }

    /// <summary>At least a good part of the window (its title bar area) must be on the screens as they are now.</summary>
    private bool IsOnScreen()
    {
        if (double.IsNaN(Left) || double.IsNaN(Top) || Width < 100 || Height < 100)
        {
            return false;
        }

        var screens = new Rect(
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth,
            SystemParameters.VirtualScreenHeight);
        var titleBar = new Rect(Left + 40, Top, Math.Max(0, Width - 80), 40);
        var visible = Rect.Intersect(screens, titleBar);
        return !visible.IsEmpty && visible.Width >= 100 && visible.Height >= 20;
    }
}
