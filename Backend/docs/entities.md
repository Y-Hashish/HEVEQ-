# Domain Entities & Navigation Properties

This file lists the database-mapped entities defined in `HEVEQ.Domain` layer.

## Address

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| UserId | `Guid` | No | - |
| Label | `string` | Yes | - |
| Governorate | `string` | No | - |
| District | `string` | No | - |
| Street | `string` | Yes | - |
| Latitude | `decimal` | Yes | - |
| Longitude | `decimal` | Yes | - |
| IsDefault | `bool` | No | - |
| CreatedAt | `DateTime` | No | - |
| User | `ApplicationUser` | No | ApplicationUser |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| User | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "label": "sample_label",
  "governorate": "sample_governorate",
  "district": "sample_district",
  "street": "sample_street",
  "latitude": 99.99,
  "longitude": 99.99,
  "isDefault": true,
  "createdAt": "2026-07-05T19:17:54Z"
}
```

---

## AiInteractionLog

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| AgentType | `AiAgentType` | No | - |
| InvocationContext | `string` | No | - |
| EntityType | `string` | Yes | - |
| EntityId | `Guid` | Yes | - |
| AiRecommendation | `string` | Yes | - |
| AiRiskScore | `int` | Yes | - |
| AdminOverride | `string` | Yes | - |
| AdminOverrideById | `Guid` | Yes | - |
| AdminOverrideAt | `DateTime` | Yes | - |
| LatencyMs | `int` | Yes | - |
| InputTokens | `int` | Yes | - |
| OutputTokens | `int` | Yes | - |
| CreatedAt | `DateTime` | No | - |
| AdminOverrideBy | `ApplicationUser` | Yes | ApplicationUser |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| AdminOverrideBy | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "agentType": {},
  "invocationContext": "sample_invocationcontext",
  "entityType": "sample_entitytype",
  "entityId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "aiRecommendation": "sample_airecommendation",
  "aiRiskScore": 1,
  "adminOverride": "sample_adminoverride",
  "adminOverrideById": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "adminOverrideAt": "2026-07-05T19:17:54Z",
  "latencyMs": 1,
  "inputTokens": 1,
  "outputTokens": 1,
  "createdAt": "2026-07-05T19:17:54Z"
}
```

---

## ApplicationUser

*Inherits from: `IdentityUser<Guid>`*

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| FirstName | `string` | No | - |
| LastName | `string` | No | - |
| IsActive | `bool` | No | - |
| FcmToken | `string` | Yes | - |
| CreatedAt | `DateTime` | No | - |
| UpdatedAt | `DateTime` | No | - |
| Addresses | `ICollection<Address>` | No | Address |
| RefreshTokens | `ICollection<RefreshToken>` | No | RefreshToken |
| ProviderProfile | `ProviderProfile` | Yes | ProviderProfile |
| CustomerProfile | `CustomerProfile` | Yes | CustomerProfile |
| EmployeeProfile | `EmployeeProfile` | Yes | EmployeeProfile |
| Documents | `ICollection<Document>` | No | Document |
| BookingsAsCustomer | `ICollection<Booking>` | No | Booking |
| MarketplaceListingsAsSeller | `ICollection<MarketplaceListing>` | No | MarketplaceListing |
| MarketplaceOrdersAsBuyer | `ICollection<MarketplaceOrder>` | No | MarketplaceOrder |
| ReviewsWritten | `ICollection<Review>` | No | Review |
| ReviewsReceived | `ICollection<Review>` | No | Review |
| SubmittedTickets | `ICollection<Ticket>` | No | Ticket |
| AssignedTickets | `ICollection<Ticket>` | No | Ticket |
| ResolvedTickets | `ICollection<Ticket>` | No | Ticket |
| EscalatedTickets | `ICollection<Ticket>` | No | Ticket |
| TicketMessages | `ICollection<TicketMessage>` | No | TicketMessage |
| TicketAttachments | `ICollection<TicketAttachment>` | No | TicketAttachment |
| JobCompletionEvidenceFormsSubmitted | `ICollection<JobCompletionEvidenceForm>` | No | JobCompletionEvidenceForm |
| JobCompletionEvidenceFormsReviewed | `ICollection<JobCompletionEvidenceForm>` | No | JobCompletionEvidenceForm |
| FieldVerificationFormsDispatched | `ICollection<FieldVerificationForm>` | No | FieldVerificationForm |
| FieldVerificationFormsDispatchedByAdmin | `ICollection<FieldVerificationForm>` | No | FieldVerificationForm |
| FieldVerificationFormsDecidedByAdmin | `ICollection<FieldVerificationForm>` | No | FieldVerificationForm |
| ConversationsInitiated | `ICollection<Conversation>` | No | Conversation |
| ConversationsParticipated | `ICollection<Conversation>` | No | Conversation |
| Messages | `ICollection<Message>` | No | Message |
| ConversationReadReceipts | `ICollection<ConversationReadReceipt>` | No | ConversationReadReceipt |
| Notifications | `ICollection<Notification>` | No | Notification |
| SearchQueryLogs | `ICollection<SearchQueryLog>` | No | SearchQueryLog |
| PlatformSettingsUpdated | `ICollection<PlatformSetting>` | No | PlatformSetting |
| AiInteractionLogsAdminOverrides | `ICollection<AiInteractionLog>` | No | AiInteractionLog |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Addresses | `ICollection<Address>` | Many | Inferred relation to `Address` |
| RefreshTokens | `ICollection<RefreshToken>` | Many | Inferred relation to `RefreshToken` |
| ProviderProfile | `ProviderProfile` | One / Zero-to-One | Inferred relation to `ProviderProfile` |
| CustomerProfile | `CustomerProfile` | One / Zero-to-One | Inferred relation to `CustomerProfile` |
| EmployeeProfile | `EmployeeProfile` | One / Zero-to-One | Inferred relation to `EmployeeProfile` |
| Documents | `ICollection<Document>` | Many | Inferred relation to `Document` |
| BookingsAsCustomer | `ICollection<Booking>` | Many | Inferred relation to `Booking` |
| MarketplaceListingsAsSeller | `ICollection<MarketplaceListing>` | Many | Inferred relation to `MarketplaceListing` |
| MarketplaceOrdersAsBuyer | `ICollection<MarketplaceOrder>` | Many | Inferred relation to `MarketplaceOrder` |
| ReviewsWritten | `ICollection<Review>` | Many | Inferred relation to `Review` |
| ReviewsReceived | `ICollection<Review>` | Many | Inferred relation to `Review` |
| SubmittedTickets | `ICollection<Ticket>` | Many | Inferred relation to `Ticket` |
| AssignedTickets | `ICollection<Ticket>` | Many | Inferred relation to `Ticket` |
| ResolvedTickets | `ICollection<Ticket>` | Many | Inferred relation to `Ticket` |
| EscalatedTickets | `ICollection<Ticket>` | Many | Inferred relation to `Ticket` |
| TicketMessages | `ICollection<TicketMessage>` | Many | Inferred relation to `TicketMessage` |
| TicketAttachments | `ICollection<TicketAttachment>` | Many | Inferred relation to `TicketAttachment` |
| JobCompletionEvidenceFormsSubmitted | `ICollection<JobCompletionEvidenceForm>` | Many | Inferred relation to `JobCompletionEvidenceForm` |
| JobCompletionEvidenceFormsReviewed | `ICollection<JobCompletionEvidenceForm>` | Many | Inferred relation to `JobCompletionEvidenceForm` |
| FieldVerificationFormsDispatched | `ICollection<FieldVerificationForm>` | Many | Inferred relation to `FieldVerificationForm` |
| FieldVerificationFormsDispatchedByAdmin | `ICollection<FieldVerificationForm>` | Many | Inferred relation to `FieldVerificationForm` |
| FieldVerificationFormsDecidedByAdmin | `ICollection<FieldVerificationForm>` | Many | Inferred relation to `FieldVerificationForm` |
| ConversationsInitiated | `ICollection<Conversation>` | Many | Inferred relation to `Conversation` |
| ConversationsParticipated | `ICollection<Conversation>` | Many | Inferred relation to `Conversation` |
| Messages | `ICollection<Message>` | Many | Inferred relation to `Message` |
| ConversationReadReceipts | `ICollection<ConversationReadReceipt>` | Many | Inferred relation to `ConversationReadReceipt` |
| Notifications | `ICollection<Notification>` | Many | Inferred relation to `Notification` |
| SearchQueryLogs | `ICollection<SearchQueryLog>` | Many | Inferred relation to `SearchQueryLog` |
| PlatformSettingsUpdated | `ICollection<PlatformSetting>` | Many | Inferred relation to `PlatformSetting` |
| AiInteractionLogsAdminOverrides | `ICollection<AiInteractionLog>` | Many | Inferred relation to `AiInteractionLog` |

