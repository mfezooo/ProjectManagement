# Project Management API

Production-grade .NET 9 Web API following Clean Architecture, with JWT auth, CQRS via MediatR, FluentValidation, EF Core (SQL Server), Redis caching, and Serilog.

## Overview

A multi-user Project & Task management system. Each authenticated user owns their projects and tasks; `Admin` users have global access. All endpoints return a uniform `ApiResponse<T>` and are versioned under `/api/v1/...`.

## Architecture

```
+-------------------------------------------------------------+
|                    ProjectManagement.API                    |
|  Controllers, Program.cs, Middleware, CurrentUserService,   |
|  Swagger, Serilog, JWT, API versioning                      |
+----------------------------+--------------------------------+
                             |
              +--------------+----------------+
              |                               |
              v                               v
+--------------------------+    +-----------------------------+
|  ProjectManagement.      |    |  ProjectManagement.         |
|  Application             |    |  Infrastructure             |
|  - CQRS commands/queries |    |  - AppDbContext             |
|  - FluentValidation      |    |  - EF configurations        |
|  - Pipeline behaviors    |    |  - Migrations               |
|  - DTOs + mappings       |    |  - JwtService               |
|  - Interfaces            |    |  - BCryptPasswordHasher     |
|  - ApiResponse, errors   |    |  - RedisCacheService        |
+------------+-------------+    +--------------+--------------+
             |                                 |
             +---------------+-----------------+
                             |
                             v
                +---------------------------+
                | ProjectManagement.Domain  |
                | Entities + Enums          |
                | (zero dependencies)       |
                +---------------------------+

                +---------------------------+
                | ProjectManagement.Tests   |
                | xUnit + EF InMemory + Moq |
                +---------------------------+
```

**Dependency rule:** Domain knows nothing. Application depends on Domain only. Infrastructure depends on Application. API depends on Application + Infrastructure.

## Tech Stack

- .NET 9 / ASP.NET Core Web API
- EF Core 9 + SQL Server
- JWT Bearer (HMAC SHA256, manual hashing with BCrypt.Net-Next)
- MediatR for CQRS, FluentValidation pipeline behavior
- StackExchange.Redis distributed cache (via `Microsoft.Extensions.Caching.StackExchangeRedis`)
- Asp.Versioning.Mvc + ApiExplorer (URL-segment versioning)
- Swashbuckle.AspNetCore (Swagger UI w/ Authorize button)
- Serilog (Console + rolling file)
- xUnit / FluentAssertions / Moq / EF InMemory

## Prerequisites

- .NET 9 SDK
- SQL Server 2022 (LocalDB, Express, or full)
- Redis 7 (optional — falls back to in-memory cache if connection string omitted)
- `dotnet-ef` global tool: `dotnet tool install --global dotnet-ef`

## Setup (local, without Docker)

```bash
# 1. Restore packages
dotnet restore

# 2. Configure connection strings & JWT secret
#    Edit ProjectManagement.API/appsettings.Development.json
#    - ConnectionStrings:DefaultConnection -> your SQL Server
#    - ConnectionStrings:Redis             -> your Redis (or remove)
#    - JwtSettings:Secret                  -> at least 32 chars

# 3. Apply the EF migration
dotnet ef database update \
    --project ProjectManagement.Infrastructure \
    --startup-project ProjectManagement.API

# 4. Build
dotnet build

# 5. Run
dotnet run --project ProjectManagement.API
```

Swagger UI: **http://localhost:5050/swagger**

Click **Authorize** and paste `Bearer <jwt token>` after registering/logging in.

## Run with Docker Compose

```bash
docker compose up --build
```

This starts:
- `api` -> http://localhost:8080 (Swagger at `/swagger`)
- `db`  -> SQL Server 2022 on localhost:1433 (sa / `Your_strong_Password123!`)
- `redis` -> Redis 7 on localhost:6379

Apply the migration once the containers are up:

```bash
docker exec -it projectmanagement-api \
    dotnet ProjectManagement.API.dll --apply-migrations
```

(Or run `dotnet ef database update` from your host pointing the connection string at `localhost,1433`.)

## Tests

```bash
dotnet test
```

xUnit + FluentAssertions + Moq + EF InMemory. Covers handlers and validators for Auth, Projects, and Tasks.

## Endpoints (all under `/api/v1`)

| Method | Route                                   | Auth          | Description                |
|--------|-----------------------------------------|---------------|----------------------------|
| POST   | `/api/v1/auth/register`                 | Anonymous     | Register a new user        |
| POST   | `/api/v1/auth/login`                    | Anonymous     | Log in, returns JWT        |
| GET    | `/api/v1/projects`                      | Authenticated | List visible projects      |
| GET    | `/api/v1/projects/{id}`                 | Authenticated | Get a project              |
| POST   | `/api/v1/projects`                      | Authenticated | Create a project           |
| PUT    | `/api/v1/projects/{id}`                 | Authenticated | Update a project           |
| DELETE | `/api/v1/projects/{id}`                 | Authenticated | Delete a project           |
| GET    | `/api/v1/projects/{projectId}/tasks`    | Authenticated | List tasks in a project    |
| POST   | `/api/v1/projects/{projectId}/tasks`    | Authenticated | Create a task              |
| PATCH  | `/api/v1/tasks/{id}/status`             | Authenticated | Update task status         |
| DELETE | `/api/v1/tasks/{id}`                    | Authenticated | Delete a task              |

Non-Admin users can only access their own projects/tasks; Admins can access any.

## Conventions

- Every endpoint returns `ApiResponse<T>`:
  ```json
  { "success": true, "message": "...", "data": { ... }, "errors": null }
  ```
- Errors are mapped by the global exception middleware:
  - `ValidationException` -> 400 (with `errors` list)
  - `NotFoundException` -> 404
  - `UnauthorizedException` -> 401
  - `ForbiddenException` -> 403
  - `ConflictException` -> 409
  - anything else -> 500 (generic message; full stack logged)
- `CancellationToken` plumbed end-to-end.
- DTOs always returned to clients; entities never leak past the Application layer.
- No repository pattern — handlers use `IApplicationDbContext` + LINQ.
- Manual mapping (`project.ToDto()`); no AutoMapper.

## Caching

- `GET /api/v1/projects` is cached per user as `projects:user:{userId}` (TTL 5 minutes). Invalidated on create/update/delete project.
- `GET /api/v1/projects/{id}/tasks` is cached as `tasks:project:{projectId}` (TTL 5 minutes). Invalidated on any task mutation in that project.
- All cache calls are wrapped in `try/catch` — Redis being down logs a warning and falls through to the database.

## Bonus Features

- **Serilog** wired in `Program.cs` via `UseSerilog`, with Console + rolling daily file sink at `logs/log-<date>.txt`. Config picked up from `Serilog` section in `appsettings*.json`.
- **Dockerfile** (multi-stage SDK -> ASP.NET runtime, exposes 8080) and **docker-compose.yml** spinning up api + SQL Server 2022 + Redis 7.

## Notes

- The JWT secret in `appsettings.json` is a placeholder — replace it for any non-dev usage.
- The seeded admin account is not created automatically. Promote a user to Admin by updating the `Role` column to `1` directly in SQL until an admin bootstrap endpoint is added.
