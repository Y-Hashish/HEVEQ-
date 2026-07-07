# AI Context: HEVEQ Backend Interface

This index acts as a prompt-priming context file for AI coding assistants modifying the HEVEQ application stack.

## Architecture Guidelines
- **Patterns**: Strict Clean Architecture (Presentation, Application, Domain, Infrastructure).
- **CQRS**: Feature folders containing command, queries, handlers, validators, and DTO definitions. Do not place business logic directly in controllers.
- **Validation**: Enforced via FluentValidation. All incoming REST commands must map to a validator.

## Naming Conventions
- **Controllers**: Named `<Entity>Controller.cs` (e.g. `BookingsController`). If admin-only, prefix with `Admin` (e.g. `AdminTicketsController`).
- **Commands/Queries**: Named `<Action><Entity>Command` or `Get<Detail>Query`.
- **Database Tables**: Pluralized naming (e.g. `Bookings`, `EscrowRecords`), mapped explicitly in Configurations.

## Important Constants & Enums
- **Platform Fee**: Configured under `PlatformCommissionRate = 0.10m` (10%).
- **Booking Status Transitions**:
  `Draft` -> `PendingProviderResponse` -> `ConfirmedPendingPayment` -> `Active` -> `InProgress` -> `PendingCustomerConfirmation` -> `Completed`.
- **Disputed Escrow Flow**: Dispute triggers `EscrowStatus.Frozen`. Admin resolution updates status to `ResolvedReleased` (released to provider) or `ResolvedRefunded` (returned to customer).

## Common Pitfalls & Code Warnings
- **Spatial Geometry**: In spatial entities (`Address`, `Booking`), Entity Framework coordinates must use the `Point` geometry type from `NetTopologySuite.Geometries`.
- **Typo Directories**: Note that the Auth feature command folder has a typo `HEVEQ.Application/Features/Auth/Commad` (instead of Command), and registration is named `Regiser` (instead of Register). Do not attempt to rename these folders without refactoring all namespace imports.