### Example Object (JSON)
```json
{
  "firstName": "sample_firstname",
  "lastName": "sample_lastname",
  "isActive": true,
  "fcmToken": "sample_fcmtoken",
  "createdAt": "2026-07-05T19:17:54Z",
  "updatedAt": "2026-07-05T19:17:54Z"
}
```

---

## BlackoutDate

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| ListingId | `Guid` | No | - |
| OperatorId | `Guid` | Yes | - |
| Date | `DateOnly` | No | - |
| Reason | `string` | Yes | - |
| CreatedAt | `DateTime` | No | - |
| Listing | `ServiceListing` | No | ServiceListing |
| Operator | `Operator` | Yes | Operator |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Listing | `ServiceListing` | One / Zero-to-One | Inferred relation to `ServiceListing` |
| Operator | `Operator` | One / Zero-to-One | Inferred relation to `Operator` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "listingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "operatorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "date": "2026-07-05T19:17:54Z",
  "reason": "sample_reason",
  "createdAt": "2026-07-05T19:17:54Z"
}
```

---

## Booking

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| CustomerId | `Guid` | No | - |
| BookingNumber | `string` | No | - |
| ServiceListingId | `Guid` | No | - |
| AssignedOperatorId | `Guid` | Yes | - |
| JobTitle | `string` | No | - |
| JobDescription | `string` | Yes | - |
| Governorate | `string` | No | - |
| District | `string` | No | - |
| Street | `string` | Yes | - |
| Latitude | `decimal` | Yes | - |
| Longitude | `decimal` | Yes | - |
| ServiceLocationGeo | `Point` | Yes | - |
| SiteContactName | `string` | Yes | - |
| SiteContactPhone | `string` | Yes | - |
| AccessRequirements | `string` | Yes | - |
| SafetyNotes | `string` | Yes | - |
| RequestedStartDate | `DateOnly` | No | - |
| RequestedStartTime | `TimeOnly` | No | - |
| EstimatedDurationHours | `decimal` | No | - |
| ActualDurationHours | `decimal` | Yes | - |
| HourlyRateSnapshot | `decimal` | No | - |
| EstimatedTotal | `decimal` | No | - |
| SurchargeAmount | `decimal` | Yes | - |
| IsOutOfZoneBooking | `bool` | No | - |
| OutOfZoneDistanceKm | `decimal` | Yes | - |
| OutOfZoneSurchargeAmount | `decimal` | Yes | - |
| OutOfZoneSurchargeAcceptedAt | `DateTime` | Yes | - |
| PrioritySurchargeAmount | `decimal` | Yes | - |
| FuelSurchargeAmount | `decimal` | Yes | - |
| Status | `BookingStatus` | No | - |
| ProviderRejectionReason | `string` | Yes | - |
| CancellationReason | `string` | Yes | - |
| CancelledAt | `DateTime` | Yes | - |
| CancellationInitiatedByRole | `BookingCancellationInitiator` | Yes | - |
| CancellationRefundPct | `decimal` | Yes | - |
| ProviderCancellationPenaltyApplied | `bool` | No | - |
| ReassignedToBookingId | `Guid` | Yes | - |
| OriginalBookingId | `Guid` | Yes | - |
| ReassignedAt | `DateTime` | Yes | - |
| CreatedAt | `DateTime` | No | - |
| ConfirmedAt | `DateTime` | Yes | - |
| RejectedAt | `DateTime` | Yes | - |
| PaymentCapturedAt | `DateTime` | Yes | - |
| StartedAt | `DateTime` | Yes | - |
| CompletedMarkedAt | `DateTime` | Yes | - |
| CompletionConfirmedAt | `DateTime` | Yes | - |
| DisputeOpenedAt | `DateTime` | Yes | - |
| FieldVerificationDispatchedAt | `DateTime` | Yes | - |
| Timestamp | `byte[]` | No | - |
| Customer | `ApplicationUser` | No | ApplicationUser |
| ServiceListing | `ServiceListing` | No | ServiceListing |
| AssignedOperator | `Operator` | Yes | Operator |
| ReassignedToBooking | `Booking` | Yes | Booking |
| OriginalBooking | `Booking` | Yes | Booking |
| ReassignedFromBookings | `ICollection<Booking>` | No | Booking |
| TimeAdjustmentRequests | `ICollection<BookingTimeAdjustmentRequest>` | No | BookingTimeAdjustmentRequest |
| OperatorAssignments | `ICollection<OperatorAssignment>` | No | OperatorAssignment |
| EscrowRecords | `ICollection<EscrowRecord>` | No | EscrowRecord |
| Reviews | `ICollection<Review>` | No | Review |
| Tickets | `ICollection<Ticket>` | No | Ticket |
| JobCompletionEvidenceForms | `ICollection<JobCompletionEvidenceForm>` | No | JobCompletionEvidenceForm |
| FieldVerificationForms | `ICollection<FieldVerificationForm>` | No | FieldVerificationForm |
| Conversations | `ICollection<Conversation>` | No | Conversation |
| ProviderIncidents | `ICollection<ProviderIncident>` | No | ProviderIncident |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Customer | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| ServiceListing | `ServiceListing` | One / Zero-to-One | Inferred relation to `ServiceListing` |
| AssignedOperator | `Operator` | One / Zero-to-One | Inferred relation to `Operator` |
| ReassignedToBooking | `Booking` | One / Zero-to-One | Inferred relation to `Booking` |
| OriginalBooking | `Booking` | One / Zero-to-One | Inferred relation to `Booking` |
| ReassignedFromBookings | `ICollection<Booking>` | Many | Inferred relation to `Booking` |
| TimeAdjustmentRequests | `ICollection<BookingTimeAdjustmentRequest>` | Many | Inferred relation to `BookingTimeAdjustmentRequest` |
| OperatorAssignments | `ICollection<OperatorAssignment>` | Many | Inferred relation to `OperatorAssignment` |
| EscrowRecords | `ICollection<EscrowRecord>` | Many | Inferred relation to `EscrowRecord` |
| Reviews | `ICollection<Review>` | Many | Inferred relation to `Review` |
| Tickets | `ICollection<Ticket>` | Many | Inferred relation to `Ticket` |
| JobCompletionEvidenceForms | `ICollection<JobCompletionEvidenceForm>` | Many | Inferred relation to `JobCompletionEvidenceForm` |
| FieldVerificationForms | `ICollection<FieldVerificationForm>` | Many | Inferred relation to `FieldVerificationForm` |
| Conversations | `ICollection<Conversation>` | Many | Inferred relation to `Conversation` |
| ProviderIncidents | `ICollection<ProviderIncident>` | Many | Inferred relation to `ProviderIncident` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingNumber": "01012345678",
  "serviceListingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "assignedOperatorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "jobTitle": "sample_jobtitle",
  "jobDescription": "sample_jobdescription",
  "governorate": "sample_governorate",
  "district": "sample_district",
  "street": "sample_street",
  "latitude": 99.99,
  "longitude": 99.99,
  "serviceLocationGeo": 1,
  "siteContactName": "sample_sitecontactname",
  "siteContactPhone": "01012345678",
  "accessRequirements": "sample_accessrequirements",
  "safetyNotes": "sample_safetynotes",
  "requestedStartDate": "2026-07-05T19:17:54Z",
  "requestedStartTime": "2026-07-05T19:17:54Z",
  "estimatedDurationHours": 99.99,
  "actualDurationHours": 99.99,
  "hourlyRateSnapshot": 99.99,
  "estimatedTotal": 99.99,
  "surchargeAmount": 99.99,
  "isOutOfZoneBooking": true,
  "outOfZoneDistanceKm": 99.99,
  "outOfZoneSurchargeAmount": 99.99,
  "outOfZoneSurchargeAcceptedAt": "2026-07-05T19:17:54Z",
  "prioritySurchargeAmount": 99.99,
  "fuelSurchargeAmount": 99.99,
  "status": {},
  "providerRejectionReason": "sample_providerrejectionreason",
  "cancellationReason": "sample_cancellationreason",
  "cancelledAt": "2026-07-05T19:17:54Z",
  "cancellationInitiatedByRole": {},
  "cancellationRefundPct": 99.99,
  "providerCancellationPenaltyApplied": true,
  "reassignedToBookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "originalBookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "reassignedAt": "2026-07-05T19:17:54Z",
  "createdAt": "2026-07-05T19:17:54Z",
  "confirmedAt": "2026-07-05T19:17:54Z",
  "rejectedAt": "2026-07-05T19:17:54Z",
  "paymentCapturedAt": "2026-07-05T19:17:54Z",
  "startedAt": "2026-07-05T19:17:54Z",
  "completedMarkedAt": "2026-07-05T19:17:54Z",
  "completionConfirmedAt": "2026-07-05T19:17:54Z",
  "disputeOpenedAt": "2026-07-05T19:17:54Z",
  "fieldVerificationDispatchedAt": "2026-07-05T19:17:54Z",
  "timestamp": {}
}
```

