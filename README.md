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
- 🔁 Periodic refresh of pending transactions as they settle (every 6 hours)
- 🛡️ Use a dedicated sign-in wallet — your main Safe remains untouched
- 📊 Full transaction data stored: merchant, MCC, amounts, currencies, onchain tx hashes
- 🐳 Ready-to-deploy Docker image published on Docker Hub

## Architecture

```
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
```

## Tech Stack

- .NET 10 Worker Service
- PostgreSQL + Entity Framework Core (with snake_case naming)
- Quartz.NET for scheduled jobs
- Nethereum for SIWE message signing

## Quick Start — Docker (recommended)

Only requires Docker. No .NET SDK, no manual migrations.

1. **Clone and enter the project**
   ```bash
   git clone https://github.com/pierrick1213/gnosispay-sync.git
   cd gnosispay-sync
   ```

2. **Create your `.env`**
   ```bash
   cp .env.example .env
   ```

   Edit `.env` and set at minimum:
   - `GNOSISPAY_PRIVATE_KEY` — private key of your dedicated sign-in wallet (see [Security](#security))

3. **Start the stack**
   ```bash
   docker compose up -d
   ```

That's it. Compose will pull `pierrick1213/gnosispay-sync:latest` from Docker Hub,
start PostgreSQL alongside it, apply EF Core migrations automatically, run SIWE auth,
and backfill all your transactions on the first run.

**Logs:**
```bash
docker compose logs -f worker
```

**Pin a specific version** — set `GNOSISPAY_SYNC_IMAGE=pierrick1213/gnosispay-sync:1.0.0` in `.env`.

**Build locally instead of pulling** — useful when iterating on the code:
```bash
PULL_POLICY=never docker compose up -d --build
```

## Quick Start — Local (.NET)

For contributors or if you prefer running without Docker.

**Prerequisites:** .NET 10 SDK, PostgreSQL 14+ running locally.

1. **Clone and enter the project**
   ```bash
   git clone https://github.com/pierrick1213/gnosispay-sync.git
   cd gnosispay-sync
   ```

2. **Set up configuration**
   ```bash
   cp appsettings.example.json appsettings.json
   ```

   Edit `appsettings.json`:
   - `ConnectionStrings:Postgres` — your PostgreSQL connection string
   - `GnosisPay:PrivateKey` — private key of your sign-in wallet

3. **Run the worker**
   ```bash
   dotnet run
   ```

Migrations are applied automatically at startup — no `dotnet ef database update` needed.

## Security

### Use a dedicated sign-in wallet

**Do not use your main Safe owner wallet** with this project. Instead:

1. Create a new EOA (Externally Owned Account) in MetaMask or Rabby
2. Add it as a Sign-in wallet in the Gnosis Pay app (Settings → Authorised wallets)
3. Use this new wallet's private key in your `.env` or `appsettings.json`

This way, even if your config leaks, no funds can be moved. The sign-in wallet
only authorizes API reads — it has no on-chain authority over your Safe.

### Never commit your secrets

The `.gitignore` excludes `appsettings.json`, `appsettings.*.json` (except
`appsettings.example.json`) and `.env`. The `.dockerignore` also excludes them
from the Docker image, so pulling the published image from Docker Hub never
contains any secret.

## Configuration

### Environment variables (Docker)

| Variable | Description | Default |
|-----|-------------|---------|
| `POSTGRES_USER` | Postgres username | `postgres` |
| `POSTGRES_PASSWORD` | Postgres password | `postgres` |
| `POSTGRES_DB` | Postgres database name | `gnosispaysync` |
| `POSTGRES_PORT` | Host port exposed for Postgres | `5432` |
| `GNOSISPAY_API_BASE_URL` | Gnosis Pay API URL | `https://api.gnosispay.com` |
| `GNOSISPAY_PRIVATE_KEY` | Sign-in wallet private key | *required* |
| `GNOSISPAY_SYNC_IMAGE` | Worker image to pull | `pierrick1213/gnosispay-sync:latest` |
| `PULL_POLICY` | Compose pull policy (`always`/`missing`/`never`) | `always` |

### `appsettings.json` keys

| Key | Description | Default |
|-----|-------------|---------|
| `ConnectionStrings:Postgres` | PostgreSQL connection string | *required* |
| `GnosisPay:ApiBaseUrl` | Gnosis Pay API URL | `https://api.gnosispay.com` |
| `GnosisPay:PrivateKey` | Sign-in wallet private key | *required* |
| `GnosisPay:SiweDomain` | SIWE domain claim | `localhost` |
| `GnosisPay:SiweUri` | SIWE URI claim | `https://api.gnosispay.com/` |
| `GnosisPay:SiweStatement` | SIWE statement | `Sign in with Ethereum to Gnosis Pay` |
| `GnosisPay:ChainId` | EVM chain ID (Gnosis = 100) | `100` |
| `GnosisPay:JwtTtlInSeconds` | SIWE JWT lifetime | `86400` |

When running in Docker, any `appsettings.json` key can be overridden via env vars
using the `__` separator (e.g. `GnosisPay__ChainId=100`).

## License

MIT — see LICENSE file

## Contributing

PRs welcome! This project is intended as a starting point that you can fork and
adapt to your needs.

## Disclaimer

This is an unofficial client for the Gnosis Pay API. Not affiliated with Gnosis
or Gnosis Pay. Use at your own risk.
