# HasapNama — Shared CRM for several apps

A lightweight CRM for small businesses ("hasapnama" = "account book" in Turkmen).
One company, one server, one database, many apps.

## Layout

```
hasapnama/
└── src/
    ├── HasapNama.Shared/   net10.0   DTOs + Entity models shared by everything
    ├── HasapNama.Server/   net10.0   ASP.NET Core Web API (JWT, EF Core, Npgsql/PostgreSQL)
    └── HasapNama.App/      MAUI Blazor Hybrid (BlazorWebView)
                             net10.0-android  -> Android app
                             net10.0-windows  -> Windows desktop app      (build on Windows)
```

- **One server hosts all the data.** The server stores every record keyed by
  `OwnerId`, so several apps/users/screens connect to the same server and see
  only their own data.
- **The database is PostgreSQL** (via `Npgsql` + EF Core). No SQLite anywhere.
- **Desktop + mobile share one Blazor UI.** The same `Components/` Blazor pages
  render in the Android app and the Windows app.

## Quick start (server + database)

```bash
# 1. Start PostgreSQL (needs Docker; or point appsettings to any Postgres)
docker compose up -d

# 2. Point the API at your Postgres
cp src/HasapNama.Server/appsettings.json src/HasapNama.Server/appsettings.Development.json
#   edit ConnectionStrings:Default to match your Postgres (host, db, user, pass)

# 3. Run the API (creates the schema + an admin user on first start)
dotnet run --project src/HasapNama.Server

# 4. Open http://localhost:5080/swagger
#    Sign in with admin / admin123 (created automatically on first run)
```

## Building the clients

### Android
```bash
dotnet build src/HasapNama.App -f net10.0-android
# APK: src/HasapNama.App/bin/Debug/net10.0-android/com.enverhas.ipk-...
```

### Windows (must be run on a Windows machine or CI)
```powershell
dotnet build src/HasapNama.App -f net10.0-windows10.0.19041.0
```

## Notes for the team

- The Blazor app needs to reach the server. On the Android emulator the host is
  `10.0.2.2`; on a real device set `HasapNama:Api:BaseUrl` (or pass the LAN IP).
- The JWT signing key lives in `appsettings.json` (`Jwt:Key`) — override it in
  production (at least 32 chars).
- Add more entity lists (deals, tasks, ...) the same way: model in `Shared`,
  endpoints in `Server/Program.cs`, page in `App/Components/Pages`.
