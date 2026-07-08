# FINAL PATCH NOTES v21

## AI analysis for dispute-created tickets

Fixed the gap where tickets created from the normal support page were analyzed by AI, while tickets created by opening a booking dispute were created without AI analysis.

### Backend changes

1. `DisputeBookingCommandHandler`
   - Injected `IComplaintAnalysisService`.
   - Builds a structured Arabic complaint context from the booking dispute data.
   - Runs AI complaint analysis before creating the linked ticket.
   - Saves the AI result on the ticket:
     - `AiSummary`
     - `Priority`
     - `AiEscalationPriority`
     - `AiIdentifiedIssue`
     - `AiClaimedImpact`
   - Returns the created `TicketId` in `DisputeBookingResponseDto` instead of `null`.
   - If AI analysis fails, the dispute is still created and a fallback Arabic summary is stored so the admin knows manual review is needed.

2. `OpenMarketplaceOrderDisputeCommandHandler`
   - Added the same AI analysis behavior for marketplace disputes for consistency.
   - Saves AI analysis on the generated admin ticket.
   - Uses Arabic ticket subject and Arabic success message.

### No migration

No database migration was required because the `Ticket` entity already contains the AI fields.

### Test cases

1. Open a normal support ticket from `/support-tickets` and verify AI summary appears in `/admin/tickets`.
2. Open a booking dispute from a completed booking and verify the generated ticket now contains AI summary and priority.
3. Open a marketplace order dispute and verify the generated ticket also contains AI analysis.
4. Temporarily disable AI service and verify dispute creation still succeeds with fallback Arabic summary.
