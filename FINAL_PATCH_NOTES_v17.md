# FINAL PATCH NOTES v17

## Changes

1. Google Maps short links
- Added backend endpoint `POST /api/map-links/resolve` to resolve Google Maps links, including `maps.app.goo.gl` short links, and extract coordinates from the resolved URL or page body.
- Updated address inputs in:
  - `create-booking`
  - saved addresses
  - provider profile
- The frontend now first parses direct coordinates locally, then falls back to the backend resolver for short links.

2. Register phone confirmation
- New registered users now have `PhoneNumberConfirmed = true` temporarily until OTP verification is implemented as future work.

3. Document notifications
- Added the following notification helper methods:
  - `DocumentLowConfidence`
  - `DocumentExpired`
  - `DocumentExpiringSoon`

4. Service details provider governorate
- Added provider/listing governorate to the public service details response.
- Displayed it in the provider card on the service details page.

5. AI Search reindex
- `POST /api/admin/search/reindex` now syncs both:
  - Active ServiceListings
  - Active MarketplaceListings
- Added marketplace Qdrant indexing into `marketplace_listings` collection.
- Updated marketplace vector search to query the marketplace collection instead of reusing service listing collection.
- Implemented `MarketplaceListingReadRepository` to hydrate active marketplace listings from SQL Server.

## Database
No EF migration was added.

## Local verification needed
Run locally:

```bash
cd Backend
dotnet build HEVEQ.Api/HEVEQ.Api.csproj
```

Frontend dependencies were not available in this environment after extraction, so run locally:

```bash
cd Frontend
npm install
npm run build
```