---

## BookingTimeAdjustmentRequest

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| BookingId | `Guid` | No | - |
| RequestedAdditionalHrs | `decimal` | No | - |
| AdditionalCostAmount | `decimal` | No | - |
| Status | `BookingTimeAdjustmentStatus` | No | - |
| ProviderNote | `string` | Yes | - |
| CustomerAcknowledgedAt | `DateTime` | Yes | - |
| CreatedAt | `DateTime` | No | - |
| Booking | `Booking` | No | Booking |
| EscrowRecords | `ICollection<EscrowRecord>` | No | EscrowRecord |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Booking | `Booking` | One / Zero-to-One | Inferred relation to `Booking` |
| EscrowRecords | `ICollection<EscrowRecord>` | Many | Inferred relation to `EscrowRecord` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "requestedAdditionalHrs": 99.99,
  "additionalCostAmount": 99.99,
  "status": "2026-07-05T19:17:54Z",
  "providerNote": "sample_providernote",
  "customerAcknowledgedAt": "2026-07-05T19:17:54Z",
  "createdAt": "2026-07-05T19:17:54Z"
}
```

---

## Category

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `int` | No | - |
| Name | `string` | No | - |
| Slug | `string` | No | - |
| Type | `CategoryType` | No | - |
| ParentId | `int` | Yes | - |
| Parent | `Category` | Yes | Category |
| Children | `ICollection<Category>` | No | Category |
| ServiceListings | `ICollection<ServiceListing>` | No | ServiceListing |
| MarketplaceListings | `ICollection<MarketplaceListing>` | No | MarketplaceListing |
| PricingAggregates | `ICollection<CategoryPricingAggregate>` | No | CategoryPricingAggregate |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Parent | `Category` | One / Zero-to-One | Inferred relation to `Category` |
| Children | `ICollection<Category>` | Many | Inferred relation to `Category` |
| ServiceListings | `ICollection<ServiceListing>` | Many | Inferred relation to `ServiceListing` |
| MarketplaceListings | `ICollection<MarketplaceListing>` | Many | Inferred relation to `MarketplaceListing` |
| PricingAggregates | `ICollection<CategoryPricingAggregate>` | Many | Inferred relation to `CategoryPricingAggregate` |

### Example Object (JSON)
```json
{
  "id": 1,
  "name": "sample_name",
  "slug": "sample_slug",
  "type": {},
  "parentId": 1
}
```

---

## CategoryPricingAggregate

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| CategoryId | `int` | No | - |
| LocationGovernorate | `string` | No | - |
| PriceType | `PriceType` | No | - |
| MedianPrice | `decimal` | No | - |
| Percentile25 | `decimal` | No | - |
| Percentile75 | `decimal` | No | - |
| MinPrice | `decimal` | No | - |
| MaxPrice | `decimal` | No | - |
| SampleCount | `int` | No | - |
| ComputedAt | `DateTime` | No | - |
| Category | `Category` | No | Category |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Category | `Category` | One / Zero-to-One | Inferred relation to `Category` |

### Example Object (JSON)
```json
{
  "categoryId": 1,
  "locationGovernorate": "sample_locationgovernorate",
  "priceType": {},
  "medianPrice": 99.99,
  "percentile25": 99.99,
  "percentile75": 99.99,
  "minPrice": 99.99,
  "maxPrice": 99.99,
  "sampleCount": 1,
  "computedAt": "2026-07-05T19:17:54Z"
}
```

---

## Conversation

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| ServiceListingId | `Guid` | Yes | - |
| MarketplaceListingId | `Guid` | Yes | - |
| BookingId | `Guid` | Yes | - |
| InitiatedById | `Guid` | No | - |
| ParticipantId | `Guid` | No | - |
| IsLocked | `bool` | No | - |
| CreatedAt | `DateTime` | No | - |
| LockedAt | `DateTime` | Yes | - |
| ServiceListing | `ServiceListing` | Yes | ServiceListing |
| MarketplaceListing | `MarketplaceListing` | Yes | MarketplaceListing |
| Booking | `Booking` | Yes | Booking |
| InitiatedBy | `ApplicationUser` | No | ApplicationUser |
| Participant | `ApplicationUser` | No | ApplicationUser |
| Messages | `ICollection<Message>` | No | Message |
| ReadReceipts | `ICollection<ConversationReadReceipt>` | No | ConversationReadReceipt |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| ServiceListing | `ServiceListing` | One / Zero-to-One | Inferred relation to `ServiceListing` |
| MarketplaceListing | `MarketplaceListing` | One / Zero-to-One | Inferred relation to `MarketplaceListing` |
| Booking | `Booking` | One / Zero-to-One | Inferred relation to `Booking` |
| InitiatedBy | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| Participant | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| Messages | `ICollection<Message>` | Many | Inferred relation to `Message` |
| ReadReceipts | `ICollection<ConversationReadReceipt>` | Many | Inferred relation to `ConversationReadReceipt` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "serviceListingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "marketplaceListingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "initiatedById": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "participantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "isLocked": true,
  "createdAt": "2026-07-05T19:17:54Z",
  "lockedAt": "2026-07-05T19:17:54Z"
}
```

---

## ConversationReadReceipt

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| ConversationId | `Guid` | No | - |
| UserId | `Guid` | No | - |
| LastReadMessageId | `Guid` | Yes | - |
| LastReadAt | `DateTime` | No | - |
| Conversation | `Conversation` | No | Conversation |
| User | `ApplicationUser` | No | ApplicationUser |
| LastReadMessage | `Message` | Yes | Message |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Conversation | `Conversation` | One / Zero-to-One | Inferred relation to `Conversation` |
| User | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| LastReadMessage | `Message` | One / Zero-to-One | Inferred relation to `Message` |

### Example Object (JSON)
```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "lastReadMessageId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "lastReadAt": "2026-07-05T19:17:54Z"
}
```

---

## CustomerProfile

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| UserId | `Guid` | No | - |
| TrustScore | `decimal` | No | - |
| CancellationRate | `decimal` | Yes | - |
| DisputeFrequencyScore | `decimal` | Yes | - |
| PaymentFailureCount | `int` | No | - |
| ReviewAuthenticityScore | `decimal` | Yes | - |
| RequiresAdditionalVerification | `bool` | No | - |
| TotalBookings | `int` | No | - |
| TrustScoreLastComputedAt | `DateTime` | Yes | - |
| CreatedAt | `DateTime` | No | - |
| UpdatedAt | `DateTime` | No | - |
| User | `ApplicationUser` | No | ApplicationUser |
| TrustScoreHistory | `ICollection<CustomerTrustScoreHistory>` | No | CustomerTrustScoreHistory |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| User | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| TrustScoreHistory | `ICollection<CustomerTrustScoreHistory>` | Many | Inferred relation to `CustomerTrustScoreHistory` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "trustScore": 99.99,
  "cancellationRate": 99.99,
  "disputeFrequencyScore": 99.99,
  "paymentFailureCount": 1,
  "reviewAuthenticityScore": 99.99,
  "requiresAdditionalVerification": true,
  "totalBookings": 1,
  "trustScoreLastComputedAt": "2026-07-05T19:17:54Z",
  "createdAt": "2026-07-05T19:17:54Z",
  "updatedAt": "2026-07-05T19:17:54Z"
}
```

---

## CustomerTrustScoreHistory

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| CustomerProfileId | `Guid` | No | - |
| TrustScore | `decimal` | No | - |
| TriggerEvent | `string` | Yes | - |
| RecordedAt | `DateTime` | No | - |
| CustomerProfile | `CustomerProfile` | No | CustomerProfile |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| CustomerProfile | `CustomerProfile` | One / Zero-to-One | Inferred relation to `CustomerProfile` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerProfileId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "trustScore": 99.99,
  "triggerEvent": "sample_triggerevent",
  "recordedAt": "2026-07-05T19:17:54Z"
}
```

---

