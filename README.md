# GnosisPay Sync

Automatic synchronization of your Gnosis Pay card transactions to a PostgreSQL database. 
Self-hosted, privacy-first, built with .NET 10.

## Why this project?

Gnosis Pay is a great DeFi debit card, but there's no built-in way to export your 
transaction history or build your own analytics dashboard. This worker solves that 
by periodically fetching your transactions via the official Gnosis Pay API and 
storing them in your own PostgreSQL database.

## Features

- 🔐 SIWE (Sign-In with Ethereum) authentication, no API key required
- 🔄 Automatic backfill on first run
- ⏰ Hourly sync of new transactions via Quartz.NET scheduler
- 🔁 Automatic refresh of pending transactions as they settle
- 🛡️ Use a dedicated sign-in wallet — your main Safe remains untouched
- 📊 Full transaction data stored: merchant, MCC, amounts, currencies, onchain tx hashes

## Architecture

\`\`\`
┌─────────────────────────┐
│  GnosisPay Sync Worker  │
│                         │
│  - SIWE Authentication  │
│  - Transactions Fetcher │
│  - Quartz Jobs          │
└────────────┬────────────┘
             │
     ┌───────┴──────┐
     │              │
     ▼              ▼
 Gnosis Pay     PostgreSQL
    API         (your DB)
\`\`\`

## Tech Stack

- .NET 10 Worker Service
- PostgreSQL + Entity Framework Core (with snake_case naming)
- Quartz.NET for scheduled jobs
- Nethereum for SIWE message signing

## Prerequisites

- .NET 10 SDK
- PostgreSQL 14+ running locally or accessible
- A Gnosis Pay account with an activated card
- A dedicated EOA wallet for signing (recommended, see Security section)

## Quick Start

1. **Clone and enter the project**
   \`\`\`bash
   git clone https://github.com/YOUR_USERNAME/gnosispay-sync-template.git
   cd gnosispay-sync-template
   \`\`\`

2. **Set up configuration**
   \`\`\`bash
   cp appsettings.example.json appsettings.json
   \`\`\`
   
   Edit `appsettings.json`:
   - `ConnectionStrings:Postgres` — your PostgreSQL connection string
   - `GnosisPay:PrivateKey` — private key of your sign-in wallet (see Security)

3. **Create the database**
   \`\`\`bash
   dotnet ef database update
   \`\`\`

4. **Run the worker**
   \`\`\`bash
   dotnet run
   \`\`\`

On first run, the worker will:
- Generate a JWT using SIWE authentication
- Detect the empty database and backfill all your transactions
- Schedule hourly syncs for new transactions

## Security

### Use a dedicated Sign-in wallet

**Do not use your main Safe owner wallet** with this project. Instead:

1. Create a new EOA (Externally Owned Account) in MetaMask or Rabby
2. Add it as a Sign-in wallet in the Gnosis Pay app (Settings → Authorised wallets)
3. Use this new wallet's private key in `appsettings.json`

This way, even if the config file leaks, no funds can be moved. The sign-in wallet 
only authorizes API reads, it has no on-chain authority over your Safe.

### Never commit your secrets

The `.gitignore` already excludes `appsettings.json`. Only `appsettings.example.json` 
should be committed.

## Configuration options

| Key | Description | Default |
|-----|-------------|---------|
| `GnosisPay:ApiBaseUrl` | Gnosis Pay API URL | `https://api.gnosispay.com` |
| `GnosisPay:PrivateKey` | Sign-in wallet private key | *required* |

## License

MIT — see LICENSE file

## Contributing

PRs welcome! This project is intended as a starting point that you can fork and 
adapt to your needs.

## Disclaimer

This is an unofficial client for the Gnosis Pay API. Not affiliated with Gnosis 
or Gnosis Pay. Use at your own risk.