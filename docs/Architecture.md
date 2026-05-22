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