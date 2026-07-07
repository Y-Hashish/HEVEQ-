# Complete API Reference

All endpoints grouped by controller.

> [!NOTE]
> All request payloads should be sent with `Content-Type: application/json` unless indicated otherwise.

## AddressController
- **Route Prefix**: `api/address`
- **Default Class Auth**: `AllowAnonymous`

### HTTPGET `api/address/my`
- **Action Method**: `GetMyAddresses`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `List<AddressDTO>`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetMyAddressQuery`.

---

### HTTPPOST `api/address/create`
- **Action Method**: `CreateAddress`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Request Body**: `CreateAddressesCommand`
*Raw object, see body structure.*

**Response DTO**: `IActionResult`
**Example Request Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `Direct Controller Logic`.

---

### HTTPPUT `api/address/{id}`
- **Action Method**: `UpdateAddress`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Request Body**: `UpdateMyAddressCommand`
*Raw object, see body structure.*

**Response DTO**: `IActionResult`
**Example Request Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `Direct Controller Logic`.

---

### HTTPDELETE `api/address/{id}`
- **Action Method**: `DeleteAddress`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `Direct Controller Logic`.

---

### HTTPPATCH `api/address/{id}/set-default`
- **Action Method**: `SetDefaultAddress`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `Direct Controller Logic`.

---

## AdminAccountVerificationsController
- **Route Prefix**: `api/admin/account-verifications`
- **Default Class Auth**: `Admin`

### HTTPGET `api/admin/account-verifications`
- **Action Method**: `GetPendingVerifications`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Query Parameters**:
- `query` (`GetAccountVerificationsQuery`)

**Response DTO**: `PaginatedVerificationsResponse`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetAccountVerificationsQuery`.

---

## AdminDashboardController
- **Route Prefix**: `api/admin/`
- **Default Class Auth**: `Admin`

### HTTPGET `api/admin//dashboard/summary`
- **Action Method**: `GetSummary`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Response DTO**: `AdminDashboardSummaryDTO`
| Property | Type |
| :--- | :--- |
| TotalUsers | `int` |
| ActiveUsers | `int` |
| TotalProviders | `int` |
| PendingServiceListings | `int` |
| PendingMarketplaceListings | `int` |
| PendingDocuments | `int` |
| ActiveBookings | `int` |
| OpenTickets | `int` |
| DisputedBookings | `int` |
| EscrowFrozenCount | `int` |

**Example Response Payload**:
```json
{
  "totalUsers": 1,
  "activeUsers": 1,
  "totalProviders": 1,
  "pendingServiceListings": 1,
  "pendingMarketplaceListings": 1,
  "pendingDocuments": 1,
  "activeBookings": 1,
  "openTickets": 1,
  "disputedBookings": 1,
  "escrowFrozenCount": 1
}
```

**Business Notes**:
- Automatically handled by MediatR request `GetDashboardSummaryQuery`.

---

### HTTPGET `api/admin//pending-actions`
- **Action Method**: `GetPendingActions`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Query Parameters**:
- `query` (`GetPendingActionsQuery`)

**Response DTO**: `PaginatedPendingActionsResponse`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetPendingActionsQuery`.

---

## AdminDisputesController
- **Route Prefix**: `api/admin/disputes`
- **Default Class Auth**: `Amdin`

### HTTPGET `api/admin/disputes`
- **Action Method**: `GetDisputes`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Amdin`

**Query Parameters**:
- `query` (`GetAdminDisputesQuery`)

**Response DTO**: `PaginatedAdminDisputesResponse`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetAdminDisputesQuery`.

---

### HTTPPOST `api/admin/disputes/bookings/{bookingId}/release-to-provider`
- **Action Method**: `ReleaseToProvider`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Amdin`

**Request Body**: `ReleaseBookingDisputeCommand`
*Raw object, see body structure.*

**Response DTO**: `ReleaseBookingDisputeResponse`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `ReleaseBookingDisputeCommand`.

---

### HTTPPOST `api/admin/disputes/bookings/{bookingId}/refund-customer`
- **Action Method**: `RefundCustomer`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Amdin`

**Request Body**: `RefundBookingDisputeCommand`
*Raw object, see body structure.*

**Response DTO**: `RefundBookingDisputeResponse`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `RefundBookingDisputeCommand`.

---

### HTTPPOST `api/admin/disputes/bookings/{bookingId}/partial-settlement`
- **Action Method**: `PartialSettlement`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Amdin`

**Request Body**: `PartialSettleBookingDisputeCommand`
*Raw object, see body structure.*

**Response DTO**: `PartialSettleBookingDisputeResponse`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `PartialSettleBookingDisputeCommand`.

---

## AdminDocumentsController
- **Route Prefix**: `api/admin/documents`
- **Default Class Auth**: `Admin`

### HTTPGET `api/admin/documents`
- **Action Method**: `GetAll`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Query Parameters**:
- `query` (`GetAdminDocumentsQuery`)

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `Direct Controller Logic`.

---

### HTTPPOST `api/admin/documents/{id:guid}/approve`
- **Action Method**: `Approve`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Response DTO**: `ApproveDocumentResponse`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `ApproveDocumentCommand`.

---

### HTTPPOST `api/admin/documents/{id:guid}/reject`
- **Action Method**: `Reject`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Request Body**: `RejectDocumentRequest`
*Raw object, see body structure.*

**Response DTO**: `RejectDocumentResponse`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Allows Providers to reject bookings. Triggers post-rejection re-engagement search.

---

## AdminEmployeesController
- **Route Prefix**: `api/admin/employees`
- **Default Class Auth**: `Admin`

### HTTPGET `api/admin/employees`
- **Action Method**: `GetAll`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Response DTO**: `List<EmployeeProfileDto>`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetAllEmployeesQuery`.

---

### HTTPPOST `api/admin/employees`
- **Action Method**: `Create`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Request Body**: `CreateEmployeeCommand`
*Raw object, see body structure.*

**Response DTO**: `EmployeeProfileDto`
| Property | Type |
| :--- | :--- |
| UserId | `Guid` |
| FirstName | `string` |
| LastName | `string` |
| UserName | `string` |
| Email | `string?` |
| PhoneNumber | `string?` |
| Id | `Guid` |
| EmployeeCode | `string` |
| Department | `string?` |
| AssignedGovernorate | `string?` |
| IsAvailableForDispatch | `bool` |
| TotalVerificationsCompleted | `int` |
| TotalTicketsHandled | `int` |
| CreatedAt | `DateTime` |
| UpdatedAt | `DateTime` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "sample_firstname",
  "lastName": "sample_lastname",
  "userName": "sample_username",
  "email": "user@heveq.com",
  "phoneNumber": "01012345678",
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
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

**Business Notes**:
- Automatically handled by MediatR request `CreateEmployeeCommand`.

---

### HTTPPUT `api/admin/employees/{id:guid}`
- **Action Method**: `Update`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Request Body**: `UpdateEmployeeCommand`
*Raw object, see body structure.*

**Response DTO**: `EmployeeProfileDto`
| Property | Type |
| :--- | :--- |
| UserId | `Guid` |
| FirstName | `string` |
| LastName | `string` |
| UserName | `string` |
| Email | `string?` |
| PhoneNumber | `string?` |
| Id | `Guid` |
| EmployeeCode | `string` |
| Department | `string?` |
| AssignedGovernorate | `string?` |
| IsAvailableForDispatch | `bool` |
| TotalVerificationsCompleted | `int` |
| TotalTicketsHandled | `int` |
| CreatedAt | `DateTime` |
| UpdatedAt | `DateTime` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "sample_firstname",
  "lastName": "sample_lastname",
  "userName": "sample_username",
  "email": "user@heveq.com",
  "phoneNumber": "01012345678",
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
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

**Business Notes**:
- Automatically handled by MediatR request `UpdateEmployeeCommand`.

---

## AdminFieldVerificationsController
- **Route Prefix**: `api/admin/field-verifications`
- **Default Class Auth**: `Admin`

### HTTPPOST `api/admin/field-verifications/dispatch`
- **Action Method**: `DispatchVerification`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Request Body**: `DispatchFieldVerificationCommand`
*Raw object, see body structure.*

**Response DTO**: `DispatchFieldVerificationResponse`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `DispatchFieldVerificationCommand`.

---

### HTTPPOST `api/admin/field-verifications/{id}/decision`
- **Action Method**: `SaveDecision`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Request Body**: `FieldVerificationDecisionCommand`
*Raw object, see body structure.*

**Response DTO**: `FieldVerificationDecisionResponse`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `FieldVerificationDecisionCommand`.

---

## AdminMarketplaceDisputesController
- **Route Prefix**: `api/admin/disputes/marketplace-orders`
- **Default Class Auth**: `Admin`

### HTTPPOST `api/admin/disputes/marketplace-orders/{orderId}/release-to-seller`
- **Action Method**: `ReleaseToSeller`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Request Body**: `ReleaseMarketplaceDisputeCommand`
*Raw object, see body structure.*

**Response DTO**: `ReleaseMarketplaceDisputeResponse`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `ReleaseMarketplaceDisputeCommand`.

---

### HTTPPOST `api/admin/disputes/marketplace-orders/{orderId}/refund-buyer`
- **Action Method**: `RefundBuyer`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Request Body**: `RefundMarketplaceDisputeCommand`
*Raw object, see body structure.*

**Response DTO**: `RefundMarketplaceDisputeResponse`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `RefundMarketplaceDisputeCommand`.

---

## AdminMarketplaceListingsController
- **Route Prefix**: `api/admin/marketplace-listings`
- **Default Class Auth**: `Admin`

### HTTPGET `api/admin/marketplace-listings/pending`
- **Action Method**: `GetPendingMarketplaceListings`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Query Parameters**:
- `query` (`GetPendingMarketplaceListingsQuery`)

**Response DTO**: `PaginatedPendingMarketplaceResponse`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetPendingMarketplaceListingsQuery`.

