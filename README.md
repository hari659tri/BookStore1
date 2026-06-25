# Book Store Management System

Production-style full stack Book Store Management System built as a Coforge trainee learning project.

## Tech Stack

- ASP.NET Core Web API targeting .NET 8
- Entity Framework Core Code First with SQL Server
- ASP.NET Core Identity, JWT authentication, role-based authorization
- Clean Architecture with Domain, Application, Infrastructure, and API projects
- React, Vite, TypeScript, Redux Toolkit, Axios, React Router
- Swagger/OpenAPI, Serilog, FluentValidation, AutoMapper

## Features

- Register and login with JWT
- Admin and Customer roles
- Book, author, and category management
- Search, filter, sort, and paginate books
- Customer cart and checkout
- Order history and admin order management
- Normalized SQL Server schema with seed data

## Default Users

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@bookstore.local` | `Admin@12345` |
| Customer | `customer@bookstore.local` | `Customer@12345` |

Change these credentials before any real deployment.

## Run Locally

1. Install .NET 8 SDK.
2. Install Node.js 20.19+ or 22.12+.
3. Start SQL Server.
4. Update `src/BookStore.Api/appsettings.json` if your SQL Server name is different.
5. Restore and create the database:

```powershell
dotnet restore
dotnet ef database update --project src/BookStore.Infrastructure --startup-project src/BookStore.Api
dotnet run --project src/BookStore.Api
```

6. Start the React app:

```powershell
cd client
npm install
copy .env.example .env
npm run dev
```

Swagger runs at `https://localhost:7287/swagger` in development.

## Project Structure

```text
src/
  BookStore.Domain/
  BookStore.Application/
  BookStore.Infrastructure/
  BookStore.Api/
tests/
  BookStore.UnitTests/
  BookStore.IntegrationTests/
client/
  src/
docs/
```

## Deployment

The intended deployment path is Azure App Service for the API, Azure SQL Database for data, and Azure Static Web Apps for the React client. See `docs/deployment-guide.md`.
