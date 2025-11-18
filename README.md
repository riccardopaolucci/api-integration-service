# Market Data Cache Microservice (.NET 8)

A small .NET 8 Web API project where I'm building a simple service that fetches market data by symbol and stores/caches it in a database. Still early stages - setting up the basic API structure and local environment.

## Tech Stack (initial plan)
- .NET 8 Web API  
- PostgreSQL  
- EF Core  
- Docker (later)  

## Getting Started

### Prerequisites
- .NET 8 SDK
- PostgreSQL (local)

### Run locally
```bash
dotnet build
dotnet run --project src/Api