---

### HTTPPOST `api/admin/marketplace-listings/{id}/approve`
- **Action Method**: `ApproveListing`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `Direct Controller Logic`.

---

### HTTPPOST `api/admin/marketplace-listings/{id}/reject`
- **Action Method**: `RejectListing`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Request Body**: `RejectMarketplaceListingCommand`
*Raw object, see body structure.*

**Response DTO**: `RejectMarketplaceListingResponse`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Allows Providers to reject bookings. Triggers post-rejection re-engagement search.

---

### HTTPGET `api/admin/marketplace-listings/{id}/review-details`
- **Action Method**: `GetReviewDetails`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `Direct Controller Logic`.

---

## AdminServiceListingsController
- **Route Prefix**: `api/admin/service-listings`
- **Default Class Auth**: `Admin`

### HTTPGET `api/admin/service-listings/pending`
- **Action Method**: `GetPendingListings`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Query Parameters**:
- `query` (`GetPendingServiceListingsQuery`)

**Response DTO**: `PaginatedPendingListingsResponse`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetPendingServiceListingsQuery`.

---

### HTTPPOST `api/admin/service-listings/{id}/approve`
- **Action Method**: `ApproveListing`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `Direct Controller Logic`.

---

### HTTPPOST `api/admin/service-listings/{id}/reject`
- **Action Method**: `RejectListing`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Request Body**: `RejectServiceListingCommand`
*Raw object, see body structure.*

**Response DTO**: `RejectServiceListingResponse`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Allows Providers to reject bookings. Triggers post-rejection re-engagement search.

---

### HTTPGET `api/admin/service-listings/{id}/review-details`
- **Action Method**: `GetReviewDetails`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `Direct Controller Logic`.

---

## AdminTicketsController
- **Route Prefix**: `api/admin/tickets`
- **Default Class Auth**: `Admin`

### HTTPGET `api/admin/tickets`
- **Action Method**: `GetTickets`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Query Parameters**:
- `query` (`GetAdminTicketsQuery`)

**Response DTO**: `PaginatedAdminTicketsResponse`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetAdminTicketsQuery`.

---

### HTTPGET `api/admin/tickets/{id}`
- **Action Method**: `GetTicketDetails`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `Direct Controller Logic`.

---

### HTTPPOST `api/admin/tickets/{id}/messages`
- **Action Method**: `AddMessage`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Request Body**: `AddTicketMessageCommand`
*Raw object, see body structure.*

**Response DTO**: `AddTicketMessageResult`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `AddTicketMessageCommand`.

---

### HTTPPOST `api/admin/tickets/{id}/resolve`
- **Action Method**: `ResolveTicket`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Request Body**: `ResolveTicketCommand`
*Raw object, see body structure.*

**Response DTO**: `ResolveTicketResponse`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `ResolveTicketCommand`.

---

## AdminUsersController
- **Route Prefix**: `api/adminusers`
- **Default Class Auth**: `Admin`

### HTTPGET `api/adminusers/users`
- **Action Method**: `GetUsers`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Query Parameters**:
- `query` (`GetAdminUsersQuery`)

**Response DTO**: `PaginatedUsersResponse`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetAdminUsersQuery`.

---

### HTTPPATCH `api/adminusers/users/{id}/status`
- **Action Method**: `UpdateUserStatus`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Admin`

**Request Body**: `UpdateUserStatusCommand`
*Raw object, see body structure.*

**Response DTO**: `UpdateUserStatusDTO`
| Property | Type |
| :--- | :--- |
| Id | `Guid` |
| IsActive | `bool` |
| StatusText | `string` |
| StatusAr | `string` |
| Message | `string` |
| IsSuccess | `bool` |
| StatusCode | `int` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "isActive": true,
  "statusText": "sample_statustext",
  "statusAr": "sample_statusar",
  "message": "sample_message",
  "isSuccess": true,
  "statusCode": 1
}
```

**Business Notes**:
- Automatically handled by MediatR request `UpdateUserStatusCommand`.

---

## AuthController
- **Route Prefix**: `api/auth`
- **Default Class Auth**: `AllowAnonymous`

### HTTPPOST `api/auth/Register`
- **Action Method**: `Register`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `Direct Controller Logic`.

---

### HTTPPOST `api/auth/Login`
- **Action Method**: `login`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `Direct Controller Logic`.

---

### HTTPPOST `api/auth/refresh-token`
- **Action Method**: `RefreshToken`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Request Body**: `RefreshTokenCommand`
*Raw object, see body structure.*

**Response DTO**: `IActionResult`
**Example Request Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `Direct Controller Logic`.

---

### HTTPPOST `api/auth/logout`
- **Action Method**: `Logout`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `Direct Controller Logic`.

---

### HTTPGET `api/auth/me`
- **Action Method**: `GetMe`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `GetMeResponse`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `MeQuery`.

---

### HTTPPOST `api/auth/confirm-email`
- **Action Method**: `ConfirmEmail`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Request Body**: `ConfirmEmailCommand`
*Raw object, see body structure.*

**Response DTO**: `IActionResult`
**Example Request Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `Direct Controller Logic`.

---

### HTTPPOST `api/auth/resend-confirmation-email`
- **Action Method**: `ResendConfirmationEmail`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `Direct Controller Logic`.

---

## BookingsController
- **Route Prefix**: `api/bookings`
- **Default Class Auth**: `AllowAnonymous`

### HTTPPOST `api/bookings`
- **Action Method**: `CreateBooking`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Request Body**: `CreateBookingRequest`
*Raw object, see body structure.*

**Response DTO**: `CreateBookingResponseDto`
| Property | Type |
| :--- | :--- |
| Id | `Guid` |
| BookingNumber | `string` |
| Status | `string` |
| StatusAr | `string` |
| ServiceTitle | `string` |
| ProviderCompany | `string` |
| RequestedStartDate | `DateOnly` |
| RequestedStartTime | `TimeOnly` |
| EstimatedDurationHours | `decimal` |
| HourlyRateSnapshot | `decimal` |
| EstimatedTotal | `decimal` |
| Message | `string` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingNumber": "01012345678",
  "status": "sample_status",
  "statusAr": "sample_statusar",
  "serviceTitle": "sample_servicetitle",
  "providerCompany": "sample_providercompany",
  "requestedStartDate": "2026-07-05T19:17:54Z",
  "requestedStartTime": "2026-07-05T19:17:54Z",
  "estimatedDurationHours": 99.99,
  "hourlyRateSnapshot": 99.99,
  "estimatedTotal": 99.99,
  "message": "sample_message"
}
```

**Business Notes**:
- Automatically handled by MediatR request `CreateBookingCommand`.

---

### HTTPPOST `api/bookings/{bookingId:guid}/accept`
- **Action Method**: `AcceptBooking`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `bookingId` (`Guid`)

**Request Body**: `AcceptBookingRequest`
*Raw object, see body structure.*

