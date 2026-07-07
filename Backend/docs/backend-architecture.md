# Backend Architecture & Technology Stack

The HEVEQ backend utilizes an ASP.NET Core Clean Architecture layout. This separates the business domain, application use cases, external infrastructure dependencies, and presentation interfaces.

```
┌────────────────────────────────────────────────────────┐
│                      HEVEQ.Api                         │
│   (Controllers, SignalR Hubs, Middleware, Web Root)    │
└───────────┬────────────────────────────────┬───────────┘
            │                                │
            ▼                                ▼
┌────────────────────────────────────────────────────────┐
│                  HEVEQ.Application                     │
│    (CQRS features, MediatR, DTOs, FluentValidation)    │
└───────────┬────────────────────────────────┬───────────┘
            │                                │
            ▼                                ▼
┌─────────────────────────┐      ┌───────────────────────┐
│     HEVEQ.Domain        │      │  HEVEQ.Infrastructure │
│  (Entities, Enums)      │◄─────┤ (EF Core, Stripe, AI) │
└─────────────────────────┘      └───────────────────────┘
```

## Folder Structure & Layers

### 1. Presentation Layer (`HEVEQ.Api`)
Contains Web API controllers, custom middlewares, and real-time SignalR hubs.
- `Controllers/`: Exposes REST endpoints grouped by business concern (e.g. `BookingsController`, `AdminTicketsController`, `PublicServiceListingsController`).
- `Middleware/`: Global `ExceptionHandlingMiddleware` intercepts runtime exceptions and formats standard JSON error payloads.
- `Realtime/`: Houses `RealtimeHub` and SignalR services for broadcasting message events and notifications.
- `Requests/`: Standard HTTP models binding request payloads that are subsequently mapped into MediatR commands.

### 2. Application Layer (`HEVEQ.Application`)
Orchestrates application behavior using MediatR.
- `Features/`: Feature folders (e.g., `Bookings`, `Search`, `ServiceListings`, `Auth`) containing:
  - `Commands/` & `Queries/`: MediatR request structures.
  - `Handlers/`: Domain logic execution.
  - `Validators/`: Command validations using FluentValidation.
  - `DTOs/`: Response projections.
- `Common/`: Infrastructure interfaces (`IApplicationDbContext`, `ICurrentUserService`, `IJwtService`, `IPaymentCheckoutService`), mapping profiles (AutoMapper), custom exceptions, and scheduling parameters.

### 3. Domain Layer (`HEVEQ.Domain`)
Self-contained, containing no dependencies on other layers.
- `Entities/`: Contains EF Core data models (e.g., `Booking`, `EscrowRecord`, `CustomerProfile`).
- `Enums/`: Domain-specific enumerations (e.g., `BookingStatus`, `EscrowStatus`, `TrustLevel`).
- `Identity/`: Contains `ApplicationUser` extending `IdentityUser<Guid>`.

### 4. Infrastructure Layer (`HEVEQ.Infrastructure`)
Contains concrete implementations of application interfaces.
- `Persistence/`:
  - `ApplicationDbContext`: Implements `IApplicationDbContext`.
  - `Configurations/`: Module-specific EF Core mappings (fluent configuration, indexes, relationships).
  - `Qdrant/`: Concrete client syncing service listings to the vector index.
- `AI/`:
  - `Search/`: Query expansion and semantic matching using Semantic Kernel.
  - `PostRejectionReengagement/`: AI agent suggesting alternative listings.
- `Payments/Stripe/`: Implements checkout session generation and transaction capture.
- `Services/`: Concrete services for Email (SMTP), background scheduling (Hangfire), and storage (Azure/Local File System).

## Data Flow & Architecture Patterns
1. **CQRS with MediatR**: The presentation layer sends commands/queries to MediatR. Command/Query handlers encapsulate the transaction lifecycle.
2. **Global Exception Handling**: Returns appropriate JSON objects containing HTTP status codes corresponding to the exception type:
   - `InvalidOperationException` / `ValidationException` -> `400 Bad Request`
   - `UnauthorizedAccessException` -> `401 Unauthorized`
   - `KeyNotFoundException` -> `404 Not Found`
   - Uncaught Exception -> `500 Internal Server Error`
3. **FluentValidation Integration**: Executes before commands reach their handlers via a pipeline behavior.
4. **Database Spatial Extensions**: Utilizes `NetTopologySuite` geography types for proximity calculations (e.g., matching provider service zones to site coordinates).
