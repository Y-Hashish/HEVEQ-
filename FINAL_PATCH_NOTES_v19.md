# FINAL PATCH NOTES v19

## Changes

1. Services page
- Removed the large rentals hero/categories intro section.
- Added Egypt's 27 governorates as a typeahead input for the governorate filter.
- The user can type Arabic prefixes such as "قا" and select القاهرة, while the frontend sends the normalized backend value.

2. Marketplace page
- Removed the marketplace hero banner so the page starts directly from filters and listings.

3. Provider bookings tracking
- Added a unified provider bookings page: `/provider-bookings`.
- Added sidebar entry: حجوزات الخدمات.
- The page shows all service bookings in one place with filters:
  - all
  - new requests
  - waiting customer payment
  - active/in progress
  - completed/closed
  - rejected/cancelled
- Provider can accept/reject pending bookings from the same page.
- Added backend endpoint `GET /api/provider/bookings` using the existing `GetProviderBookingsQuery`.

4. Duplicate booking protection
- Backend now prevents a customer from creating another booking on the same service if they already have an open booking on that service.
- Completed, rejected, cancelled, or refunded old bookings do not block future bookings.

5. Booking time overlap protection
- Existing confirmed/active booking overlap checks remain active in both create booking and provider accept flow.
- The UI keeps removing only slots that overlap with blocking bookings for the selected duration.

6. Admin AI review formatting
- Listing review AI flags and recommendation text are now split into clean list items instead of one dense paragraph.

7. Create listing image display order
- Service listing photo display order now increments from the highest existing display order.
- Marketplace listing photo upload also starts from the highest existing display order, not just count.

## Verification

- TypeScript compile check passed:
  `cd Frontend && ./node_modules/.bin/tsc -p tsconfig.app.json --noEmit`

## Notes

- No database migration was added.
- Backend build was not run in this environment because .NET SDK is unavailable.