**Response DTO**: `AcceptBookingResponseDto`
| Property | Type |
| :--- | :--- |
| Id | `Guid` |
| BookingNumber | `string` |
| Status | `string` |
| StatusAr | `string` |
| AssignedOperatorName | `string` |
| Message | `string` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingNumber": "01012345678",
  "status": "sample_status",
  "statusAr": "sample_statusar",
  "assignedOperatorName": "sample_assignedoperatorname",
  "message": "sample_message"
}
```

**Business Notes**:
- Allows Providers to accept booking requests and assign an operator to the job.

---

### HTTPPOST `api/bookings/{bookingId:guid}/reject`
- **Action Method**: `RejectBooking`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `bookingId` (`Guid`)

**Request Body**: `RejectBookingRequest`
*Raw object, see body structure.*

**Response DTO**: `RejectBookingResponseDto`
| Property | Type |
| :--- | :--- |
| Id | `Guid` |
| BookingNumber | `string` |
| Status | `string` |
| StatusAr | `string` |
| ProviderRejectionReason | `string` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingNumber": "01012345678",
  "status": "sample_status",
  "statusAr": "sample_statusar",
  "providerRejectionReason": "sample_providerrejectionreason"
}
```

**Business Notes**:
- Allows Providers to reject bookings. Triggers post-rejection re-engagement search.

---

### HTTPPOST `api/bookings/{bookingId:guid}/cancel`
- **Action Method**: `CancelBooking`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `bookingId` (`Guid`)

**Request Body**: `CancelBookingRequest`
*Raw object, see body structure.*

**Response DTO**: `CancelBookingResponseDto`
| Property | Type |
| :--- | :--- |
| BookingId | `Guid` |
| Status | `string` |
| StatusAr | `string` |
| RefundPercentage | `decimal` |
| Message | `string` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "sample_status",
  "statusAr": "sample_statusar",
  "refundPercentage": 99.99,
  "message": "sample_message"
}
```

**Business Notes**:
- Automatically handled by MediatR request `CancelBookingCommand`.

---

### HTTPPOST `api/bookings/{bookingId:guid}/start`
- **Action Method**: `StartBooking`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `bookingId` (`Guid`)

**Response DTO**: `StartBookingResponseDto`
| Property | Type |
| :--- | :--- |
| Id | `Guid` |
| BookingNumber | `string` |
| Status | `string` |
| StatusAr | `string` |
| StartedAt | `DateTime?` |
| Message | `string` |

**Example Response Payload**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingNumber": "01012345678",
  "status": "sample_status",
  "statusAr": "sample_statusar",
  "startedAt": "2026-07-05T19:17:54Z",
  "message": "sample_message"
}
```

**Business Notes**:
- Automatically handled by MediatR request `StartBookingCommand`.

---

### HTTPPOST `api/bookings/{bookingId:guid}/complete-by-provider`
- **Action Method**: `CompleteByProvider`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `bookingId` (`Guid`)

**Request Body**: `CompleteBookingByProviderRequest`
*Raw object, see body structure.*

**Response DTO**: `CompleteBookingByProviderResponseDto`
| Property | Type |
| :--- | :--- |
| BookingId | `Guid` |
| Status | `string` |
| StatusAr | `string` |
| EvidenceFormId | `Guid` |
| Message | `string` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "sample_status",
  "statusAr": "sample_statusar",
  "evidenceFormId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "sample_message"
}
```

**Business Notes**:
- Automatically handled by MediatR request `CompleteBookingByProviderCommand`.

---

### HTTPPOST `api/bookings/{bookingId:guid}/confirm-completion`
- **Action Method**: `ConfirmCompletion`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `bookingId` (`Guid`)

**Response DTO**: `ConfirmBookingCompletionResponseDto`
| Property | Type |
| :--- | :--- |
| BookingId | `Guid` |
| Status | `string` |
| StatusAr | `string` |
| Message | `string` |

**Example Response Payload**:
```json
{
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "sample_status",
  "statusAr": "sample_statusar",
  "message": "sample_message"
}
```

**Business Notes**:
- Automatically handled by MediatR request `ConfirmBookingCompletionCommand`.

---

### HTTPPOST `api/bookings/{bookingId:guid}/dispute`
- **Action Method**: `DisputeBooking`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `bookingId` (`Guid`)

**Request Body**: `DisputeBookingRequest`
*Raw object, see body structure.*

**Response DTO**: `DisputeBookingResponseDto`
| Property | Type |
| :--- | :--- |
| BookingId | `Guid` |
| Status | `string` |
| StatusAr | `string` |
| TicketId | `Guid?` |
| Message | `string` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "sample_status",
  "statusAr": "sample_statusar",
  "ticketId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "sample_message"
}
```

**Business Notes**:
- Automatically handled by MediatR request `DisputeBookingCommand`.

---

### HTTPPOST `api/bookings/{bookingId:guid}/time-adjustments`
- **Action Method**: `CreateTimeAdjustment`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `bookingId` (`Guid`)

**Request Body**: `CreateTimeAdjustmentRequest`
*Raw object, see body structure.*

**Response DTO**: `CreateTimeAdjustmentResponseDto`
| Property | Type |
| :--- | :--- |
| Id | `Guid` |
| BookingId | `Guid` |
| BookingNumber | `string` |
| RequestedAdditionalHrs | `decimal` |
| AdditionalCostAmount | `decimal` |
| Status | `string` |
| StatusAr | `string` |
| ProviderNote | `string` |
| Message | `string` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingNumber": "01012345678",
  "requestedAdditionalHrs": 99.99,
  "additionalCostAmount": 99.99,
  "status": "sample_status",
  "statusAr": "sample_statusar",
  "providerNote": "sample_providernote",
  "message": "sample_message"
}
```

**Business Notes**:
- Automatically handled by MediatR request `CreateTimeAdjustmentCommand`.

---

### HTTPPOST `api/bookings/time-adjustments/{id:guid}/approve`
- **Action Method**: `ApproveTimeAdjustment`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `id` (`Guid`)

**Response DTO**: `TimeAdjustmentDecisionResponseDto`
| Property | Type |
| :--- | :--- |
| Id | `Guid` |
| BookingId | `Guid` |
| BookingNumber | `string` |
| RequestedAdditionalHrs | `decimal` |
| AdditionalCostAmount | `decimal` |
| BookingEstimatedDurationHours | `decimal` |
| BookingEstimatedTotal | `decimal` |
| Status | `string` |
| StatusAr | `string` |
| CustomerAcknowledgedAt | `DateTime?` |
| Message | `string` |

**Example Response Payload**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingNumber": "01012345678",
  "requestedAdditionalHrs": 99.99,
  "additionalCostAmount": 99.99,
  "bookingEstimatedDurationHours": 99.99,
  "bookingEstimatedTotal": 99.99,
  "status": "sample_status",
  "statusAr": "sample_statusar",
  "customerAcknowledgedAt": "2026-07-05T19:17:54Z",
  "message": "sample_message"
}
```

**Business Notes**:
- Automatically handled by MediatR request `ApproveTimeAdjustmentCommand`.

---

### HTTPPOST `api/bookings/time-adjustments/{id:guid}/reject`
- **Action Method**: `RejectTimeAdjustment`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `id` (`Guid`)

**Response DTO**: `TimeAdjustmentDecisionResponseDto`
| Property | Type |
| :--- | :--- |
| Id | `Guid` |
| BookingId | `Guid` |
| BookingNumber | `string` |
| RequestedAdditionalHrs | `decimal` |
| AdditionalCostAmount | `decimal` |
| BookingEstimatedDurationHours | `decimal` |
| BookingEstimatedTotal | `decimal` |
| Status | `string` |
| StatusAr | `string` |
| CustomerAcknowledgedAt | `DateTime?` |
| Message | `string` |

**Example Response Payload**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingNumber": "01012345678",
  "requestedAdditionalHrs": 99.99,
  "additionalCostAmount": 99.99,
  "bookingEstimatedDurationHours": 99.99,
  "bookingEstimatedTotal": 99.99,
  "status": "sample_status",
  "statusAr": "sample_statusar",
  "customerAcknowledgedAt": "2026-07-05T19:17:54Z",
  "message": "sample_message"
}
```

**Business Notes**:
- Allows Providers to reject bookings. Triggers post-rejection re-engagement search.

---

### HTTPPOST `api/bookings/time-adjustments/{id:guid}/payment/checkout`
- **Action Method**: `CheckoutTimeAdjustmentPayment`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `id` (`Guid`)

**Request Body**: `PaymentCheckoutRequest`
*Raw object, see body structure.*

**Response DTO**: `TimeAdjustmentPaymentCheckoutResponseDto`
| Property | Type |
| :--- | :--- |
| TimeAdjustmentRequestId | `Guid` |
| BookingId | `Guid` |
| BookingNumber | `string` |
| Amount | `decimal` |
| Currency | `string` |
| PaymentProvider | `string` |
| CheckoutUrl | `string` |
| Status | `string` |
| PaymentGatewayReference | `string` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{
  "timeAdjustmentRequestId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingNumber": "01012345678",
  "amount": 99.99,
  "currency": "sample_currency",
  "paymentProvider": "sample_paymentprovider",
  "checkoutUrl": "sample_checkouturl",
  "status": "sample_status",
  "paymentGatewayReference": "sample_paymentgatewayreference"
}
```

