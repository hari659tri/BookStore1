# Deployment Guide

## Azure SQL

1. Create Azure SQL Server and Azure SQL Database.
2. Allow Azure services and your local IP during setup.
3. Store the connection string as an App Service setting named `ConnectionStrings__DefaultConnection`.
4. Run migrations from CI/CD or locally against the Azure SQL connection.

## API: Azure App Service

1. Create an App Service targeting .NET 8.
2. Configure:
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `Jwt__Issuer`
   - `Jwt__Audience`
   - `Jwt__Secret`
   - `AllowedOrigins__0`
3. Publish `src/BookStore.Api`.
4. Confirm `/swagger` is disabled or restricted if required by the deployment policy.

## Frontend: Azure Static Web Apps

1. Build from `client`.
2. Set `VITE_API_BASE_URL` to the production API URL ending in `/api/v1`.
3. Deploy `client/dist`.

## GitHub

Initialize and push when credentials are available:

```powershell
git init
git add .
git commit -m "Initial Book Store Management System"
gh repo create book-store-management --public --source . --remote origin --push
```
