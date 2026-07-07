# FINAL PATCH NOTES v10

## Backend error handling
- Added Arabic handling for `InvalidOperationException` and `ArgumentException` in `ExceptionHandlingMiddleware`.
- Backend now returns HTTP 400 with a clear Arabic message for business rule violations instead of returning the generic unexpected-error message.
- Added Arabic mappings for booking availability errors, address errors, minimum booking duration errors, out-of-zone errors, and service listing status errors.
- Updated the optional `GlobalExceptionHandler` fallback with Arabic details as a defensive change.

## Frontend error handling
- Extended `getErrorMessage` mappings so common backend business-rule messages are displayed in Arabic.
- Fixed extraction of `exceptionMessage` when the backend response also contains the generic Arabic message.

## Create booking scheduling UX
- Replaced the free `type=date` and `type=time` selection with a guided availability calendar.
- The customer can only select days that match the service listing availability.
- Time selection now shows only valid start times inside the provider's open/close window.
- Time slots are calculated using the selected duration, so selecting 16:00 for a 4-hour booking on an 08:00-17:00 day is blocked.
- If the customer changes duration and the selected time becomes invalid, the time is cleared and an Arabic warning is shown.

## Build check
- Frontend build was verified with Angular CLI after using the cached node_modules from the previous workspace.
- .NET build was not run because the current environment does not include the .NET SDK.
