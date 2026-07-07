# HEVEQ Final Patch Notes v6

## Admin Dashboard
- Fixed `NaN` in open disputes by normalizing dashboard summary values in Angular.
- Added backend dashboard counters for `DisputedMarketplaceOrders` and `PendingFieldVerifications`.
- Removed the AI high risk card from the admin dashboard.
- Fixed dashboard pending action routes so listing review notifications open the correct review page with query params.

## Employee Dashboard Behavior
- `/admin` no longer calls admin-only summary endpoints for employee users.
- Field verification employees see only their assigned field visit tasks and a shortcut to `/employee/field-visits`.
- Support employees see only support ticket tasks and a shortcut to `/admin/tickets`.
- `/Auth/me` now returns `employeeDepartment` and `isAvailableForDispatch` so the frontend can choose the correct employee workspace.

## Notifications Navigation
- Notification clicks now navigate to the related page instead of only marking as read.
- Added route mapping for bookings, marketplace orders, tickets, field verifications, service listing reviews, marketplace listing reviews, documents, messages, escrow, and earnings.
- Admin listing review supports direct opening via query params: `/admin/listing-review?type=service&id=...` and `/admin/listing-review?type=marketplace&id=...`.
- Bookings, marketplace orders, marketplace sales, admin tickets, and employee field visits now support query-param deep links.

## Navbar Dropdown
- Removed Reviews from the user popup.
- Removed Support from the user popup because it is available in the sidebar.
- Account verification now appears only for customer/provider roles, not admin or employee.

## Arabic Error Handling
- Added frontend error translation for common backend English messages.
- Added backend Arabic error response filter and mapper for action results and exception middleware responses.
- Validation and common error responses are normalized to Arabic where possible.

## Build Verification
- Frontend build passed with `npm run build`.
- .NET SDK is not available in this environment, so run `dotnet build` locally.
