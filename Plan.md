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
8. [Open decisions](#8-open-decisions)

---

## 1. Approach

- **Layered clean architecture.** Dependencies point inward: the Domain depends on nothing, Application depends on Domain, Infrastructure (persistence, identity, email) depends on Application and Domain, and the Api project composes everything. EF Core mappings live in Infrastructure and use fluent configuration, never attributes on domain classes.
- **DDD and TDD together.** They are not alternatives.
  - **DDD** shapes the domain layer. The strategic work is done in `Design.md`. The tactical work is entities with behavior, aggregate roots that reference each other by ID (as the ERD does), and domain exceptions for rule violations. Likely aggregate roots: User, Unit, Lease, MaintenanceTicket, Skill.
  - **TDD** drives the domain and application layers, where the rules are pure logic: ticket transitions, lease overlap, assignment eligibility. Controllers and EF mappings are covered by integration tests written afterwards, not test-first.
- **Vertical slices.** Each milestone delivers domain, persistence, application, API, tests, and a small piece of UI, so it ends with something that can be demonstrated.
- **Every week ends with** passing CI, an updated README, and an ADR for any significant decision.

Suggested solution layout:

```
src/
  PropertyManagement.Domain
  PropertyManagement.Application
  PropertyManagement.Infrastructure
  PropertyManagement.Api
tests/
  PropertyManagement.Domain.UnitTests
  PropertyManagement.Application.UnitTests
  PropertyManagement.Infrastructure.IntegrationTests
  PropertyManagement.Api.IntegrationTests
  PropertyManagement.Architecture.Tests
frontend/
docs/            (Design.md, Plan.md, ADRs)
docker-compose.yml
```

---

## 2. Stack

**Chosen**

| Component | Notes |
|---|---|
| .NET 10, ASP.NET Core Web API | |
| EF Core with Npgsql | Migrations, fluent configuration. |
| PostgreSQL | Needs the `citext` extension (case-insensitive email) and `btree_gist` (lease-overlap exclusion constraint). |
| JWT authentication | Use the password hasher from ASP.NET Core Identity on its own, without adopting the full Identity user model. This fits the separate credentials table and avoids custom cryptography. |
| OpenAPI and a UI | Since .NET 9 the template no longer includes Swashbuckle. The built-in `Microsoft.AspNetCore.OpenApi` generates the document, and a UI (Scalar or Swagger UI) is added separately. |
| FluentValidation | Input shape in the Application layer. Business invariants stay in the domain. |

**Suggested additions**

| Component | Why |
|---|---|
| Serilog | Structured logging with correlation IDs. |
| Global exception handler and Problem Details (built in) | One place that maps domain exceptions to HTTP errors. |
| Health checks and rate limiting (built in) | Production-minded basics at low cost. |
| `BackgroundService` | Monthly charge generation in the financial milestone. |
| NetArchTest | Architecture tests that enforce the layer rules. |
| `.editorconfig`, analyzers, central package management, nullable reference types | Consistency and early error detection. |
| GitHub Actions | Build, test, format check, Docker build. |
| Hand-written mapping or Mapperly | AutoMapper and MediatR now have commercial licensing for current versions, and MediatR is not needed here. |

**Skip for now:** CQRS, event buses, caching, anything microservice-shaped.

---

## 3. Testing strategy

**Decided:** the structure, tools, and placement below are accepted.

| Level | Project | Tools | Covers |
|---|---|---|---|
| Domain unit | `Domain.UnitTests` | xUnit, Shouldly | Ticket state machine, lease rules, invariants. No mocks, no database. |
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

1. **Local development:** a `docker-compose.yml` with PostgreSQL from Week 1.
2. **Tests:** Testcontainers starts a throwaway PostgreSQL through Docker, locally and in CI.
3. **Packaging:** a multi-stage Dockerfile for the API and a Compose file that starts the API, database, and frontend with one command. This comes at the end.

---

## 5. Frontend

A minimal frontend, built mainly to exercise the system. Scope: login, then a screen per role.

- **Tenant:** my lease and occupants, open a ticket, comment, confirm work, payment history.
- **Technician:** assigned tickets, start work, work done, request category review, comment.
- **Manager:** ticket list and review queue, assign from the eligible list, close or cancel, accounts, units, finances.
- **Admin:** units and manager accounts.

It is built in thin slices after each backend milestone. Generate the API client from the OpenAPI document so the frontend cannot drift from the API, and configure CORS on the API.

**Technology (decided):** Blazor WebAssembly in the early milestones, then React with TypeScript in later stages, to gain experience with both. The OpenAPI-generated client is the stable contract between the API and either frontend, and no code is shared between them. Only one frontend ships in the final Docker Compose setup.

**Switch point (decided):** the start of M4 (Week 9). The earlier screens are simple, so porting them is cheap, and the ticket flows are the richest UI. Week 9 is dedicated to React setup and porting, which adds one week to the plan. The Blazor screens built in M1 to M3 are retired after the port, and the last Blazor commit is tagged.

---

## 6. Milestones and weekly plan

### M0: Foundations (Week 1)

- Solution with four source projects and the test projects, `.editorconfig`, analyzers, central package management.
- Docker Compose with PostgreSQL, user-secrets for connection strings, GitHub Actions for build and test.
- Architecture test project with the first dependency rules.
- `Design.md` and `Plan.md` into `/docs`, README skeleton, first ADRs (architecture, ID strategy, error format, authentication approach, mapping approach).
- Decide the open technical conventions (section 8).

**Done when:** CI is green and `docker compose up` provides a database.

### M1: Identity and accounts (Weeks 2 to 4)

- **Week 2:** User hierarchy in the domain (Tenant, Employee, Manager, Technician, Admin). EF mappings with table-per-type, the credentials table, and case-insensitive unique email. First migration. Testcontainers setup and persistence tests.
- **Week 3:** Login use case, password hashing, JWT issuance, role claims, authorization skeleton. FluentValidation, Problem Details, global exception handling, OpenAPI with a UI. Decide the bootstrapping approach and seed the first admin. Write an ADR on where the frontend stores the JWT and how it refreshes it.
- **Week 4:** Account endpoints: create, view, edit, deactivate, and change email (with an email sender abstraction that logs in development). Audit fields through a `SaveChanges` interceptor. API tests for the account rows of the matrix. Login and accounts screens (Blazor).

**Done when:** the admin creates a manager, who creates a tenant and a technician, and you can log in as each.

### M2: Building structure (Weeks 5 and 6)

- **Week 5:** Unit aggregate (types, nullable-field rules, status), rate history, check constraints, unit endpoints, units screen (Blazor). Decommissioning rules that depend on leases and tickets are completed in later milestones.
- **Week 6:** Appliances, skills and technician skills, wage history and its permissions (technicians see their own, managers see all).

**Done when:** the right roles can manage units, appliances, skills, and wages.

### M3: Leasing (Weeks 7 and 8)

- **Week 7:** Lease and occupants. Overlap exclusion constraint. Rules: private units only, age check. "Own lease" authorization handler. Read-only access for former tenants.
- **Week 8:** Move-out request workflow with `requested_by`, occupant endpoints, unit status derived from leases, tenant screens (Blazor).

**Done when:** the whole tenancy lifecycle works.

### M4: React switch and maintenance tickets (Weeks 9 to 13)

- **Week 9:** React frontend foundation: Vite and TypeScript project, TypeScript client generated from the OpenAPI document, login and token handling following the Week 3 ADR, a role-aware layout, and a port of the earlier screens (login, accounts, units, appliances, skills, leases, move-out). Tag the last Blazor commit. React is the only frontend from here on.
- **Week 10:** Ticket aggregate and state machine, written test-first: the transitions table, a comment on every status change, Closed is final, category and assignment rules, `closed_reason`. Persistence for ticket, comments, and events, plus the `xmin` concurrency token.
- **Week 11:** Use cases for open, assign (eligible-technician list plus enforcement in the endpoint), start work, work done, resolve, rework, close, cancel, and comments. Every action writes its event. Invalid transitions return 409.
- **Week 12:** Category review request and the manager's queue. Role-specific response models. Tenant visibility during the lease. Filtering and pagination. Unit maintenance history. Wire the unit decommission guard (no unfinished tickets).
- **Week 13:** Matrix-driven API tests for every ticket row and role. A concurrency conflict test. React flows for all roles. Cleanup buffer.

**Done when:** the full ticket lifecycle is demonstrable in the UI for every role.

### M5: Financial milestone (Weeks 14 to 16)

- **Week 14:** Charge and Payment domain and persistence, uniqueness per lease, period, and type. Manual payment recording. Derived status (paid, partial, overdue).
- **Week 15:** Monthly charge generation as an idempotent background service, tested with a fake `TimeProvider`. Tenant payment history. Manager financial reports.
- **Week 16:** Time entries and labor cost (hours times the wage effective on that date), cost per ticket, unit, and category. Matching screens.

**Done when:** the reports reconcile in integration tests.

### M6: Hardening and release (Weeks 17 and 18)

- **Week 17:** API Dockerfile and full-stack Compose. Health checks and rate limiting. Serilog with correlation IDs. Index review of the main queries. Demo seed data for every role.
- **Week 18:** README with an architecture diagram and one-command setup. Polished OpenAPI documentation. Final CI (build, test, format, Docker build). Demo screenshots. Tagged release.

**Done when:** a fresh clone plus `docker compose up` lets you log in as each role.

---

## 7. If time runs short

M1 to M4 already deliver the core system, so M5 (financial) can shrink or slip without breaking the project's story. Within M6, keep the Docker packaging and README, and trim the rest first.

---

## 8. Open decisions

Carried over from `Design.md` section 9, with the week where each is needed:

| Decision | Needed by |
|---|---|
| Architecture and technical conventions (authentication scheme details, error format, API versioning, logging) | Week 1 |
| Bootstrapping of the first admin account | Week 3 |
| Whether managers can edit other users' profile data | Week 4 |
| Dismissing a category review request | Week 12 |
| Ticket priority | Week 10 |
| What happens when an assigned technician becomes unavailable after work has started (current answer: cancel and open a new ticket) | Week 11 |
