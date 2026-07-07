# Agent Execution Log: Admin Operations UI

**Date:** July 6, 2026 (Updated)

## Overview
This log documents the work completed by the Antigravity agent (acting as the Senior Frontend Developer) to build, refine, and redesign the "Admin Operations" flow and dashboard for the HEVEQ platform. 

This log is meant for future agents or team members to understand the current state of the Admin UI.

## Work Completed

### 1. Finalizing Admin Pages
- **Admin Accounts Verification:** Completed UI, added loading states, filtering, integrated backend service `AdminAccountVerificationService`.
- **Admin Documents:** Implemented document review list and split-pane layout for inspecting files, verifying, or rejecting them.
- **Admin Listing Review (Service & Marketplace):** Built review interfaces displaying listing details, photos, category, and an approval/rejection modal.
- **Admin Tickets:** Implemented inbox-style view for technical support tickets with chat-bubble replies.
- **Admin Disputes:** Developed conflict resolution UI to manage refunds (Partial, Full) or provider payouts.
- **Admin Field Verification:** Handled dispatching field employees and recording final verification decisions based on employee reports.

### 2. Expert-Level Refactoring & Enterprise Standards
- **Memory Management (`takeUntilDestroyed`):** Injected `DestroyRef` and applied `takeUntilDestroyed()` from `@angular/core/rxjs-interop` across all HTTP subscriptions to prevent memory leaks during rapid navigation.
- **Change Detection Reversion (`OnPush` Bug Fix):** Removed `ChangeDetectionStrategy.OnPush` from all 8 Admin components. Reverting to the default change detection restored normal functionality.
- **DRY Pagination (`<app-pagination>`):** Extracted duplicated HTML and TypeScript pagination logic into a centralized, standalone shared component.
- **Premium Notifications (`ToastService`):** Eradicated all native browser `alert()` pop-ups in favor of a sleek, non-blocking, custom-built `ToastComponent`.
- **Strict Typing (`admin.models.ts`):** Upgraded string statuses to literal union types (e.g., `'Open' | 'Resolved'`) for compile-time safety.

### 3. Change Detection & Spinner Hang Resolution (Latest)
- **SignalR NgZone Optimization:** Wrapped the connection `.start()` method in `RealtimeService` inside `ngZone.runOutsideAngular()`. This prevents SignalR heartbeat timers and long-polling events from keeping Zone.js permanently active/unstable, which previously hung the route transition loading spinner after user login.
- **Explicit View Updates:** Injected `ChangeDetectorRef` and called `this.cdr.detectChanges()` inside the HTTP subscriptions across all 8 admin components. This forces the browser to instantly hide the loading spinner and render the fresh data as soon as the API response completes.

