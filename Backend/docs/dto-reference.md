# DTO (Data Transfer Object) Reference

A catalog of standard DTO definitions used by the API layer to convey responses back to the application.

## AcceptBookingResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| BookingNumber | `string` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| AssignedOperatorName | `string` | `string` |
| Message | `string` | `string` |

- **Inferred Entity Map**: `Booking`

---

## AddressDTO
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| Label | `string` | `string` |
| Governorate | `string` | `string` |
| District | `string` | `string` |
| Street | `string` | `string` |
| Latitude | `decimal?` | `number` |
| Longitude | `decimal?` | `number` |
| IsDefault | `bool` | `boolean` |

- **Inferred Entity Map**: `Address`

---

## AddressDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| Label | `string?` | `string` |
| Governorate | `string` | `string` |
| District | `string` | `string` |
| Street | `string?` | `string` |
| Latitude | `decimal?` | `number` |
| Longitude | `decimal?` | `number` |
| IsDefault | `bool` | `boolean` |

- **Inferred Entity Map**: `Address`

---

## AdminDashboardSummaryDTO
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| TotalUsers | `int` | `number` |
| ActiveUsers | `int` | `number` |
| TotalProviders | `int` | `number` |
| PendingServiceListings | `int` | `number` |
| PendingMarketplaceListings | `int` | `number` |
| PendingDocuments | `int` | `number` |
| ActiveBookings | `int` | `number` |
| OpenTickets | `int` | `number` |
| DisputedBookings | `int` | `number` |
| EscrowFrozenCount | `int` | `number` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## AdminDisputeDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| Type | `string` | `string` |
| ReferenceNumber | `string` | `string` |
| CustomerName | `string` | `string` |
| ProviderName | `string` | `string` |
| Amount | `decimal` | `number` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| CreatedAt | `DateTime` | `string` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## AdminDocumentDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| DocumentId | `Guid` | `string` |
| DocumentType | `string` | `string` |
| FileUrl | `string` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| UploadedAt | `DateTime` | `string` |
| ExpiryDate | `DateOnly?` | `string` |
| ExpiryStatus | `string?` | `string` |
| ExtractedText | `string?` | `string` |
| ConfidenceScore | `decimal?` | `number` |
| KeyFieldsPresent | `bool?` | `boolean` |
| FailureReason | `string?` | `string` |
| AdminNote | `string?` | `string` |
| User | `AdminDocumentUserDto?` | `object` |
| LinkedEntity | `AdminDocumentLinkedEntityDto?` | `object` |

- **Inferred Entity Map**: `Document`

---

## AdminDocumentLinkedEntityDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Type | `string` | `string` |
| Id | `Guid` | `string` |
| Title | `string` | `string` |

- **Inferred Entity Map**: `Document`

---

## AdminDocumentUserDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| DisplayName | `string` | `string` |
| Role | `string` | `string` |

- **Inferred Entity Map**: `Document`

---

## AdminTicketDetailsDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| TicketNumber | `string` | `string` |
| Subject | `string` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| SubmittedBy | `TicketSubmitterDto` | `object` |
| Messages | `List<TicketMessageDto>` | `array` |
| LinkedBooking | `LinkedBookingDto` | `object` |
| LinkedMarketplaceOrder | `LinkedMarketplaceOrderDto` | `object` |

- **Inferred Entity Map**: `Ticket`

---

## AdminTicketDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| TicketNumber | `string` | `string` |
| Subject | `string` | `string` |
| Category | `string` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| SubmittedByName | `string` | `string` |
| CreatedAt | `DateTime` | `string` |
| Items | `List<AdminTicketDto>` | `number` |
| TotalCount | `int` | `number` |
| Page | `int` | `number` |
| PageSize | `int` | `number` |

- **Inferred Entity Map**: `Ticket`

---

## BookingActionsDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| CanAccept | `bool` | `boolean` |
| CanReject | `bool` | `boolean` |
| CanStart | `bool` | `boolean` |
| CanComplete | `bool` | `boolean` |
| CanConfirmCompletion | `bool` | `boolean` |
| CanDispute | `bool` | `boolean` |
| CanCancel | `bool` | `boolean` |
| CanProviderCancel | `bool` | `boolean` |
| CanPay | `bool` | `boolean` |

- **Inferred Entity Map**: `Booking`

---

## BookingCreateContextAddressDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| Label | `string?` | `string` |
| Governorate | `string` | `string` |
| District | `string` | `string` |
| Street | `string?` | `string` |
| Latitude | `decimal?` | `number` |
| Longitude | `decimal?` | `number` |
| IsDefault | `bool` | `boolean` |

- **Inferred Entity Map**: `Address`

---

## BookingCreateContextAvailabilityDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| DayOfWeek | `int` | `number` |
| DayName | `string` | `string` |
| DayNameAr | `string` | `string` |
| OpenTime | `TimeOnly` | `string` |
| CloseTime | `TimeOnly` | `string` |

- **Inferred Entity Map**: `Booking`

---

## BookingCreateContextDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| ServiceListingId | `Guid` | `string` |
| ServiceTitle | `string` | `string` |
| ProviderCompany | `string` | `string` |
| HourlyRate | `decimal?` | `number` |
| DailyRate | `decimal?` | `number` |
| MinimumBookingHours | `int` | `number` |
| Availability | `IReadOnlyList<BookingCreateContextAvailabilityDto>` | `array` |
| DefaultAddress | `BookingCreateContextAddressDto?` | `object` |
| CustomerEligibility | `BookingCustomerEligibilityDto` | `object` |

- **Inferred Entity Map**: `Booking`

---

## BookingCustomerEligibilityDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| CanBook | `bool` | `boolean` |
| MissingRequirements | `IReadOnlyList<string>` | `string` |

- **Inferred Entity Map**: `Booking`

---

## BookingDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| BookingNumber | `string` | `string` |
| ServiceListingId | `Guid` | `string` |
| ServiceTitle | `string` | `string` |
| CustomerName | `string` | `string` |
| ProviderCompany | `string` | `string` |
| OperatorName | `string?` | `string` |
| AvailableActions | `BookingActionsDto` | `object` |
| Timeline | `IReadOnlyList<BookingTimelineItemDto>` | `string` |
| EscrowStatus | `string?` | `string` |
| EscrowStatusAr | `string?` | `string` |
| CustomerId | `Guid` | `string` |
| JobTitle | `string` | `string` |
| JobDescription | `string?` | `string` |
| Governorate | `string` | `string` |
| District | `string` | `string` |
| Street | `string?` | `string` |
| Latitude | `decimal?` | `number` |
| Longitude | `decimal?` | `number` |
| RequestedStartDate | `DateOnly` | `string` |
| RequestedStartTime | `TimeOnly` | `string` |
| EstimatedDurationHours | `decimal` | `number` |
| HourlyRateSnapshot | `decimal` | `number` |
| EstimatedTotal | `decimal` | `number` |
| SurchargeAmount | `decimal?` | `number` |
| IsOutOfZoneBooking | `bool` | `boolean` |
| OutOfZoneDistanceKm | `decimal?` | `number` |
| OutOfZoneSurchargeAmount | `decimal?` | `number` |
| OutOfZoneSurchargeAcceptedAt | `DateTime?` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| AssignedOperatorId | `Guid?` | `string` |
| ProviderRejectionReason | `string?` | `string` |
| CancellationReason | `string?` | `string` |
| CreatedAt | `DateTime` | `string` |
| ConfirmedAt | `DateTime?` | `string` |
| PaymentCapturedAt | `DateTime?` | `string` |
| StartedAt | `DateTime?` | `string` |
| CompletedMarkedAt | `DateTime?` | `string` |
| CompletionConfirmedAt | `DateTime?` | `string` |
| DisputeOpenedAt | `DateTime?` | `string` |

- **Inferred Entity Map**: `Booking`

---

## BookingEscrowDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| BookingId | `Guid` | `string` |
| GrossAmount | `decimal` | `number` |
| PlatformCommission | `decimal` | `number` |
| ProviderPayout | `decimal` | `number` |
| VatAmount | `decimal` | `number` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| CapturedAt | `DateTime?` | `string` |
| ReleasedAt | `DateTime?` | `string` |
| FrozenAt | `DateTime?` | `string` |

- **Inferred Entity Map**: `Booking`

---

## BookingNextActionDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Label | `string` | `string` |
| LabelAr | `string` | `string` |
| ActionKey | `string?` | `string` |

- **Inferred Entity Map**: `Booking`

---

## BookingPaymentCheckoutResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| BookingId | `Guid` | `string` |
| Amount | `decimal` | `number` |
| Currency | `string` | `string` |
| PaymentProvider | `string` | `string` |
| CheckoutUrl | `string` | `string` |
| Status | `string` | `string` |
| PaymentGatewayReference | `string` | `string` |

- **Inferred Entity Map**: `Booking`

---

## BookingPaymentConfirmResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| BookingId | `Guid` | `string` |
| BookingStatus | `string` | `string` |
| BookingStatusAr | `string` | `string` |
| EscrowStatus | `string` | `string` |
| EscrowStatusAr | `string` | `string` |
| Message | `string` | `string` |

- **Inferred Entity Map**: `Booking`

---

## BookingTimelineItemDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Key | `string` | `string` |
| Label | `string` | `string` |
| LabelAr | `string` | `string` |
| Date | `DateTime?` | `string` |
| Done | `bool` | `boolean` |

- **Inferred Entity Map**: `Booking`

---

## BookingTrackerDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| BookingId | `Guid` | `string` |
| BookingNumber | `string` | `string` |
| CurrentStatus | `string` | `string` |
| CurrentStatusAr | `string` | `string` |
| Timeline | `IReadOnlyList<BookingTimelineItemDto>` | `string` |
| NextAction | `BookingNextActionDto` | `object` |
| AvailableActions | `BookingActionsDto` | `object` |

- **Inferred Entity Map**: `Booking`

---

## CancelBookingResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| BookingId | `Guid` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| RefundPercentage | `decimal` | `number` |
| Message | `string` | `string` |