**Business Notes**:
- Automatically handled by MediatR request `CheckoutTimeAdjustmentPaymentCommand`.

---

### HTTPPOST `api/bookings/time-adjustments/{id:guid}/payment/mock-confirm`
- **Action Method**: `ConfirmTimeAdjustmentPaymentForDemo`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `id` (`Guid`)

**Request Body**: `PaymentConfirmRequest`
*Raw object, see body structure.*

**Response DTO**: `TimeAdjustmentPaymentConfirmResponseDto`
| Property | Type |
| :--- | :--- |
| TimeAdjustmentRequestId | `Guid` |
| BookingId | `Guid` |
| BookingNumber | `string` |
| TimeAdjustmentStatus | `string` |
| TimeAdjustmentStatusAr | `string` |
| EscrowStatus | `string` |
| EscrowStatusAr | `string` |
| BookingEstimatedDurationHours | `decimal` |
| BookingEstimatedTotal | `decimal` |
| Message | `string` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{
  "timeAdjustmentRequestId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingNumber": "01012345678",
  "timeAdjustmentStatus": "sample_timeadjustmentstatus",
  "timeAdjustmentStatusAr": "sample_timeadjustmentstatusar",
  "escrowStatus": "sample_escrowstatus",
  "escrowStatusAr": "sample_escrowstatusar",
  "bookingEstimatedDurationHours": 99.99,
  "bookingEstimatedTotal": 99.99,
  "message": "sample_message"
}
```

**Business Notes**:
- Automatically handled by MediatR request `ConfirmTimeAdjustmentPaymentCommand`.

---

### HTTPGET `api/bookings/my`
- **Action Method**: `GetMyBookings`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Query Parameters**:
- `status` (`BookingStatus?`)

**Response DTO**: `CustomerBookingsResponseDto`
| Property | Type |
| :--- | :--- |
| Items | `IReadOnlyList<CustomerBookingListItemDto>` |
| TotalCount | `int` |

**Example Response Payload**:
```json
{
  "items": [],
  "totalCount": 1
}
```

**Business Notes**:
- Automatically handled by MediatR request `GetCustomerBookingsQuery`.

---

### HTTPGET `api/bookings/provider`
- **Action Method**: `GetProviderBookings`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Response DTO**: `IReadOnlyList<ProviderBookingListItemDto>`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetProviderBookingsQuery`.

---

### HTTPGET `api/bookings/create-context/{serviceListingId:guid}`
- **Action Method**: `GetBookingCreateContext`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `serviceListingId` (`Guid`)

**Response DTO**: `BookingCreateContextDto`
| Property | Type |
| :--- | :--- |
| ServiceListingId | `Guid` |
| ServiceTitle | `string` |
| ProviderCompany | `string` |
| HourlyRate | `decimal?` |
| DailyRate | `decimal?` |
| MinimumBookingHours | `int` |
| Availability | `IReadOnlyList<BookingCreateContextAvailabilityDto>` |
| DefaultAddress | `BookingCreateContextAddressDto?` |
| CustomerEligibility | `BookingCustomerEligibilityDto` |

**Example Response Payload**:
```json
{
  "serviceListingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "serviceTitle": "sample_servicetitle",
  "providerCompany": "sample_providercompany",
  "hourlyRate": 99.99,
  "dailyRate": 99.99,
  "minimumBookingHours": 1,
  "availability": [],
  "defaultAddress": {},
  "customerEligibility": {}
}
```

**Business Notes**:
- Automatically handled by MediatR request `GetBookingCreateContextQuery`.

---

### HTTPGET `api/bookings/{id:guid}`
- **Action Method**: `GetBookingById`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `id` (`Guid`)

**Response DTO**: `BookingDto`
| Property | Type |
| :--- | :--- |
| Id | `Guid` |
| BookingNumber | `string` |
| ServiceListingId | `Guid` |
| ServiceTitle | `string` |
| CustomerName | `string` |
| ProviderCompany | `string` |
| OperatorName | `string?` |
| AvailableActions | `BookingActionsDto` |
| Timeline | `IReadOnlyList<BookingTimelineItemDto>` |
| EscrowStatus | `string?` |
| EscrowStatusAr | `string?` |
| CustomerId | `Guid` |
| JobTitle | `string` |
| JobDescription | `string?` |
| Governorate | `string` |
| District | `string` |
| Street | `string?` |
| Latitude | `decimal?` |
| Longitude | `decimal?` |
| RequestedStartDate | `DateOnly` |
| RequestedStartTime | `TimeOnly` |
| EstimatedDurationHours | `decimal` |
| HourlyRateSnapshot | `decimal` |
| EstimatedTotal | `decimal` |
| SurchargeAmount | `decimal?` |
| IsOutOfZoneBooking | `bool` |
| OutOfZoneDistanceKm | `decimal?` |
| OutOfZoneSurchargeAmount | `decimal?` |
| OutOfZoneSurchargeAcceptedAt | `DateTime?` |
| Status | `string` |
| StatusAr | `string` |
| AssignedOperatorId | `Guid?` |
| ProviderRejectionReason | `string?` |
| CancellationReason | `string?` |
| CreatedAt | `DateTime` |
| ConfirmedAt | `DateTime?` |
| PaymentCapturedAt | `DateTime?` |
| StartedAt | `DateTime?` |
| CompletedMarkedAt | `DateTime?` |
| CompletionConfirmedAt | `DateTime?` |
| DisputeOpenedAt | `DateTime?` |

**Example Response Payload**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingNumber": "01012345678",
  "serviceListingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "serviceTitle": "sample_servicetitle",
  "customerName": "sample_customername",
  "providerCompany": "sample_providercompany",
  "operatorName": "sample_operatorname",
  "availableActions": {},
  "timeline": "2026-07-05T19:17:54Z",
  "escrowStatus": "sample_escrowstatus",
  "escrowStatusAr": "sample_escrowstatusar",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "jobTitle": "sample_jobtitle",
  "jobDescription": "sample_jobdescription",
  "governorate": "sample_governorate",
  "district": "sample_district",
  "street": "sample_street",
  "latitude": 99.99,
  "longitude": 99.99,
  "requestedStartDate": "2026-07-05T19:17:54Z",
  "requestedStartTime": "2026-07-05T19:17:54Z",
  "estimatedDurationHours": 99.99,
  "hourlyRateSnapshot": 99.99,
  "estimatedTotal": 99.99,
  "surchargeAmount": 99.99,
  "isOutOfZoneBooking": true,
  "outOfZoneDistanceKm": 99.99,
  "outOfZoneSurchargeAmount": 99.99,
  "outOfZoneSurchargeAcceptedAt": "2026-07-05T19:17:54Z",
  "status": "sample_status",
  "statusAr": "sample_statusar",
  "assignedOperatorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "providerRejectionReason": "sample_providerrejectionreason",
  "cancellationReason": "sample_cancellationreason",
  "createdAt": "2026-07-05T19:17:54Z",
  "confirmedAt": "2026-07-05T19:17:54Z",
  "paymentCapturedAt": "2026-07-05T19:17:54Z",
  "startedAt": "2026-07-05T19:17:54Z",
  "completedMarkedAt": "2026-07-05T19:17:54Z",
  "completionConfirmedAt": "2026-07-05T19:17:54Z",
  "disputeOpenedAt": "2026-07-05T19:17:54Z"
}
```

**Business Notes**:
- Automatically handled by MediatR request `GetBookingByIdQuery`.

---

### HTTPPOST `api/bookings/{id:guid}/payment/checkout`
- **Action Method**: `CheckoutBookingPayment`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `id` (`Guid`)

**Request Body**: `PaymentCheckoutRequest`
*Raw object, see body structure.*

**Response DTO**: `BookingPaymentCheckoutResponseDto`
| Property | Type |
| :--- | :--- |
| BookingId | `Guid` |
| Amount | `decimal` |
| Currency | `string` |
| PaymentProvider | `string` |
| CheckoutUrl | `string` |
| Status | `string` |
| PaymentGatewayReference | `string` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "amount": 99.99,
  "currency": "sample_currency",
  "paymentProvider": "sample_paymentprovider",
  "checkoutUrl": "sample_checkouturl",
  "status": "sample_status",
  "paymentGatewayReference": "sample_paymentgatewayreference"
}
```

**Business Notes**:
- Automatically handled by MediatR request `CheckoutBookingPaymentCommand`.