## Document

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| UserId | `Guid` | Yes | - |
| ServiceListingId | `Guid` | Yes | - |
| MarketplaceListingId | `Guid` | Yes | - |
| OperatorId | `Guid` | Yes | - |
| DocumentType | `DocumentType` | No | - |
| FileUrl | `string` | No | - |
| Status | `DocumentVerificationStatus` | No | - |
| ExtractedText | `string` | Yes | - |
| ConfidenceScore | `decimal` | Yes | - |
| KeyFieldsPresent | `bool` | Yes | - |
| ExpiryDate | `DateOnly` | Yes | - |
| ExpiryStatus | `DocumentExpiryStatus` | Yes | - |
| FailureReason | `string` | Yes | - |
| AdminNote | `string` | Yes | - |
| UploadedAt | `DateTime` | No | - |
| VerifiedAt | `DateTime` | Yes | - |
| User | `ApplicationUser` | Yes | ApplicationUser |
| ServiceListing | `ServiceListing` | Yes | ServiceListing |
| MarketplaceListing | `MarketplaceListing` | Yes | MarketplaceListing |
| Operator | `Operator` | Yes | Operator |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| User | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| ServiceListing | `ServiceListing` | One / Zero-to-One | Inferred relation to `ServiceListing` |
| MarketplaceListing | `MarketplaceListing` | One / Zero-to-One | Inferred relation to `MarketplaceListing` |
| Operator | `Operator` | One / Zero-to-One | Inferred relation to `Operator` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "serviceListingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "marketplaceListingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "operatorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "documentType": {},
  "fileUrl": "sample_fileurl",
  "status": {},
  "extractedText": "sample_extractedtext",
  "confidenceScore": 99.99,
  "keyFieldsPresent": true,
  "expiryDate": "2026-07-05T19:17:54Z",
  "expiryStatus": {},
  "failureReason": "sample_failurereason",
  "adminNote": "sample_adminnote",
  "uploadedAt": "2026-07-05T19:17:54Z",
  "verifiedAt": "2026-07-05T19:17:54Z"
}
```

---

## DomainEventQueueItem

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| EventType | `string` | No | - |
| EntityType | `string` | No | - |
| EntityId | `Guid` | No | - |
| Status | `DomainEventQueueStatus` | No | - |
| RetryCount | `int` | No | - |
| FailureReason | `string` | Yes | - |
| ProcessedAt | `DateTime` | Yes | - |
| CreatedAt | `DateTime` | No | - |

### Navigation Properties & Relationships
*No navigation properties.*

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "eventType": "sample_eventtype",
  "entityType": "sample_entitytype",
  "entityId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": {},
  "retryCount": 1,
  "failureReason": "sample_failurereason",
  "processedAt": "2026-07-05T19:17:54Z",
  "createdAt": "2026-07-05T19:17:54Z"
}
```

---

## EmployeeProfile

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| UserId | `Guid` | No | - |
| EmployeeCode | `string` | No | - |
| Department | `string` | Yes | - |
| AssignedGovernorate | `string` | Yes | - |
| IsAvailableForDispatch | `bool` | No | - |
| TotalVerificationsCompleted | `int` | No | - |
| TotalTicketsHandled | `int` | No | - |
| CreatedAt | `DateTime` | No | - |
| UpdatedAt | `DateTime` | No | - |
| User | `ApplicationUser` | No | ApplicationUser |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| User | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "employeeCode": "sample_employeecode",
  "department": "sample_department",
  "assignedGovernorate": "sample_assignedgovernorate",
  "isAvailableForDispatch": true,
  "totalVerificationsCompleted": 1,
  "totalTicketsHandled": 1,
  "createdAt": "2026-07-05T19:17:54Z",
  "updatedAt": "2026-07-05T19:17:54Z"
}
```

---

## EscrowRecord

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| BookingId | `Guid` | Yes | - |
| MarketplaceOrderId | `Guid` | Yes | - |
| AdjustmentRequestId | `Guid` | Yes | - |
| GrossAmount | `decimal` | No | - |
| PlatformCommission | `decimal` | No | - |
| CommissionRateSnapshot | `decimal` | No | - |
| ProviderPayout | `decimal` | No | - |
| VatAmount | `decimal` | No | - |
| PartialSettleCustomerAmt | `decimal` | Yes | - |
| PartialSettleProviderAmt | `decimal` | Yes | - |
| Status | `EscrowStatus` | No | - |
| PaymentGatewayReference | `string` | Yes | - |
| AdditionalHoldDays | `int` | No | - |
| EarliestReleaseAt | `DateTime` | Yes | - |
| CapturedAt | `DateTime` | Yes | - |
| HeldAt | `DateTime` | Yes | - |
| ReleasedAt | `DateTime` | Yes | - |
| FrozenAt | `DateTime` | Yes | - |
| FreezeReason | `string` | Yes | - |
| CreatedAt | `DateTime` | No | - |
| Timestamp | `byte[]` | No | - |
| Booking | `Booking` | Yes | Booking |
| MarketplaceOrder | `MarketplaceOrder` | Yes | MarketplaceOrder |
| AdjustmentRequest | `BookingTimeAdjustmentRequest` | Yes | BookingTimeAdjustmentRequest |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Booking | `Booking` | One / Zero-to-One | Inferred relation to `Booking` |
| MarketplaceOrder | `MarketplaceOrder` | One / Zero-to-One | Inferred relation to `MarketplaceOrder` |
| AdjustmentRequest | `BookingTimeAdjustmentRequest` | One / Zero-to-One | Inferred relation to `BookingTimeAdjustmentRequest` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "marketplaceOrderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "adjustmentRequestId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "grossAmount": 99.99,
  "platformCommission": 99.99,
  "commissionRateSnapshot": 99.99,
  "providerPayout": 99.99,
  "vatAmount": 99.99,
  "partialSettleCustomerAmt": 99.99,
  "partialSettleProviderAmt": 99.99,
  "status": {},
  "paymentGatewayReference": "sample_paymentgatewayreference",
  "additionalHoldDays": 1,
  "earliestReleaseAt": "2026-07-05T19:17:54Z",
  "capturedAt": "2026-07-05T19:17:54Z",
  "heldAt": "2026-07-05T19:17:54Z",
  "releasedAt": "2026-07-05T19:17:54Z",
  "frozenAt": "2026-07-05T19:17:54Z",
  "freezeReason": "sample_freezereason",
  "createdAt": "2026-07-05T19:17:54Z",
  "timestamp": {}
}
```

---

## FieldVerificationForm

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| BookingId | `Guid` | No | - |
| DispatchedEmployeeId | `Guid` | No | - |
| DispatchedByAdminId | `Guid` | No | - |
| DecidedByAdminId | `Guid` | Yes | - |
| LinkedEvidenceFormId | `Guid` | No | - |
| DispatchInstructions | `string` | Yes | - |
| VisitStatus | `VisitStatus` | No | - |
| EmployeeNotes | `string` | Yes | - |
| FieldVerificationOutcome | `FieldVerificationOutcome` | Yes | - |
| AiSimilarityScore | `decimal` | Yes | - |
| AiSimilarityNotes | `string` | Yes | - |
| AdminDecision | `FieldVerificationAdminDecision` | No | - |
| AdminDecisionNote | `string` | Yes | - |
| DispatchedAt | `DateTime` | No | - |
| VisitedAt | `DateTime` | Yes | - |
| FormSubmittedAt | `DateTime` | Yes | - |
| DecidedAt | `DateTime` | Yes | - |
| CreatedAt | `DateTime` | No | - |
| Booking | `Booking` | No | Booking |
| DispatchedEmployee | `ApplicationUser` | No | ApplicationUser |
| DispatchedByAdmin | `ApplicationUser` | No | ApplicationUser |
| DecidedByAdmin | `ApplicationUser` | Yes | ApplicationUser |
| LinkedEvidenceForm | `JobCompletionEvidenceForm` | No | JobCompletionEvidenceForm |
| Photos | `ICollection<FieldVerificationPhoto>` | No | FieldVerificationPhoto |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Booking | `Booking` | One / Zero-to-One | Inferred relation to `Booking` |
| DispatchedEmployee | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| DispatchedByAdmin | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| DecidedByAdmin | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| LinkedEvidenceForm | `JobCompletionEvidenceForm` | One / Zero-to-One | Inferred relation to `JobCompletionEvidenceForm` |
| Photos | `ICollection<FieldVerificationPhoto>` | Many | Inferred relation to `FieldVerificationPhoto` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "dispatchedEmployeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "dispatchedByAdminId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "decidedByAdminId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "linkedEvidenceFormId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "dispatchInstructions": "sample_dispatchinstructions",
  "visitStatus": {},
  "employeeNotes": "sample_employeenotes",
  "fieldVerificationOutcome": {},
  "aiSimilarityScore": 99.99,
  "aiSimilarityNotes": "sample_aisimilaritynotes",
  "adminDecision": {},
  "adminDecisionNote": "sample_admindecisionnote",
  "dispatchedAt": "2026-07-05T19:17:54Z",
  "visitedAt": "2026-07-05T19:17:54Z",
  "formSubmittedAt": "2026-07-05T19:17:54Z",
  "decidedAt": "2026-07-05T19:17:54Z",
  "createdAt": "2026-07-05T19:17:54Z"
}
```

---

## FieldVerificationPhoto

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| FieldVerificationFormId | `Guid` | No | - |
| PhotoUrl | `string` | No | - |
| Caption | `string` | Yes | - |
| DisplayOrder | `int` | No | - |
| CreatedAt | `DateTime` | No | - |
| FieldVerificationForm | `FieldVerificationForm` | No | FieldVerificationForm |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| FieldVerificationForm | `FieldVerificationForm` | One / Zero-to-One | Inferred relation to `FieldVerificationForm` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fieldVerificationFormId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "photoUrl": "sample_photourl",
  "caption": "sample_caption",
  "displayOrder": 1,
  "createdAt": "2026-07-05T19:17:54Z"
}
```