- **Inferred Entity Map**: `Booking`

---

## CategoryDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `int` | `number` |
| Name | `string` | `string` |
| Slug | `string` | `string` |
| Type | `string` | `string` |
| ParentId | `int?` | `number` |

- **Inferred Entity Map**: `Category`

---

## CompleteBookingByProviderResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| BookingId | `Guid` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| EvidenceFormId | `Guid` | `string` |
| Message | `string` | `string` |

- **Inferred Entity Map**: `Booking`

---

## ConfirmBookingCompletionResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| BookingId | `Guid` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| Message | `string` | `string` |

- **Inferred Entity Map**: `Booking`

---

## ConversationListItemDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| Title | `string` | `string` |
| ContextType | `string` | `string` |
| ReferenceId | `Guid?` | `string` |
| OtherPartyName | `string` | `string` |
| LastMessagePreview | `string?` | `string` |
| LastMessageAt | `DateTime?` | `string` |
| UnreadCount | `int` | `number` |
| IsLocked | `bool` | `boolean` |
| Items | `List<ConversationListItemDto>` | `array` |
| TotalCount | `int` | `number` |

- **Inferred Entity Map**: `Conversation`

---

## CreateAddressDTO
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| Label | `string` | `string` |
| IsDefault | `bool` | `boolean` |

- **Inferred Entity Map**: `Address`

---

## CreateBookingResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| BookingNumber | `string` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| ServiceTitle | `string` | `string` |
| ProviderCompany | `string` | `string` |
| RequestedStartDate | `DateOnly` | `string` |
| RequestedStartTime | `TimeOnly` | `string` |
| EstimatedDurationHours | `decimal` | `number` |
| HourlyRateSnapshot | `decimal` | `number` |
| EstimatedTotal | `decimal` | `number` |
| Message | `string` | `string` |

- **Inferred Entity Map**: `Booking`

---

## CreateServiceListingResultDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| NextStep | `string` | `string` |
| Message | `string` | `string` |

- **Inferred Entity Map**: `ServiceListing`

---

## CreateTimeAdjustmentResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| BookingId | `Guid` | `string` |
| BookingNumber | `string` | `string` |
| RequestedAdditionalHrs | `decimal` | `number` |
| AdditionalCostAmount | `decimal` | `number` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| ProviderNote | `string` | `string` |
| Message | `string` | `string` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## CustomerBookingListItemDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| BookingNumber | `string` | `string` |
| ServiceTitle | `string` | `string` |
| ProviderCompany | `string` | `string` |
| RequestedStartDate | `DateOnly` | `string` |
| RequestedStartTime | `TimeOnly` | `string` |
| EstimatedDurationHours | `decimal` | `number` |
| EstimatedTotal | `decimal` | `number` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| CanCancel | `bool` | `boolean` |
| CanConfirmCompletion | `bool` | `boolean` |
| CanDispute | `bool` | `boolean` |

- **Inferred Entity Map**: `Booking`

---

## CustomerBookingsResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Items | `IReadOnlyList<CustomerBookingListItemDto>` | `array` |
| TotalCount | `int` | `number` |

- **Inferred Entity Map**: `Booking`

---

## CustomerDashboardSummaryDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| ActiveBookings | `int` | `number` |
| PendingBookings | `int` | `number` |
| CompletedBookings | `int` | `number` |
| OpenDisputes | `int` | `number` |
| MarketplacePurchases | `int` | `number` |
| UnreadNotifications | `int` | `number` |
| TrustScore | `decimal` | `number` |
| RequiresAdditionalVerification | `bool` | `boolean` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## CustomerProfileDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| UserId | `Guid` | `string` |
| DisplayName | `string` | `string` |
| FirstName | `string` | `string` |
| LastName | `string` | `string` |
| UserName | `string` | `string` |
| Email | `string?` | `string` |
| PhoneNumber | `string?` | `string` |
| IsPhoneVerified | `bool` | `boolean` |
| DefaultAddress | `AddressDto?` | `object` |
| Id | `Guid` | `string` |
| TrustScore | `decimal` | `number` |
| CancellationRate | `decimal?` | `number` |
| DisputeFrequencyScore | `decimal?` | `number` |
| PaymentFailureCount | `int` | `number` |
| ReviewAuthenticityScore | `decimal?` | `number` |
| RequiresAdditionalVerification | `bool` | `boolean` |
| TotalBookings | `int` | `number` |
| TrustScoreLastComputedAt | `DateTime?` | `string` |
| CreatedAt | `DateTime` | `string` |
| UpdatedAt | `DateTime` | `string` |

- **Inferred Entity Map**: `CustomerProfile`

---

## CustomerTrustHistoryDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| TrustScore | `decimal` | `number` |
| TriggerEvent | `string?` | `string` |
| RecordedAt | `DateTime` | `string` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## DeleteAddressDTO
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| IsSuccess | `bool` | `boolean` |
| Message | `string` | `string` |
| StatusCode | `int` | `number` |
| Notice | `string` | `string` |