---

### HTTPPOST `api/bookings/{id:guid}/payment/mock-confirm`
- **Action Method**: `ConfirmBookingPaymentForDemo`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `id` (`Guid`)

**Request Body**: `PaymentConfirmRequest`
*Raw object, see body structure.*

**Response DTO**: `BookingPaymentConfirmResponseDto`
| Property | Type |
| :--- | :--- |
| BookingId | `Guid` |
| BookingStatus | `string` |
| BookingStatusAr | `string` |
| EscrowStatus | `string` |
| EscrowStatusAr | `string` |
| Message | `string` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingStatus": "sample_bookingstatus",
  "bookingStatusAr": "sample_bookingstatusar",
  "escrowStatus": "sample_escrowstatus",
  "escrowStatusAr": "sample_escrowstatusar",
  "message": "sample_message"
}
```

**Business Notes**:
- Automatically handled by MediatR request `ConfirmBookingPaymentCommand`.

---

### HTTPGET `api/bookings/{id:guid}/escrow`
- **Action Method**: `GetBookingEscrow`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `id` (`Guid`)

**Response DTO**: `BookingEscrowDto`
| Property | Type |
| :--- | :--- |
| BookingId | `Guid` |
| GrossAmount | `decimal` |
| PlatformCommission | `decimal` |
| ProviderPayout | `decimal` |
| VatAmount | `decimal` |
| Status | `string` |
| StatusAr | `string` |
| CapturedAt | `DateTime?` |
| ReleasedAt | `DateTime?` |
| FrozenAt | `DateTime?` |

**Example Response Payload**:
```json
{
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "grossAmount": 99.99,
  "platformCommission": 99.99,
  "providerPayout": 99.99,
  "vatAmount": 99.99,
  "status": "sample_status",
  "statusAr": "sample_statusar",
  "capturedAt": "2026-07-05T19:17:54Z",
  "releasedAt": "2026-07-05T19:17:54Z",
  "frozenAt": "2026-07-05T19:17:54Z"
}
```

**Business Notes**:
- Automatically handled by MediatR request `GetBookingEscrowQuery`.

---

### HTTPGET `api/bookings/{id:guid}/tracker`
- **Action Method**: `GetBookingTracker`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `id` (`Guid`)

**Response DTO**: `BookingTrackerDto`
| Property | Type |
| :--- | :--- |
| BookingId | `Guid` |
| BookingNumber | `string` |
| CurrentStatus | `string` |
| CurrentStatusAr | `string` |
| Timeline | `IReadOnlyList<BookingTimelineItemDto>` |
| NextAction | `BookingNextActionDto` |
| AvailableActions | `BookingActionsDto` |

**Example Response Payload**:
```json
{
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookingNumber": "01012345678",
  "currentStatus": "sample_currentstatus",
  "currentStatusAr": "sample_currentstatusar",
  "timeline": "2026-07-05T19:17:54Z",
  "nextAction": {},
  "availableActions": {}
}
```

**Business Notes**:
- Automatically handled by MediatR request `GetBookingTrackerQuery`.

---

## CategoriesController
- **Route Prefix**: `api/categories`
- **Default Class Auth**: `AllowAnonymous`

### HTTPGET `api/categories`
- **Action Method**: `GetAllCategories`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Query Parameters**:
- `type` (`CategoryType?`)

**Response DTO**: `List<CategoryDto>`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetCategoriesQuery`.

---

## ConversationsController
- **Route Prefix**: `api/conversations`
- **Default Class Auth**: `Authenticated`

### HTTPGET `api/conversations/my`
- **Action Method**: `GetMy`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `ConversationListResult`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetMyConversationsQuery`.

---

### HTTPPOST `api/conversations`
- **Action Method**: `Start`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Request Body**: `StartConversationCommand`
*Raw object, see body structure.*

**Response DTO**: `StartConversationResult`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `StartConversationCommand`.

---

### HTTPGET `api/conversations/{id:guid}/messages`
- **Action Method**: `GetMessages`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Query Parameters**:
- `page` (`int`)
- `pageSize` (`int`)

**Response DTO**: `ConversationMessagesResult`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetConversationMessagesQuery`.

---

### HTTPPOST `api/conversations/{id:guid}/messages`
- **Action Method**: `SendMessage`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Request Body**: `SendMessageBody`
*Raw object, see body structure.*

**Response DTO**: `SendMessageResult`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `SendMessageCommand`.

---

### HTTPPATCH `api/conversations/{id:guid}/read`
- **Action Method**: `MarkRead`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `MarkReadResult`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `MarkConversationReadCommand`.

---

## CustomerDashboardController
- **Route Prefix**: `api/customer/dashboard`
- **Default Class Auth**: `Customer`

### HTTPGET `api/customer/dashboard/summary`
- **Action Method**: `GetSummary`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Customer`

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `GetCustomerDashboardSummaryQuery`.

---

## CustomerProfileController
- **Route Prefix**: `api/customer-profile`
- **Default Class Auth**: `Customer`

### HTTPGET `api/customer-profile/me`
- **Action Method**: `GetMe`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Customer`

**Response DTO**: `CustomerProfileDto`
| Property | Type |
| :--- | :--- |
| UserId | `Guid` |
| DisplayName | `string` |
| FirstName | `string` |
| LastName | `string` |
| UserName | `string` |
| Email | `string?` |
| PhoneNumber | `string?` |
| IsPhoneVerified | `bool` |
| DefaultAddress | `AddressDto?` |
| Id | `Guid` |
| TrustScore | `decimal` |
| CancellationRate | `decimal?` |
| DisputeFrequencyScore | `decimal?` |
| PaymentFailureCount | `int` |
| ReviewAuthenticityScore | `decimal?` |
| RequiresAdditionalVerification | `bool` |
| TotalBookings | `int` |
| TrustScoreLastComputedAt | `DateTime?` |
| CreatedAt | `DateTime` |
| UpdatedAt | `DateTime` |

**Example Response Payload**:
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "displayName": "sample_displayname",
  "firstName": "sample_firstname",
  "lastName": "sample_lastname",
  "userName": "sample_username",
  "email": "user@heveq.com",
  "phoneNumber": "01012345678",
  "isPhoneVerified": true,
  "defaultAddress": {},
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
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

**Business Notes**:
- Automatically handled by MediatR request `GetCustomerProfileQuery`.

---

### HTTPGET `api/customer-profile/me/trust-history`
- **Action Method**: `GetMyTrustHistory`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Customer`

**Response DTO**: `List<CustomerTrustHistoryDto>`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetCustomerTrustHistoryQuery`.

---

### HTTPPUT `api/customer-profile/me`
- **Action Method**: `UpdateMe`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Customer`

**Request Body**: `UpdateCustomerProfileCommand`
*Raw object, see body structure.*

**Response DTO**: `CustomerProfileDto`
| Property | Type |
| :--- | :--- |
| UserId | `Guid` |
| DisplayName | `string` |
| FirstName | `string` |
| LastName | `string` |
| UserName | `string` |
| Email | `string?` |
| PhoneNumber | `string?` |
| IsPhoneVerified | `bool` |
| DefaultAddress | `AddressDto?` |
| Id | `Guid` |
| TrustScore | `decimal` |
| CancellationRate | `decimal?` |
| DisputeFrequencyScore | `decimal?` |
| PaymentFailureCount | `int` |
| ReviewAuthenticityScore | `decimal?` |
| RequiresAdditionalVerification | `bool` |
| TotalBookings | `int` |
| TrustScoreLastComputedAt | `DateTime?` |
| CreatedAt | `DateTime` |
| UpdatedAt | `DateTime` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "displayName": "sample_displayname",
  "firstName": "sample_firstname",
  "lastName": "sample_lastname",
  "userName": "sample_username",
  "email": "user@heveq.com",
  "phoneNumber": "01012345678",
  "isPhoneVerified": true,
  "defaultAddress": {},
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
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

**Business Notes**:
- Automatically handled by MediatR request `UpdateCustomerProfileCommand`.

---

## DocumentsController
- **Route Prefix**: `api/documents`
- **Default Class Auth**: `Authenticated`

### HTTPPOST `api/documents`
- **Action Method**: `Upload`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Request Body**: `UploadDocumentRequest`
*Raw object, see body structure.*

**Response DTO**: `UploadDocumentResponse`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `UploadDocumentCommand`.

---

### HTTPGET `api/documents/my`
- **Action Method**: `GetMy`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `List<DocumentDto>`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetMyDocumentsQuery`.

---

### HTTPDELETE `api/documents/{id:guid}`
- **Action Method**: `Delete`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `DeleteDocumentCommand`.

