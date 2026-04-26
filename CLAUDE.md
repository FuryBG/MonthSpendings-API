# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build MonthSpendings.sln

# Run (development)
dotnet run --project MonthSpendings/MonthSpendings.csproj

# Apply migrations (runs automatically on startup — no manual step needed)
dotnet ef database update --project Infrastructure --startup-project MonthSpendings

# Add a migration
dotnet ef migrations add <MigrationName> --project Infrastructure --startup-project MonthSpendings
```

There are no test projects currently.

## Architecture

Clean Architecture with 4 layers:

- **Domain** — entity models only, no dependencies
- **Application** — use cases, DTOs, interfaces, mappers; depends on Domain
- **Infrastructure** — EF Core `AppDbContext`, repositories, migrations, external service clients; depends on Application + Domain
- **MonthSpendings** — ASP.NET Core 10 Web API entry point; depends on Application + Infrastructure

A fifth project, **EnableBanking**, wraps the third-party open-banking SDK.

All DI registrations live in `MonthSpendings/Program.cs`.

## Key Patterns

### Use Case pattern
Every business operation is a dedicated class implementing an interface, e.g. `ICreateBudgetUseCase` → `CreateBudgetUseCase`. All return `CaseResult<T>` with `Successful`, `Data`, and `ErrorMessage`. Controllers call use cases directly.

### Unit of Work + Repository
`IUnitOfWork` in Application exposes all repositories and wraps DB transactions (`BeginTransaction` / `CommitTransaction` / `RollbackTransaction`). Repositories are transient and only accessed through the UoW.

### Mappers
Extension methods convert between entities and DTOs, e.g. `budget.ToDto()`, `budgetDto.ToEntity()`. Located in `Application/Mappers/`.

## Domains

| Domain | Key entities | Notes |
|---|---|---|
| Budget | Budget, BudgetPeriod, BudgetCategory | Multi-user via BudgetInvite |
| Spending | Spending | Linked to a BudgetCategory and BudgetPeriod |
| Banking | BankConsent, BankAccount, BankTransaction | Via EnableBanking OAuth consent flow |
| User | AppUser | Google OAuth; GoogleId stored |
| Currency | Currency | 44 currencies seeded on migration |

## Database

PostgreSQL via EF Core 10. Connection strings:
- **Development:** `localhost:5432` (see `appsettings.Development.json`)
- **Production:** Neon serverless PostgreSQL (see `appsettings.json`)

Migrations are applied automatically at startup (`Program.cs`). Migration files live in `Infrastructure/Migrations/`.

## Background Service

`TransactionSyncBackgroundService` (registered as a hosted service) calls `IBankSyncWorker` on a configurable interval (default 15 min, `EnableBanking:TransactionSyncIntervalInMinutes`). The worker fetches debit transactions from EnableBanking, deduplicates by a computed `TransactionId` (unique index in DB), and records a `LastSync` timestamp on the consent.

## Authentication

JWT Bearer tokens. `UserService` extracts `UserId` from JWT claims and is scoped. `RequireHttpsMetadata = false` in development. Token lifetime is 100 years (by design). Google OAuth stores `GoogleId` and `GooglePhotoAddress` on `AppUser`.

## External Integrations

- **EnableBanking API** — requires `EnableBanking:AppCertPath` and `EnableBanking:AppKeyId` in config; certificate is included for production
- **Expo push notifications** — `IPushNotificationService` via `Expo.Server.SDK`
- **Swagger** — enabled in development at `/swagger`
