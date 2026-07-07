# Authentication & Authorization Architecture

HEVEQ employs token-based security via JSON Web Tokens (JWT) and ASP.NET Core Identity.

## Identity Configuration
- **ApplicationUser**: In `ApplicationUser.cs`, the user extends `IdentityUser<Guid>` to support unique identifiers globally.
- **Roles**: The system seeds 4 standard roles (`IdentitySeeder.cs`):
  1. `Admin`
  2. `Customer`
  3. `Provider`
  4. `Employee` (Used for Operators and Field agents)

## Authentication Flow

```
  Customer/Provider              HEVEQ.Api (Auth)               Stripe / Database
         │                              │                               │
         ├────── POST /login ──────────►│                               │
         │                              ├────── Validate & Query ──────►│
         │◄───── JWT + Refresh ─────────┤                               │
         │                              │                               │
         ├────── Auth Header ──────────►│                               │
         │   (Bearer <JWT Token>)       ├────── Check Claims ──────────►│
```

1. **User Sign In / Sign Up**:
   - Route: `/api/auth/register` (Registers as `Customer` or `Provider`).
   - Route: `/api/auth/login` (Returns access token and refresh token).
2. **Access Token Details**:
   - Format: Standard JWT Bearer token.
   - Lifetime: Defined in configuration.
   - Claims embedded:
     - `ClaimTypes.NameIdentifier` (User ID / Guid)
     - `ClaimTypes.Email`
     - `ClaimTypes.Role` (e.g. `Customer`, `Provider`, `Admin`)
3. **Token Rotation**:
   - Route: `/api/auth/refresh-token` validates the refresh token stored in the `RefreshTokens` database table, rotating it to generate a new JWT access token.
4. **Current User Context**:
   - Injected in handlers via `ICurrentUserService`. Resolves claims from `HttpContext.User`.

## Authorization System
- **Role-based Attributes**: Implemented using standard ASP.NET `[Authorize(Roles = "Customer")]` attributes on controller actions.
- **AllowAnonymous**: Used on public discovery endpoints like `/api/public/search/` and `/api/public/service-listings`.
- **SignalR Hub Auth**:
  - The SignalR hub endpoint `/hubs/realtime` extracts the JWT from the `access_token` query parameter during the handshake if it cannot be passed in standard headers:
  ```csharp
  var accessToken = context.Request.Query["access_token"];
  ```
