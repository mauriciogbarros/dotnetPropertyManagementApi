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
  - Program.cs
- Application/
  - DTOs/
- Domain/
  - Entities/
    - BaseEntity.cs
    - Property.cs
    - Unit.cs
    - Users/
      - Manager.cs
      - Technician.cs
      - Tenant.cs
      - User.cs
  - Enums/
    - UserRoles.cs
- Infrastructure/
  - Authentication/
  - Persistence/
- Tests/
  - Unit/
  - Integration/

### References
Rule: outer -> inner
- `Infrastructure` -> `Domain`
- `Application` -> `Domain`
- `Application` -> `Infrastructure`
- `Api` -> `Application`
- `Unit` -> `Domain`
- `Unit` -> `Application`
- `Api` -> `Integration`

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