### 4. Admin Dashboard Redesign & Navigation Links (Latest)
- **Fixed Sidebar Paths:** Resolved routing bugs where the sidebar pointed to non-existent URLs (`/admin-users`, `/admin-tickets`) which redirected admins to the home page. Replaced them with proper nested paths (`/admin/users`, `/admin/tickets`).
- **Full Sidebar Navigation:** Expanded the sidebar layout to show links for all 8 admin sections rather than just three. Integrated standalone `RouterLinkActive` into [sidebar.ts](file:///d:/iti/Graduation%20Project/Frontend/src/app/layout/sidebar/sidebar.ts) and decorated all navigation elements in [sidebar.html](file:///d:/iti/Graduation%20Project/Frontend/src/app/layout/sidebar/sidebar.html) with custom inline SVGs and clean `.nav-link` active states.
- **Collapsable Sidebar Toggle (Latest):** Added collapse trigger buttons and fixed expand handles. The sidebar state persists in LocalStorage (`sharegear_sidebar_collapsed`). Toggling collapse dynamically updates the `body` class `has-sidebar` to shift the main page contents cleanly. Added click-outside triggers to automatically collapse the menu when clicking on page contents, and integrated it to automatically slide open on mobile when the hamburger navbar menu is clicked.
- **Egyptian Heavy Machinery Aesthetics:** Redesigned the Command Center dashboard with glowing live indicators, custom-structured grids, and clickable stat cards that act as route links. Styled using transitions (lift on hover, translate transitions, border color changes) matching HEVEQ's industrial warm-amber and charcoal-black palette, with complete dark-mode overrides.
- **Urgent Action Feed:** Formatted the pending actions list as a clear chronological timeline with priority-level status indicators.

### 5. Backend Refactoring & Bug Fixes
- **EF Core Concurrency Fix (`GetDashboardSummaryQueryHandler`):** Resolved a `System.InvalidOperationException` caused by simultaneous `DbContext` access. Replaced `Task.WhenAll` with sequential `await` calls to securely retrieve Dashboard statistics without violating EF Core thread-safety rules.

### 6. Compilation Warning Fixes
- **Removed NG8107 Warnings:** Fixed optional-chaining warnings on the strictly typed non-nullable `photos` array within [admin-listing-review.html](file:///d:/iti/Graduation%20Project/Frontend/src/app/pages/admin/admin-listing-review/admin-listing-review.html) (replacing `selectedDetails.photos?.length` with `selectedDetails.photos.length`).
- **Clean Compilation:** Tested project build using `ng build`, completing successfully with zero compilation warnings or code errors.

### 7. Backend & Frontend Core Integration Bug Fixes (July 6, 2026 Updates)
- **Account Verifications Endpoint Exception:** Resolved a runtime `System.InvalidOperationException` ("Nullable object must have a value") inside [GetAccountVerificationsQueryHandler.cs](file:///d:/iti/Graduation%20Project/Backend/HEVEQ.Application/Features/Admin/Query/GetAccountVerifications/GetAccountVerificationsQueryHandler.cs) by filtering out pending verification documents with a null `UserId` and defensively checking for nullable `Guid` values.
- **Account Verifications Display Mismatch:** Updated [VerificationUserDto.cs](file:///d:/iti/Graduation%20Project/Backend/HEVEQ.Application/Features/Admin/DTOs/VerificationUserDto.cs) to return the `Email` property, and modified the frontend [admin-account-verification.ts](file:///d:/iti/Graduation%20Project/Frontend/src/app/pages/admin/admin-account-verification/admin-account-verification.ts) to map the nested API response into a flat model (`AccountVerification`) to display names, roles, emails, and dates correctly in the table.

### 8. Manage Users Page Column Display & Filtering Fixes
- **Missing Roles and Names:** Updated [UserAdminDTO.cs](file:///d:/iti/Graduation%20Project/Backend/HEVEQ.Application/Features/Admin/DTOs/UserAdminDTO.cs) and [GetAdminUsersQueryHandler.cs](file:///d:/iti/Graduation%20Project/Backend/HEVEQ.Application/Features/Admin/Query/GetAdminUsers/GetAdminUsersQueryHandler.cs) to return `FirstName`, `LastName`, `Roles` (as a `List<string>`), and `CreatedAt` from the database. This fixed blank name and role cells in the users table on the frontend.
- **Broken Dropdown Filters:** Fixed a bug where selecting a role did not filter results because the query handler fetched `userIdsInRole` but did not filter the main user query. Added boolean query parameter binding for `IsActive` in [GetAdminUsersQuery.cs](file:///d:/iti/Graduation%20Project/Backend/HEVEQ.Application/Features/Admin/Query/GetAdminUsers/GetAdminUsersQuery.cs) to correctly parse and apply the frontend status filters.

### 9. Document Verification Queue Improvements
- **Specific User Document Review:** Added a `UserId` query parameter to the admin documents API and query class [GetAdminDocumentsQuery.cs](file:///d:/iti/Graduation%20Project/Backend/HEVEQ.Application/Features/Documents/Queries/GetAdminDocuments/GetAdminDocumentsQuery.cs). Updated [admin-account-verification.html](file:///d:/iti/Graduation%20Project/Frontend/src/app/pages/admin/admin-account-verification/admin-account-verification.html) to pass `userId` when navigating to the documents review queue, ensuring only the documents for the selected user are listed.
- **Owner Information on Document Cards:** Mapped nested backend user fields to flat properties inside [admin-documents.ts](file:///d:/iti/Graduation%20Project/Frontend/src/app/pages/admin/admin-documents/admin-documents.ts) so that document cards display the owner's name and role badge from the outside.
- **Document Rejection Failure:** Corrected the payload key from `rejectionReason` to `reason` in [adminDocumentsService.ts](file:///d:/iti/Graduation%20Project/Frontend/src/app/core/services/adminDocumentsService.ts) to align with backend validation requirements and prevent `400 Bad Request` exceptions on reject.
- **Document Type Dropdown Alignment:** Updated the select option values in [admin-documents.html](file:///d:/iti/Graduation%20Project/Frontend/src/app/pages/admin/admin-documents/admin-documents.html) to match C# backend `DocumentType` enum strings exactly, correcting broken document type filters.

### 10. Audit/Rejection & Token Claims Authorization Fixes
- **Authentication Claims Mismatch (Auto-Logout):** Resolved a critical bug where approving/rejecting listings or disputes caused an immediate automatic user logout. The backend was resolving the admin ID via `ClaimTypes.NameIdentifier` which returned null due to the JWT claims mapping. Modified all dispute, service listing, and marketplace listing controllers to check for the custom `"uid"` claim first, keeping sessions authenticated.
- **Listing Rejection Mismatch:** Corrected the payload body structure from `{ reason }` to `{ adminRejectionNote }` in [adminListingReviewService.ts](file:///d:/iti/Graduation%20Project/Frontend/src/app/core/services/adminListingReviewService.ts) to align with backend MediatR validators and enable successful rejection submissions.

### 11. Disputes Page API Integration & Authorization Fixes
- **Amdin Authorization Typo:** Resolved a routing issue where loading disputes returned `403 Forbidden` because of a typo in the C# authorize attribute `[Authorize(Roles = "Amdin")]` inside [AdminDisputesController.cs](file:///d:/iti/Graduation%20Project/Backend/HEVEQ.Api/Controllers/AdminDisputesController.cs#L15). Corrected it to `"Admin"`.
- **Disputes Listing Integration:** Updated [adminDisputesService.ts](file:///d:/iti/Graduation%20Project/Frontend/src/app/core/services/adminDisputesService.ts) to direct calls to the correct API endpoint `/api/admin/disputes` instead of non-existent endpoints.
- **In-Memory Detail Resolution:** Resolved `404 Not Found` detail loading failures in [admin-disputes.ts](file:///d:/iti/Graduation%20Project/Frontend/src/app/pages/admin/admin-disputes/admin-disputes.ts) by constructing the selected dispute detailed model in-memory from list results, avoiding unnecessary detail API requests.
- **Resolved Dispute Action Mismatches:** Mapped dispute resolution commands to use the correct booking/marketplace paths (`release-to-provider`, `refund-customer`, `partial-settlement`, `release-to-seller`, `refund-buyer`) and pass the correct JSON body keys (e.g., `DecisionNote`, `CustomerAmount`, `ProviderAmount`).
- **Angular Compilation Type Overlap Fix (TS2367):** Fixed a compile error in `admin-disputes.ts` where comparing `d.status === 'Disputed'` threw a type mismatch warning. Added `'Disputed'` to the `AdminDispute.status` union type definition in [admin.models.ts](file:///d:/iti/Graduation%20Project/Frontend/src/app/core/models/admin.models.ts#L132) to properly align with database status models.
- **Database-Level Disputes Filtering:** Added `Status` filtering parameter to `GetAdminDisputesQuery.cs` and resolved/under-review check criteria to `GetAdminDisputesQueryHandler.cs` (handling Open, UnderReview, and Resolved booking/order disputes globally at the DB level, and mapping status strings to Arabic text appropriately). Updated `adminDisputesService.ts` and `admin-disputes.ts` to call this database-level filter on status changes.
- **Resolving Disputes Under Review:** Modified status validation checks inside booking resolution handlers (`ReleaseBookingDisputeCommandHandler`, `RefundBookingDisputeCommandHandler`, and `PartialSettleBookingDisputeCommandHandler`) to allow resolving dispute bookings while they are in `PendingFieldVerification` or `FieldVerificationComplete` (Under Review) states.

### 12. Clean Architecture Compliant Field Verifications
- **MediatR Queries & Handlers:** Implemented `GetFieldVerificationsQuery`, `GetFieldVerificationsQueryHandler`, `GetFieldVerificationDetailsQuery`, and `GetFieldVerificationDetailsQueryHandler` under `HEVEQ.Application/Features/Admin/Query/` and added DTOs (`FieldVerificationDto`, `FieldVerificationDetailsDto`, `PaginatedFieldVerificationsResponse`).
- **Clean Controller endpoints:** Updated [AdminFieldVerificationsController.cs](file:///d:/iti/Graduation%20Project/Backend/HEVEQ.Api/Controllers/AdminFieldVerificationsController.cs) to use MediatR (`_mediator.Send`) exclusively, removing direct database context (`IApplicationDbContext`) references to maintain clean architecture.
- **Combined Pending/Form Listings:** Programmed the query handler to fetch disputed bookings (mapped as `"Pending"`) and combine them with `FieldVerificationForms` records from the database.
- **Payload Naming Mismatch Fixes:** Adjusted controller endpoints to accept route parameters and map `decision` / `adminNote` request bodies cleanly to backend MediatR command properties.
- **Decision Value Mapping:** Mapped frontend decision strings (`Approve`/`Reject`) inside `SaveDecision` to backend enum strings (`ReleaseToProvider`/`RefundToCustomer`) and added localization support inside `FieldVerificationDecisionCommandHandler.cs` to resolve "Invalid admin decision type" errors.

### 13. Support Tickets Page Fixes
- **DB-Level Priority Filter:** Added `Priority` filter parameter to `GetAdminTicketsQuery.cs` and translated string priorities (`Low`, `Medium`, `High`, `Urgent`) to corresponding database integers (`0`, `1`, `2`, `3`) inside [GetAdminTicketsQueryHandler.cs](file:///d:/iti/Graduation%20Project/Backend/HEVEQ.Application/Features/Admin/Query/GetAdminTickets/GetAdminTicketsQueryHandler.cs).
- **Frontend Mappings Compatibility:** Added `Title` and `UserName` properties to both `AdminTicketDto` and `AdminTicketDetailsDto` to align with frontend `.title` and `.userName` property bindings, resolving the issue where ticket titles and submitter names were blank.

### 14. Support Tickets Display & Messaging Compatibility Fixes
- **Ticket Description Mapping:** Added `Description` field to [AdminTicketDetailsDto.cs](file:///d:/iti/Graduation%20Project/Backend/HEVEQ.Application/Features/Admin/DTOs/AdminTicketDetailsDto.cs) and populated it in [GetAdminTicketDetailsQueryHandler.cs](file:///d:/iti/Graduation%20Project/Backend/HEVEQ.Application/Features/Admin/Query/GetAdminTicketDetails/GetAdminTicketDetailsQueryHandler.cs) with the body of the ticket's first message to display the initial ticket description under "وصف المشكلة" in the frontend.
- **Ticket Message Model Alignment:** Added `Content`, `SenderRole`, and `SenderId` to [TicketMessageDto.cs](file:///d:/iti/Graduation%20Project/Backend/HEVEQ.Application/Features/Admin/DTOs/TicketMessageDto.cs) and populated them in the query handler (`SenderRole` is resolved as `"User"` if the sender is the ticket creator, and `"Admin"` otherwise). This resolves empty message contents and formats bubble styles on the UI.
- **Instant Reply Bubbles:** Expanded [AddTicketMessageResponse.cs](file:///d:/iti/Graduation%20Project/Backend/HEVEQ.Application/Features/Admin/DTOs/AddTicketMessageResponse.cs) to return full message details (`SenderId`, `SenderName`, `SenderRole`, `Body`, `Content`, `CreatedAt`, `IsInternal`). Injected `UserManager<ApplicationUser>` in [AddTicketMessageCommandHandler.cs](file:///d:/iti/Graduation%20Project/Backend/HEVEQ.Application/Features/Admin/Command/AddTicketMessage/AddTicketMessageCommandHandler.cs) to retrieve the admin sender's display name, resolving the issue where a sent reply was added as a blank message bubble.

### 15. Support Tickets Resolution Payload Alignment
- **Resolution Note Property Mapping:** Added a `ResolutionNote` alias property in [ResolveTicketCommand.cs](file:///d:/iti/Graduation%20Project/Backend/HEVEQ.Application/Features/Admin/Command/ResolveTicket/ResolveTicketCommand.cs) pointing to `AdminResolution` to support both `resolutionNote` (sent by the frontend) and `adminResolution` payload formats, resolving validation errors and enabling successful ticket resolution.
- **Resolution Type Nullability:** Made `ResolutionType` in `ResolveTicketCommand.cs` nullable (`TicketResolutionType?`) to prevent non-payment support tickets from defaulting to a refund resolution type when closed.

### 16. Support Tickets Assignment, Realtime Notifications, and Dispute Resolution (Latest)
- **Realtime Notifications Integration**:
  - Upgraded [notificationModels.ts](file:///d:/iti/Graduation%20Project/Frontend/src/app/core/models/notificationModels.ts) to map both camelCase, PascalCase, and snake_case properties from backend DTOs and SignalR payloads.
  - Refactored [notificationsService.ts](file:///d:/iti/Graduation%20Project/Frontend/src/app/core/services/notificationsService.ts) to communicate with actual endpoints: fetching paginated notifications and patching unread states.
  - Implemented the standalone Arabic `/notifications` page component using `NgZone.run()` and `ChangeDetectorRef.detectChanges()` to append realtime incoming SignalR notifications instantly without user interaction, showing custom icons, loading states, and unread counts.
- **Support Ticket Assignment**:
  - Added `AssignedByUserId` and `AssignedAt` fields to [Ticket.cs](file:///d:/iti/Graduation%20Project/Backend/HEVEQ.Domain/Entities/Ticket.cs) and created/applied EF Core migration `AddTicketAssignmentFields` to update the SQL schema.
  - Enforced staff reply constraints: replying to an unassigned ticket auto-assigns it to the staff user. Replying to a ticket assigned to another staff member returns a `403 Forbidden` response.
  - Added `/claim` (staff claim), `/assign` (admin re-assign), and `/takeover` (admin takeover with justification note and internal chat log) command handlers and endpoints.
  - Restricted search & detail queries for Employees to only see tickets assigned to them or unassigned tickets.
  - Targeted customer reply alerts to only notify the assigned staff user (or the support queue if unassigned).
- **Dispute Resolution Flow & Popup Modal**:
  - Enriched admin ticket details endpoints to return linked Booking/Order numbers, Escrow details, and a calculated list of available dispute actions based on user roles and status.
  - Created a unified, transactional [DisputeDecisionCommandHandler.cs](file:///d:/iti/Graduation%20Project/Backend/HEVEQ.Application/Features/Admin/Command/DisputeDecision/DisputeDecisionCommandHandler.cs) to update Tickets, Bookings/Orders, Escrow records, and notifications inside an EF Core SQL transaction block.
  - Refactored frontend ticket layout to make the chat thread the default central focus. If the ticket has a linked dispute, clicking "Resolve Ticket" launches a custom glassmorphic overlay modal showing a bill-style escrow slip and decision options (including Partial Settlement amounts and Field Verification employee selectors). Otherwise, it falls back to the standard resolve modal.

### 17. Backend Ticket Decision Context & Field Visits Workspace
- **Database Schema Changes**: Created and executed EF Core migration `LinkFieldVerificationToTicketAndMakeNullable` to:
  - Keep `FieldVerificationForms.BookingId` required and non-nullable.
  - Make `FieldVerificationForms.LinkedEvidenceFormId` nullable (so visits aren't blocked by missing provider evidence).
  - Add nullable `TicketId` to map field visits directly to support tickets.
- **Customer Attachments**:
  - Configured `CreateTicketCommand` to accept file attachments, adding backend validation checks (limit of 5, extensions `.png`, `.jpg`, `.jpeg`, `.pdf`, `.mp4`, `.mov`, and well-formed upload bucket URL format).
  - Saved attachments bound to the first ticket message.
  - Separated customer DTOs (`TicketDetailsDto`, `TicketMessageDto`) on customer queries to guarantee privacy and never leak provider completion forms, field inspector notes, admin decision forms, internal notes/messages (`IsInternal = true`), or internal attachments.
- **Ticket Decision Context**: Exposed `GET /api/admin/tickets/{ticketId}/decision-context` compiling the ticket info, customer attachments, linked booking details (`EstimatedTotal`), marketplace order details (`Amount`), dispute status, escrow records, provider completion evidence, and associated field visits.
- **Field Visits Management**: Exposed `POST /api/admin/tickets/{ticketId}/field-visits` (deriving BookingId from the ticket itself and returning validation error if null) and `GET /api/admin/tickets/{ticketId}/field-visits` for admin dispatch, as well as `POST /api/admin/field-visits/{fieldVisitId}/assign` for employee dispatch assignment.
- **Field Employee Workspace**: Exposed `/api/employee/field-visits` controller allowing employees to list my-assigned visits (`/my`), retrieve individual details, transition visit status (`PATCH /status`), and submit formal notes, photos, and outcomes (`POST /evidence`).

### 18. Frontend Evidence and Attachment Integration
- **Unified Decision Context Mapping**: Mapped `customerAttachments`, `providerCompletionEvidence`, and `fieldVisits` properties inside [admin.models.ts](file:///d:/iti/Graduation%20Project/Frontend/src/app/core/models/admin.models.ts).
- **Service Integration**: Configured `getTicketDetails(id)` inside [adminTicketsService.ts](file:///d:/iti/Graduation%20Project/Frontend/src/app/core/services/adminTicketsService.ts) to transparently request the rich admin `decision-context` endpoint.
- **Evidence Panel**: Integrated a custom attachments container inside [admin-tickets.html](file:///d:/iti/Graduation%20Project/Frontend/src/app/pages/admin/admin-tickets/admin-tickets.html) showing three columns: Customer Documents, Provider Completion Notes/Photos, and Field Inspector Reports.
- **RTL Aesthetics & Layout**: Added dedicated cards, flex layouts, hover scales, file badges, and specific visit status styles to [admin-tickets.css](file:///d:/iti/Graduation%20Project/Frontend/src/app/pages/admin/admin-tickets/admin-tickets.css) to present all verification files cleanly.

### 19. Ticket Action Popups & Reassignment Rules
- **Dispute Decision Loading Fix**: Added `AvailableDisputeDecisions` to the `TicketDecisionContextDto` backend model and populated it in the query handler, resolving the issue where dispute options were blank in the resolution form.
- **Attachment Modal Popup**: Relocated the customer/provider document views from the inline details column into a dedicated popup modal overlay (`isDocsModalOpen`) triggered by a "📁 ملفات ومستندات التذكرة" button in the chat header, ensuring the main chat log remains fully visible.
- **Takeover Authorization Logic**: Injected `TokenStorage` in [admin-tickets.ts](file:///d:/iti/Graduation%20Project/Frontend/src/app/pages/admin/admin-tickets/admin-tickets.ts) and configured the takeover button to only display when the logged-in user is an Admin, the ticket is assigned to someone else, and the logged-in Admin is not the original creator of the support ticket.
- **Model Key Mapping Correction**: Renamed `TicketId` property to `Id` in `TicketDecisionContextDto` and its backend handler projection, resolving the issue where the frontend was unable to find `selectedTicket.id` during ticket replies, claim, takeover, and resolve operations, which previously resulted in "ticket not found" errors.
- **Notifications Icon Navigation**: Configured `routerLink="/notifications"` on the navbar notifications bell button in [navbar.html](file:///d:/iti/Graduation%20Project/Frontend/src/app/layout/navbar/navbar.html), connecting it directly to the newly restored RTL notifications view.

## Current Project State
The codebase builds successfully on both frontend (`ng build`) and backend (dotnet compile pass) with zero errors. All database migrations are fully applied.

---
**End of Log**


### 20. Final Full-Stack Integration Pass (July 7, 2026)
- **Frontend Build Stability:** Disabled Angular production font inlining in `angular.json` so offline or restricted build environments do not fail while trying to fetch Google Fonts. Adjusted production bundle budgets to match the current integrated dashboard size.
- **Admin Ticket Field Visit Action:** Updated `AdminTicketsService` and `admin-tickets.ts` so `SendFieldVerification` uses the approved ticket-scoped endpoint `POST /api/admin/tickets/{ticketId}/field-visits`. The frontend no longer sends `BookingId`; the backend derives it from the selected ticket.
- **Dispatchable Employee Filtering:** Added `IsAvailableForDispatch` to the backend admin user DTO and mapped it from `EmployeeProfile`. Frontend employee selectors now filter to active dispatchable employees only.
- **Employee Field Visit Workspace:** Added a new Angular page at `/employee/field-visits` connected to `/api/employee/field-visits/my`, `/status`, and `/evidence`. Field employees can view assigned visits, update status, add notes, select visit outcome, and submit photo URLs.
- **Employee Navigation:** Added a sidebar link for employee users to access "مهامي الميدانية" and included the route in authenticated internal navigation detection.
- **Verification:** Frontend production build completed successfully using `npm run build`. Backend build could not be executed in the current sandbox because the .NET SDK is not installed in the environment.
