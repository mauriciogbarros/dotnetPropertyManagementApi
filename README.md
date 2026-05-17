# Property Management API
A role-based property management API, built with .NET and Clean Architecture principles.

This API allows **managers** to manage property **units**, **tenants**, and **technicians**.

#### Motivation
This is a portfolio project demonstrating API design, authentication, authorization, and data modeling using .NET and PostgreSQL.

## Key Features
### Manager Capabilities
- Create new **users** (**managers**, **technicians**, **tenants**)
- View information about **users** and **units**
- Update their own information, a **user**, or a **unit**
- Deactivate a **user** or a **unit**

### Technician Capabilities
- View their personal information
- Update their personal information
- View **units**

### Tenant Capabilities
- View their personal information 
- View the information of their **unit**
- Update their personal information

## Technology Stack
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- Swagger
- OpenAPI
- FluentValidation

## Architecture Overview
This project follows a layered, clean architecture approach to keep the codebase maintainable and scalable.

**dotnetPropertyManagementApi**
- Api/
  - Controllers/
- Application/
  - Abstractions/
  - DependencyInjection/
  - Dtos/
- Domain/
  - Abstractions/
  - Entities/
  - Enums/
  - ValueObjects/
- Infrastructure/
  - Persistence
    - DependencyInjection/
    - DesignTime/
    - Migrations/
    - Repositories
- Tests/
  - Unit/
  - Integration/

### Key rules
- Domain
  - Pure business logic
  - No EF Core, no ASP.NET, no dependencies
- Application
  - Use cases, interfaces, DTOs
  - Depends only on Domain
  - Defines repository contracts
- Infrastructure
  - DbContext, migrations, repository implementations
  - Depends on Application + Domain
- API
  - Controllers, HTTP concerns
  - Depends on Application
  - References Infrastructure **only to register services**

```
Domain <---- Application
  ^             |
  |             |
Infrastructure  |
  ^             |
  |             |
 API -----------
```

### References
Rule: outer -> inner
- `API` -> `Application`
- `API` -> `Infrastructure`
- `Application` -> `Infrastructure`
- `Application` -> `Domain`
- `Infrastructure` -> `Domain` 

## API Endpoints Overview
### Manager Endpoints
|Method|Endpoint|Description|
|:---:|:---|:---|

### Technician Endpoints
|Method|Endpoint|Description|
|:---:|:---|:---|

### Tenant Endpoints
|Method|Endpoint|Description|
|:---:|:---|:---|