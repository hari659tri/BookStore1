# API Testing Guide

Run the API and open Swagger at `https://localhost:7287/swagger`.

## Authentication

1. `POST /api/v1/auth/login`
2. Use `admin@bookstore.local` / `Admin@12345`
3. Copy the token from the response.
4. Click Swagger `Authorize` and paste the token.

## Smoke Test Flow

1. `GET /api/v1/books`
2. `POST /api/v1/books` as Admin.
3. Login as `customer@bookstore.local`.
4. `POST /api/v1/cart/items`
5. `GET /api/v1/cart`
6. `POST /api/v1/orders`
7. `GET /api/v1/orders`
8. Login as Admin and run `GET /api/v1/admin/orders`.

## Expected Security

- Anonymous users can read books, authors, and categories.
- Customers can use cart and order endpoints.
- Admin-only endpoints reject customer tokens with `403`.
- Missing or invalid JWT tokens return `401`.
