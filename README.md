
# Market Data Cache Microservice (.NET 8)

![Backend CI](https://github.com/riccardopaolucci/api-integration-service/actions/workflows/backend-ci.yml/badge.svg)
![Frontend CI](https://github.com/riccardopaolucci/api-integration-service/actions/workflows/frontend-ci.yml/badge.svg)

## Overview

A .NET 8 Web API that retrieves market data quotes by symbol, caches them in a PostgreSQL database, and exposes them via a REST API.
The project is designed to demonstrate production-style backend patterns including persistence, caching rules, integration testing, and containerisation.

---

## Tech Stack

- .NET 8 Web API
- Entity Framework Core
- PostgreSQL
- Docker & Docker Compose
- xUnit (unit + integration tests)

---

## ▶️ Run locally (no Docker)

### Prerequisites

- .NET 8 SDK
- PostgreSQL running locally

### Steps

```bash
# From repo root
dotnet ef database update \
  -p src/MarketData.Api/MarketData.Api.csproj \
  -s src/MarketData.Api/MarketData.Api.csproj

dotnet run --project src/MarketData.Api/MarketData.Api.csproj
```

### Endpoints

* Swagger UI: `https://localhost:5xxx/swagger`
* Health check: `/healthz`

> On startup, the API automatically applies EF Core migrations and seeds demo market data if the database is empty.

---



## ▶️ Run with Docker Compose

This runs the API and PostgreSQL together in a production-like environment.

<pre class="overflow-visible! px-0!" data-start="1294" data-end="1341"><div class="contain-inline-size rounded-2xl corner-superellipse/1.1 relative bg-token-sidebar-surface-primary"><div class="sticky top-[calc(--spacing(9)+var(--header-height))] @w-xl/main:top-9"><div class="absolute end-0 bottom-0 flex h-9 items-center pe-2"><div class="bg-token-bg-elevated-secondary text-token-text-secondary flex items-center gap-4 rounded-sm px-2 font-sans text-xs"></div></div></div><div class="overflow-y-auto p-4" dir="ltr"><code class="whitespace-pre! language-bash"><span><span>cd</span><span> docker
docker compose up --build
</span></span></code></div></div></pre>

### Services

* API: `http://localhost:8080/swagger`
* PostgreSQL:
  * Host: `localhost`
  * Port: `5432`
  * Database: `marketdata`
  * User: `marketuser`
  * Password: `marketpass`

> When running via Docker, the API container automatically:
>
> * Applies database migrations on startup
> * Seeds initial market data
> * Waits for the database to become healthy before starting
>

---



## Testing

The project includes both unit tests and integration tests.

* Unit tests validate domain logic in isolation.
* Integration tests spin up the API with:
  * In-memory EF Core database
  * Fake external market data client
  * Test authentication scheme (no JWT required)

<pre class="overflow-visible! px-0!" data-start="2008" data-end="2031"><div class="contain-inline-size rounded-2xl corner-superellipse/1.1 relative bg-token-sidebar-surface-primary"><div class="sticky top-[calc(--spacing(9)+var(--header-height))] @w-xl/main:top-9"><div class="absolute end-0 bottom-0 flex h-9 items-center pe-2"><div class="bg-token-bg-elevated-secondary text-token-text-secondary flex items-center gap-4 rounded-sm px-2 font-sans text-xs"></div></div></div><div class="overflow-y-auto p-4" dir="ltr"><code class="whitespace-pre! language-bash"><span><span>dotnet </span><span>test</span></span></code></div></div></pre>

---



## Notes

* Configuration is environment-based (`Development`, `Test`, Docker).
* Integration tests mirror production wiring while remaining deterministic.
* Docker setup reflects real-world deployment patterns (multi-stage build, health checks, auto-migrations).
