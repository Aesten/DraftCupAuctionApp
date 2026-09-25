# Draft Cup Auction

A Windows app for hosting draft cup auctions offline: captains bid on players with their own budget until every team is full. It's built to be shown on stream.

## Running it

Download `DraftCupAuction.exe` (from the latest run of the **Build** workflow on GitHub, or from a release) and run it. It's a single self-contained file for 64-bit Windows 10 or 11, with nothing else to install. It follows the Windows light/dark setting and accent color.

## How it's organized

The app works like a document editor: you open one **tournament** at a time. The start page lists the tournaments on this PC, and lets you create a new one or import a file. The ☰ menu (top left) floats over the page, and has the same list plus export, duplicate, close and delete.

A tournament holds:

- **Player pool**: shared by the whole tournament. Players have classes (INF / ARC / CAV).
  - Add players from the first row of the list: type the name, tick the classes (or type them: `Alice, inf cav`) and press Enter.
  - **Import…** adds the players of a player list (CSV, e.g. from Excel) or an auction plan (see [Player lists](#player-lists)). **Export…** saves the pool as a spreadsheet in the same format, so it can be imported again.
  - The pool is shown **A–Z** by default, so opening it during an auction doesn't reveal who comes next.
  - Filter it with the search box (it narrows the list as you type), the class buttons, and **All / Available / Unavailable** (not bought yet / already bought). Click a player's class icons to switch those classes on or off.
  - **Auction order** shows the order used by divisions that don't shuffle. Drag the handles (or press Alt+↑/↓) to rearrange it.
  - Each player shows where they stand: available, in a running auction, or bought (division, team and price).
- **Divisions**: one auction each, with its own:
  - captains, each with the class they signed up with and their own budget;
  - team size (5 to 10 players besides the captain);
  - player order (shuffled, or the pool's auction order);
  - number of upcoming players revealed on screen (3 by default);
  - half budget cap setting.

  Divisions can be auctioned in any order, on different days and different computers. **Whichever division starts first gets the whole pool; each later one gets the pool minus the players already bought.**

Each division has three pages, switched from the top bar: **Configure**, **Auction** and **Teams**.

### Running an auction

The auction page is meant to be screen-shared (F11 for full screen). It shows only what viewers need, all at once and without scrolling:

- at the top, in three parts: the auctioneer's controls (left), the player on the block with their classes (center), and the next few players (right; the rest of the queue stays hidden, but **Remaining players** under them lists everyone still in the queue in alphabetical order);
- below, every team: the captain and their class, the remaining budget, the most it can bid, the roster with prices, the empty spots, and how many players of each class it has (captain included).

The layout is made for 8 teams (4 × 2) and adapts to 4–10 teams and to rosters of 5–10 players, shrinking only if the window is too small.

- Click the winning team's card, type the price and press **Enter** (or click **Sold!**). The price box accepts `2.5` as well as `2,5`, and **−** / **+** change it by 0.1.
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
- Prices go in steps of 0.1 and can't exceed what the team has left.
- **Half budget cap**: while it's on, a team can only spend down to half of its starting budget (rounded up to 0.1). It can be switched on and off at any time, and the auction screen shows when it's on. Fixing a sale afterwards only checks that the team stays within its budget.

When a division is done, share its teams from **Teams**: **Copy as text** (formatted for Discord) or a spreadsheet (CSV).

### Keyboard

| Key | Action |
| --- | --- |
| Enter (in the price box) | Sell to the selected team |
| Up / Down (in the price box) | Price ±0.1 |
| Ctrl+Z | Undo the last auction action |
| Alt+Up / Alt+Down (pool, auction order) | Move the selected player |
| F11 | Full screen on / off |
| Escape | Close the menu, or leave full screen |

## Moving a tournament between computers

Tournaments are saved automatically after every change in `%LOCALAPPDATA%\DraftCupAuction` (**Open the data folder** in the menu). To hand one over:

1. **Export to a file…** from the menu. This gives you a `.draftcup.json` file (plain JSON).
2. On the other computer, **Import** it: use the start page or the menu, drop the file on the window, or open the file with the app.
3. Run a division there, export again, and import the file back on the first computer.

When you import a copy of a tournament you already have, the two are **merged**. For each division, and for the pool, whichever copy changed it last wins. The app lists what will change before applying it. If two copies auctioned at the same time bought the same player, you're warned. Players bought elsewhere are also taken out of any auction still running.

**Import** also accepts the `.json` files of the previous version of the app: an auction plan or an auction state (including an auction in progress) becomes a tournament with one division.

### Player lists

**Import** (start page or menu) turns a player list or an auction plan into a new tournament, and **Import…** in the player pool adds its players to the current pool. Both show the expected formats before asking for the file. They are the formats of the previous version of the app:

- **CSV** (e.g. from Excel: **File › Save As › CSV**): the player list the previous version exported, one column per class marked `x`. The pool's **Export…** file has the same columns (plus where each player ended up) and can be imported as it is.

  ```csv
  Player,INF,ARC,CAV
  Alice,x,,
  Bob,,x,x
  ```

- **JSON**: an auction plan of the previous version. Captains can also have a `"class"` (the previous version didn't have one).

  ```json
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
  ```

Each save keeps the previous version as a backup. Deleted tournaments, and tournaments before a reset or an import, are copied to the `Deleted` folder rather than erased.

## Building from source

Requirements: the [.NET 10 SDK](https://dotnet.microsoft.com/download). Any IDE with WPF support works (Rider, or Visual Studio 2026), and both include a XAML designer with a live preview.

```sh
dotnet build                     # everything
dotnet test                      # auction rules, pool sharing, storage, import and merge tests
dotnet run --project AuctionApp  # start the app (Windows only)

# Release: one self-contained DraftCupAuction.exe in ./publish
dotnet publish AuctionApp -c Release -o publish
```

The app only runs on Windows, but it also builds on Linux and macOS, so CI and cloud editors can compile it and run the tests.

### Project layout

| Project | Contents |
| --- | --- |
| `AuctionApp.Core` | Everything that isn't UI: the data model (`Tournament`, `Division`, `AuctionSession`), the auction rules (`AuctionEngine`), how the pool is shared (`TournamentRules`), validation, saving, importing, merging and exporting. Plain .NET, no Windows dependency. |
| `AuctionApp` | The WPF app (XAML views + view models, [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)). The look comes from WPF's built-in Fluent theme (`ThemeMode="System"` in `App.xaml`) plus shared styles in `Themes/Styles.xaml`. |
| `AuctionApp.Tests` | xUnit tests for `AuctionApp.Core`. |
