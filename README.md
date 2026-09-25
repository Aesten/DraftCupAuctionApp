# Draft Cup Auction

A Windows app for running draft cup auctions: captains bid on players with a fixed budget until every team is full.

## Running it

Download `DraftCupAuction.exe` (from the latest run of the **Build** workflow on GitHub, or from a release) and run it. It's a single self-contained file for 64-bit Windows 10 or 11, with nothing else to install. It follows the Windows light/dark setting and accent color.

## How it works

Everything happens inside the app and is saved automatically after every change. You never need to handle files.

1. **New draft**: give it a title, then fill in the **Setup** page:
   - **Captains** and their budgets (any number of teams).
   - **Players** and their classes (INF / ARC / CAV). Type in the empty last row of the list, or use **Paste a list…** to add many at once from a spreadsheet, a sign-up form or a Discord message (`Alice, inf cav`).
   - **Stages** (optional): several auctions run one after the other, e.g. a *High tier* followed by a *Low tier*. Assign each player to a stage. Each stage can limit how many players a team may buy during it. Teams and budgets carry over from one stage to the next.
2. **Start auction**. The setup is checked first (missing names, duplicates, not enough players…).
3. On the **Auction** page, for each player on the block:
   - click the winning team's card, type the price and press **Enter** (or click **Sold!**), or
   - **Skip** the player. Skipped players can be brought back one by one, or all sent back to the queue.
   - Every card shows the team's remaining budget, the most it can bid right now and its empty spots.
   - **Undo** (Ctrl+Z) reverts the last action. Hovering a bought player also lets you take them back.
   - When a stage is over, **Start *next stage*** asks whether the players nobody bought should be auctioned again in the next stage or set aside.
4. **Finish**, then share the teams from the **Results** page: **Copy as text** (formatted for Discord), a spreadsheet (CSV) or a backup file.

### Rules the app enforces

- A team can't buy more players than the team size (captain not included), nor more than a stage's limit.
- Prices go in steps of 0.1 and can't exceed what the team has left.
- **Half budget cap**: while it's on, a team can only spend down to half of its starting budget (rounded up to 0.1); the other half stays reserved. It can be switched on and off at any time during the auction.

### Keyboard

| Key | Action |
| --- | --- |
| Enter (in the price box) | Sell to the selected team |
| Up / Down (in the price box) | Price ±0.1 |
| Ctrl+Z | Undo the last auction action |
| F11 | Full screen (for streams and projectors) |

### Where the data lives

Drafts are stored in `%LOCALAPPDATA%\DraftCupAuction` (**Open the data folder** on the home page). Each save keeps the previous version as a backup, and deleted or reset drafts are moved to the `Deleted` folder rather than erased.

To move a draft to another PC, use **Export backup** and **Import** on the other side. **Import** also accepts the `.json` files of the previous version of the app (auction plans and auction states, including an auction in progress).

## Building from source

Requirements: the [.NET 10 SDK](https://dotnet.microsoft.com/download). Any IDE with WPF support works (Rider, or Visual Studio 2026), and both include a XAML designer with a live preview.

```sh
dotnet build                     # everything
dotnet test                      # rules, storage and import tests
dotnet run --project AuctionApp  # start the app (Windows only)

# Release: one self-contained DraftCupAuction.exe in ./publish
dotnet publish AuctionApp -c Release -o publish
```

The app only runs on Windows, but it also builds on Linux and macOS, so CI and cloud editors can compile it and run the tests.

### Project layout

| Project | Contents |
| --- | --- |
| `AuctionApp.Core` | Everything that isn't UI: the data model, the auction rules (`AuctionEngine`), setup validation, saving, importing and exporting. Plain .NET, no Windows dependency. |
| `AuctionApp` | The WPF app (XAML views + view models, [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)). The look comes from WPF's built-in Fluent theme (`ThemeMode="System"` in `App.xaml`) plus a few shared styles in `Themes/Styles.xaml`. |
| `AuctionApp.Tests` | xUnit tests for `AuctionApp.Core`. |