---

## MarketPlaceListingsController
- **Route Prefix**: `api/marketplace-listings`
- **Default Class Auth**: `AllowAnonymous`

### HTTPGET `api/marketplace-listings`
- **Action Method**: `GetAll`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Query Parameters**:
- `query` (`GetMarketPlaceListingsQuery`)

**Response DTO**: `PagedResult<MarketPlaceListingDTO>`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetMarketPlaceListingsQuery`.

---

### HTTPGET `api/marketplace-listings/{id:guid}`
- **Action Method**: `GetById`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Response DTO**: `MarketplaceListingDetailsDto`
| Property | Type |
| :--- | :--- |
| Id | `Guid` |
| Title | `string` |
| Description | `string` |
| Price | `decimal` |
| Condition | `string` |
| ConditionAr | `string` |
| Specifications | `string?` |
| Photos | `List<MarketplaceListingPhotoDto>` |
| Seller | `ListingSellerDto` |
| CanBuyNow | `bool` |
| ManagementInfo | `ListingManagementInfoDto?` |

**Example Response Payload**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "sample_title",
  "description": "sample_description",
  "price": 99.99,
  "condition": "sample_condition",
  "conditionAr": "sample_conditionar",
  "specifications": "sample_specifications",
  "photos": [],
  "seller": [],
  "canBuyNow": true,
  "managementInfo": []
}
```

**Business Notes**:
- Automatically handled by MediatR request `GetMarketplaceListingByIdQuery`.

---

### HTTPGET `api/marketplace-listings`
- **Action Method**: `GetMyListings`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Query Parameters**:
- `query` (`GetProviderMarketplaceListingsQuery`)

**Response DTO**: `PagedResult<ProviderMarketPlaceListingDTO>`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetProviderMarketplaceListingsQuery`.

---

### HTTPPOST `api/marketplace-listings`
- **Action Method**: `Create`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Request Body**: `CreateMarketplaceListingRequest`
*Raw object, see body structure.*

**Response DTO**: `CreateMarketplaceListingResponse`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `CreateMarketPlaceListingCommand`.

---

### HTTPPUT `api/marketplace-listings/{id:guid}`
- **Action Method**: `Update`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Request Body**: `UpdateMarketplaceListingRequest`
*Raw object, see body structure.*

**Response DTO**: `IActionResult`
**Example Request Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `UpdateMarketplaceListingCommand`.

---

### HTTPDELETE `api/marketplace-listings/{id:guid}`
- **Action Method**: `Delete`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `DeleteMarketPlaceListingCommand`.

---

### HTTPPOST `api/marketplace-listings/{id:guid}/photos`
- **Action Method**: `AddPhoto`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Request Body**: `AddMarketplacePhotoRequest`
*Raw object, see body structure.*

**Response DTO**: `Guid`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `AddMarketplaceListingPhotoCommand`.

---

### HTTPDELETE `api/marketplace-listings/{id:guid}/photos/{photoId:guid}`
- **Action Method**: `DeletePhoto`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Route Parameters**:
- `id` (`Guid`)
- `photoId` (`Guid`)

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `DeleteMarketplaceListingPhotoCommand`.

---

## MarketPlaceOrderController
- **Route Prefix**: `api/marketplace-orders`
- **Default Class Auth**: `Authenticated`

### HTTPPOST `api/marketplace-orders`
- **Action Method**: `Create`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Request Body**: `CreateMarketPlaceOrderRequest`
*Raw object, see body structure.*

**Response DTO**: `CreateMarketplaceOrderResponse`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `CreateMarketPlaceOrderCommand`.

---

### HTTPGET `api/marketplace-orders/my/purchases`
- **Action Method**: `GetMyPurchases`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `List<PurchaseOrderDto>`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetCustomerOrderQuery`.

---

### HTTPGET `api/marketplace-orders/my/sales`
- **Action Method**: `GetMySales`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `List<SaleOrderDto>`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetProviderOrderQuery`.

---

### HTTPGET `api/marketplace-orders/{id:guid}`
- **Action Method**: `GetOrderById`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Route Parameters**:
- `id` (`Guid`)

**Response DTO**: `MarketplaceOrderDto`
| Property | Type |
| :--- | :--- |
| Id | `Guid` |
| BuyerId | `Guid` |
| BuyerName | `string` |
| SellerId | `Guid` |
| SellerName | `string` |
| ListingId | `Guid` |
| ListingTitle | `string` |
| Amount | `decimal` |
| DeliveryAddress | `string?` |
| DeliveryPreference | `string?` |
| TrackingNumber | `string?` |
| Status | `string` |
| StatusAr | `string` |
| SellerConfirmedAt | `DateTime?` |
| DispatchedAt | `DateTime?` |
| DeliveredAt | `DateTime?` |
| ConfirmedByBuyerAt | `DateTime?` |
| CancellationReason | `string?` |
| CancelledAt | `DateTime?` |
| CancellationInitiatedByRole | `string?` |
| ReturnShippingCost | `decimal?` |
| ReturnShippingAcceptedByBuyerAt | `DateTime?` |
| CreatedAt | `DateTime` |
| ViewerRole | `string` |

**Example Response Payload**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "buyerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "buyerName": "sample_buyername",
  "sellerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "sellerName": "sample_sellername",
  "listingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "listingTitle": "sample_listingtitle",
  "amount": 99.99,
  "deliveryAddress": "sample_deliveryaddress",
  "deliveryPreference": "sample_deliverypreference",
  "trackingNumber": "01012345678",
  "status": "sample_status",
  "statusAr": "sample_statusar",
  "sellerConfirmedAt": "2026-07-05T19:17:54Z",
  "dispatchedAt": "2026-07-05T19:17:54Z",
  "deliveredAt": "2026-07-05T19:17:54Z",
  "confirmedByBuyerAt": "2026-07-05T19:17:54Z",
  "cancellationReason": "sample_cancellationreason",
  "cancelledAt": "2026-07-05T19:17:54Z",
  "cancellationInitiatedByRole": "sample_cancellationinitiatedbyrole",
  "returnShippingCost": 99.99,
  "returnShippingAcceptedByBuyerAt": "2026-07-05T19:17:54Z",
  "createdAt": "2026-07-05T19:17:54Z",
  "viewerRole": "sample_viewerrole"
}
```

**Business Notes**:
- Automatically handled by MediatR request `GetMarketplaceOrderByIdQuery`.

---

### HTTPPOST `api/marketplace-orders/{id:guid}/seller-confirm`
- **Action Method**: `SellerConfirm`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `OrderActionResponse`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `ConfirmMarketplaceOrderCommand`.

---

### HTTPPOST `api/marketplace-orders/{id:guid}/dispatch`
- **Action Method**: `Dispatch`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Request Body**: `DispatchMarketplaceOrderRequest`
*Raw object, see body structure.*

**Response DTO**: `OrderActionResponse`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `DispatchMarketplaceOrderCommand`.

---

### HTTPPOST `api/marketplace-orders/{id:guid}/deliver`
- **Action Method**: `Deliver`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `OrderActionResponse`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `DeliverMarketplaceOrderCommand`.

---

### HTTPPOST `api/marketplace-orders/{id:guid}/complete`
- **Action Method**: `Complete`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `OrderActionResponse`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `CompleteMarketplaceOrderCommand`.

---

### HTTPPOST `api/marketplace-orders/{id:guid}/cancel`
- **Action Method**: `Cancel`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Request Body**: `CancelMarketplaceOrderRequest`
*Raw object, see body structure.*

**Response DTO**: `OrderActionResponse`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `CancelMarketplaceOrderCommand`.

---

### HTTPGET `api/marketplace-orders/{id:guid}/tracking`
- **Action Method**: `GetTracking`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `OrderTrackingDto`
| Property | Type |
| :--- | :--- |
| Id | `Guid` |
| OrderNumber | `string` |
| ListingTitle | `string` |
| BuyerName | `string` |
| SellerName | `string` |
| Amount | `decimal` |
| DeliveryPreference | `string?` |
| TrackingNumber | `string?` |
| Status | `string` |
| StatusAr | `string` |
| EscrowStatus | `string?` |
| Timeline | `List<OrderTrackingTimelineItemDto>` |
| AvailableActions | `OrderTrackingActionsDto` |

