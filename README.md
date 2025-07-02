# KzBarry

KzBarry is a REST API template developed in .NET for streamlining the development process. Includes user management and authentication, using Entity Framework Core and JWT for security. The project is designed to be easily deployed and extended.

## Features

- RESTful API endpoints for user management and login
- Role based authentication and authorization with JWT
- SQL Server connection via Entity Framework Core
- Flexible configuration using `appsettings.json`
- Automatic endpoint documentation with Swagger
- Exception handling

## Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local or remote)

## Configuration File (`appsettings.json`)

The `appsettings.json` file contains critical configuration for the API. The most important sections are:

- **Jwt**: Settings for authentication tokens.
  - `Key`: Secret key used to sign JWT tokens. **Must be kept secure and never shared.**
  - `Issuer`: The entity that issues the JWT.
  - `Audience`: The intended audience for the JWT.
  - `ExpiresInMinutes`: How long the JWT access token is valid (in minutes).
  - `RefreshTokenExpiresInDays`: How long refresh tokens are valid (in days).
- **RefreshTokenCleanup**: Background process to clean up expired refresh tokens.
  - `Enabled`: Enables or disables the cleanup process (`true` or `false`).
  - `IntervalMinutes`: How often (in minutes) the cleanup job runs.
- **ConnectionStrings**: Database connection details.
  - `DefaultConnection`: Connection string for SQL Server. Should include server, database name, and authentication details.

**Security Recommendations:**
- Never commit real secrets or credentials to version control. Use `appsettings.Development.json` or environment variables for sensitive data.
- Rotate secrets periodically and restrict access.

Example:
```json
{
  "Jwt": {
    "Key": "SECRET_KEY_THAT_HAS_AT_LEAST_16_CHARACTERS",
    "Issuer": "KzBarry",
    "Audience": "Audience",
    "ExpiresInMinutes": 15,
    "RefreshTokenExpiresInDays": 7
  },
  "RefreshTokenCleanup": {
    "Enabled": true,
    "IntervalMinutes": 60
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Initial Catalog=KzBarry;Integrated Security=True;"
  }
}
```

## Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/kzsoftworks/kztemplate-barrydotnet.git
   cd kztemplate-barrydotnet
   ```

2. Configure the JWT keys in `appsettings.json`:
   ```json
   "Jwt": {
     "Key": "SECRET_KEY_THAT_HAS_AT_LEAST_16_CHARACTERS",
     "Issuer": "KzBarry",
     "Audience": "Audience",
     "ExpiresInMinutes": 15,
     "RefreshTokenExpiresInDays": 7
   }
   ```

3. Complete the [Database Setup](#database-setup) steps before running the project.

## Database Models

The main database entities are:

### User
Represents an application user. Each user can have multiple refresh tokens.

| Field        | Type      | Description                  |
|--------------|-----------|------------------------------|
| Id           | Guid      | Unique identifier (PK)       |
| Email        | string    | User email (unique)          |
| PasswordHash | string    | Hashed password              |
| Role         | enum      | User role (e.g., Admin/User) |
| ...          | ...       | Other user details           |

### RefreshToken
Stores refresh tokens for secure authentication.

| Field      | Type    | Description                              |
|------------|---------|------------------------------------------|
| Id         | Guid    | Unique identifier (PK)                   |
| Token      | string  | Secure random string, the refresh token  |
| Expires    | DateTime| Expiration date/time                     |
| UserId     | Guid    | Foreign key to User                      |

## Database Setup

1. Install SQL Server locally.

2. Create a new database for KzBarry.

3. Configure the connection string in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Initial Catalog=KzBarry;Integrated Security=True;"
   }
   ```
   > ⚠️ Do not commit your real credentials. Use an `appsettings.Development.json` file for sensitive data.

4. Apply database migrations:
   ```bash
   dotnet ef database update
   ```

## Running the Project

To run the project locally:

```bash
dotnet run
```

The API will be available at: `https://localhost:5041` (or the configured port).

## Endpoints

Endpoints can be public, require to be authenticated or even require to be authorized based on the user rol.


Once running, access the Swagger documentation at:

```
https://localhost:5041/swagger
```

### Users
Retrieve the logged user
```bash
GET /api/users/self
```

Retrieve all the users. Admin endpoint
```bash
GET /api/users
```

