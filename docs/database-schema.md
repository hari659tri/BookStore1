# Database Schema

The database is created with EF Core Code First.

```mermaid
erDiagram
    USERS ||--o{ CARTS : owns
    USERS ||--o{ ORDERS : places
    ROLES ||--o{ USER_ROLES : contains
    USERS ||--o{ USER_ROLES : assigned
    AUTHORS ||--o{ BOOKS : writes
    CATEGORIES ||--o{ BOOKS : classifies
    CARTS ||--o{ CART_ITEMS : contains
    BOOKS ||--o{ CART_ITEMS : added
    ORDERS ||--o{ ORDER_ITEMS : contains
    BOOKS ||--o{ ORDER_ITEMS : ordered
```

Core tables:

- `Users`, `Roles`, `UserRoles`, plus standard Identity claims/login/token tables.
- `Authors`, `Categories`, `Books`.
- `Carts`, `CartItems`.
- `Orders`, `OrderItems`.

Important constraints:

- Unique book ISBN.
- Unique category name.
- Unique order number.
- One cart per user.
- Decimal precision `18,2` for money.
- Restricted deletes for book history and cascade deletes for cart/order child rows.
