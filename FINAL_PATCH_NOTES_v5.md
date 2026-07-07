# FINAL PATCH NOTES v5

## Admin Listing Approval Fix

### Service listing approval
- Fixed `ApproveServiceListingCommandHandler` so admin approval checks the real linked data instead of relying on AI quality fields.
- Approval now loads:
  - Photos
  - Availability
  - ServiceListingOperators and Operator
- Minimum approval requirements are now:
  - At least 3 photos
  - At least 1 active operator
  - At least 1 availability schedule
- The old bug was caused by checking `QsOperator > 0`, which can be null even when the provider linked an operator.

### Marketplace listing approval
- Updated marketplace approval to require at least 3 photos, matching the frontend publishing rule.

### Admin frontend error display
- Admin listing review now shows the real backend error message in the toast instead of the generic message.

## Build check
- Frontend build was verified successfully with `npm run build`.
- Backend build still needs to be verified locally with .NET SDK.
