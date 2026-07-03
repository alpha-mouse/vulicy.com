# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Vulicy is a map of colonial street names with renaming proposals: an ASP.NET Core 10 minimal-API backend (`Vulicy.Web`) serving a React/Vite SPA (`Vulicy.UI`, has its own CLAUDE.md) backed by PostgreSQL + PostGIS.

## Commands

```bash
# Backend (solution file is Vulicy.slnx)
dotnet build                                          # build whole solution
dotnet test Vulicy.Tests                              # run unit tests (xunit)
dotnet test Vulicy.Tests --filter "FullyQualifiedName~SomeTestName"   # single test
cd Vulicy.Web && dotnet run                           # run API at http://localhost:5165

# Frontend
cd Vulicy.UI && npm run dev                           # dev server at http://localhost:5173, proxies /api → :5165
cd Vulicy.UI && npm run build                         # builds INTO Vulicy.Web/wwwroot (backend serves the SPA)
cd Vulicy.UI && npm run lint

# EF migrations (design-time factory needs no DB connection)
dotnet ef migrations add <Name> --project Vulicy.DB
```

## Running / testing the app locally

- Requires a local PostgreSQL 18 + PostGIS on `localhost:5432`; connection string in `Vulicy.Web/appsettings.Development.json`. **The app will not start without a reachable database** — migrations are applied automatically at startup (`InitializeDatabases` in [DatabaseExtensions.cs](Vulicy.Web/Infrastructure/DatabaseExtensions.cs)); never run `dotnet ef database update` manually.
- Unit tests (`Vulicy.Tests`) need no database — pure unit tests, fast.
- For full-stack manual testing use the Vite dev server (`:5173`), which proxies `/api` to the backend. The backend alone also serves the last-built SPA from `wwwroot` at `:5165`.
- Discourse and AWS secrets in dev config are optional: without Discourse keys forum integration just doesn't work; without AWS keys `AuditPersistenceHostedService` fails (README says drop it in that case).

## Architecture

Project dependency flow: `Vulicy.Web` → `Vulicy.Services` → `Vulicy.DB` → `Vulicy.Domain` (entities + repository interfaces; implementations live in `Vulicy.DB/Repositories`).

Conventions that will break the build/startup or silently fail if missed:

- **Convention-based DI** ([RegistrationExtensions.cs](Vulicy.Web/Infrastructure/RegistrationExtensions.cs)): any exported class `Foo` implementing an interface named `IFoo` is auto-registered as scoped — no manual registration. Classes named `*Config` are bound to the same-named config section and registered as singletons.
- **Source-generated JSON** (`CreateSlimBuilder`, reflection serialization off): every new request/response DTO must be added to a `JsonSerializerContext` ([VulicyWebSerializerContext.cs](Vulicy.Web/Infrastructure/VulicyWebSerializerContext.cs) or the Services one), otherwise serialization throws at runtime.
- **Startup guards** (app throws on start if violated): every non-GET endpoint must have `.RequireAuthorization(...)` or `.AllowAnonymous()` ([EndpointAuthorizationCheck.cs](Vulicy.Web/Infrastructure/EndpointAuthorizationCheck.cs)); every `.Validate<TModel>()` endpoint filter must have a matching handler parameter. FluentValidation validators are auto-registered from the `Vulicy.Services` assembly.
- **Endpoints** are minimal-API groups in `Vulicy.Web/Endpoints/*.cs`, each wired via a `MapXxx()` extension called from [Program.cs](Vulicy.Web/Program.cs).
- **EF Core**: `NoTracking` by default; 30s command timeout enforced by `CommandTimeoutInterceptor` — long-running import work opts out via `DbCommandTimeout.Unlimited()`. Geometry types are NetTopologySuite (PostGIS), serialized as GeoJSON.
- **Auth**: cookie auth (`Vulicy.Auth`) with Discourse SSO; admin-only endpoints use the `RequireAdminPolicy` policy. **Audit**: `AuditMiddleware` queues records that a hosted service persists to DynamoDB.
