# FINAL PATCH NOTES v9

## Fixes included

1. Admin ticket AI summary no longer covers the ticket chat
   - Removed the inline AI summary panel from the chat body.
   - Added a header action button: `🤖 ملخص الذكاء الاصطناعي`.
   - The AI summary now opens in a modal popup and can be closed without blocking chat usage.

2. Service booking reviews now appear on service details pages
   - `GET /api/reviews/service-listing/{serviceListingId}` now returns published booking reviews in two cases:
     - Reviews with `Review.ServiceListingId == serviceListingId`.
     - Older/newly created reviews where `Review.ServiceListingId` is null but `Review.Booking.ServiceListingId == serviceListingId`.
   - This fixes the case where a review created for booking `00000000-0000-0000-0000-000000007002` did not appear on service `00000000-0000-0000-0000-000000005002`.

3. Future submitted reviews now store listing IDs correctly
   - Booking reviews now save `ServiceListingId` from the booking.
   - Marketplace order reviews now save `MarketplaceListingId` from the order listing.

## Build verification

Frontend build was run successfully:

```bash
cd Frontend
npm run build
```

## Backend verification required locally

The environment used for patching does not include .NET SDK, so please run locally:

```bash
cd Backend
dotnet build HEVEQ.Api/HEVEQ.Api.csproj
dotnet run --project HEVEQ.Api
```

No database migration was added in this patch.
