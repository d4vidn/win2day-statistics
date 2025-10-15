# win2day-statistics

CLI tool to download and analyze your Win2Day game history.

## What it does

- Fetches your game history from Win2Day
- Generates Excel reports with statistics per game:
  - Total bets, wins, profit
  - RTP (Return to Player %)
  - Biggest bet, biggest win, biggest loss
  - Times played, first/last played dates
- Sortable/filterable Excel tables
- Category filtering (Casino, Poker, Sports, etc.)

## Installation

Run in PowerShell:
```powershell
irm https://raw.githubusercontent.com/d4vidn/win2day-statistics/master/install.ps1 | iex
```

Restart your terminal, then use `w2ds`.

## Usage
```bash
# Login (opens browser, you login manually)
w2ds login

# Get all game history → creates Excel file
w2ds history

# Get 500 most recent games
w2ds history --limit 500

# Filter by category
w2ds history --category casino
w2ds history -l 200 -c poker

# List available categories
w2ds categories

# Remove saved login
w2ds logout
```

## Requirements

- Windows 10/11
- Chrome browser (for login)

## Uninstall
```powershell
irm https://raw.githubusercontent.com/d4vidn/win2day-statistics/master/uninstall.ps1 | iex
```

## Disclaimer

This tool is for educational purposes only. Use at your own risk. The author is not responsible for any misuse or violations of Win2Day's terms of service.

## License

MIT