Retrieve specified user. Admin endpoint
```bash
GET /api/users/{id}
```

Create new user. Admin endpoint
```bash
POST /api/users
```

Modify existing user. Admin endpoint
```bash
PUT /api/users
```

Delete existing user. Admin endpoint
```bash
DELETE /api/users
```

### Auth

Create a new user, returns JWT and refresh token. Public endpoint
```bash
POST /api/auth/register
```

Login, returns JWT and refresh token. Public endpoint
```bash
POST /api/auth/login
```

Refresh JWT using a valid refresh token. Requires refresh token in body and user authentication (token in header).
```bash
POST /api/auth/refresh
```
Body:
```json
{
  "refreshToken": "<refresh_token>"
}
```

Logout, revokes the provided refresh token. Requires refresh token in body and user authentication (token in header).
```bash
POST /api/auth/logout
```
Body:
```json
{
  "refreshToken": "<refresh_token>"
}
```

## Typical Authentication Flow (with curl)

Below is a step-by-step example of how to authenticate, access user data, refresh a token, and logout using curl commands.

### 1. Login (obtain JWT and refresh token)
```bash
curl -X POST https://localhost:5041/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com", "password":"yourPassword"}'
```
**Response:**
```json
{
  "token": "<jwt_token>",
  "refreshToken": "<refresh_token>"
}
```

### 2. Get self (authenticated request)
```bash
curl -X GET https://localhost:5041/api/users/self \
  -H "Authorization: Bearer <jwt_token>"
```

### 3. Refresh JWT using refresh token
```bash
curl -X POST https://localhost:5041/api/auth/refresh \
  -H "Authorization: Bearer <jwt_token>" \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"<refresh_token>"}'
```
**Response:**
```json
{
  "token": "<new_jwt_token>",
  "refreshToken": "<new_refresh_token>"
}
```

### 4. Logout (revoke refresh token)
```bash
curl -X POST https://localhost:5041/api/auth/logout \
  -H "Authorization: Bearer <jwt_token>" \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"<refresh_token>"}'
```

## Architecture

The solution is organized to promote separation of concerns, extensibility, and maintainability. Below are the main architectural components:

### Folder Structure

- `Controllers/` — API controllers (HTTP endpoints)
- `Utils/` — Shared utilities (extensions, filters, helpers, AutoMapper profiles)
- `Models/`
  - `DTOs/` — Data Transfer Objects for requests and responses
  - `Entities/` — Domain entities (mapped to the database)
  - `Enums/` — Enumerations used in the domain
- `Repositories/` — Data access and persistence logic
- `Services/` — Business logic
  - `Background/` — Background services
- `Data/` — Database context and migrations
- `appsettings.json` — Application configuration

### Key Components

- **Exception Filter (`ApiExceptionFilter`)**: Captures and transforms unhandled exceptions into standard, readable HTTP responses. Applied globally to prevent internal details from leaking and to keep error responses consistent.

- **Semi-Automatic Documentation (Swagger)**: Integrated Swagger/OpenAPI generates interactive and semi-automatic documentation for all endpoints. Common response codes and behaviors (such as standard error responses, validation errors, and authorization requirements) are automatically documented and shared across endpoints using custom filters and code conventions. This reduces repetitive work and keeps the documentation consistent and up to date. The Swagger UI allows you to test the API from the browser and visualize contracts and models.

- **Generic Repository**: Implements basic CRUD methods and reduces code duplication in specific repositories. Facilitates code reuse and unit testing.

- **Background Services**: Example: `RefreshTokenCleanupService`, which periodically cleans up expired refresh tokens. Fully configurable via `appsettings.json`.

- **Dependency Injection**: All services, repositories, and utilities are registered in the .NET DI container, enabling easy testing, extensibility, and decoupling.

- **Flexible Configuration**: Uses `appsettings.json` to define JWT keys, connection strings, background job intervals, etc. Behavior can be changed without recompiling.

- **Auto Auditing for Entities**: Entities that implement the auditing interface automatically track `CreatedAt` and `UpdatedAt` timestamps. The `DataContext` applies these values on insert and update operations, ensuring consistent audit data across the system without requiring manual handling in business logic.

- **AutoMapper**: Used to map between entities and DTOs, simplifying data transformation between layers.

This architecture allows the solution to scale, add new features, and maintain best practices for modern .NET API development.