- **Inferred Entity Map**: `Address`

---

## DisputeBookingResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| BookingId | `Guid` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| TicketId | `Guid?` | `string` |
| Message | `string` | `string` |

- **Inferred Entity Map**: `Booking`

---

## DocumentDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| UserId | `Guid?` | `string` |
| ServiceListingId | `Guid?` | `string` |
| MarketplaceListingId | `Guid?` | `string` |
| OperatorId | `Guid?` | `string` |
| DocumentType | `DocumentType` | `object` |
| FileUrl | `string` | `string` |
| Status | `DocumentVerificationStatus` | `object` |
| StatusAr | `string` | `string` |
| ExpiryDate | `DateOnly?` | `string` |
| FailureReason | `string?` | `string` |
| UploadedAt | `DateTime` | `string` |
| VerifiedAt | `DateTime?` | `string` |

- **Inferred Entity Map**: `Document`

---

## EmployeeProfileDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| UserId | `Guid` | `string` |
| FirstName | `string` | `string` |
| LastName | `string` | `string` |
| UserName | `string` | `string` |
| Email | `string?` | `string` |
| PhoneNumber | `string?` | `string` |
| Id | `Guid` | `string` |
| EmployeeCode | `string` | `string` |
| Department | `string?` | `string` |
| AssignedGovernorate | `string?` | `string` |
| IsAvailableForDispatch | `bool` | `boolean` |
| TotalVerificationsCompleted | `int` | `number` |
| TotalTicketsHandled | `int` | `number` |
| CreatedAt | `DateTime` | `string` |
| UpdatedAt | `DateTime` | `string` |

- **Inferred Entity Map**: `EmployeeProfile`

---

## LinkedBookingDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| BookingReference | `string` | `string` |

- **Inferred Entity Map**: `Booking`

---

## LinkedMarketplaceOrderDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| OrderNumber | `string` | `string` |

- **Inferred Entity Map**: `MarketplaceOrder`

---

## ListingManagementInfoDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| AiRiskScore | `int?` | `number` |
| AiRiskLevel | `string?` | `string` |
| AiRiskFlags | `string?` | `string` |
| AdminRejectionNote | `string?` | `string` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## ListingSellerDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| DisplayName | `string` | `string` |
| AverageRating | `decimal?` | `number` |
| TotalReviewsCount | `int?` | `number` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## MarketPlaceListingDTO
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| Title | `string` | `string` |
| Price | `decimal` | `number` |
| Condition | `string` | `string` |
| ConditionAr | `string` | `string` |
| Location | `string?` | `string` |
| CoverPhotoUrl | `string?` | `string` |
| SellerName | `string` | `string` |
| CategoryName | `string` | `string` |
| TransactionMethod | `string` | `string` |
| AverageRating | `decimal?` | `number` |
| TotalReviewsCount | `int?` | `number` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## MarketplaceEarningsSummaryDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| GrossSales | `decimal` | `number` |
| PlatformCommission | `decimal` | `number` |
| SellerPayout | `decimal` | `number` |
| HeldAmount | `decimal` | `number` |
| ReleasedAmount | `decimal` | `number` |
| CompletedOrdersCount | `int` | `number` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## MarketplaceListingDetailsDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| Title | `string` | `string` |
| Description | `string` | `string` |
| Price | `decimal` | `number` |
| Condition | `string` | `string` |
| ConditionAr | `string` | `string` |
| Specifications | `string?` | `string` |
| Photos | `List<MarketplaceListingPhotoDto>` | `array` |
| Seller | `ListingSellerDto` | `array` |
| CanBuyNow | `bool` | `boolean` |
| ManagementInfo | `ListingManagementInfoDto?` | `array` |

- **Inferred Entity Map**: `MarketplaceListing`

---

## MarketplaceListingPhotoDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| PhotoUrl | `string` | `string` |
| DisplayOrder | `int` | `number` |

- **Inferred Entity Map**: `MarketplaceListing`

---

## MarketplaceListingReviewDetailsDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| Title | `string` | `string` |
| Description | `string` | `string` |
| CategoryName | `string` | `string` |
| Seller | `ReviewSellerDto` | `object` |
| Price | `decimal` | `number` |
| Condition | `string` | `string` |
| ConditionAr | `string` | `string` |
| Photos | `List<ReviewPhotoDto>` | `array` |
| Documents | `List<ReviewDocumentDto>` | `array` |
| AiRiskScore | `int?` | `number` |
| AiRiskLevel | `string` | `string` |
| AiRiskFlags | `string` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |

- **Inferred Entity Map**: `MarketplaceListing`

---

## MarketplaceOrderDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| BuyerId | `Guid` | `string` |
| BuyerName | `string` | `string` |
| SellerId | `Guid` | `string` |
| SellerName | `string` | `string` |
| ListingId | `Guid` | `string` |
| ListingTitle | `string` | `string` |
| Amount | `decimal` | `number` |
| DeliveryAddress | `string?` | `string` |
| DeliveryPreference | `string?` | `string` |
| TrackingNumber | `string?` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| SellerConfirmedAt | `DateTime?` | `string` |
| DispatchedAt | `DateTime?` | `string` |
| DeliveredAt | `DateTime?` | `string` |
| ConfirmedByBuyerAt | `DateTime?` | `string` |
| CancellationReason | `string?` | `string` |
| CancelledAt | `DateTime?` | `string` |
| CancellationInitiatedByRole | `string?` | `string` |
| ReturnShippingCost | `decimal?` | `number` |
| ReturnShippingAcceptedByBuyerAt | `DateTime?` | `string` |
| CreatedAt | `DateTime` | `string` |
| ViewerRole | `string` | `string` |

- **Inferred Entity Map**: `MarketplaceOrder`

---

## MarketplaceOrderPaymentCheckoutResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| MarketplaceOrderId | `Guid` | `string` |
| OrderNumber | `string` | `string` |
| Amount | `decimal` | `number` |
| Currency | `string` | `string` |
| PaymentProvider | `string` | `string` |
| CheckoutUrl | `string` | `string` |
| Status | `string` | `string` |
| PaymentGatewayReference | `string` | `string` |

- **Inferred Entity Map**: `MarketplaceOrder`

---

## MarketplaceOrderPaymentConfirmResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| MarketplaceOrderId | `Guid` | `string` |
| OrderStatus | `string` | `string` |
| OrderStatusAr | `string` | `string` |
| EscrowStatus | `string` | `string` |
| EscrowStatusAr | `string` | `string` |
| Message | `string` | `string` |

- **Inferred Entity Map**: `MarketplaceOrder`

---

## MessageDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| SenderName | `string` | `string` |
| MessageType | `string` | `string` |
| Body | `string?` | `string` |
| CreatedAt | `DateTime` | `string` |
| ConversationId | `Guid` | `string` |
| Items | `List<MessageDto>` | `array` |
| TotalCount | `int` | `number` |

- **Inferred Entity Map**: `Message`

---

## NotificationItemDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| Title | `string` | `string` |
| Body | `string?` | `string` |
| EventType | `string` | `string` |
| ReferenceId | `string?` | `string` |
| ReferenceType | `string?` | `string` |
| IsRead | `bool` | `boolean` |
| SentAt | `DateTime` | `string` |
| Items | `List<NotificationItemDto>` | `array` |
| UnreadCount | `int` | `number` |
| TotalCount | `int` | `number` |

- **Inferred Entity Map**: `Notification`

---

## OperatorDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| ProviderProfileId | `Guid` | `string` |
| FullName | `string` | `string` |
| YearsOfExperience | `int?` | `number` |
| Specialization | `string?` | `string` |
| LicenseType | `string?` | `string` |
| LicenseNumber | `string?` | `string` |
| LicenseExpiryDate | `DateOnly?` | `string` |
| ProfilePhotoUrl | `string?` | `string` |
| IsActive | `bool` | `boolean` |
| CreatedAt | `DateTime` | `string` |

- **Inferred Entity Map**: `Operator`

---

## OrderTrackingActionsDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| CanSellerConfirm | `bool` | `boolean` |
| CanDispatch | `bool` | `boolean` |
| CanMarkDelivered | `bool` | `boolean` |
| CanComplete | `bool` | `boolean` |
| CanCancel | `bool` | `boolean` |
| CanDispute | `bool` | `boolean` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## OrderTrackingDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| OrderNumber | `string` | `string` |
| ListingTitle | `string` | `string` |
| BuyerName | `string` | `string` |
| SellerName | `string` | `string` |
| Amount | `decimal` | `number` |
| DeliveryPreference | `string?` | `string` |
| TrackingNumber | `string?` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| EscrowStatus | `string?` | `string` |
| Timeline | `List<OrderTrackingTimelineItemDto>` | `string` |
| AvailableActions | `OrderTrackingActionsDto` | `object` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## PendingActionDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Type | `string` | `string` |
| Title | `string` | `string` |
| ReferenceId | `Guid` | `string` |
| Priority | `string` | `string` |
| CreatedAt | `DateTime` | `string` |
| ActionUrl | `string` | `string` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## PendingMarketplaceListingDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| Title | `string` | `string` |
| OwnerName | `string` | `string` |
| CategoryName | `string` | `string` |
| Price | `decimal` | `number` |
| PhotosCount | `int` | `number` |
| AiRiskScore | `int?` | `number` |
| AiRiskLevel | `string` | `string` |
| AiRiskFlags | `string` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| SubmittedAt | `DateTime` | `string` |

- **Inferred Entity Map**: `MarketplaceListing`

---

## PendingServiceListingDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| Title | `string` | `string` |
| OwnerName | `string` | `string` |
| CategoryName | `string` | `string` |
| PhotosCount | `int` | `number` |
| OperatorsCount | `int` | `number` |
| DocumentsCount | `int` | `number` |
| QualityScore | `int` | `number` |
| AiRiskScore | `int?` | `number` |
| AiRiskLevel | `string` | `string` |
| AiRiskFlags | `string` | `string` |
| AiRecommendation | `string` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| SubmittedAt | `DateTime` | `string` |