**Example Response Payload**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "orderNumber": "01012345678",
  "listingTitle": "sample_listingtitle",
  "buyerName": "sample_buyername",
  "sellerName": "sample_sellername",
  "amount": 99.99,
  "deliveryPreference": "sample_deliverypreference",
  "trackingNumber": "01012345678",
  "status": "sample_status",
  "statusAr": "sample_statusar",
  "escrowStatus": "sample_escrowstatus",
  "timeline": "2026-07-05T19:17:54Z",
  "availableActions": {}
}
```

**Business Notes**:
- Automatically handled by MediatR request `GetMarketplaceOrderTrackingQuery`.

---

### HTTPPOST `api/marketplace-orders/{id:guid}/payment/checkout`
- **Action Method**: `CheckoutMarketplaceOrderPayment`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Route Parameters**:
- `id` (`Guid`)

**Request Body**: `PaymentCheckoutRequest`
*Raw object, see body structure.*

**Response DTO**: `MarketplaceOrderPaymentCheckoutResponseDto`
| Property | Type |
| :--- | :--- |
| MarketplaceOrderId | `Guid` |
| OrderNumber | `string` |
| Amount | `decimal` |
| Currency | `string` |
| PaymentProvider | `string` |
| CheckoutUrl | `string` |
| Status | `string` |
| PaymentGatewayReference | `string` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{
  "marketplaceOrderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "orderNumber": "01012345678",
  "amount": 99.99,
  "currency": "sample_currency",
  "paymentProvider": "sample_paymentprovider",
  "checkoutUrl": "sample_checkouturl",
  "status": "sample_status",
  "paymentGatewayReference": "sample_paymentgatewayreference"
}
```

**Business Notes**:
- Automatically handled by MediatR request `CheckoutMarketplaceOrderPaymentCommand`.

---

### HTTPPOST `api/marketplace-orders/{id:guid}/dispute`
- **Action Method**: `OpenDispute`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Request Body**: `OpenDisputeRequest`
*Raw object, see body structure.*

**Response DTO**: `OpenDisputeResponse`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `OpenMarketplaceOrderDisputeCommand`.

---

### HTTPPOST `api/marketplace-orders/{id:guid}/payment/mock-confirm`
- **Action Method**: `ConfirmMarketplaceOrderPaymentForDemo`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Route Parameters**:
- `id` (`Guid`)

**Request Body**: `PaymentConfirmRequest`
*Raw object, see body structure.*

**Response DTO**: `MarketplaceOrderPaymentConfirmResponseDto`
| Property | Type |
| :--- | :--- |
| MarketplaceOrderId | `Guid` |
| OrderStatus | `string` |
| OrderStatusAr | `string` |
| EscrowStatus | `string` |
| EscrowStatusAr | `string` |
| Message | `string` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{
  "marketplaceOrderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "orderStatus": "sample_orderstatus",
  "orderStatusAr": "sample_orderstatusar",
  "escrowStatus": "sample_escrowstatus",
  "escrowStatusAr": "sample_escrowstatusar",
  "message": "sample_message"
}
```

**Business Notes**:
- Automatically handled by MediatR request `ConfirmMarketplaceOrderPaymentCommand`.

---

### HTTPGET `api/marketplace-orders/{id:guid}/escrow`
- **Action Method**: `GetEscrow`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `MarketplaceEscrowDto`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetMarketplaceEscrowQuery`.

---

## MediaController
- **Route Prefix**: `api/media`
- **Default Class Auth**: `Authenticated`

### HTTPPOST `api/media/images`
- **Action Method**: `UploadImage`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `ImageUploadResponse`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `UploadImageCommand`.

---

## NotificationsController
- **Route Prefix**: `api/notifications`
- **Default Class Auth**: `Authenticated`

### HTTPGET `api/notifications/my`
- **Action Method**: `GetMy`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Query Parameters**:
- `isRead` (`bool?`)
- `page` (`int`)
- `pageSize` (`int`)

**Response DTO**: `NotificationListDto`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetMyNotificationsQuery`.

---

### HTTPPATCH `api/notifications/{id:guid}/read`
- **Action Method**: `MarkRead`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `MarkNotificationReadResult`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `MarkNotificationReadCommand`.

---

## ProfileCompletionController
- **Route Prefix**: `api/profilecompletion`
- **Default Class Auth**: `AllowAnonymous`

### HTTPGET `api/profilecompletion/completion-context`
- **Action Method**: `GetCompletionContext`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Response DTO**: `ProfileCompletionContextDto`
| Property | Type |
| :--- | :--- |
| Role | `string` |
| ProfileCompleted | `bool` |
| PhoneVerified | `bool` |
| HasDefaultAddress | `bool` |
| ProviderProfileCompleted | `bool?` |
| MissingRequirements | `List<string>` |

**Example Response Payload**:
```json
{
  "role": "sample_role",
  "profileCompleted": true,
  "phoneVerified": true,
  "hasDefaultAddress": true,
  "providerProfileCompleted": true,
  "missingRequirements": "sample_missingrequirements"
}
```

**Business Notes**:
- Automatically handled by MediatR request `GetCompletionContextQuery`.

---

## ProviderBookingsController
- **Route Prefix**: `api/provider/bookings`
- **Default Class Auth**: `Provider`

### HTTPGET `api/provider/bookings/requests`
- **Action Method**: `GetBookingRequests`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Provider`

**Response DTO**: `ProviderBookingRequestsResponseDto`
| Property | Type |
| :--- | :--- |
| Items | `IReadOnlyList<ProviderBookingRequestItemDto>` |
| TotalCount | `int` |

**Example Response Payload**:
```json
{
  "items": [],
  "totalCount": 1
}
```

**Business Notes**:
- Automatically handled by MediatR request `GetProviderBookingRequestsQuery`.

---

### HTTPGET `api/provider/bookings/active-jobs`
- **Action Method**: `GetActiveJobs`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Provider`

**Response DTO**: `ProviderActiveJobsResponseDto`
| Property | Type |
| :--- | :--- |
| Items | `IReadOnlyList<ProviderActiveJobItemDto>` |

**Example Response Payload**:
```json
{
  "items": []
}
```

**Business Notes**:
- Automatically handled by MediatR request `GetProviderActiveJobsQuery`.

---

## ProviderCalendarController
- **Route Prefix**: `api/provider/calendar`
- **Default Class Auth**: `Provider`

### HTTPGET `api/provider/calendar`
- **Action Method**: `Get`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Provider`

**Query Parameters**:
- `from` (`DateOnly`)
- `to` (`DateOnly`)

**Response DTO**: `ProviderCalendarResultDto`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetProviderCalendarQuery`.

---

## ProviderDashboardController
- **Route Prefix**: `api/provider/dashboard`
- **Default Class Auth**: `Provider`

### HTTPGET `api/provider/dashboard/summary`
- **Action Method**: `GetSummary`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Provider`

**Response DTO**: `ProviderDashboardSummaryDto`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetProviderDashboardSummaryQuery`.

---

## ProviderEarningsController
- **Route Prefix**: `api/provider/earnings`
- **Default Class Auth**: `Provider`

### HTTPGET `api/provider/earnings/service-summary`
- **Action Method**: `GetServiceSummary`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Provider`

**Query Parameters**:
- `from` (`DateOnly`)
- `to` (`DateOnly`)

**Response DTO**: `ProviderEarningsServiceSummaryDto`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetProviderEarningsServiceSummaryQuery`.

---

## ProviderOperatorsController
- **Route Prefix**: `api/provider/operators`
- **Default Class Auth**: `Provider`

### HTTPGET `api/provider/operators`
- **Action Method**: `GetAll`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Provider`

**Query Parameters**:
- `includeInactive` (`bool`)

**Response DTO**: `List<OperatorDto>`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetProviderOperatorsQuery`.

---

### HTTPPOST `api/provider/operators`
- **Action Method**: `Create`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Provider`

**Request Body**: `CreateOperatorCommand`
*Raw object, see body structure.*

**Response DTO**: `OperatorDto`
| Property | Type |
| :--- | :--- |
| Id | `Guid` |
| ProviderProfileId | `Guid` |
| FullName | `string` |
| YearsOfExperience | `int?` |
| Specialization | `string?` |
| LicenseType | `string?` |
| LicenseNumber | `string?` |
| LicenseExpiryDate | `DateOnly?` |
| ProfilePhotoUrl | `string?` |
| IsActive | `bool` |
| CreatedAt | `DateTime` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
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

**Business Notes**:
- Automatically handled by MediatR request `CreateOperatorCommand`.

---

