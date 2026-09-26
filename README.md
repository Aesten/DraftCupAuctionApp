# Draft Cup Auction

A Windows app for hosting draft cup auctions offline: captains bid on players with their own budget until every team is full. It's built to be shown on stream.

- **Made for streaming**: the auction screen shows the player on the block, the next players and every team's roster and budget at once, without scrolling.
- **Two formats**: **Random Pick** (players come up in a random order) or **Captain Pick** (captains name the player they want from a board sorted by tier and class).
- **Tournaments with several divisions** sharing one player pool: players bought in one division are out of the next.
- **Nothing to save**: every change is saved automatically. A tournament can be exported to a file, run on another PC, and merged back.
- **Player lists** in CSV or JSON, to bring in a sign-up sheet.
- **Mistakes are easy to fix**: undo, or refund, reprice, move or swap any sold player.
- A native Windows 11 look, in light or dark.

## Download

1. Get `DraftCupAuction.exe` from the latest successful run of the [**Build** workflow](https://github.com/Aesten/DraftCupAuctionApp/actions/workflows/build.yml?query=branch%3Amaster+is%3Asuccess) (open the run, then **Artifacts › DraftCupAuction**; this needs a GitHub account and comes as a zip), or from a release when there is one.
2. Run it. It's a single file of about 1 MB, for 64-bit Windows 10 or 11, and needs no setup of its own.

The app needs the **.NET 10 Desktop Runtime**, a one-time install per PC (a newer version works too). If it's missing, the app says so when started and offers to open the download page: pick the **Desktop Runtime** for **x64**, install it, and start the app again. It can also be installed ahead of time from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0), or from a terminal:

```sh
winget install Microsoft.DotNet.DesktopRuntime.10
```

## How it's organized

The app works like a document editor: you open one **tournament** at a time. The start page lists the tournaments on this PC, and lets you create a new one or import one. A new tournament asks for its format, **Random Pick** or **Captain Pick** (see [Captain Pick](#captain-pick)); it can be switched from the menu until an auction starts.

Once a tournament is open, the ☰ menu (top left) has:

- the open tournament: export, duplicate for a new event, close, delete;
- the recent tournaments, to switch between them;
- new tournament, import, the data folder, and the **Light / Dark** theme.

Click ☰ again (or press Esc) to close it. The theme starts like Windows' own setting and follows it until you pick one; the choice is remembered on this PC. The app also uses the Windows accent color.

A tournament holds:

- **Player pool**: shared by the whole tournament. Players have classes (INF / ARC / CAV).
  - Add players from the first row of the list: type the name, tick the classes (or type them: `Alice, inf cav`) and press Enter.
  - **Import…** adds the players of a player list, for example from a sign-up sheet, and **Export…** saves the pool as one (see [Player lists](#player-lists)).
  - The pool is sorted **A–Z**, or by **Date added**. Auctions always shuffle the players, so neither reveals who comes next.
  - Filter it with the search box (it narrows the list as you type), the class buttons, and **All / Available / Unavailable** (not bought yet / already bought). Click a player's class icons to switch those classes on or off.
  - Each player shows where they stand: available, in a running auction, or bought (division, team and price).
- **Divisions**: one auction each, with its own:
  - captains, each with the class they signed up with and their own budget;
  - team size (5 to 10 players besides the captain);
  - number of upcoming players revealed on screen (3 by default);
  - half budget cap setting.

  Players come up in a random order, shuffled when the auction starts. Divisions can be auctioned in any order, on different days and different computers. **Whichever division starts first gets the whole pool; each later one gets the pool minus the players already bought.**

Each division has three pages, switched from the top bar: **Configure**, **Auction** and **Teams**.

### Running an auction

The auction page is meant to be screen-shared (F11 for full screen). It shows only what viewers need, all at once and without scrolling:

- at the top, in three parts: the auctioneer's controls (left), the player on the block with their classes (center), and the next few players (right; the rest of the queue stays hidden, but **Remaining players** under them lists everyone still in the queue in alphabetical order);
- below, every team: the captain and their class, what the team can still spend (in large), a budget bar, the roster with prices and empty spots, how many players of each class it has (captain included) and how full it is (e.g. 2/6).
  - The large number is the budget left or, while the half budget cap is on, what's left above the half (the most the team can bid).
  - The bar goes from 0 (left) to the team's starting budget (right) and is filled up to what's left. While the cap is on, the locked half is striped.

The layout is made for 8 teams (4 × 2) and adapts to 4–10 teams and to rosters of 5–10 players, shrinking only if the window is too small.

- Click the winning team's card, type the price (Enter confirms it) and click **Sold!**. The price box accepts `2.5` as well as `2,5`, and **−** / **+** change it by 0.1.
- If the price is more than the team may spend, **Sold!** says why in a small popup, and offers to **sell anyway**.
- **Skip** a player nobody wants. **Skipped players** opens the full list: put one back on the block, or send them all back to the queue.
- Click a bought player to fix the sale:
  - refund and put them back on the block, or send them to the skipped list;
  - change the price;
  - move them to another team (the first team is refunded);
  - swap them with a player not bought yet, at the same price.
- **More** has the half budget cap switch, **Undo** (also Ctrl+Z) and **Finish the auction**.
- The pool can be edited mid-auction, and the auction follows along:
  - new players go to the skipped list, ready whenever you want them;
  - name and class changes show up immediately;
  - removing a player who was already sold takes them off the team and refunds the price.
- A team is a slot led by its captain: captains' names and classes can be changed during the auction, on the **Configure** page. Budgets, the team size and the list of captains are locked until the auction is reset.

Rules the app enforces:

- A team can't buy more players than the division's team size.
- Prices go in steps of 0.1. Going over what the team has left needs the auctioneer's confirmation (**Sell anyway**).
- **Half budget cap**: while it's on, a team can only spend down to half of its starting budget (rounded up to 0.1), unless the auctioneer confirms a sale anyway. It can be switched on and off at any time, and the auction screen shows when it's on. Fixing a sale afterwards only checks that the team stays within its budget.

When a division is done, share its teams from **Teams**: **Copy as text** (formatted for Discord) or a spreadsheet (CSV).

### Captain Pick

In a Captain Pick tournament, players don't come up in a random order: captains name the player they want.

- In the player pool, each player has **one class** and a **tier**, from 1 (best) to 5. Click the tier's number, or select players and press **1** to **5**. The pool can also be sorted by tier.
- Each tier has a **minimum bid**: 2.0, 1.5, 1.0, 0.5 and 0.1 by default, for the whole tournament (☰ menu › **Minimum bids…** to change them). The captain who picks a player bids that amount; the others can bid higher.
- During the auction, **Pick board** opens a separate window with every player still available, as one grid: a row per tier, a column per class, names in alphabetical order. It can go on another screen or be shown on stream (F11 for full screen), and it scales so nothing is ever cut or scrolled.
- When a captain names a player, click them on the board: they go on the block, highlighted on the board, with the price set to their tier's minimum. Sell as usual. **Put back** returns them to the board if they were picked by mistake.
- Selling under the minimum asks for confirmation (**Sell anyway**), like going over a budget. There's no queue or skipped list: players nobody buys stay on the board, and a sold player taken back returns to the block or to the board.
- The auction screen shows how many players are left in each tier and class, where Random Pick shows the next players.

### Keyboard

| Key | Action |
| --- | --- |
| Enter (in a text box) | Confirm and leave the box (in the price box: confirm the price, it doesn't sell) |
| Up / Down (in the price box) | Price ±0.1 |
| Ctrl+Enter (auction page) | Sold! to the selected team |
| Enter / Escape (pick board) | Close the board and go back to the auction (Enter once a player is picked) |
| Delete (in the player pool) | Remove the selected players |
| 1 to 5 (in the player pool, Captain Pick) | Set the tier of the selected players |
| Ctrl+Z | Undo the last auction action |
| F11 | Full screen on / off |
| Escape | Close the menu, or leave full screen |

## Moving a tournament between computers

Tournaments are saved automatically after every change in `%LOCALAPPDATA%\DraftCupAuction` (**Open the data folder** in the menu). To hand one over:

1. **Export the tournament…** from the menu. This gives you a `.draftcup.json` file holding everything: the pool, the divisions, and their auctions as they stand (sales, queue order, skipped players, undo aside).
2. On the other computer, **Import a tournament…**: use the start page or the menu, drop the file on the window, or open the file with the app.
3. Run a division there, export again, and import the file back on the first computer.

When you import a copy of a tournament you already have, the two are **merged**. For each division, and for the pool, whichever copy changed it last wins. The app lists what will change before applying it. If two copies auctioned at the same time bought the same player, you're warned. Players bought elsewhere are also taken out of any auction still running.

Each save keeps the previous version as a backup. Deleted tournaments, and tournaments before a reset or an import, are copied to the `Deleted` folder rather than erased.

## Player lists

A player list is only players, names and classes: the link between a sign-up sheet and the app. In the player pool, **Import…** adds the players of a list (names already in the pool are skipped) and shows the format first. **Export…** saves the pool as a list. There are two layouts, both from the previous version of the app:

- **CSV**, the layout it exported: one column per class, marked `x`. From Excel: **File › Save As › CSV**. A column of classes written out (`inf cav`) is read too.

  ```csv
  Player,INF,ARC,CAV
  Alice,x,,
  Bob,,x,x
  ```

- **JSON**, players the way its files stored them. Classes are `inf`, `arc` and `cav`.

  ```json
  {
    "players": [
      { "name": "Alice", "classes": ["inf"] },
      { "name": "Bob", "classes": ["arc", "cav"] }
    ]
  }
  ```

In a Captain Pick tournament, lists also carry each player's tier: a `Tier` column after the classes (`Alice,x,,,1`, or a line like `Alice, inf, 1`), or `"tier": 1` in JSON. A player listed with several classes keeps only the first one.

## Building from source

Requirements: the [.NET 10 SDK](https://dotnet.microsoft.com/download). Any IDE with WPF support works (Rider, or Visual Studio 2026), and both include a XAML designer with a live preview.

```sh
dotnet build                     # everything
dotnet test                      # auction rules, pool sharing, storage, import and merge tests
dotnet run --project AuctionApp  # start the app (Windows only)

# Release: one small DraftCupAuction.exe in ./publish (needs the .NET 10 Desktop Runtime to run)
dotnet publish AuctionApp -c Release -o publish
```

The app only runs on Windows, but it also builds on Linux and macOS, so CI and cloud editors can compile it and run the tests.

### Project layout

| Project | Contents |
| --- | --- |
| `AuctionApp.Core` | Everything that isn't UI: the data model (`Tournament`, `Division`, `AuctionSession`), the auction rules (`AuctionEngine`), how the pool is shared (`TournamentRules`), validation, saving, importing, merging and exporting. Plain .NET, no Windows dependency. |
| `AuctionApp` | The WPF app (XAML views + view models, [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)). The look comes from WPF's built-in Fluent theme (`ThemeMode` in `App.xaml`, switched to light or dark by `Services/AppSettings.cs`) plus shared styles in `Themes/Styles.xaml`. |
| `AuctionApp.Tests` | xUnit tests for `AuctionApp.Core`. |
