# FINAL PATCH NOTES v18

## Booking schedule conflict protection

### Backend
- Added `BookingScheduleConflictHelper` to centralize booking overlap logic and committed booking statuses.
- Added `BookingUnavailableSlotDto` and `BookingCreateContextDto.UnavailableSlots`.
- `GET /api/bookings/create-context/{serviceListingId}` now returns unavailable time intervals for already committed bookings during the next 60 days.
- `POST /api/bookings` now validates that the requested time does not overlap with any committed booking for the same service listing.
- Provider booking acceptance now blocks accepting a pending request if another committed booking already overlaps with the same service listing and time range.
- Operator assignment conflict check remains in place, so both service-level and operator-level conflicts are protected.

### Frontend
- `/create-booking/{serviceListingId}` now removes unavailable slots from the selectable start-time dropdown.
- If a confirmed/active booking exists from 08:00 to 10:00, any new slot that overlaps with that interval is hidden.
- If all valid times are blocked by existing committed bookings, the customer sees an Arabic warning and must choose another day or duration.

## Business rule covered
- Pending requests can still overlap until the provider chooses one.
- Once one request is accepted/committed, accepting any other overlapping request for the same service listing is blocked with a clear Arabic message.
- The backend validates conflicts again during create and accept to prevent stale frontend data from creating conflicts.

## Database
- No migration required.