- **Inferred Entity Map**: `ServiceListing`

---

## PendingVerificationDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| DocumentId | `Guid` | `string` |
| DocumentType | `string` | `string` |
| FileUrl | `string` | `string` |
| Status | `string` | `string` |
| UploadedAt | `DateTime` | `string` |
| User | `VerificationUserDto` | `object` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## ProfileCompletionContextDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Role | `string` | `string` |
| ProfileCompleted | `bool` | `boolean` |
| PhoneVerified | `bool` | `boolean` |
| HasDefaultAddress | `bool` | `boolean` |
| ProviderProfileCompleted | `bool?` | `boolean` |
| MissingRequirements | `List<string>` | `string` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## ProviderActiveJobItemDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| BookingNumber | `string` | `string` |
| ServiceTitle | `string` | `string` |
| CustomerName | `string` | `string` |
| OperatorName | `string` | `string` |
| ScheduledStart | `DateTime` | `string` |
| ScheduledEnd | `DateTime` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| CanStart | `bool` | `boolean` |
| CanComplete | `bool` | `boolean` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## ProviderActiveJobsResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Items | `IReadOnlyList<ProviderActiveJobItemDto>` | `array` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## ProviderBookingListItemDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| BookingId | `Guid` | `string` |
| BookingNumber | `string` | `string` |
| CustomerName | `string` | `string` |
| ServiceListingTitle | `string` | `string` |
| RequestedStartDate | `DateOnly` | `string` |
| RequestedStartTime | `TimeOnly` | `string` |
| EstimatedDurationHours | `decimal` | `number` |
| EstimatedTotal | `decimal` | `number` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| CanAccept | `bool` | `boolean` |
| CanReject | `bool` | `boolean` |

- **Inferred Entity Map**: `Booking`

---

## ProviderBookingRequestItemDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| BookingNumber | `string` | `string` |
| CustomerName | `string` | `string` |
| ServiceTitle | `string` | `string` |
| JobTitle | `string` | `string` |
| Location | `string` | `string` |
| RequestedStartDate | `DateOnly` | `string` |
| RequestedStartTime | `TimeOnly` | `string` |
| EstimatedDurationHours | `decimal` | `number` |
| EstimatedTotal | `decimal` | `number` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| CanAccept | `bool` | `boolean` |
| CanReject | `bool` | `boolean` |

- **Inferred Entity Map**: `Booking`

---

## ProviderBookingRequestsResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Items | `IReadOnlyList<ProviderBookingRequestItemDto>` | `array` |
| TotalCount | `int` | `number` |

- **Inferred Entity Map**: `Booking`

---

## ProviderMarketPlaceListingDTO
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| Title | `string` | `string` |
| Price | `decimal` | `number` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| PhotosCount | `int` | `number` |
| AiRiskScore | `int?` | `number` |
| AiRiskLevel | `string?` | `string` |
| CreatedAt | `DateTime` | `string` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## ProviderProfileCardDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| ProviderProfileId | `Guid` | `string` |
| CompanyName | `string` | `string` |
| AverageRating | `decimal` | `number` |
| TotalReviewsCount | `int` | `number` |
| CompletedBookingsCount | `int` | `number` |
| TrustScore | `decimal` | `number` |
| TrustLevel | `TrustLevel` | `object` |
| ActiveSince | `DateTime` | `string` |

- **Inferred Entity Map**: `ProviderProfile`

---

## ProviderProfileDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| UserId | `Guid` | `string` |
| FirstName | `string` | `string` |
| LastName | `string` | `string` |
| UserName | `string` | `string` |
| Email | `string?` | `string` |
| PhoneNumber | `string?` | `string` |
| Id | `Guid` | `string` |
| CompanyName | `string` | `string` |
| BusinessDescription | `string?` | `string` |
| BaseLatitude | `decimal?` | `number` |
| BaseLongitude | `decimal?` | `number` |
| ServiceRadiusKm | `int` | `number` |
| AverageRating | `decimal` | `number` |
| TotalReviewsCount | `int` | `number` |
| CompletedBookingsCount | `int` | `number` |
| ResponseRate | `decimal` | `number` |
| TrustScore | `decimal` | `number` |
| TrustLevel | `TrustLevel` | `object` |
| OnboardingTier | `int` | `number` |
| SearchRankingModifier | `decimal` | `number` |
| TrustScoreLastComputedAt | `DateTime?` | `string` |
| CreatedAt | `DateTime` | `string` |
| UpdatedAt | `DateTime` | `string` |

- **Inferred Entity Map**: `ProviderProfile`

---

## ProviderServiceListingsResultDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Items | `List<ProviderServiceListingDto>` | `array` |
| TotalCount | `int` | `number` |

- **Inferred Entity Map**: `ServiceListing`

---

## ProviderTrustHistoryDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| TrustScore | `decimal` | `number` |
| TrustLevel | `TrustLevel` | `object` |
| ComponentRating | `decimal?` | `number` |
| ComponentCompletion | `decimal?` | `number` |
| ComponentResponse | `decimal?` | `number` |
| ComponentDocs | `decimal?` | `number` |
| ComponentIncident | `decimal?` | `number` |
| TriggerEvent | `string?` | `string` |
| RecordedAt | `DateTime` | `string` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## RealtimeMessageDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| ConversationId | `Guid` | `string` |
| SenderId | `Guid` | `string` |
| SenderName | `string` | `string` |
| Body | `string?` | `string` |
| MessageType | `string` | `string` |
| SentAt | `DateTime` | `string` |

- **Inferred Entity Map**: `Message`

---

## RejectBookingResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| BookingNumber | `string` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| ProviderRejectionReason | `string` | `string` |

- **Inferred Entity Map**: `Booking`

---

## ReviewDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| ReviewerName | `string` | `string` |
| Rating | `int` | `number` |
| Comment | `string?` | `string` |
| CreatedAt | `DateTime` | `string` |
| Items | `List<ReviewDto>` | `array` |
| AverageRating | `decimal` | `number` |
| TotalCount | `int` | `number` |
| Id | `Guid` | `string` |
| Rating | `int` | `number` |
| IsPublished | `bool` | `boolean` |
| Message | `string` | `string` |

- **Inferred Entity Map**: `Review`

---

## ReviewPricingDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| HourlyRate | `decimal?` | `number` |
| DailyRate | `decimal?` | `number` |
| MinimumBookingHours | `int` | `number` |

- **Inferred Entity Map**: `Review`

---

## ReviewProviderDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| CompanyName | `string` | `string` |
| Email | `string` | `string` |
| PhoneNumber | `string` | `string` |
| Id | `Guid` | `string` |
| Url | `string` | `string` |
| Id | `Guid` | `string` |
| Name | `string` | `string` |
| Day | `string` | `string` |
| Hours | `string` | `string` |
| Id | `Guid` | `string` |
| Type | `string` | `string` |
| FileUrl | `string` | `string` |
| Id | `Guid` | `string` |
| Title | `string` | `string` |
| Description | `string` | `string` |
| CategoryName | `string` | `string` |
| Provider | `ReviewProviderDto` | `object` |
| Pricing | `ReviewPricingDto` | `object` |
| Photos | `List<ReviewPhotoDto>` | `array` |
| Operators | `List<ReviewOperatorDto>` | `array` |
| Availability | `List<ReviewAvailabilityDto>` | `array` |
| Documents | `List<ReviewDocumentDto>` | `array` |
| QualityScore | `int` | `number` |
| AiRiskScore | `int?` | `number` |
| AiRiskLevel | `string` | `string` |
| AiRiskFlags | `string` | `string` |
| AiRecommendation | `string` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |

- **Inferred Entity Map**: `Review`

---

## ReviewSellerDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| DisplayName | `string` | `string` |
| Email | `string` | `string` |

- **Inferred Entity Map**: `Review`

---

## SaleOrderDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| ListingId | `Guid` | `string` |
| ListingTitle | `string` | `string` |
| BuyerId | `Guid` | `string` |
| BuyerName | `string` | `string` |
| Amount | `decimal` | `number` |
| DeliveryAddress | `string?` | `string` |
| DeliveryPreference | `string?` | `string` |
| TrackingNumber | `string?` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| SellerConfirmedAt | `DateTime?` | `string` |
| DispatchedAt | `DateTime?` | `string` |
| DeliveredAt | `DateTime?` | `string` |
| ConfirmedByBuyerAt | `DateTime?` | `string` |
| CancellationReason | `string?` | `string` |
| CancelledAt | `DateTime?` | `string` |
| CancellationInitiatedByRole | `string?` | `string` |
| ReturnShippingCost | `decimal?` | `number` |
| ReturnShippingAcceptedByBuyerAt | `DateTime?` | `string` |
| CreatedAt | `DateTime` | `string` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## ServiceListingDetailDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| ProviderProfileId | `Guid` | `string` |
| CategoryId | `int` | `number` |
| Title | `string` | `string` |
| Description | `string` | `string` |
| Governorate | `string?` | `string` |
| Region | `string?` | `string` |
| Tags | `string?` | `string` |
| EquipmentModel | `string?` | `string` |
| EquipmentCapacity | `string?` | `string` |
| EquipmentCondition | `EquipmentCondition?` | `object` |
| YearOfManufacture | `int?` | `number` |
| EquipmentRegistrationNumber | `string?` | `string` |
| HourlyRate | `decimal?` | `number` |
| DailyRate | `decimal?` | `number` |
| MinimumBookingHours | `int` | `number` |
| Status | `ServiceListingStatus` | `array` |
| QualityScore | `int?` | `number` |
| CreatedAt | `DateTime` | `string` |
| UpdatedAt | `DateTime` | `string` |
| Photos | `ICollection<ServiceListingPhotoDto>` | `array` |
| Operators | `ICollection<ServiceListingOperatorDto>` | `array` |
| Availability | `ICollection<ServiceListingAvailabilityDto>` | `array` |

- **Inferred Entity Map**: `ServiceListing`

