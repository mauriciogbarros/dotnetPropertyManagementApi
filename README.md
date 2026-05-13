# dotnetPropertyManagementAPI
A role-based property management API, built with .NET and Clean Architecture principles.

This API allows **managers** to manage property **units** and **tenants**.

#### Motivation
This is a portfolio project demonstrating API design, authentication, authorization, and data modeling using .NET and PostgreSQL.

## Key Features
### Manager Capabilities
- Create new **users** (**tenants** or **managers**);
- View information about **tenants** and **units**;
- Update their own information, a **unit**;
- Deactivate a **tenant** or a **unit**.

### Tenant Capabilities
- View their own information and the information of their **unit**;
- Update their own information;