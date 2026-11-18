# Property Management System: Development Plan

**Companion to:** [Design.md](Design.md)
**Date:** 2026-11-05
**Duration:** 18 weeks, assuming about 10 to 12 hours per week

## Contents

1. [Approach](#1-approach)
2. [Stack](#2-stack)
3. [Testing strategy](#3-testing-strategy)
4. [Docker](#4-docker)
5. [Frontend](#5-frontend)
6. [Milestones and weekly plan](#6-milestones-and-weekly-plan)
7. [If time runs short](#7-if-time-runs-short)

---

## 1. Approach

- **Layered clean architecture.** Dependencies point inward: the Domain depends on nothing, Application depends on Domain, Infrastructure (persistence, identity, email) depends on Application and Domain, and the Api project composes everything. EF Core mappings live in Infrastructure and use fluent configuration, never attributes on domain classes.
- **DDD and TDD together.** They are not alternatives.
  - **DDD** shapes the domain layer. The strategic work is done in `Design.md`. The tactical work is entities with behavior, aggregate roots that reference each other by ID (as the ERD does), and domain exceptions for rule violations. Likely aggregate roots: User, Unit, Lease, MaintenanceTicket, Skill.
  - **TDD** drives the domain and application layers, where the rules are pure logic: ticket transitions, lease overlap, assignment eligibility. Controllers and EF mappings are covered by integration tests written afterwards, not test-first.
- **Vertical slices.** Each milestone delivers domain, persistence, application, API, tests, and a small piece of UI, so it ends with something that can be demonstrated.
- **Every week ends with** passing CI, an updated README, and an ADR for any significant decision.

Solution layout (already scaffolded):

```
PropertyManagement/
  backend/
    PropertyManagement.Domain/
    PropertyManagement.Application/
    PropertyManagement.Infrastructure/
    PropertyManagement.Api/
  tests/
    PropertyManagement.Domain.UnitTests/
    PropertyManagement.Application.UnitTests/
    PropertyManagement.Infrastructure.IntegrationTests/
    PropertyManagement.Api.IntegrationTests/
    PropertyManagement.Architecture.Tests/
  frontend/
  docs/            (Design.md, Plan.md, ADRs)
  docker-compose.yml
  global.json
  Directory.Packages.props
  Directory.Build.props
  .editorconfig
```

---

## 2. Stack

**Decided**

| Component | Notes |
|---|---|
| .NET 10, ASP.NET Core Web API | SDK pinned to `10.0.402` via `global.json`. |
| EF Core with Npgsql | Migrations, fluent configuration. |
| PostgreSQL | Needs the `citext` extension (case-insensitive email) and `btree_gist` (lease-overlap exclusion constraint). |
| JWT authentication | HttpOnly cookies. 30-minute access token, 7-day refresh token rotated on use. Password hashing via ASP.NET Core Identity's hasher only, without adopting the full Identity user model. |
| Serilog | Structured logging, console sink for local development. |
| OpenAPI and a UI | Built-in `Microsoft.AspNetCore.OpenApi` generates the document. A UI (Scalar or Swagger UI) is added separately. |
| FluentValidation | Input shape in the Application layer. Business invariants stay in the domain. |
| Problem Details (RFC 7807) | Built-in `ProblemDetails`. One global exception handler maps domain exceptions to HTTP errors. |
| Health checks and rate limiting | Built-in. Production-minded basics at low cost. |
| `BackgroundService` | Monthly charge generation in the financial milestone. |
| NetArchTest | Architecture tests that enforce the layer rules. |
| Central package management | `Directory.Packages.props` and `Directory.Build.props` at solution root. |
| GitHub Actions | Build, test, format check. |
| Hand-written mapping or Mapperly | AutoMapper and MediatR now have commercial licensing for current versions, and MediatR is not needed here. |

**Skip for now:** CQRS, event buses, caching, API versioning, anything microservice-shaped.

---

## 3. Testing strategy

| Level | Project | Tools | Covers |
|---|---|---|---|
| Domain unit | `Domain.UnitTests` | xUnit | Ticket state machine, lease rules, invariants. No mocks, no database. |
| Application unit | `Application.UnitTests` | xUnit, NSubstitute | Use-case orchestration, assignment eligibility, authorization handlers. |
| Persistence integration | `Infrastructure.IntegrationTests` | Testcontainers (PostgreSQL), Respawn | Mappings, migrations, constraints, queries. |
| API integration | `Api.IntegrationTests` | WebApplicationFactory, Testcontainers | End-to-end per role, including forbidden paths and error responses. |
| Architecture | `Architecture.Tests` | NetArchTest | Domain depends on nothing. Application never references Infrastructure. |

Placement notes:

- **Set up the test database at the first EF mapping**, not after the persistence layer is finished. Exclusion constraints, partial unique indexes, check constraints, and `citext` exist only in real PostgreSQL, so the in-memory provider and SQLite cannot verify them.
- **Turn the permission matrix into a data-driven test.** Each row of the matrix combined with each role gives an expected allow or deny. This keeps the matrix in `Design.md` and the implementation in agreement.
- Use a fake `TimeProvider` for anything time-dependent, such as charge generation.

---

## 4. Docker

Docker sits outside the architectural layers. It is tooling, used in three ways:

1. **Local development:** `docker-compose.yml` runs PostgreSQL only. API and frontend are started with `dotnet run` or `dotnet watch`. Already in place.
2. **Tests:** Testcontainers starts a throwaway PostgreSQL through Docker, locally and in CI.
3. **Packaging:** a multi-stage Dockerfile for the API and a Compose file that starts the API, database, and frontend with one command. This comes at the end (M6).

A `.env.example` file at the repo root will be created alongside the admin seeder (M1, Week 3), listing all required environment variables with placeholder values.

---

## 5. Frontend

A minimal Blazor WebAssembly frontend. Scope: login, then a screen per role.

- **Tenant:** my lease and occupants, open a ticket, comment, confirm work, payment history.
- **Technician:** assigned tickets, start work, work done, request category review, comment.
- **Manager:** ticket list and review queue, assign from the eligible list, close or cancel, accounts, units, finances.
- **Admin:** units and manager accounts.

Built in thin slices after each backend milestone. The API client is generated from the OpenAPI document so the frontend cannot drift from the API. CORS is configured on the API.

---

## 6. Milestones and weekly plan

### M0: Foundations (Week 1) — ✅ Complete

- Solution with four source projects and five test projects.
- `.editorconfig`, central package management (`Directory.Packages.props`, `Directory.Build.props`).
- `global.json` pinning SDK to `10.0.402`.
- Docker Compose with PostgreSQL.
- GitHub Actions CI: restore, format check, build, test.
- Architecture test project scaffolded.
- `Design.md` and `Plan.md` in `/docs`.
- All open decisions from `Design.md` section 9 resolved and recorded.

**Done when:** CI is green and `docker compose up` provides a database. ✅

### M1: Identity and accounts (Weeks 2 to 4)

- **Week 2:** User hierarchy in the domain (Tenant, Employee, Manager, Technician, Admin). EF mappings with table-per-type, the credentials table, and case-insensitive unique email. First migration. Testcontainers setup and persistence tests.
- **Week 3:** Login use case, password hashing, JWT issuance (HttpOnly cookies, 30-minute access token, 7-day refresh token rotated on use), role claims, authorization skeleton. FluentValidation, Problem Details, global exception handling, Serilog, OpenAPI with a UI. EF Core data seeder for the first admin account (credentials from environment variables). Create `.env.example`.
- **Week 4:** Account endpoints: create, view, edit (name, phone, date of birth), deactivate, reactivate, and change email (with an email sender abstraction that logs in development). Audit fields through a `SaveChanges` interceptor. API tests for the account rows of the permission matrix. Login and accounts screens (Blazor).

**Done when:** the admin creates a manager, who creates a tenant and a technician, and you can log in as each.

### M2: Building structure (Weeks 5 and 6)

- **Week 5:** Unit aggregate (types, nullable-field rules, status), rate history, check constraints, unit endpoints, units screen (Blazor). Decommissioning rules that depend on leases and tickets are completed in later milestones.
- **Week 6:** Appliances, skills and technician skills, wage history and its permissions (technicians see their own, managers see all).

**Done when:** the right roles can manage units, appliances, skills, and wages.

### M3: Leasing (Weeks 7 and 8)

- **Week 7:** Lease and occupants. Overlap exclusion constraint. Partial unique index on `lease(tenant_id) WHERE status = 'Active'`. Rules: private units only, age check, no new lease while one is active. Lease renewal by extending end date. `can_request_move_out` flag on Lease. "Own lease" authorization handler. Read-only access for former tenants.
- **Week 8:** Move-out request workflow: Pending → Approved/Rejected/Cancelled, `requested_by`, pre-conditions (open tickets reassigned, charges settled), occupant endpoints, unit status derived from leases, tenant screens (Blazor).

**Done when:** the whole tenancy lifecycle works.

### M4: Maintenance tickets (Weeks 9 to 13)

- **Week 9:** Ticket aggregate and state machine, written test-first: the transitions table, a comment on every status change, Closed is final, category and assignment rules, `closed_reason`. Persistence for ticket, comments, and events, plus the `xmin` concurrency token.
- **Week 10:** Use cases for open, assign (eligible-technician list plus enforcement in the endpoint), start work, work done, resolve, rework, close, cancel, and comments. Every action writes its event. Invalid transitions return 409.
- **Week 11:** Category review request, category review dismissal (`CategoryReviewDismissed` event, manager with required comment), and the manager's review queue. Role-specific response models. Tenant visibility during the lease. Filtering and pagination. Unit maintenance history. Wire the unit decommission guard (no unfinished tickets). Wire the technician termination guard (no open assigned tickets, reassign to manager before terminating).
- **Week 12:** Matrix-driven API tests for every ticket row and role. A concurrency conflict test. Blazor flows for all roles.
- **Week 13:** Cleanup buffer.

**Done when:** the full ticket lifecycle is demonstrable in the UI for every role.

### M5: Financial milestone (Weeks 14 to 16)

- **Week 14:** Charge and Payment domain and persistence, uniqueness per `(lease, period, type)`. Manual payment recording. Derived status (paid, partial, overdue). Manager-only charge voiding with required reason.
- **Week 15:** Monthly charge generation as an idempotent background service, tested with a fake `TimeProvider`. Tenant payment history. Manager financial reports.
- **Week 16:** Time entries and labor cost (hours times the wage effective on that date), cost per ticket, unit, and category. Matching screens.

**Done when:** the reports reconcile in integration tests.

### M6: Hardening and release (Weeks 17 and 18)

- **Week 17:** API Dockerfile and full-stack Compose. Health checks and rate limiting. Serilog correlation IDs. Index review of the main queries. Demo seed data for every role.
- **Week 18:** README with an architecture diagram and one-command setup. Polished OpenAPI documentation. Final CI (build, test, format, Docker build). Demo screenshots. Tagged release.

**Done when:** a fresh clone plus `docker compose up` lets you log in as each role.

---

## 7. If time runs short

M1 to M4 already deliver the core system, so M5 (financial) can shrink or slip without breaking the project's story. Within M6, keep the Docker packaging and README, and trim the rest first.