---

## JobCompletionEvidenceForm

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| BookingId | `Guid` | No | - |
| SubmittedByUserId | `Guid` | No | - |
| ReviewedByAdminId | `Guid` | Yes | - |
| ProviderNotes | `string` | Yes | - |
| Status | `EvidenceFormStatus` | No | - |
| AdminReviewNote | `string` | Yes | - |
| SubmittedAt | `DateTime` | Yes | - |
| ReviewedAt | `DateTime` | Yes | - |
| CreatedAt | `DateTime` | No | - |
| Booking | `Booking` | No | Booking |
| SubmittedByUser | `ApplicationUser` | No | ApplicationUser |
| ReviewedByAdmin | `ApplicationUser` | Yes | ApplicationUser |
| Photos | `ICollection<JobCompletionEvidencePhoto>` | No | JobCompletionEvidencePhoto |
| FieldVerificationForms | `ICollection<FieldVerificationForm>` | No | FieldVerificationForm |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Booking | `Booking` | One / Zero-to-One | Inferred relation to `Booking` |
| SubmittedByUser | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| ReviewedByAdmin | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| Photos | `ICollection<JobCompletionEvidencePhoto>` | Many | Inferred relation to `JobCompletionEvidencePhoto` |
| FieldVerificationForms | `ICollection<FieldVerificationForm>` | Many | Inferred relation to `FieldVerificationForm` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "submittedByUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "reviewedByAdminId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "providerNotes": "sample_providernotes",
  "status": {},
  "adminReviewNote": "sample_adminreviewnote",
  "submittedAt": "2026-07-05T19:17:54Z",
  "reviewedAt": "2026-07-05T19:17:54Z",
  "createdAt": "2026-07-05T19:17:54Z"
}
```

---

## JobCompletionEvidencePhoto

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| FormId | `Guid` | No | - |
| PhotoUrl | `string` | No | - |
| Caption | `string` | Yes | - |
| DisplayOrder | `int` | No | - |
| CreatedAt | `DateTime` | No | - |
| Form | `JobCompletionEvidenceForm` | No | JobCompletionEvidenceForm |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Form | `JobCompletionEvidenceForm` | One / Zero-to-One | Inferred relation to `JobCompletionEvidenceForm` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "formId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "photoUrl": "sample_photourl",
  "caption": "sample_caption",
  "displayOrder": 1,
  "createdAt": "2026-07-05T19:17:54Z"
}
```

---

## MarketplaceListing

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| SellerId | `Guid` | No | - |
| CategoryId | `int` | No | - |
| Title | `string` | No | - |
| Condition | `ProductCondition` | No | - |
| YearOfManufacture | `int` | Yes | - |
| Description | `string` | No | - |
| Specifications | `string` | Yes | - |
| Price | `decimal` | No | - |
| IsNegotiable | `bool` | No | - |
| TransactionMethod | `MarketplaceTransactionMethod` | No | - |
| Governorate | `string` | Yes | - |
| District | `string` | Yes | - |
| Status | `MarketplaceListingStatus` | No | - |
| AiRiskScore | `int` | Yes | - |
| AiRiskLevel | `string` | Yes | - |
| AiRiskFlags | `string` | Yes | - |
| VideoUrl | `string` | Yes | - |
| QdrantPointId | `string` | Yes | - |
| LastEmbeddedAt | `DateTime` | Yes | - |
| EmbeddingStatus | `EmbeddingStatus` | No | - |
| AdminRejectionNote | `string` | Yes | - |
| RejectedByAdminId | `Guid` | Yes | - |
| RejectedAt | `DateTime` | Yes | - |
| SubmissionCount | `int` | No | - |
| CreatedAt | `DateTime` | No | - |
| UpdatedAt | `DateTime` | No | - |
| Seller | `ApplicationUser` | No | ApplicationUser |
| Category | `Category` | No | Category |
| RejectedByAdmin | `ApplicationUser` | Yes | ApplicationUser |
| Photos | `ICollection<MarketplaceListingPhoto>` | No | MarketplaceListingPhoto |
| Orders | `ICollection<MarketplaceOrder>` | No | MarketplaceOrder |
| Documents | `ICollection<Document>` | No | Document |
| Reviews | `ICollection<Review>` | No | Review |
| Conversations | `ICollection<Conversation>` | No | Conversation |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Seller | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| Category | `Category` | One / Zero-to-One | Inferred relation to `Category` |
| RejectedByAdmin | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| Photos | `ICollection<MarketplaceListingPhoto>` | Many | Inferred relation to `MarketplaceListingPhoto` |
| Orders | `ICollection<MarketplaceOrder>` | Many | Inferred relation to `MarketplaceOrder` |
| Documents | `ICollection<Document>` | Many | Inferred relation to `Document` |
| Reviews | `ICollection<Review>` | Many | Inferred relation to `Review` |
| Conversations | `ICollection<Conversation>` | Many | Inferred relation to `Conversation` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "sellerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "categoryId": 1,
  "title": "sample_title",
  "condition": {},
  "yearOfManufacture": 1,
  "description": "sample_description",
  "specifications": "sample_specifications",
  "price": 99.99,
  "isNegotiable": true,
  "transactionMethod": {},
  "governorate": "sample_governorate",
  "district": "sample_district",
  "status": [],
  "aiRiskScore": 1,
  "aiRiskLevel": "sample_airisklevel",
  "aiRiskFlags": "sample_airiskflags",
  "videoUrl": "sample_videourl",
  "qdrantPointId": "sample_qdrantpointid",
  "lastEmbeddedAt": "2026-07-05T19:17:54Z",
  "embeddingStatus": {},
  "adminRejectionNote": "sample_adminrejectionnote",
  "rejectedByAdminId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "rejectedAt": "2026-07-05T19:17:54Z",
  "submissionCount": 1,
  "createdAt": "2026-07-05T19:17:54Z",
  "updatedAt": "2026-07-05T19:17:54Z"
}
```

---

## MarketplaceListingPhoto

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| ListingId | `Guid` | No | - |
| PhotoUrl | `string` | No | - |
| DisplayOrder | `int` | No | - |
| CreatedAt | `DateTime` | No | - |
| Listing | `MarketplaceListing` | No | MarketplaceListing |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Listing | `MarketplaceListing` | One / Zero-to-One | Inferred relation to `MarketplaceListing` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "listingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "photoUrl": "sample_photourl",
  "displayOrder": 1,
  "createdAt": "2026-07-05T19:17:54Z"
}
```

---

## MarketplaceOrder

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| OrderNumber | `string` | No | - |
| BuyerId | `Guid` | No | - |
| ListingId | `Guid` | No | - |
| Amount | `decimal` | No | - |
| DeliveryAddress | `string` | Yes | - |
| DeliveryPreference | `DeliveryPreference` | Yes | - |
| TrackingNumber | `string` | Yes | - |
| Status | `MarketplaceOrderStatus` | No | - |
| SellerConfirmedAt | `DateTime` | Yes | - |
| DispatchedAt | `DateTime` | Yes | - |
| DeliveredAt | `DateTime` | Yes | - |
| ConfirmedByBuyerAt | `DateTime` | Yes | - |
| CancellationReason | `string` | Yes | - |
| CancelledAt | `DateTime` | Yes | - |
| CancellationInitiatedByRole | `MarketplaceOrderCancellationInitiator` | Yes | - |
| ReturnShippingCost | `decimal` | Yes | - |
| ReturnShippingAcceptedByBuyerAt | `DateTime` | Yes | - |
| CreatedAt | `DateTime` | No | - |
| Buyer | `ApplicationUser` | No | ApplicationUser |
| Listing | `MarketplaceListing` | No | MarketplaceListing |
| EscrowRecords | `ICollection<EscrowRecord>` | No | EscrowRecord |
| Reviews | `ICollection<Review>` | No | Review |
| Tickets | `ICollection<Ticket>` | No | Ticket |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Buyer | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| Listing | `MarketplaceListing` | One / Zero-to-One | Inferred relation to `MarketplaceListing` |
| EscrowRecords | `ICollection<EscrowRecord>` | Many | Inferred relation to `EscrowRecord` |
| Reviews | `ICollection<Review>` | Many | Inferred relation to `Review` |
| Tickets | `ICollection<Ticket>` | Many | Inferred relation to `Ticket` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "orderNumber": "01012345678",
  "buyerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "listingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "amount": 99.99,
  "deliveryAddress": "sample_deliveryaddress",
  "deliveryPreference": {},
  "trackingNumber": "01012345678",
  "status": {},
  "sellerConfirmedAt": "2026-07-05T19:17:54Z",
  "dispatchedAt": "2026-07-05T19:17:54Z",
  "deliveredAt": "2026-07-05T19:17:54Z",
  "confirmedByBuyerAt": "2026-07-05T19:17:54Z",
  "cancellationReason": "sample_cancellationreason",
  "cancelledAt": "2026-07-05T19:17:54Z",
  "cancellationInitiatedByRole": {},
  "returnShippingCost": 99.99,
  "returnShippingAcceptedByBuyerAt": "2026-07-05T19:17:54Z",
  "createdAt": "2026-07-05T19:17:54Z"
}
```

---

## Message

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| ConversationId | `Guid` | No | - |
| SenderId | `Guid` | No | - |
| Content | `string` | Yes | - |
| MessageType | `MessageType` | No | - |
| AttachmentUrl | `string` | Yes | - |
| IsBlocked | `bool` | No | - |
| SentAt | `DateTime` | No | - |
| Conversation | `Conversation` | No | Conversation |
| Sender | `ApplicationUser` | No | ApplicationUser |
| ReadReceiptsAsLastRead | `ICollection<ConversationReadReceipt>` | No | ConversationReadReceipt |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Conversation | `Conversation` | One / Zero-to-One | Inferred relation to `Conversation` |
| Sender | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| ReadReceiptsAsLastRead | `ICollection<ConversationReadReceipt>` | Many | Inferred relation to `ConversationReadReceipt` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "senderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "content": "sample_content",
  "messageType": {},
  "attachmentUrl": "sample_attachmenturl",
  "isBlocked": true,
  "sentAt": "2026-07-05T19:17:54Z"
}
```