### HTTPPUT `api/provider/operators/{id:guid}`
- **Action Method**: `Update`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Provider`

**Request Body**: `UpdateOperatorCommand`
*Raw object, see body structure.*

**Response DTO**: `OperatorDto`
| Property | Type |
| :--- | :--- |
| Id | `Guid` |
| ProviderProfileId | `Guid` |
| FullName | `string` |
| YearsOfExperience | `int?` |
| Specialization | `string?` |
| LicenseType | `string?` |
| LicenseNumber | `string?` |
| LicenseExpiryDate | `DateOnly?` |
| ProfilePhotoUrl | `string?` |
| IsActive | `bool` |
| CreatedAt | `DateTime` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
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

**Business Notes**:
- Automatically handled by MediatR request `UpdateOperatorCommand`.

---

### HTTPDELETE `api/provider/operators/{id:guid}`
- **Action Method**: `Delete`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Provider`

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `DeleteOperatorCommand`.

---

## ProviderProfileCardController
- **Route Prefix**: `api/provider`
- **Default Class Auth**: `Authenticated`

### HTTPGET `api/provider/profile-card/{providerProfileId:guid}`
- **Action Method**: `GetProfileCard`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `ProviderProfileCardDto`
| Property | Type |
| :--- | :--- |
| ProviderProfileId | `Guid` |
| CompanyName | `string` |
| AverageRating | `decimal` |
| TotalReviewsCount | `int` |
| CompletedBookingsCount | `int` |
| TrustScore | `decimal` |
| TrustLevel | `TrustLevel` |
| ActiveSince | `DateTime` |

**Example Response Payload**:
```json
{
  "providerProfileId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "companyName": "sample_companyname",
  "averageRating": 99.99,
  "totalReviewsCount": 1,
  "completedBookingsCount": 1,
  "trustScore": 99.99,
  "trustLevel": {},
  "activeSince": "2026-07-05T19:17:54Z"
}
```

**Business Notes**:
- Automatically handled by MediatR request `GetProviderProfileCardQuery`.

---

## ProviderProfileController
- **Route Prefix**: `api/provider/profile`
- **Default Class Auth**: `Provider`

### HTTPGET `api/provider/profile/me`
- **Action Method**: `GetMe`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Provider`

**Response DTO**: `ProviderProfileDto`
| Property | Type |
| :--- | :--- |
| UserId | `Guid` |
| FirstName | `string` |
| LastName | `string` |
| UserName | `string` |
| Email | `string?` |
| PhoneNumber | `string?` |
| Id | `Guid` |
| CompanyName | `string` |
| BusinessDescription | `string?` |
| BaseLatitude | `decimal?` |
| BaseLongitude | `decimal?` |
| ServiceRadiusKm | `int` |
| AverageRating | `decimal` |
| TotalReviewsCount | `int` |
| CompletedBookingsCount | `int` |
| ResponseRate | `decimal` |
| TrustScore | `decimal` |
| TrustLevel | `TrustLevel` |
| OnboardingTier | `int` |
| SearchRankingModifier | `decimal` |
| TrustScoreLastComputedAt | `DateTime?` |
| CreatedAt | `DateTime` |
| UpdatedAt | `DateTime` |

**Example Response Payload**:
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "sample_firstname",
  "lastName": "sample_lastname",
  "userName": "sample_username",
  "email": "user@heveq.com",
  "phoneNumber": "01012345678",
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "companyName": "sample_companyname",
  "businessDescription": "sample_businessdescription",
  "baseLatitude": 99.99,
  "baseLongitude": 99.99,
  "serviceRadiusKm": 1,
  "averageRating": 99.99,
  "totalReviewsCount": 1,
  "completedBookingsCount": 1,
  "responseRate": 99.99,
  "trustScore": 99.99,
  "trustLevel": {},
  "onboardingTier": 1,
  "searchRankingModifier": 99.99,
  "trustScoreLastComputedAt": "2026-07-05T19:17:54Z",
  "createdAt": "2026-07-05T19:17:54Z",
  "updatedAt": "2026-07-05T19:17:54Z"
}
```

**Business Notes**:
- Automatically handled by MediatR request `GetProviderProfileQuery`.

---

### HTTPPUT `api/provider/profile/me`
- **Action Method**: `UpdateMe`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Provider`

**Request Body**: `UpdateProviderProfileCommand`
*Raw object, see body structure.*

**Response DTO**: `ProviderProfileDto`
| Property | Type |
| :--- | :--- |
| UserId | `Guid` |
| FirstName | `string` |
| LastName | `string` |
| UserName | `string` |
| Email | `string?` |
| PhoneNumber | `string?` |
| Id | `Guid` |
| CompanyName | `string` |
| BusinessDescription | `string?` |
| BaseLatitude | `decimal?` |
| BaseLongitude | `decimal?` |
| ServiceRadiusKm | `int` |
| AverageRating | `decimal` |
| TotalReviewsCount | `int` |
| CompletedBookingsCount | `int` |
| ResponseRate | `decimal` |
| TrustScore | `decimal` |
| TrustLevel | `TrustLevel` |
| OnboardingTier | `int` |
| SearchRankingModifier | `decimal` |
| TrustScoreLastComputedAt | `DateTime?` |
| CreatedAt | `DateTime` |
| UpdatedAt | `DateTime` |

**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "sample_firstname",
  "lastName": "sample_lastname",
  "userName": "sample_username",
  "email": "user@heveq.com",
  "phoneNumber": "01012345678",
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "companyName": "sample_companyname",
  "businessDescription": "sample_businessdescription",
  "baseLatitude": 99.99,
  "baseLongitude": 99.99,
  "serviceRadiusKm": 1,
  "averageRating": 99.99,
  "totalReviewsCount": 1,
  "completedBookingsCount": 1,
  "responseRate": 99.99,
  "trustScore": 99.99,
  "trustLevel": {},
  "onboardingTier": 1,
  "searchRankingModifier": 99.99,
  "trustScoreLastComputedAt": "2026-07-05T19:17:54Z",
  "createdAt": "2026-07-05T19:17:54Z",
  "updatedAt": "2026-07-05T19:17:54Z"
}
```

**Business Notes**:
- Automatically handled by MediatR request `UpdateProviderProfileCommand`.

---

### HTTPGET `api/provider/profile/me/trust-history`
- **Action Method**: `GetTrustHistory`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Provider`

**Response DTO**: `List<ProviderTrustHistoryDto>`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetProviderTrustHistoryQuery`.

---

## PublicServiceListingsController
- **Route Prefix**: `api/public/service-listings`
- **Default Class Auth**: `AllowAnonymous`

### HTTPGET `api/public/service-listings`
- **Action Method**: `GetPublicListings`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Query Parameters**:
- `query` (`GetPublicServiceListingsQuery`)

**Response DTO**: `PublicPaginatedList<PublicServiceListingDto>`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetPublicServiceListingsQuery`.

---

### HTTPGET `api/public/service-listings/{id}`
- **Action Method**: `GetPublicListingById`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `GetPublicServiceListingByIdQuery`.

---

### HTTPGET `api/public/service-listings/{id}`
- **Action Method**: `GetPublicListingById`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Response DTO**: `IActionResult`
**Business Notes**:
- Automatically handled by MediatR request `GetPublicServiceListingByIdQuery`.

---

## ReviewsController
- **Route Prefix**: `api/reviews`
- **Default Class Auth**: `AllowAnonymous`

### HTTPPOST `api/reviews`
- **Action Method**: `Submit`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Request Body**: `SubmitReviewCommand`
*Raw object, see body structure.*

**Response DTO**: `SubmitReviewResult`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `SubmitReviewCommand`.

---

### HTTPGET `api/reviews/for-user/{userId:guid}`
- **Action Method**: `GetForUser`
- **Authentication Required**: `No`
- **Roles Allowed**: `Any`

**Response DTO**: `ReviewListDto`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetUserReviewsQuery`.

---

## TicketsController
- **Route Prefix**: `api/tickets`
- **Default Class Auth**: `Authenticated`

### HTTPPOST `api/tickets`
- **Action Method**: `Create`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Request Body**: `CreateTicketCommand`
*Raw object, see body structure.*

**Response DTO**: `CreateTicketResult`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `CreateTicketCommand`.

---

### HTTPGET `api/tickets/my`
- **Action Method**: `GetMy`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `MyTicketsResult`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetMyTicketsQuery`.

---

### HTTPGET `api/tickets/{id:guid}`
- **Action Method**: `GetDetails`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Response DTO**: `TicketDetailsDto`
**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `GetTicketDetailsQuery`.

---

### HTTPPOST `api/tickets/{id:guid}/messages`
- **Action Method**: `AddMessage`
- **Authentication Required**: `Yes`
- **Roles Allowed**: `Authenticated`

**Request Body**: `AddTicketMessageBody`
*Raw object, see body structure.*

**Response DTO**: `AddTicketMessageResult`
**Example Request Payload**:
```json
{}
```

**Example Response Payload**:
```json
{}
```

**Business Notes**:
- Automatically handled by MediatR request `AddTicketMessageCommand`.

---

