# HEVEQ Final Patch Notes v8

## Admin Document Review
- Added `adminNote` support on the frontend document review screen.
- AI/OCR notes now appear inside the document details panel.
- If no AI admin note exists, the OCR failure reason is shown when available.

## Admin Tickets
- Added AI ticket summarization fields to admin ticket decision context:
  - `aiSummary`
  - `aiIdentifiedIssue`
  - `aiClaimedImpact`
  - `aiEscalationPriority`
- The admin ticket details page now displays the AI summary panel above the original customer description.

## Admin Listing Review
- Fixed service listing provider data.
- The backend no longer treats `ProviderProfileId` as the user id.
- Service listing review details now load provider company name, email, and phone from `ProviderProfile.User`.
- Marketplace listing review details are normalized in the frontend so seller data appears in the same provider information card.

## Admin Disputes
- Changed dispute currency display from SAR to Egyptian Pounds.
- The admin dispute list and details now show `ج.م` for disputed amounts.

## Reviews Flow
- Added service-listing-specific public reviews endpoint:
  - `GET /api/reviews/service-listing/{serviceListingId}`
- This returns only published reviews connected to completed booking reviews for that service listing.
- Service details page now displays booking reviews for the selected service.

## Marketplace Seller Reviews
- Added marketplace seller reviews endpoint:
  - `GET /api/reviews/marketplace-seller/{sellerId}`
- This returns only published reviews from marketplace orders for that seller.
- Product details page now displays reviews from previous marketplace orders for the same seller, so buyers can evaluate the seller even if the exact sold product is no longer listed.

## Frontend Build
- Verified with:
  - `npm run build`

## Backend Build
- Not verified in this environment because .NET SDK is unavailable.
- Please run locally:
  - `dotnet build HEVEQ.Api/HEVEQ.Api.csproj`
  - `dotnet run --project HEVEQ.Api`

## Database Changes
- No new migration added in this patch.
