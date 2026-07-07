# HEVEQ Final Complete Project - Integration Patch v2

## Scope of this patch
This version is based on the last complete integrated project and adds the missing final-flow fixes requested during testing.

## Frontend changes

### Auth
- Added `phoneNumber` to the register request model and register page form.
- Updated register success handling to support the backend response when email confirmation is required.

### Marketplace order payment
- After creating a marketplace order, the customer is redirected directly to the marketplace order payment page:
  - `/marketplace-orders/{id}/payment`
- Added a dedicated marketplace order payment page that loads order details, escrow details, and starts Stripe checkout or mock confirmation.
- Updated the payment success page to return to marketplace orders when the payment is for a marketplace order.

### Conversations and realtime
- Added chat start actions on public product details and service details pages.
- Product details can start a conversation with the seller before purchase.
- Service details can start a conversation with the provider before booking.
- The messages page now accepts `conversationId` in the query string and opens the requested conversation automatically.
- Added provider sidebar access to conversations.
- App startup now connects SignalR realtime after login and disconnects on logout.
- Navbar notification icon now displays the unread notification count.

### Loading state refresh fix
- Added an HTTP UI refresh interceptor to force Angular to refresh after HTTP completion. This fixes pages that stayed on `جاري التحميل...` until the user clicked the screen.

### Provider earnings
- The earnings date filter now defaults to the beginning of the current year and ends at the current date.

### Image uploads
- Service listing photos now use file upload through `/api/media/images`, then save the returned Cloudinary URL.
- Marketplace listing photos now use multi-file upload through `/api/media/images`, then save returned Cloudinary URLs.
- The old manual image URL textarea was removed from the create listing UI.

### Marketplace listing create and edit
- Marketplace listing creation requires at least 3 uploaded images before submission to avoid unintentional draft status.
- Added edit support for marketplace listings from the provider equipment page.
- Draft marketplace listings can now be opened and edited.
- Existing marketplace listing photos appear during edit and can be removed.
- New selected marketplace photos are shown before submission.

## Backend changes

### Marketplace listing details
- Extended `MarketplaceListingDetailsDto` with fields required for editing:
  - `CategoryId`
  - `YearOfManufacture`
  - `IsNegotiable`
  - `TransactionMethod`
  - `Governorate`
  - `District`
  - `Status`
  - `StatusAr`
  - `VideoUrl`

### Marketplace listing update behavior
- `UpdateMarketplaceListingCommandHandler` now loads listing photos.
- `Linked photo count >= 3` determines whether a non-active edited listing can move to `PendingReview`.
- If there are fewer than 3 photos, the listing remains `Draft`.
- Active listings still move to `PendingReview` after updates.
- AI moderation is only queued when AI-sensitive fields changed and the listing is ready for review.

## Validation performed

### Frontend
Executed successfully:

```bash
cd Frontend
npm install --no-audit --no-fund
npm run build
```

### Backend
Backend build was not executed in this environment because the .NET SDK is not installed here.
Please run locally:

```bash
cd Backend
dotnet build HEVEQ.Api/HEVEQ.Api.csproj
dotnet ef database update --project HEVEQ.Infrastructure --startup-project HEVEQ.Api
```

## Final test checklist

1. Register as customer/provider/admin/employee and verify phone number validation.
2. Login and verify realtime notifications connect without page refresh.
3. Provider creates service listing and uploads actual image files.
4. Provider creates marketplace listing with fewer than 3 images and verify frontend blocks submission.
5. Provider creates marketplace listing with 3 images and verify listing is saved with uploaded Cloudinary URLs.
6. Provider opens equipment page and edits draft marketplace listing.
7. Customer opens product details and starts chat with seller.
8. Customer creates marketplace order and verify redirect to payment page.
9. Customer completes marketplace payment and returns to marketplace orders.
10. Customer opens service details and starts chat with provider.
11. Provider opens conversations and sees their conversations.
12. Provider opens earnings and verifies date range starts at Jan 1 of current year.
13. Open previously stuck pages and verify loading clears without clicking the screen.
14. Admin ticket decision context and field visits remain available from the previous integration.
