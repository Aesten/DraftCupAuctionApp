using System.IO;
using System.Windows;
using System.Windows.Threading;
using AuctionApp.Core.Storage;
using AuctionApp.Services;
using AuctionApp.ViewModels;
using AuctionApp.Views;

namespace AuctionApp;

public partial class App : Application
{
    private Mutex? _singleInstance;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Two copies of the app editing the same draft would overwrite each other's saves.
        _singleInstance = new Mutex(true, @"Local\DraftCupAuction.SingleInstance", out var isFirstInstance);
        if (!isFirstInstance)
        {
            MessageBox.Show("Draft Cup Auction is already open.", "Draft Cup Auction", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        DraftStore store;
        try
        {
            store = new DraftStore(DraftStore.DefaultRootDirectory);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"The app can't create its data folder:\n{ex.Message}", "Draft Cup Auction", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        DispatcherUnhandledException += (_, args) =>
        {
            LogError(store, args.Exception);
            new DialogService().ShowError("Something went wrong", args.Exception.Message + "\n\nYour drafts are saved after every change, so nothing should be lost.");
            args.Handled = true;
        };

        var window = new MainWindow { DataContext = new MainViewModel(store, new DialogService()) };
        MainWindow = window;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _singleInstance?.Dispose();
        base.OnExit(e);
    }

    private static void LogError(DraftStore store, Exception exception)
    {
        try
        {
            File.AppendAllText(Path.Combine(store.RootDirectory, "errors.log"), $"[{DateTime.Now:u}] {exception}\n\n");
        }
        catch (IOException)
        {
            // Logging is best effort.
        }
    }
}
