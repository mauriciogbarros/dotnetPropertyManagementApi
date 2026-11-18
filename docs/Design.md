# Property Management System: Design Document

**Status:** design baseline, before implementation
**Date:** 2026-11-04
**Stack:** .NET 10, PostgreSQL

## Contents

1. [Overview](#1-overview)
2. [Actors and responsibilities](#2-actors-and-responsibilities)
3. [Entities and their roles](#3-entities-and-their-roles)
4. [Entity-relationship diagram](#4-entity-relationship-diagram)
5. [Workflows](#5-workflows)
6. [Permission matrix](#6-permission-matrix)
7. [Business rules and invariants](#7-business-rules-and-invariants)
8. [Decision log](#8-decision-log)
9. [Future improvements](#10-future-improvements)

---

## 1. Overview

### 1.1 Purpose

The system manages the day-to-day operation of a single residential building. It keeps track of who lives where, what rent is due and what has been paid, and how maintenance problems are reported, assigned, fixed, and recorded.

It serves two groups of people. **Building staff** (managers and technicians) need one reliable place to see units, tenants, finances, and open maintenance work. **Tenants** need a simple way to report a problem in their unit, follow its progress, confirm that it was fixed, and look at their own payment history.

This project is a **portfolio piece** built to practice .NET development and system design.

### 1.2 Scope

**In scope**

- One building made of units (private apartments, public spaces, and offices).
- Accounts for tenants, employees (managers and technicians), and administrators.
- Leases, occupants, move-out requests, monthly rent charges, and recorded payments.
- Maintenance tickets with categories, assignment by skill, a comment journal, and a full event timeline.
- Role-based and record-based access control.
- A minimal frontend built with Blazor WebAssembly.

**Not in scope for this design**

- Multiple buildings or properties (planned as a later improvement).
- Co-tenants, meaning several people who each sign the same lease (later improvement).
- Online payment processing. Payments are recorded manually by managers.
- Notifications, payroll, and general employee time tracking.

### 1.3 System at a glance

- A building contains **units** and **users**.
- A unit can be:
  - *private* (rented to a tenant);
  - *public* (corridor, laundry room, garage, pool); or 
  - *office* (manager's office, rental office).
- A user can be:
  - **Tenant**
    - Rents a private unit through a **lease**;
    - Can open a **maintenance ticket**, coment on it, and set its status to *resolved*;
    - Make a move out **request**.
  - **Employee**
    - Part of ther personnel responsible for running the building;
    - Can be:
      - **Technician**, responsible for the maintenance work and have **skills** that match a **ticket** **category**, and can view the **maintenance history** for all the **units**.
      - **Manager**, responsible for overseeing finances, accounts, leases, and maintenance.
  - **Admin**, responsible to maintain and bootstrap the system, they are not part of the building's payroll.
- A **lease** generates a monthly **charge** that is settled by a **payment**.
- A **maintenance ticket** against a **unit** can be opened by the **tenant** or any **manager**, and must have a **category**.
  - Once open, a **manager** assigns a **technician** whose **skill** matches the ticket's **category**.
  - The **technician** inspects, works, and reports the work done.
  - The **tenant** confirms.
  - A **manager** closes the **ticket**.
  - Every step is recorded.

### 1.4 Delivery phasing

The core system covers accounts and access, units and appliances, leases, occupants, move-out requests, and maintenance tickets with categories and events. A **financial milestone** follows: time entries (decided), plus charges, payments, and financial reports (planned, see section 9).

---

## 2. Actors and responsibilities

| Actor | Responsibilities |
|---|---|
| **Tenant** | Views own payment history. Requests move-out. Opens maintenance tickets for their own unit and comments on tickets they opened. Views the maintenance history of their unit for the period of their lease. Confirms completed work by setting a ticket to Resolved. Manages the occupants (family members) on their lease. Edits their own profile. |
| **Technician** | Has a list of skills. Works on tickets assigned to them: starts work, reports work done, comments. Views the maintenance history of the whole property. Logs time on for the work done on their assigned tickets. Sees their own wage history. May request a category review on an assigned Open ticket. |
| **Manager** | Tracks financial data for the whole property. Creates tenant, technician, and manager accounts. Views all information about the property and each unit. Manages leases, occupants, and move-out requests. Adjusts unit rates and wages. Manages the skill list and appliances. Opens and assigns tickets, changes categories, and is the only role that can close a ticket. |
| **Admin** | Programmer-type account that maintains the building's structure: creates units, edits core unit data, and decommissions units. Creates, views, edits, and deactivates manager accounts (account data only, never wages). Changes any account's email. The first admin is how a new installation gets its first manager. Has no access to tickets, leases, payments, or wages. They are not part of the building's payroll. |
| **Occupant** | Not an actor. A household member listed on a lease (for example, a family member). Has no login. |

---

## 3. Entities and their roles

### 3.1 People and access

| Entity | Role in the system | Key points |
|---|---|---|
| **User** | The base record for every person with an account. | First name, last name, date of birth, phone number, email, active flag, created and updated timestamps. Email is the login identifier and is unique (case-insensitive).  Subtypes are Tenant, Employee, and Admin. |
| **UserCredential** | Holds the secrets and login state of a user. | Password hash, failed-attempt count, lockout end, last login. Kept apart from `User` so the domain model has no authentication concerns. |
| **Tenant** | A person who rents a private unit. | Emergency contact and status. Which unit they occupy comes from their lease, never from a column on the tenant. |
| **Employee** (abstract) | A staff member. | Hire date, termination date, employment status, emergency contact. Subtypes are Manager and Technician. |
| **Manager** | An employee with operational authority over the building. | No extra data. The role determines what they can see and do (section 6). |
| **Technician** | An employee who performs maintenance work. | Skills, assigned tickets, time entries. |
| **Admin** | A user type for structural and system administration. | No extra data. Can view and edit manager accounts. Has no access to tickets, leases, payments, or wages (a boundary that may be revisited later). |
| **WageHistory** | The hourly wage of an employee over time. | Wage, effective-from and effective-to dates, and who changed it. Append-only, so past labor costs stay correct. |
| **Skill** | A capability a technician can have, and also the list of ticket categories. | One shared list (for example Plumbing, Electrical, HVAC, General maintenance). Managed by managers. |
| **TechnicianSkill** | Links technicians to skills. | Optional certification expiry date. |

### 3.2 Building structure

| Entity | Role in the system | Key points |
|---|---|---|
| **Unit** | Any space in the building. | Type (Private, Public, Office), unique name (for example 101 or "Laundry room"), floor, area in m², bedrooms, bathrooms, balcony and kitchen flags, status (for example available, occupied, under maintenance, decommissioned). Bedrooms, bathrooms, balcony, and kitchen are only filled for spaces where they apply. Units are never deleted, only decommissioned. |
| **UnitRateHistory** | The listed monthly rent of a private unit over time. | Rate, effective date, who set it. The current listed rate is the latest entry. It applies to future leases only. Public and office units have no rate. |
| **Appliance** | A piece of equipment installed in a unit. | Type, brand, model, serial number, install date, free-form specifications (JSONB), status. A ticket can refer to one. |

### 3.3 Leasing and money

| Entity | Role in the system | Key points |
|---|---|---|
| **Lease** | Connects a tenant to a private unit for a period. | Start and end dates, agreed monthly rent, status. Replaces any "current tenant" column. The agreed rent is fixed per lease, so later rate changes never rewrite history. A unit with no active lease is vacant. |
| **Occupant** | A household member living under a lease. | Name, date of birth, relationship. Belongs to a lease. Not a user. |
| **MoveOutRequest** | The workflow for ending a tenancy. | Requested date, status, who requested it (tenant or manager on their behalf), who reviewed it, notes. Approval ends the lease. |
| **Charge** | An amount a tenant owes for a period. | Lease, period, type, amount, due date, voided flag. Generated monthly per active lease, and unique per lease, period, and type so generation can safely be repeated. |
| **Payment** | Money received against a charge. | Amount, date, method, reference, recorded by. Several payments can settle one charge. Paid, partial, and overdue are derived, not stored. |

### 3.4 Maintenance

| Entity | Role in the system | Key points |
|---|---|---|
| **MaintenanceTicket** | A reported problem in a unit, followed until it is closed. | Unit, optional appliance, category (a skill), creator, assigned technician (optional), title, description, status, closed reason, timestamps. Once Closed it is final and read-only. |
| **TicketComment** | A permanent entry in the ticket's journal. | Author, text, time. Cannot be edited or deleted. |
| **TicketEvent** | A record of everything that happens to a ticket. | Event type (status changed, category changed, assigned, unassigned, category review requested, category review dismissed), from and to values, who, when, and a link to the explaining comment. Append-only. |
| **TimeEntry** | Hours a technician worked on a ticket. | Date, hours, note. Together with wage history it gives labor cost per ticket, unit, and category. Ships with the financial milestone. |

---

## 4. Entity-relationship diagram

The diagram is split into three parts to stay readable. Subtype tables (Tenant, Employee, Manager, Technician, Admin) use their parent's primary key, which is how table-per-type inheritance maps to the database. Entities that appear with only a key are **stubs** repeated from another part so you can see how the parts connect. Several `changed_by`, `reviewed_by`, and `recorded_by` columns point to managers.

### 4.1 People and access

```mermaid
erDiagram
  USER ||--|| USER_CREDENTIAL : "logs in with"
  USER ||--o| TENANT : "may be"
  USER ||--o| EMPLOYEE : "may be"
  USER ||--o| ADMIN : "may be"
  EMPLOYEE ||--o| MANAGER : "may be"
  EMPLOYEE ||--o| TECHNICIAN : "may be"
  EMPLOYEE ||--o{ WAGE_HISTORY : "paid under"
  MANAGER ||--o{ WAGE_HISTORY : "records"
  TECHNICIAN ||--o{ TECHNICIAN_SKILL : has
  SKILL ||--o{ TECHNICIAN_SKILL : "listed in"

  USER {
    uuid id PK
    string first_name
    string last_name
    date date_of_birth
    string phone_number
    string email UK
    boolean is_active
    timestamptz created_at
    timestamptz updated_at
  }
  USER_CREDENTIAL {
    uuid user_id PK, FK
    string password_hash
    int failed_attempts
    timestamptz lockout_end
    timestamptz last_login
  }
  TENANT {
    uuid user_id PK, FK
    string emergency_contact_name
    string emergency_contact_phone
    string status
  }
  EMPLOYEE {
    uuid user_id PK, FK
    date hire_date
    date termination_date
    string employment_status
    string emergency_contact_name
    string emergency_contact_phone
  }
  MANAGER {
    uuid employee_id PK, FK
  }
  TECHNICIAN {
    uuid employee_id PK, FK
  }
  ADMIN {
    uuid user_id PK, FK
  }
  WAGE_HISTORY {
    uuid id PK
    uuid employee_id FK
    decimal hourly_wage
    date effective_from
    date effective_to
    uuid changed_by FK
  }
  SKILL {
    uuid id PK
    string name UK
  }
  TECHNICIAN_SKILL {
    uuid technician_id PK, FK
    uuid skill_id PK, FK
    date certification_expiry
  }
```

### 4.2 Building, leasing, and money

```mermaid
erDiagram
  UNIT ||--o{ UNIT_RATE_HISTORY : "listed rate"
  UNIT ||--o{ APPLIANCE : contains
  UNIT ||--o{ LEASE : "leased through"
  TENANT ||--o{ LEASE : signs
  LEASE ||--o{ OCCUPANT : houses
  LEASE ||--o{ MOVE_OUT_REQUEST : "ended by"
  LEASE ||--o{ CHARGE : bills
  CHARGE ||--o{ PAYMENT : "settled by"
  USER ||--o{ MOVE_OUT_REQUEST : submits
  MANAGER ||--o{ UNIT_RATE_HISTORY : sets
  MANAGER |o--o{ MOVE_OUT_REQUEST : reviews
  MANAGER ||--o{ PAYMENT : records

  UNIT {
    uuid id PK
    string type
    string name UK
    int floor
    decimal area_m2
    int bedrooms
    int bathrooms
    boolean has_balcony
    boolean has_kitchen
    string status
    timestamptz decommissioned_at
  }
  UNIT_RATE_HISTORY {
    uuid id PK
    uuid unit_id FK
    decimal monthly_rate
    date effective_from
    uuid changed_by FK
  }
  APPLIANCE {
    uuid id PK
    uuid unit_id FK
    string type
    string brand
    string model
    string serial_number
    date installed_on
    jsonb specifications
    string status
  }
  LEASE {
    uuid id PK
    uuid unit_id FK
    uuid tenant_id FK
    date start_date
    date end_date
    decimal agreed_monthly_rent
    string status
  }
  OCCUPANT {
    uuid id PK
    uuid lease_id FK
    string first_name
    string last_name
    date date_of_birth
    string relationship
  }
  MOVE_OUT_REQUEST {
    uuid id PK
    uuid lease_id FK
    uuid requested_by FK
    date requested_move_out_date
    string status
    uuid reviewed_by FK
    timestamptz reviewed_at
    string notes
  }
  CHARGE {
    uuid id PK
    uuid lease_id FK
    date period
    string type
    decimal amount
    date due_date
    boolean is_voided
  }
  PAYMENT {
    uuid id PK
    uuid charge_id FK
    decimal amount
    date paid_on
    string method
    string reference
    uuid recorded_by FK
  }
  TENANT {
    uuid user_id PK
  }
  MANAGER {
    uuid employee_id PK
  }
  USER {
    uuid id PK
  }
```

### 4.3 Maintenance

```mermaid
erDiagram
  UNIT ||--o{ MAINTENANCE_TICKET : has
  APPLIANCE |o--o{ MAINTENANCE_TICKET : concerns
  SKILL ||--o{ MAINTENANCE_TICKET : categorizes
  USER ||--o{ MAINTENANCE_TICKET : opens
  TECHNICIAN |o--o{ MAINTENANCE_TICKET : "assigned to"
  MAINTENANCE_TICKET ||--o{ TICKET_COMMENT : has
  MAINTENANCE_TICKET ||--o{ TICKET_EVENT : logs
  MAINTENANCE_TICKET ||--o{ TIME_ENTRY : tracks
  USER ||--o{ TICKET_COMMENT : writes
  USER ||--o{ TICKET_EVENT : performs
  TICKET_COMMENT |o--o| TICKET_EVENT : explains
  TECHNICIAN ||--o{ TIME_ENTRY : logs

  MAINTENANCE_TICKET {
    uuid id PK
    uuid unit_id FK
    uuid appliance_id FK
    uuid category_id FK
    uuid created_by FK
    uuid assigned_technician_id FK
    string title
    string description
    string status
    string closed_reason
    timestamptz created_at
    timestamptz updated_at
    timestamptz work_done_at
    timestamptz resolved_at
    timestamptz closed_at
  }
  TICKET_COMMENT {
    uuid id PK
    uuid ticket_id FK
    uuid author_id FK
    string body
    timestamptz created_at
  }
  TICKET_EVENT {
    uuid id PK
    uuid ticket_id FK
    uuid comment_id FK
    string event_type
    string from_value
    string to_value
    uuid performed_by FK
    timestamptz occurred_at
  }
  TIME_ENTRY {
    uuid id PK
    uuid ticket_id FK
    uuid technician_id FK
    date work_date
    decimal hours
    string note
  }
  UNIT {
    uuid id PK
  }
  APPLIANCE {
    uuid id PK
  }
  SKILL {
    uuid id PK
    string name UK
  }
  USER {
    uuid id PK
  }
  TECHNICIAN {
    uuid employee_id PK
  }
```

### 4.4 Notes the diagram cannot express

- **Ticket event types:** `StatusChanged`, `CategoryChanged`, `Assigned`, `Unassigned`, `CategoryReviewRequested`, `CategoryReviewDismissed`. `from_value` and `to_value` hold the old and new status, category, or technician as text. `comment_id` points to the comment that explains the event and is required for `StatusChanged`, `CategoryReviewRequested`, and `CategoryReviewDismissed`.
- **Who resolved or closed a ticket** is read from `ticket_event`. The ticket itself keeps only the timestamps and the closed reason.
- **Constraints to add in migrations:**
  - Unique `(lease, period, type)` on `charge`.
  - Exclusion constraint on `(unit, date range)` on `lease`, so leases never overlap.
  - Partial unique index so a lease has at most one pending move-out request.
  - Partial unique index on `(tenant_id) WHERE status = 'Active'` on `lease`, so a tenant can have at most one active lease at a time.
  - Check constraints so Private units have a rate history and other unit types do not, and so fields like bedrooms are only set where they apply.
  - No overlapping effective-date ranges per employee in `wage_history`.
  - Case-insensitive unique email (`citext` or a unique index on the lowercased value).
- **Concurrency token** on `maintenance_ticket` using PostgreSQL's `xmin` system column, which is why it is not drawn.
- **Conventions in the diagram:** UUID keys, UTC `timestamptz`, `numeric` for money.
- **Time entry** is part of the design and the diagram, but its table is created with the financial milestone.

---

## 5. Workflows

### 5.1 Maintenance ticket

#### State diagram

```mermaid
stateDiagram-v2
  state "In Progress" as InProgress
  state "Work Done" as WorkDone
  [*] --> Open : opened by tenant or manager

  Open --> InProgress : assigned technician
  InProgress --> WorkDone : assigned technician
  WorkDone --> InProgress : tenant or manager (rework)
  WorkDone --> Resolved : tenant or manager
  Resolved --> Closed : manager
  Open --> Closed : manager (cancel)
  InProgress --> Closed : manager (cancel)
  WorkDone --> Closed : manager (cancel)
  Closed --> [*]
```

#### Typical flow

1. A tenant opens a ticket for their unit and chooses a **category**.
2. A manager assigns a technician. The assignment list contains only technicians who have the skill matching the category.
3. The technician visits the unit and inspects the problem. The ticket is still **Open**.
4. If the technician agrees with the category, they set the ticket to **In Progress** and do the work.
5. If the problem belongs to another category, the technician keeps the ticket Open, posts a comment explaining their view, and **requests a category review**. The ticket then appears in the manager's review filter. The manager either changes the category (which automatically clears the assigned technician and the manager then picks from a newly filtered list), or dismisses the review request with a required comment (which clears the ticket from the review filter without changing the category or assignment).
6. When the work is finished the technician sets **Work Done**.
7. The tenant checks the result. If satisfied they set **Resolved**. If not, they send it back to **In Progress** with a comment.
8. A manager sets **Closed**. Closing is final.

#### Transitions

| Transition | Who | Notes |
|---|---|---|
| Create (starts as Open) | Tenant (own unit, active lease), manager (any unit) | Category is required. The description is the first entry of the journal, so no comment is needed. |
| Open → In Progress | Assigned technician | Comment required. |
| In Progress → Work Done | Assigned technician | Comment required. |
| Work Done → Resolved | Tenant who opened the ticket. Manager when the ticket is stuck or was not opened by a tenant | Comment required. |
| Work Done → In Progress | Tenant who opened the ticket, manager | Rework. Comment required. |
| Resolved → Closed | Manager | Comment required. Closed reason is Completed. |
| Open, In Progress, or Work Done → Closed | Manager | Cancellation: duplicates, mistakes, withdrawn or abandoned tickets. Comment required. Closed reason is Cancelled. |

#### Workflow rules

- **Every status change must have a comment.** The comment is a normal ticket comment, linked from the `StatusChanged` event.
- **Closed is final.** A Closed ticket is fully read-only, including comments, for every role. There is no reopening. A related problem gets a new ticket.
- **Category changes** are only possible while the ticket is Open, only by a manager, and always clear the assigned technician. After work has started the category is frozen. If a wrong category is found later, cancel the ticket with a note and open a new one.
- **Assignment** is only possible to technicians whose skills include the ticket's category, and only while the ticket is Open. Once the ticket is In Progress the assigned technician cannot be changed. The assignment endpoint enforces the skill rule itself, not only the list shown to the manager.
- **Category review request:** a technician assigned to an Open ticket can request one, with a required comment. The manager's filter shows Open tickets whose latest category-related event is a review request. The request is answered when the manager changes the category, dismisses the request with a required comment, or when the ticket leaves Open.
- **Closed reason** is Completed exactly when the ticket was closed from Resolved, and Cancelled otherwise.
- **Comments are a permanent journal.** They cannot be edited or deleted. A mistake is corrected with a new comment.

### 5.2 Lease lifecycle

#### State diagram

```mermaid
stateDiagram-v2
  [*] --> Active : manager creates lease
  Active --> Ended : manager ends lease
  Ended --> [*]
```

#### Transitions

| Transition | Who | Notes |
|---|---|---|
| Create (starts as Active) | Manager | Unit must be Private, have no active lease, and the tenant must have no other active lease. Tenant must be at least 18 at start date. The start date may be today or a future date. |
| Active → Ended | Manager | All pre-conditions in section 5.3 (move-out) must be met, or the manager must resolve them as part of the move-out workflow. |
| Extend end date | Manager | Only on an Active lease. The new end date must be after the current end date. |

#### Workflow rules

- A new lease cannot be created for a unit that already has an Active lease. The current lease must end first.
- Unpaid charges survive lease end and must still be settled. They remain linked to the ended lease.
- Lease renewal means extending the end date of the current Active lease, not closing it and opening a new one.
- A lease can only be ended through the move-out request workflow (section 5.3) or directly by a manager when no move-out request is needed.

### 5.3 Move-out request

#### State diagram

```mermaid
stateDiagram-v2
  [*] --> Pending : submitted by tenant or manager
  Pending --> Approved : manager approves
  Pending --> Rejected : manager rejects
  Approved --> Cancelled : manager cancels (reason required)
  Approved --> [*] : move-out finalised, lease ended
  Rejected --> [*]
  Cancelled --> [*]
```

#### Transitions

| Transition | Who | Notes |
|---|---|---|
| Submit (starts as Pending) | Tenant (own lease, if `can_request_move_out` is true), manager | Records `requested_by` and the requested move-out date. |
| Pending → Approved | Manager | Pre-conditions must be met before finalising (see workflow rules). |
| Pending → Rejected | Manager | Reason must be recorded in `notes`. Sets `can_request_move_out` to false on the lease. |
| Approved → Cancelled | Manager | Reason must be recorded. Lease remains Active. |
| Finalise move-out | Manager | Triggered after approval once all pre-conditions are met. Ends the lease. |

#### Workflow rules

- A tenant can only submit a move-out request if `can_request_move_out` is true on their lease. This flag starts as true and is set to false when a request is rejected.
- Re-enabling move-out requests after a rejection is a deliberate manager action, separate from the rejection itself.
- A lease can have at most one Pending move-out request at a time (partial unique index).
- Before a move-out can be finalised, the manager must ensure:
  - All open maintenance tickets for the unit are resolved or closed. If any remain open, the manager must reassign `created_by` on each open ticket to themselves before finalising.
  - All charges on the lease are settled or voided.
- Once the move-out is finalised the lease status becomes Ended.

### 5.4 Payment and charge

#### State diagram

```mermaid
stateDiagram-v2
  state "Partially Paid" as PartiallyPaid
  [*] --> Unpaid : charge generated
  Unpaid --> PartiallyPaid : payment recorded (amount < balance)
  PartiallyPaid --> PartiallyPaid : further payment recorded (still < balance)
  PartiallyPaid --> Paid : payment recorded (balance settled)
  Unpaid --> Paid : payment recorded (full amount)
  Unpaid --> Voided : manager voids charge
  PartiallyPaid --> Voided : manager voids charge
  Paid --> [*]
  Voided --> [*]
```

#### Workflow rules

- Paid, Partially Paid, and Overdue are **derived states**, not stored columns. They are computed from the sum of payments against the charge and the due date.
- Charges are generated monthly per active lease and are unique per `(lease, period, type)`, so generation can safely be repeated.
- Only a manager can void a charge. A reason must be recorded as a comment or note at the time of voiding.
- Payments are recorded manually by a manager. There is no online payment processing.
- A voided charge is excluded from all balance and overdue calculations.

### 5.5 Account lifecycle

#### State diagram

```mermaid
stateDiagram-v2
  [*] --> Active : account created
  Active --> Inactive : deactivated by manager (or admin for manager accounts)
  Inactive --> Active : reactivated (see rules)
  Active --> [*]
  Inactive --> [*]
```

#### Workflow rules

- Any account can be deactivated by a manager. Manager accounts can also be deactivated by an admin.
- A deactivated account cannot log in.
- **Tenant reactivation** requires a new active lease. The manager creates the lease, which reactivates the tenant account.
- **Employee reactivation** follows the onboarding workflow (section 5.6). A new hire date is recorded.
- Rehire always creates a **new account**. A terminated employee who returns to the building gets a new user record.
- Former tenants retain read-only access to their own payment and ticket history after deactivation.

### 5.6 Employee onboarding and termination

#### Onboarding

1. A manager (or admin, for manager accounts) creates the employee account with personal details, hire date, and initial wage.
2. For a **technician**, the manager assigns the relevant skills. At least one skill is required before the technician can be assigned to a ticket.
3. The account is Active and the employee can log in.

#### Termination state diagram

```mermaid
stateDiagram-v2
  [*] --> Active : onboarded
  Active --> PendingTermination : termination initiated, open tickets exist
  PendingTermination --> Active : termination cancelled
  PendingTermination --> Terminated : all open tickets reassigned
  Active --> Terminated : termination initiated, no open tickets
  Terminated --> [*]
```

#### Termination workflow rules

- A manager initiates termination by setting the employee's `employment_status` to Terminated and recording a `termination_date`.
- If the technician has **open assigned tickets**, the system blocks termination and requires the manager to reassign every open ticket before proceeding. The manager handling the termination becomes the new `created_by` on those tickets.
- Once all open tickets are reassigned, termination is finalised: `employment_status` is set to Terminated, `is_active` is set to false, and the account cannot log in.
- A terminated employee's time entries, wage history, and ticket history remain visible to managers.
- Rehire creates a new account. The old account is never reactivated.

---

## 6. Permission matrix

**Legend:** Yes = any record. Own = only their own records. Assigned = only tickets assigned to them. A dash means denied. Everything not listed is denied.

### Accounts and profiles

| Action | Tenant | Technician | Manager | Admin |
|---|---|---|---|---|
| Create tenant or technician account | — | — | Yes | — |
| Create manager account | — | — | Yes | Yes |
| Deactivate an account | — | — | Yes | Manager accounts |
| Edit own profile, including email | Own | Own | Own | Own |
| Change any account's email (audited) | — | — | Yes | Yes |
| View other users' profiles | — | Tenant name and unit, on assigned tickets | Yes | Manager accounts |
| Edit other users' profile data (name, phone, date of birth) | — | — | Yes | Manager accounts |

Changing your own email requires the current password, a unique address, and confirmation of the new address through a link. Admins view and edit manager accounts without any access to wage history.

### Employees and wages

| Action | Tenant | Technician | Manager | Admin |
|---|---|---|---|---|
| View wage history | — | Own | Yes | — |
| Adjust any wage, including own | — | — | Yes | — |
| Manage the skill list and technician skills | — | View own | Yes | — |
| Log time entries (ticket not Closed) | — | Assigned | Yes | — |

### Units and appliances

| Action | Tenant | Technician | Manager | Admin |
|---|---|---|---|---|
| View units and appliances | Own unit | Yes | Yes | Yes |
| Create unit | — | — | — | Yes |
| Edit core data, decommission unit (rules apply) | — | — | Yes | Yes |
| Adjust listed rate, view rate history | — | — | Yes | — |
| Add, edit, or retire appliances | — | — | Yes | — |

### Leases, occupants, and move-out

| Action | Tenant | Technician | Manager | Admin |
|---|---|---|---|---|
| Create or end a lease | — | — | Yes | — |
| View leases | Own | — | Yes | — |
| Manage occupants | Own lease | — | Yes | — |
| Submit or cancel a move-out request | Own lease | — | Yes | — |
| Approve or reject a move-out request | — | — | Yes | — |

### Charges and payments

| Action | Tenant | Technician | Manager | Admin |
|---|---|---|---|---|
| View charges and payment history | Own | — | Yes | — |
| Record a payment, void a charge | — | — | Yes | — |
| Property-wide financial reports | — | — | Yes | — |

### Maintenance tickets

| Action | Tenant | Technician | Manager | Admin |
|---|---|---|---|---|
| Open a ticket, choosing a category | Own unit, active lease | — | Any unit | — |
| View ticket, comments, and event timeline | Own unit, created during their lease | Yes | Yes | — |
| Assign or reassign a technician (only while Open, eligible technicians only) | — | — | Yes | — |
| Change category (only while Open, clears the assigned technician) | — | — | Yes | — |
| Request category review (comment required, ticket Open) | — | Assigned | — | — |
| Open → In Progress | — | Assigned | — | — |
| In Progress → Work Done | — | Assigned | — | — |
| Work Done → Resolved | Opened by them | — | Yes | — |
| Work Done → In Progress | Opened by them | — | Yes | — |
| Resolved → Closed | — | — | Yes | — |
| Cancel (Open, In Progress, or Work Done → Closed) | — | — | Yes | — |
| Add a comment | Opened by them | Assigned | Yes | — |
| Edit or delete a comment | — | — | — | — |
| Reopen or change a Closed ticket, comments included | — | — | — | — |

Every status change in this table requires a comment.

### How the matrix is enforced

1. **Role** decides which endpoints a user can call at all.
2. **Resource check** decides whether the user may act on this specific record. Examples: the ticket's unit must match one of the tenant's leases and the ticket must have been created within those lease dates. A technician's transitions check that they are the currently assigned technician.
3. **Response model** decides which fields come back. Each role gets its own response shape, so a technician's ticket response never carries payment data.

Former tenants keep **read-only** access to their own payment and ticket history after their lease ends. Write actions additionally require an active lease.

---

## 7. Business rules and invariants

| Rule | Enforced in |
|---|---|
| Leases exist only on Private units. A tenant must be at least 18 at lease start. | Domain |
| A unit never has overlapping leases. | Database (exclusion constraint) and domain |
| Private units have a rate history. Other unit types have none. Fields like bedrooms are only set where they apply. | Database (check constraints) and domain |
| A unit can be decommissioned only with no active lease and no unfinished tickets. Units are never deleted. | Domain |
| A lease has at most one pending move-out request. Approval ends the lease. | Domain and partial unique index |
| A tenant can have at most one active lease at a time. | Database (partial unique index on `lease(tenant_id) WHERE status = 'Active'`) and domain (pre-check in application service before creating a lease) |
| Rent charges are unique per lease, period, and type. | Database |
| Wage and rate histories are append-only and never overlap. | Domain and database |
| Email is unique, case-insensitive. | Database |
| Only the transitions in section 5.3 are possible. Every status change needs a comment. | `MaintenanceTicket` entity |
| A Closed ticket rejects every mutation (status, category, assignment, comments, time entries) from every role. | `MaintenanceTicket` entity |
| Category changes only while Open, only by a manager, and they clear the assignment. | `MaintenanceTicket` entity |
| A technician can be assigned only if their skills include the ticket's category. | Application service (assignment) |
| Assignment and reassignment are only possible while the ticket is Open. | `MaintenanceTicket` entity |
| Closed reason is Completed only when closed from Resolved. | `MaintenanceTicket` entity |
| Comments and events are append-only: there is no update or delete operation. | Domain, no endpoints, optionally database permissions |
| A technician can log time only on a ticket assigned to them. Entries stay on the ticket after unassignment, but the unassigned technician cannot add more. | Application service |

---

## 8. Decision log

### Architecture

- Clean Architecture with four layers: Domain, Application, Infrastructure, and API.
- One test project per layer.
- Persistence is PostgreSQL, accessed through the Infrastructure layer via EF Core.
- Frontend is a minimal Blazor WebAssembly application.

### Structure and modeling

- Every kind of history (tickets, tenancies, payments, rates, wages, events) is its own entity pointing at its owner, never a list inside another entity.
- A **Lease** entity replaces the idea of a tenant "stack" on a unit. Vacancy is the absence of an active lease.
- The property is **not** a tenant of public units. Public and office units simply have no leases.
- Single building for now. Multiple properties are a later improvement.
- Co-tenants are deferred. Household members are **Occupants** (renamed from "subtenants").
- Unit "Rooms" means **bedrooms**. A **floor** and a **status** are added.
- A unit's rate is stored in `unit_rate_history`, and a lease keeps its own agreed rent, so rate changes never rewrite history.
- **Appliance** is its own table, with a JSONB specifications column.
- Removing a unit means **decommissioning** it, with rules.

### Users and access

- A pure domain `User` with a separate **credentials** table.
- **Email is the only login identifier.** There is no username. Emails are unique (case-insensitive).
- Tenant, Employee, and Admin are subclasses of User. Manager and Technician are subclasses of Employee.
- Admin is a user type with no data of its own.
- A live-in superintendent would use a separate account with a personal email for their unit.
- Wages and rates have effective-dated history. Employees have an hourly wage.
- Managers can adjust their own wage. The README records this, and attaching an approval document (for example from a board of directors) is a future improvement.
- Every user can edit their own profile, including email, with password confirmation, a unique address, and a confirmation link.
- Managers can edit any user's name, phone, and date of birth. Admins can edit the same fields for manager accounts only. Auditing of profile edits is a future improvement.
- Managers and admins can change any account's email, audited.
- Admins can create, view, edit, and deactivate manager accounts, and change any account's email. Managers can deactivate any account. The system starts with at least one admin account, from which the first manager is created.
- Admins have no access to tickets, leases, payments, or wages. This may change in the future if it becomes necessary.
- Technicians see their own wage history and only limited tenant information (name and unit) on assigned tickets.
- Admins can view units and appliances. Managers and admins can both edit core unit data and decommission units. Only admins create units.
- Managers add, edit, and retire appliances and manage the skill list.
- Former tenants keep read-only access to their own history.
- Subclassing does **not** protect financial data from technicians. Authorization policies and role-specific responses do. The README states this.

### Leasing and money

- Payments use two concepts: **Charge** (what is owed) and **Payment** (what was received).
- Tenants manage occupants on their own lease. Managers can too.
- Tenants and managers can submit and cancel move-out requests. Managers approve or reject them, and `requested_by` records who submitted it.
- A new lease cannot be created for a unit that already has an Active lease. The current lease must end first.
- Unpaid charges survive lease end and must still be settled.
- Lease renewal means extending the end date of the current Active lease, not closing it and opening a new one.
- Only a manager can void a charge. A reason must be recorded at the time of voiding.
- A move-out request rejection sets `can_request_move_out` to false on the lease. Re-enabling it is a deliberate manager action, separate from the rejection itself. Configuring automatic re-enabling rules is a future improvement.
- Before a move-out is finalised, all open tickets for the unit must be resolved or closed (with `created_by` reassigned to the manager if needed), and all charges must be settled or voided.
- The financial milestone ships after the core system is complete and includes time entries, charges, payments, and financial reports.

### Employee lifecycle

- Rehire always creates a new account. A terminated employee who returns gets a new user record.
- A terminated employee's time entries, wage history, and ticket history remain visible to managers.
- A technician cannot be terminated while they have open assigned tickets. The manager handling the termination must reassign all open tickets first, becoming the new `created_by` on each.
- Tenant reactivation requires a new active lease. Employee reactivation follows the onboarding workflow.

### Maintenance

- Ticket statuses: Open, In Progress, **Work Done**, Resolved, Closed.
- The assigned technician moves a ticket to In Progress and to Work Done. The tenant who opened it accepts by setting Resolved. A manager closes it.
- A manager can resolve a stuck ticket, and can cancel a ticket before it is Resolved with a required comment.
- Rework is possible from Work Done back to In Progress, by the tenant or a manager, with a comment.
- **Closed is final and fully read-only**, including comments. No reopening, not even by a manager. A related problem gets a new ticket.
- A ticket has a required **category**, which is a skill. Only technicians with that skill appear in the assignment list.
- **Category can only change while the ticket is Open**, only by a manager, and the change clears the assigned technician.
- A technician who disagrees with the category keeps the ticket Open, comments, and **requests a category review**. The manager has a simple filter for these.
- **Every status change must have a comment.**
- A `closed_reason` (Completed or Cancelled) is stored on the ticket.
- Ticket comments are **permanent**: no editing, no deleting, no edit history. Technician notes are ordinary comments and are visible to tenants.
- A single append-only `ticket_event` table replaces a status-history table. It records status changes, category changes, assignments, unassignments, category review requests, and category review dismissals.
- A manager can dismiss a category review request with a required comment, recorded as a `CategoryReviewDismissed` event. The ticket stays Open and the category and assignment are unchanged.
- Tenants see every ticket of their unit created during their lease.
- Technicians can log time while a ticket is Open (for the inspection visit). Entries stay on the ticket after unassignment. Time entries ship with the financial milestone.
- **Assignment** and reassignment are only possible while a ticket is Open. A manager cannot change the technician once the ticket is In Progress. If a technician becomes unavailable after work has started, the ticket must be cancelled with a comment and a new ticket opened.

### Documentation commitments for the README

- Access control is enforced by authorization policies, not by the class hierarchy.
- Managers can change their own wage, with the approval-document improvement noted.
- Comments are permanent, so a comment containing sensitive information by mistake cannot be removed.

### Technical conventions

- **Authentication:** JWT tokens issued by the API. Access token expires in 30 minutes. Refresh token expires in 7 days and is rotated on every use. Both are stored in HttpOnly cookies.
- **Error format:** Problem Details (RFC 7807) using ASP.NET Core's built-in `ProblemDetails`.
- **API versioning:** not versioned. Noted as a future improvement.
- **Logging:** Serilog from the start, console sink for local development.
- **Local startup:** Docker Compose runs PostgreSQL only. API and frontend are started with `dotnet run` or `dotnet watch`. Full containerisation noted as a future improvement.

### Bootstrapping

- An EF Core data seeder runs on startup and creates one admin account if none exists, skipping if one is already present.
- Admin credentials are read from environment variables.
- A `.env.example` file at the repo root lists the required variables with placeholder values.
- Seed data is limited to one admin account for now. More detailed seed data will be added as the system matures.

---

## 9. Future improvements

- Multiple properties.
- Co-tenants (several people on one lease).
- Approval document attached to manager wage changes.
- Auditing of profile data edits (name, phone, date of birth).
- Notifications, for example for category review requests and assignments.
- A manager-only redaction of comment text, with an audit record, if permanent comments prove too strict.
- Online payment processing.
- Ticket priority (Low, Medium, High), settable at creation and changeable by a manager while the ticket is not Closed.
- A separate ticket category table with a skill mapping, if one category ever needs several skills.
- Email verification at account creation.
- API versioning.
- Full containerisation of the API and frontend with Docker Compose.
- Configurable rules for automatically re-enabling move-out requests after a rejection.
- Approval document attached to move-out cancellations.


For that reason it is designed first: entities, relationships, business rules, workflow, and permissions are settled in this document before any database or code is created.