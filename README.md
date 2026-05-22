# Property Management API
A role-based property management API, built with .NET and Clean Architecture principles.

This API allows **managers** to manage property **units**, **tenants**, and **technicians**.

#### Motivation
This is a portfolio project demonstrating API design, authentication, authorization, and data modeling using .NET and PostgreSQL.

## Table of Contents
- [Property Management API](#property-management-api)
			- [Motivation](#motivation)
	- [Table of Contents](#table-of-contents)
	- [Key Features](#key-features)
		- [Manager Capabilities](#manager-capabilities)
		- [Technician Capabilities](#technician-capabilities)
		- [Tenant Capabilities](#tenant-capabilities)
	- [Technology Stack](#technology-stack)
	- [Installation](#installation)
	- [Usage](#usage)
	- [Contributing](#contributing)
	- [License](#license)

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

## Installation
1. Clone the repository:

```shell
git clone https://github.com/mauriciogbarros/dotnetPropertyManagementApi
```

2. Navigate to the project directory:

```shell
cd dotnetPropertyManagementApi
```

3. Restore NuGet packages:

```shell
dotnet restore
```

4. Build the project:

```shell
dotnet build
```

## Usage
1. Run the application

```shell
dotnet run --project src/API/API.csproj
```

2. Access the API in your browser or using a tool like `Thunder Client`:
- Local development: http://localhost:5299

## Contributing
Contributions are welcome! If you find any issues or have suggestions for improvements please open an issue or submit a pull request.

1. Fork the repository.
2. Create a new branch for your changes.
3. Make your changes and commit them.
4. Push your changes to your forked repository.
5. Open a pull request.

## License

