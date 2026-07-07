# HEVEQ Final Patch Notes v4

## Marketplace Order Completion and Seller Earnings
- Updated customer receipt confirmation flow for marketplace orders.
- Added explicit customer endpoint:
  - `POST /api/marketplace-orders/{id}/buyer-confirm-receipt`
- Kept the older endpoint as a backward-compatible alias:
  - `POST /api/marketplace-orders/{id}/complete`
- Both endpoints are restricted to `Customer` role.
- When the customer confirms receipt:
  - Order status becomes `Completed`.
  - `ConfirmedByBuyerAt` is set.
  - The active held escrow record is released immediately.
  - `EscrowRecord.Status` becomes `Released`.
  - `ReleasedAt` is set.
  - Seller receives marketplace completion and escrow release notifications.
- This makes the released payout appear in provider earnings.

## Marketplace Order Role Separation
- Added role restriction to seller-only operations:
  - `POST /api/marketplace-orders/{id}/seller-confirm` => Provider
  - `POST /api/marketplace-orders/{id}/dispatch` => Provider
  - `POST /api/marketplace-orders/{id}/deliver` => Provider
- Customer receipt confirmation is now separated from seller operations.

## Provider Earnings
- Fixed `ProviderEarningsController` malformed using statements.
- Added endpoint:
  - `GET /api/provider/earnings/marketplace-summary?from=YYYY-MM-DD&to=YYYY-MM-DD`
- Updated `/earnings` frontend page to load both:
  - Service earnings
  - Marketplace earnings
- Marketplace earnings now shows held amount and released amount, so confirmed marketplace orders appear as released seller payouts.

## Marketplace Sales UI
- Redesigned `/marketplace-sales` to match the same visual structure as `/bookings` and `/marketplace-orders`:
  - Left order list
  - Right details panel
  - Status badges
  - Details grid
  - Timeline section
  - Escrow/payment section
  - Action sections for confirm, dispatch, deliver, and cancel
- Fixed frontend status fallbacks to match backend enum values:
  - `PaymentCaptured`
  - `SellerConfirmed`
  - `Dispatched`

## Frontend Verification
- Ran:
  - `cd Frontend`
  - `npm install --no-audit --no-fund`
  - `npm run build`
- Angular build completed successfully.

## Backend Verification
- Dotnet SDK is not available in this execution environment, so backend build could not be run here.
- Please run locally:
  - `cd Backend`
  - `dotnet build HEVEQ.Api/HEVEQ.Api.csproj`
  - `dotnet ef database update --project HEVEQ.Infrastructure --startup-project HEVEQ.Api`
  - `dotnet run --project HEVEQ.Api`
