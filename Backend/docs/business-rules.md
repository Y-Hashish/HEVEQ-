# System Business Rules & Policies

This file outlines the critical business logic, financial calculations, and restrictions enforced across the backend application.

## 1. Booking Cancellation Policy
Enforced in `CancellationPolicyService.cs` during cancellation commands:
- **Eligible Statuses for Cancellation**: Only bookings in the following states can be cancelled:
  - `PendingProviderResponse`
  - `ConfirmedPendingPayment`
  - `Active`
- **Ineligible Statuses**: Bookings *cannot* be cancelled once the operator has started the work (`InProgress`), completion is requested (`PendingCustomerConfirmation`), the job is finished (`Completed`), or if there's a dispute (`Disputed`).
- **Refund Percentages**:
  - `PendingProviderResponse` -> **100%** Refund
  - `ConfirmedPendingPayment` -> **100%** Refund
  - `Active` (Before starting) -> **100%** Refund
  - All other status transitions refund **0%** (unless admin intervenes via a dispute).

## 2. Escrow & Commission Calculations
Enforced in the Booking and Marketplace order confirmation handlers:
- **Platform Commission Rate**: Defined in `PaymentPlatformOptions.cs` as `PlatformCommissionRate = 0.10m` (10% platform fee).
- **Service Bookings**:
  - `platformCommission = EstimatedTotal * PlatformCommissionRate`
  - `providerPayout = EstimatedTotal - platformCommission`
- **Marketplace Orders**:
  - `platformCommission = OrderAmount * PlatformCommissionRate`
  - `sellerPayout = OrderAmount - platformCommission`
- **VAT Rate**: Inferred Egypt VAT configuration is standard (14% VAT is calculated on Stripe gross transactions where specified).

## 3. SLA and Auto-Completion Job Rules (Hangfire)
The background worker executes cron schedules for contract compliance:
1. **Provider Response SLA (`booking-provider-response-sla`)**:
   - If a provider does not accept or reject a booking request within the designated SLA timeline, the booking status automatically transitions to `ProviderUnresponsive`.
2. **Customer Completion Auto-Confirm (`booking-customer-completion-auto-confirm`)**:
   - If the operator/provider submits evidence of job completion, the booking status changes to `PendingCustomerConfirmation`.
   - If the customer does not approve or file a dispute within the SLA timeframe, the system automatically completes the booking.
3. **Escrow Auto-Release (`booking-escrow-release-after-completion`)**:
   - Released completions trigger Stripe payout capture to transfer funds from the platform escrow account to the provider's account.

## 4. AI Search & Re-engagement Rules
- **Proximity Search**: Listings are filtered based on the provider's `ServiceRadiusKm` from their `BaseLatitude`/`BaseLongitude` to the customer's site.
- **AI Re-engagement Logic**:
  - Triggered immediately when a provider rejects a booking.
  - Automatically query alternative providers within the same category.
  - Alternatives must be priced at most 20% higher than the rejected booking: `maxHourlyRate = booking.HourlyRateSnapshot * 1.20`.
  - Excludes the rejecting provider's listings.
  - Generates Egyptian Arabic conversational text via OpenAI to re-engage the customer.

## 5. Trust Score Adjustments
- Providers and Customers are initialized with a base score.
- Trust scores decrease on disputes lost or SLA breaches.
- Trust scores increase upon high review scores (4 or 5 stars) and successfully completed transactions.