---

## Notification

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| UserId | `Guid` | No | - |
| EventType | `string` | No | - |
| Title | `string` | No | - |
| Body | `string` | Yes | - |
| ReferenceId | `string` | Yes | - |
| ReferenceType | `string` | Yes | - |
| IsRead | `bool` | No | - |
| Channel | `NotificationChannel` | No | - |
| SentAt | `DateTime` | No | - |
| ReadAt | `DateTime` | Yes | - |
| User | `ApplicationUser` | No | ApplicationUser |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| User | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "eventType": "sample_eventtype",
  "title": "sample_title",
  "body": "sample_body",
  "referenceId": "sample_referenceid",
  "referenceType": "sample_referencetype",
  "isRead": true,
  "channel": {},
  "sentAt": "2026-07-05T19:17:54Z",
  "readAt": "2026-07-05T19:17:54Z"
}
```

---

## Operator

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| ProviderProfileId | `Guid` | No | - |
| FullName | `string` | No | - |
| YearsOfExperience | `int` | Yes | - |
| Specialization | `string` | Yes | - |
| LicenseType | `string` | Yes | - |
| LicenseNumber | `string` | Yes | - |
| LicenseExpiryDate | `DateOnly` | Yes | - |
| ProfilePhotoUrl | `string` | Yes | - |
| IsActive | `bool` | No | - |
| CreatedAt | `DateTime` | No | - |
| ProviderProfile | `ProviderProfile` | No | ProviderProfile |
| ServiceListingOperators | `ICollection<ServiceListingOperator>` | No | ServiceListingOperator |
| BlackoutDates | `ICollection<BlackoutDate>` | No | BlackoutDate |
| OperatorAssignments | `ICollection<OperatorAssignment>` | No | OperatorAssignment |
| Documents | `ICollection<Document>` | No | Document |
| Bookings | `ICollection<Booking>` | No | Booking |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| ProviderProfile | `ProviderProfile` | One / Zero-to-One | Inferred relation to `ProviderProfile` |
| ServiceListingOperators | `ICollection<ServiceListingOperator>` | Many | Inferred relation to `ServiceListingOperator` |
| BlackoutDates | `ICollection<BlackoutDate>` | Many | Inferred relation to `BlackoutDate` |
| OperatorAssignments | `ICollection<OperatorAssignment>` | Many | Inferred relation to `OperatorAssignment` |
| Documents | `ICollection<Document>` | Many | Inferred relation to `Document` |
| Bookings | `ICollection<Booking>` | Many | Inferred relation to `Booking` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "providerProfileId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fullName": "sample_fullname",
  "yearsOfExperience": 1,
  "specialization": "sample_specialization",
  "licenseType": "sample_licensetype",
  "licenseNumber": "01012345678",
  "licenseExpiryDate": "2026-07-05T19:17:54Z",
  "profilePhotoUrl": "sample_profilephotourl",
  "isActive": true,
  "createdAt": "2026-07-05T19:17:54Z"
}
```

---

## OperatorAssignment

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| OperatorId | `Guid` | No | - |
| BookingId | `Guid` | No | - |
| ScheduledStart | `DateTime` | No | - |
| ScheduledEnd | `DateTime` | No | - |
| Status | `OperatorAssignmentStatus` | No | - |
| CreatedAt | `DateTime` | No | - |
| Operator | `Operator` | No | Operator |
| Booking | `Booking` | No | Booking |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Operator | `Operator` | One / Zero-to-One | Inferred relation to `Operator` |
| Booking | `Booking` | One / Zero-to-One | Inferred relation to `Booking` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "operatorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "scheduledStart": "2026-07-05T19:17:54Z",
  "scheduledEnd": "2026-07-05T19:17:54Z",
  "status": {},
  "createdAt": "2026-07-05T19:17:54Z"
}
```

---

## PlatformSetting

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| SettingKey | `string` | No | - |
| SettingValue | `string` | No | - |
| Description | `string` | Yes | - |
| UpdatedByAdminId | `Guid` | Yes | - |
| UpdatedAt | `DateTime` | No | - |
| UpdatedByAdmin | `ApplicationUser` | Yes | ApplicationUser |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| UpdatedByAdmin | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |

### Example Object (JSON)
```json
{
  "settingKey": "sample_settingkey",
  "settingValue": "sample_settingvalue",
  "description": "sample_description",
  "updatedByAdminId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "updatedAt": "2026-07-05T19:17:54Z"
}
```

---

## ProviderIncident

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| ProviderProfileId | `Guid` | No | - |
| BookingId | `Guid` | Yes | - |
| IncidentType | `ProviderIncidentType` | No | - |
| PenaltyApplied | `bool` | No | - |
| AdminNote | `string` | Yes | - |
| OccurredAt | `DateTime` | No | - |
| ProviderProfile | `ProviderProfile` | No | ProviderProfile |
| Booking | `Booking` | Yes | Booking |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| ProviderProfile | `ProviderProfile` | One / Zero-to-One | Inferred relation to `ProviderProfile` |
| Booking | `Booking` | One / Zero-to-One | Inferred relation to `Booking` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "providerProfileId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "incidentType": {},
  "penaltyApplied": true,
  "adminNote": "sample_adminnote",
  "occurredAt": "2026-07-05T19:17:54Z"
}
```

---

## ProviderProfile

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| UserId | `Guid` | No | - |
| CompanyName | `string` | No | - |
| BusinessDescription | `string` | Yes | - |
| BaseLatitude | `decimal` | Yes | - |
| BaseLongitude | `decimal` | Yes | - |
| ServiceRadiusKm | `int` | No | - |
| ServiceZoneCenter | `Point` | Yes | - |
| ServiceZonePoly | `Geometry` | Yes | - |
| OnboardingTier | `int` | No | - |
| AverageRating | `decimal` | No | - |
| TotalReviewsCount | `int` | No | - |
| CompletedBookingsCount | `int` | No | - |
| ResponseRate | `decimal` | No | - |
| SearchRankingModifier | `decimal` | No | - |
| TrustScore | `decimal` | No | - |
| TrustLevel | `TrustLevel` | No | - |
| TrustScoreLastComputedAt | `DateTime` | Yes | - |
| CreatedAt | `DateTime` | No | - |
| UpdatedAt | `DateTime` | No | - |
| User | `ApplicationUser` | No | ApplicationUser |
| Operators | `ICollection<Operator>` | No | Operator |
| ServiceListings | `ICollection<ServiceListing>` | No | ServiceListing |
| TrustScoreHistory | `ICollection<ProviderTrustScoreHistory>` | No | ProviderTrustScoreHistory |
| Incidents | `ICollection<ProviderIncident>` | No | ProviderIncident |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| User | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| Operators | `ICollection<Operator>` | Many | Inferred relation to `Operator` |
| ServiceListings | `ICollection<ServiceListing>` | Many | Inferred relation to `ServiceListing` |
| TrustScoreHistory | `ICollection<ProviderTrustScoreHistory>` | Many | Inferred relation to `ProviderTrustScoreHistory` |
| Incidents | `ICollection<ProviderIncident>` | Many | Inferred relation to `ProviderIncident` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "companyName": "sample_companyname",
  "businessDescription": "sample_businessdescription",
  "baseLatitude": 99.99,
  "baseLongitude": 99.99,
  "serviceRadiusKm": 1,
  "serviceZoneCenter": 1,
  "serviceZonePoly": {},
  "onboardingTier": 1,
  "averageRating": 99.99,
  "totalReviewsCount": 1,
  "completedBookingsCount": 1,
  "responseRate": 99.99,
  "searchRankingModifier": 99.99,
  "trustScore": 99.99,
  "trustLevel": {},
  "trustScoreLastComputedAt": "2026-07-05T19:17:54Z",
  "createdAt": "2026-07-05T19:17:54Z",
  "updatedAt": "2026-07-05T19:17:54Z"
}
```

---

## ProviderTrustScoreHistory

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| ProviderProfileId | `Guid` | No | - |
| TrustScore | `decimal` | No | - |
| TrustLevel | `TrustLevel` | No | - |
| ComponentRating | `decimal` | Yes | - |
| ComponentCompletion | `decimal` | Yes | - |
| ComponentResponse | `decimal` | Yes | - |
| ComponentDocs | `decimal` | Yes | - |
| ComponentIncident | `decimal` | Yes | - |
| TriggerEvent | `string` | Yes | - |
| RecordedAt | `DateTime` | No | - |
| ProviderProfile | `ProviderProfile` | No | ProviderProfile |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| ProviderProfile | `ProviderProfile` | One / Zero-to-One | Inferred relation to `ProviderProfile` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "providerProfileId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "trustScore": 99.99,
  "trustLevel": {},
  "componentRating": 99.99,
  "componentCompletion": 99.99,
  "componentResponse": 99.99,
  "componentDocs": 99.99,
  "componentIncident": 99.99,
  "triggerEvent": "sample_triggerevent",
  "recordedAt": "2026-07-05T19:17:54Z"
}
```

