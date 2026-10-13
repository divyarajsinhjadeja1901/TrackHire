# TrackHire — Job Application Management System

A full-stack web application to manage and track job applications built with ASP.NET Core Web API and vanilla JavaScript.

## Tech Stack
- **Backend:** ASP.NET Core Web API (.NET 8)
- **Database:** SQL Server + Entity Framework Core 8
- **Auth:** JWT Bearer Token Authentication
- **Frontend:** Vanilla JavaScript, HTML5, CSS3
- **Architecture:** Repository + Service Pattern

## Features
- User registration and login with JWT authentication
- Add, view, update and delete job applications
- Track application status (Applied, Interview, Offer, Rejected, Withdrawn)
- Filter applications by status
- Status history tracking
- Dashboard with application statistics
- Per-user data isolation (users only see their own data)

## API Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | /api/auth/register | Register new user | No |
| POST | /api/auth/login | Login and get token | No |
| GET | /api/applications | Get all applications | Yes |
| POST | /api/applications | Add new application | Yes |
| GET | /api/applications/{id} | Get single application | Yes |
| PATCH | /api/applications/{id}/status | Update status | Yes |
| DELETE | /api/applications/{id} | Delete application | Yes |
| POST | /api/applications/{id}/notes | Add note | Yes |

## Project Structure
TrackHire.API/
├── Controllers/    → API route handlers
├── Services/       → Business logic layer
├── Repositories/   → Data access layer
├── Models/         → Database entities
├── DTOs/           → Request/Response shapes
├── Helpers/        → JWT token generation
├── Data/           → EF Core DbContext
└── wwwroot/        → Frontend (HTML, JS, CSS)

## Setup Instructions
1. Clone the repository
2. Update connection string in `appsettings.json`
3. Run `dotnet ef database update`
4. Run `dotnet run`
5. Open `https://localhost:xxxx` in browser

## Author
Divyarajsinh Jadeja
[GitHub](https://github.com/divyarajsinhjadeja1901)


## Testing

Tests use xUnit, Moq, and the EF Core InMemory provider, and run automatically on every push through GitHub Actions.

```
dotnet test --collect:"XPlat Code Coverage"
```

- **38 tests, all passing**, covering services, repository, auth, JWT, and controllers
- **75.1% line coverage** (263 of 350 lines; generated EF Core migrations excluded)
- Per-user data isolation is verified by automated tests

[![CI](https://github.com/divyarajsinhjadeja1901/TrackHire/actions/workflows/ci.yml/badge.svg)](https://github.com/divyarajsinhjadeja1901/TrackHire/actions)

## Running locally

The JWT signing key is not stored in the repo. Set your own before running:

```
cd TrackHire.API
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "<any random string of 32+ characters>"
```