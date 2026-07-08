# FINAL PATCH NOTES v14

## Scope
This patch is based on the latest project version and addresses the requested service details UI, service listing edit/re-review flow, and favicon usage guidance.

## Changes Applied

### 1. Service details page image/gallery design
- Updated `/service-details/{serviceId}` to use the same layout pattern as `/product-details/{id}`.
- Added main image preview, thumbnail gallery, no-image fallback, sidebar price/action card, provider card, specs, availability, operators, and reviews sections.
- Fixed the service details image issue by normalizing backend photo responses that may arrive as either plain URL strings or photo objects.

### 2. Public service listing detail backend cleanup
- Removed the direct `try/catch` from `PublicServiceListingsController` that exposed English messages and stack traces.
- Public service listing errors now flow through the global middleware, with a safe Arabic not-found response for unavailable/unapproved services.
- Public service details now return real availability slots instead of an empty list.
- Review aggregation includes old reviews saved against `BookingId` only, as long as the booking belongs to the same service listing.

### 3. Service listing edit and re-review flow
- Fixed the frontend edit flow bug in `create-listing.ts`.
- Saving service edits now keeps the listing editable and allows the provider to move through the wizard.
- Any provider edit to an Active, Rejected, or PendingReview service listing moves it back to Draft.
- The provider must explicitly click `إرسال للمراجعة`.
- On submit, the listing becomes PendingReview and service AI moderation is re-run for the admin report.
- Photo, operator, and availability changes also move the service listing back to Draft so it can be submitted again for admin/AI review.

## Favicon
No favicon file was modified by code in this patch. Follow the written instructions in the chat response to place your `.ico` manually.

## Validation
- Ran TypeScript check successfully:
  - `cd Frontend`
  - `./node_modules/.bin/tsc -p tsconfig.app.json --noEmit`
- `ng build` was started but timed out in the sandbox environment. No TypeScript errors were found.
- .NET SDK is not available in the sandbox, so run the backend build locally.

## Local Checks Required
```bash
cd Backend
dotnet build HEVEQ.Api/HEVEQ.Api.csproj
dotnet run --project HEVEQ.Api
```

```bash
cd Frontend
npm ci
npm run build
npm start
```