---

## RefreshToken

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| UserId | `Guid` | No | - |
| Token | `string` | No | - |
| ExpiresAt | `DateTime` | No | - |
| IsRevoked | `bool` | No | - |
| CreatedAt | `DateTime` | No | - |
| User | `ApplicationUser` | No | ApplicationUser |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| User | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "token": "sample_token",
  "expiresAt": "2026-07-05T19:17:54Z",
  "isRevoked": true,
  "createdAt": "2026-07-05T19:17:54Z"
}
```

---

## Review

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| ReviewerId | `Guid` | No | - |
| ReviewedUserId | `Guid` | No | - |
| BookingId | `Guid` | Yes | - |
| ServiceListingId | `Guid` | Yes | - |
| MarketplaceOrderId | `Guid` | Yes | - |
| MarketplaceListingId | `Guid` | Yes | - |
| Rating | `int` | No | - |
| Comment | `string` | Yes | - |
| ModerationStatus | `ModerationStatus` | No | - |
| IsPublished | `bool` | No | - |
| CreatedAt | `DateTime` | No | - |
| PublishedAt | `DateTime` | Yes | - |
| Reviewer | `ApplicationUser` | No | ApplicationUser |
| ReviewedUser | `ApplicationUser` | No | ApplicationUser |
| Booking | `Booking` | Yes | Booking |
| ServiceListing | `ServiceListing` | Yes | ServiceListing |
| MarketplaceOrder | `MarketplaceOrder` | Yes | MarketplaceOrder |
| MarketplaceListing | `MarketplaceListing` | Yes | MarketplaceListing |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Reviewer | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| ReviewedUser | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| Booking | `Booking` | One / Zero-to-One | Inferred relation to `Booking` |
| ServiceListing | `ServiceListing` | One / Zero-to-One | Inferred relation to `ServiceListing` |
| MarketplaceOrder | `MarketplaceOrder` | One / Zero-to-One | Inferred relation to `MarketplaceOrder` |
| MarketplaceListing | `MarketplaceListing` | One / Zero-to-One | Inferred relation to `MarketplaceListing` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "reviewerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "reviewedUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "serviceListingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "marketplaceOrderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "marketplaceListingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "rating": 1,
  "comment": "sample_comment",
  "moderationStatus": {},
  "isPublished": true,
  "createdAt": "2026-07-05T19:17:54Z",
  "publishedAt": "2026-07-05T19:17:54Z"
}
```

---

## SearchQueryLog

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| UserId | `Guid` | Yes | - |
| SessionId | `string` | Yes | - |
| RawQuery | `string` | No | - |
| ExtractedIntentJson | `string` | Yes | - |
| ContextDomain | `SearchContextDomain` | No | - |
| SearchMode | `SearchMode` | No | - |
| ResultCount | `int` | No | - |
| HasLowConfidence | `bool` | No | - |
| HasZeroResults | `bool` | No | - |
| AlternativeSuggested | `bool` | No | - |
| AlternativeAcceptedAt | `DateTime` | Yes | - |
| ProcessingMs | `int` | Yes | - |
| CreatedAt | `DateTime` | No | - |
| User | `ApplicationUser` | Yes | ApplicationUser |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| User | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "sessionId": "sample_sessionid",
  "rawQuery": "sample_rawquery",
  "extractedIntentJson": "sample_extractedintentjson",
  "contextDomain": {},
  "searchMode": {},
  "resultCount": 1,
  "hasLowConfidence": true,
  "hasZeroResults": true,
  "alternativeSuggested": true,
  "alternativeAcceptedAt": "2026-07-05T19:17:54Z",
  "processingMs": 1,
  "createdAt": "2026-07-05T19:17:54Z"
}
```

---

## ServiceListing

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| ProviderProfileId | `Guid` | No | - |
| CategoryId | `int` | No | - |
| Title | `string` | No | - |
| Description | `string` | No | - |
| Tags | `string` | Yes | - |
| EquipmentModel | `string` | Yes | - |
| EquipmentCapacity | `string` | Yes | - |
| EquipmentCondition | `EquipmentCondition` | Yes | - |
| YearOfManufacture | `int` | Yes | - |
| EquipmentRegistrationNumber | `string` | Yes | - |
| HourlyRate | `decimal` | Yes | - |
| DailyRate | `decimal` | Yes | - |
| MinimumBookingHours | `int` | No | - |
| Status | `ServiceListingStatus` | No | - |
| AiRiskScore | `int` | Yes | - |
| AiRiskLevel | `string` | Yes | - |
| AiRiskFlags | `string` | Yes | - |
| AiRecommendation | `string` | Yes | - |
| QualityScore | `int` | Yes | - |
| QdrantPointId | `string` | Yes | - |
| LastEmbeddedAt | `DateTime` | Yes | - |
| EmbeddingStatus | `EmbeddingStatus` | No | - |
| AdminRejectionNote | `string` | Yes | - |
| RejectedByAdminId | `Guid` | Yes | - |
| RejectedAt | `DateTime` | Yes | - |
| SubmissionCount | `int` | No | - |
| QsDescription | `int` | Yes | - |
| QsPhotos | `int` | Yes | - |
| QsSpecs | `int` | Yes | - |
| QsPricing | `int` | Yes | - |
| QsOperator | `int` | Yes | - |
| QsDocs | `int` | Yes | - |
| QsZone | `int` | Yes | - |
| QualityScoreComputedAt | `DateTime` | Yes | - |
| CreatedAt | `DateTime` | No | - |
| UpdatedAt | `DateTime` | No | - |
| ProviderProfile | `ProviderProfile` | No | ProviderProfile |
| Category | `Category` | No | Category |
| RejectedByAdmin | `ApplicationUser` | Yes | ApplicationUser |
| Governorate | `string` | Yes | - |
| Region | `string` | Yes | - |
| Photos | `ICollection<ServiceListingPhoto>` | No | ServiceListingPhoto |
| ServiceListingOperators | `ICollection<ServiceListingOperator>` | No | ServiceListingOperator |
| Availability | `ICollection<ServiceListingAvailability>` | No | ServiceListingAvailability |
| BlackoutDates | `ICollection<BlackoutDate>` | No | BlackoutDate |
| Documents | `ICollection<Document>` | No | Document |
| Bookings | `ICollection<Booking>` | No | Booking |
| Reviews | `ICollection<Review>` | No | Review |
| Conversations | `ICollection<Conversation>` | No | Conversation |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| ProviderProfile | `ProviderProfile` | One / Zero-to-One | Inferred relation to `ProviderProfile` |
| Category | `Category` | One / Zero-to-One | Inferred relation to `Category` |
| RejectedByAdmin | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| Photos | `ICollection<ServiceListingPhoto>` | Many | Inferred relation to `ServiceListingPhoto` |
| ServiceListingOperators | `ICollection<ServiceListingOperator>` | Many | Inferred relation to `ServiceListingOperator` |
| Availability | `ICollection<ServiceListingAvailability>` | Many | Inferred relation to `ServiceListingAvailability` |
| BlackoutDates | `ICollection<BlackoutDate>` | Many | Inferred relation to `BlackoutDate` |
| Documents | `ICollection<Document>` | Many | Inferred relation to `Document` |
| Bookings | `ICollection<Booking>` | Many | Inferred relation to `Booking` |
| Reviews | `ICollection<Review>` | Many | Inferred relation to `Review` |
| Conversations | `ICollection<Conversation>` | Many | Inferred relation to `Conversation` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "providerProfileId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "categoryId": 1,
  "title": "sample_title",
  "description": "sample_description",
  "tags": "sample_tags",
  "equipmentModel": "sample_equipmentmodel",
  "equipmentCapacity": "sample_equipmentcapacity",
  "equipmentCondition": {},
  "yearOfManufacture": 1,
  "equipmentRegistrationNumber": "01012345678",
  "hourlyRate": 99.99,
  "dailyRate": 99.99,
  "minimumBookingHours": 1,
  "status": [],
  "aiRiskScore": 1,
  "aiRiskLevel": "sample_airisklevel",
  "aiRiskFlags": "sample_airiskflags",
  "aiRecommendation": "sample_airecommendation",
  "qualityScore": 1,
  "qdrantPointId": "sample_qdrantpointid",
  "lastEmbeddedAt": "2026-07-05T19:17:54Z",
  "embeddingStatus": {},
  "adminRejectionNote": "sample_adminrejectionnote",
  "rejectedByAdminId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "rejectedAt": "2026-07-05T19:17:54Z",
  "submissionCount": 1,
  "qsDescription": 1,
  "qsPhotos": 1,
  "qsSpecs": 1,
  "qsPricing": 1,
  "qsOperator": 1,
  "qsDocs": 1,
  "qsZone": 1,
  "qualityScoreComputedAt": "2026-07-05T19:17:54Z",
  "createdAt": "2026-07-05T19:17:54Z",
  "updatedAt": "2026-07-05T19:17:54Z",
  "governorate": "sample_governorate",
  "region": "sample_region"
}
```

