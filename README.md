# BasketballSim

A work-in-progress basketball franchise simulation game built with C# and WPF. Run a draft, simulate a full 82-game season, and see your team through the playoffs.

> **Note:** This project is unfinished. Expect rough edges, missing features, and at least one broken thing in the playoffs.

## What It Does

1. **Draft** — 30 teams, 450 players, serpentine order. You pick for your team; CPU auto-drafts the rest with basic position-filling logic.
2. **Season** — Simulates an 82-game season day by day. View your schedule, results, and final standings split by conference.
3. **Playoffs** — Top 8 teams from each conference bracket off in a best-of-7 format through 4 rounds.

## Requirements

- Windows (WPF only)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (optional, for IDE support)

## Running It

```bash
dotnet run --project BasketballSim/BasketballSim.csproj
```

Or open `BasketballSim.sln` in Visual Studio and hit F5.

## How to Play

1. Launch the app and click **Start Franchise**
2. Select a player and click **Draft Player** each time it's your pick
3. After the draft, review your roster in **View Current Team**
4. Click **Advance To Season** to start the regular season
5. Press `Enter` to simulate each day (82 total)
6. View standings, then continue to the playoffs
7. Press `Enter` to simulate each playoff game

`Esc` exits at any point.

## Project Structure

```
BasketballSim/
├── Models/         # Player, Team, Game, DraftPick
├── Logic/          # Draft, season, and playoff simulation logic
├── Views/          # WPF windows (XAML + code-behind)
└── Resource/       # namepool.json — player name generation by nationality
```

## Current State

| Feature | Status |
|---|---|
| League & player generation | Done |
| Serpentine draft | Done |
| 82-game season simulation | Done |
| Standings (East/West) | Done |
| Playoff bracket | Broken / WIP |
| Save & load | Not started |
| Player stats affecting outcomes | Not started |
| Real team names | Not started |

## Known Issues

- Playoff bracket progression has bugs (last commit message: "broken playoffs")
- Game simulation is purely random — player ratings don't influence game scores yet
- No save/load; everything resets when you close the app
- Teams are named "Team 1" through "Team 30"