---

## ServiceListingDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| ProviderProfileId | `Guid` | `string` |
| CategoryId | `int` | `number` |
| Title | `string` | `string` |
| Governorate | `string?` | `string` |
| Region | `string?` | `string` |
| Tags | `string?` | `string` |
| EquipmentModel | `string?` | `string` |
| HourlyRate | `decimal?` | `number` |
| DailyRate | `decimal?` | `number` |
| MinimumBookingHours | `int` | `number` |
| Status | `ServiceListingStatus` | `array` |
| QualityScore | `int?` | `number` |
| CreatedAt | `DateTime` | `string` |

- **Inferred Entity Map**: `ServiceListing`

---

## ServiceListingOperatorDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| OperatorId | `Guid` | `string` |
| FullName | `string` | `string` |
| YearsOfExperience | `int?` | `number` |
| Specialization | `string?` | `string` |
| LicenseType | `string?` | `string` |

- **Inferred Entity Map**: `Operator`

---

## ServiceListingPhotoDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| ListingId | `Guid` | `string` |
| PhotoUrl | `string` | `string` |
| DisplayOrder | `int` | `number` |

- **Inferred Entity Map**: `ServiceListing`

---

## SetDefaultAddressDTO
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| IsSuccess | `bool` | `boolean` |
| Message | `string` | `string` |
| StatusCode | `int` | `number` |

- **Inferred Entity Map**: `Address`

---

## StartBookingResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| BookingNumber | `string` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| StartedAt | `DateTime?` | `string` |
| Message | `string` | `string` |

- **Inferred Entity Map**: `Booking`

---

## TicketListItemDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| TicketNumber | `string` | `string` |
| Subject | `string` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| CreatedAt | `DateTime` | `string` |
| Items | `List<TicketListItemDto>` | `array` |
| TotalCount | `int` | `number` |

- **Inferred Entity Map**: `Ticket`

---

## TicketMessageDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| SenderName | `string` | `string` |
| Body | `string` | `string` |
| CreatedAt | `DateTime` | `string` |
| Id | `Guid` | `string` |
| TicketNumber | `string` | `string` |
| Subject | `string` | `string` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| Messages | `List<TicketMessageDto>` | `array` |

- **Inferred Entity Map**: `Message`

---

## TicketSubmitterDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| DisplayName | `string` | `string` |

- **Inferred Entity Map**: `Ticket`

---

## TimeAdjustmentDecisionResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| BookingId | `Guid` | `string` |
| BookingNumber | `string` | `string` |
| RequestedAdditionalHrs | `decimal` | `number` |
| AdditionalCostAmount | `decimal` | `number` |
| BookingEstimatedDurationHours | `decimal` | `number` |
| BookingEstimatedTotal | `decimal` | `number` |
| Status | `string` | `string` |
| StatusAr | `string` | `string` |
| CustomerAcknowledgedAt | `DateTime?` | `string` |
| Message | `string` | `string` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## TimeAdjustmentPaymentCheckoutResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| TimeAdjustmentRequestId | `Guid` | `string` |
| BookingId | `Guid` | `string` |
| BookingNumber | `string` | `string` |
| Amount | `decimal` | `number` |
| Currency | `string` | `string` |
| PaymentProvider | `string` | `string` |
| CheckoutUrl | `string` | `string` |
| Status | `string` | `string` |
| PaymentGatewayReference | `string` | `string` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## TimeAdjustmentPaymentConfirmResponseDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| TimeAdjustmentRequestId | `Guid` | `string` |
| BookingId | `Guid` | `string` |
| BookingNumber | `string` | `string` |
| TimeAdjustmentStatus | `string` | `string` |
| TimeAdjustmentStatusAr | `string` | `string` |
| EscrowStatus | `string` | `string` |
| EscrowStatusAr | `string` | `string` |
| BookingEstimatedDurationHours | `decimal` | `number` |
| BookingEstimatedTotal | `decimal` | `number` |
| Message | `string` | `string` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## UpdateAddressDTO
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| IsSuccess | `bool` | `boolean` |
| Message | `string` | `string` |
| StatusCode | `int` | `number` |

- **Inferred Entity Map**: `Address`

---

## UpdateUserStatusDTO
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| IsActive | `bool` | `boolean` |
| StatusText | `string` | `string` |
| StatusAr | `string` | `string` |
| Message | `string` | `string` |
| IsSuccess | `bool` | `boolean` |
| StatusCode | `int` | `number` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## UserAdminDTO
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| DisplayName | `string` | `string` |
| Email | `string` | `string` |
| PhoneNumber | `string` | `string` |
| Role | `string` | `string` |
| IsActive | `bool` | `boolean` |
| TrustScore | `decimal?` | `number` |
| CreatedAt | `DateTime` | `string` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

## VerificationUserDto
| Property | Type | TypeScript Equivalent |
| :--- | :--- | :--- |
| Id | `Guid` | `string` |
| DisplayName | `string` | `string` |
| Role | `string` | `string` |

- **Inferred Entity Map**: `Unable to determine from source code`

---