---

## ServiceListingAvailability

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| ListingId | `Guid` | No | - |
| DayOfWeek | `int` | No | - |
| OpenTime | `TimeOnly` | No | - |
| CloseTime | `TimeOnly` | No | - |
| Listing | `ServiceListing` | No | ServiceListing |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Listing | `ServiceListing` | One / Zero-to-One | Inferred relation to `ServiceListing` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "listingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "dayOfWeek": 1,
  "openTime": "2026-07-05T19:17:54Z",
  "closeTime": "2026-07-05T19:17:54Z"
}
```

---

## ServiceListingOperator

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| ListingId | `Guid` | No | - |
| OperatorId | `Guid` | No | - |
| Listing | `ServiceListing` | No | ServiceListing |
| Operator | `Operator` | No | Operator |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Listing | `ServiceListing` | One / Zero-to-One | Inferred relation to `ServiceListing` |
| Operator | `Operator` | One / Zero-to-One | Inferred relation to `Operator` |

### Example Object (JSON)
```json
{
  "listingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "operatorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

---

## ServiceListingPhoto

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| ListingId | `Guid` | No | - |
| PhotoUrl | `string` | No | - |
| DisplayOrder | `int` | No | - |
| CreatedAt | `DateTime` | No | - |
| Listing | `ServiceListing` | No | ServiceListing |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Listing | `ServiceListing` | One / Zero-to-One | Inferred relation to `ServiceListing` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "listingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "photoUrl": "sample_photourl",
  "displayOrder": 1,
  "createdAt": "2026-07-05T19:17:54Z"
}
```

---

## Ticket

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| TicketNumber | `string` | No | - |
| SubmittedById | `Guid` | No | - |
| AssignedToUserId | `Guid` | Yes | - |
| ResolvedByUserId | `Guid` | Yes | - |
| BookingId | `Guid` | Yes | - |
| MarketplaceOrderId | `Guid` | Yes | - |
| Subject | `string` | No | - |
| Category | `TicketCategory` | No | - |
| Priority | `int` | No | - |
| Status | `TicketStatus` | No | - |
| AiSummary | `string` | Yes | - |
| AiIdentifiedIssue | `string` | Yes | - |
| AiClaimedImpact | `string` | Yes | - |
| AiEscalationPriority | `int` | Yes | - |
| ResolutionType | `TicketResolutionType` | Yes | - |
| AdminResolution | `string` | Yes | - |
| AdminResponseDeadline | `DateTime` | Yes | - |
| EscalatedAt | `DateTime` | Yes | - |
| EscalatedToAdminId | `Guid` | Yes | - |
| FirstResponseAt | `DateTime` | Yes | - |
| ResolvedAt | `DateTime` | Yes | - |
| ClosedAt | `DateTime` | Yes | - |
| ReopenedAt | `DateTime` | Yes | - |
| CreatedAt | `DateTime` | No | - |
| UpdatedAt | `DateTime` | No | - |
| SubmittedBy | `ApplicationUser` | No | ApplicationUser |
| AssignedToUser | `ApplicationUser` | Yes | ApplicationUser |
| ResolvedByUser | `ApplicationUser` | Yes | ApplicationUser |
| EscalatedToAdmin | `ApplicationUser` | Yes | ApplicationUser |
| Booking | `Booking` | Yes | Booking |
| MarketplaceOrder | `MarketplaceOrder` | Yes | MarketplaceOrder |
| Messages | `ICollection<TicketMessage>` | No | TicketMessage |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| SubmittedBy | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| AssignedToUser | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| ResolvedByUser | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| EscalatedToAdmin | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| Booking | `Booking` | One / Zero-to-One | Inferred relation to `Booking` |
| MarketplaceOrder | `MarketplaceOrder` | One / Zero-to-One | Inferred relation to `MarketplaceOrder` |
| Messages | `ICollection<TicketMessage>` | Many | Inferred relation to `TicketMessage` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "ticketNumber": "01012345678",
  "submittedById": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "assignedToUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "resolvedByUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "marketplaceOrderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "subject": "sample_subject",
  "category": {},
  "priority": 1,
  "status": {},
  "aiSummary": "sample_aisummary",
  "aiIdentifiedIssue": "sample_aiidentifiedissue",
  "aiClaimedImpact": "sample_aiclaimedimpact",
  "aiEscalationPriority": 1,
  "resolutionType": {},
  "adminResolution": "sample_adminresolution",
  "adminResponseDeadline": "2026-07-05T19:17:54Z",
  "escalatedAt": "2026-07-05T19:17:54Z",
  "escalatedToAdminId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstResponseAt": "2026-07-05T19:17:54Z",
  "resolvedAt": "2026-07-05T19:17:54Z",
  "closedAt": "2026-07-05T19:17:54Z",
  "reopenedAt": "2026-07-05T19:17:54Z",
  "createdAt": "2026-07-05T19:17:54Z",
  "updatedAt": "2026-07-05T19:17:54Z"
}
```

---

## TicketAttachment

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| TicketMessageId | `Guid` | No | - |
| UploadedByUserId | `Guid` | No | - |
| FileUrl | `string` | No | - |
| FileName | `string` | No | - |
| FileType | `AttachmentFileType` | No | - |
| CreatedAt | `DateTime` | No | - |
| TicketMessage | `TicketMessage` | No | TicketMessage |
| UploadedByUser | `ApplicationUser` | No | ApplicationUser |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| TicketMessage | `TicketMessage` | One / Zero-to-One | Inferred relation to `TicketMessage` |
| UploadedByUser | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "ticketMessageId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "uploadedByUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileUrl": "sample_fileurl",
  "fileName": "sample_filename",
  "fileType": {},
  "createdAt": "2026-07-05T19:17:54Z"
}
```

---

## TicketMessage

### Properties & Primitive Fields
| Property | Type | Nullable | Navigation Target |
| :--- | :--- | :---: | :--- |
| Id | `Guid` | No | - |
| TicketId | `Guid` | No | - |
| SenderId | `Guid` | No | - |
| Body | `string` | No | - |
| MessageType | `TicketMessageType` | No | - |
| IsInternal | `bool` | No | - |
| CreatedAt | `DateTime` | No | - |
| Ticket | `Ticket` | No | Ticket |
| Sender | `ApplicationUser` | No | ApplicationUser |
| Attachments | `ICollection<TicketAttachment>` | No | TicketAttachment |

### Navigation Properties & Relationships
| Property | Type | Multiplicity | Description |
| :--- | :--- | :--- | :--- |
| Ticket | `Ticket` | One / Zero-to-One | Inferred relation to `Ticket` |
| Sender | `ApplicationUser` | One / Zero-to-One | Inferred relation to `ApplicationUser` |
| Attachments | `ICollection<TicketAttachment>` | Many | Inferred relation to `TicketAttachment` |

### Example Object (JSON)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "ticketId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "senderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "body": "sample_body",
  "messageType": {},
  "isInternal": true,
  "createdAt": "2026-07-05T19:17:54Z"
}
```

---

