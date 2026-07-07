export type BookingStatusValue = string | number

export interface BookingListResponse {
  items: BookingListItem[]
  totalCount: number
}

export interface BookingListItem {
  id: string
  bookingNumber: string
  serviceListingId?: string | null
  serviceTitle?: string | null
  providerName?: string | null
  customerName?: string | null
  jobTitle?: string | null
  jobDescription?: string | null
  governorate?: string | null
  district?: string | null
  street?: string | null
  requestedStartDate?: string | null
  requestedStartTime?: string | null
  estimatedDurationHours?: number | null
  status: BookingStatusValue
  statusAr?: string | null
  totalPrice?: number | null
  finalPrice?: number | null
  hasReview?: boolean
  createdAt?: string | null
  updatedAt?: string | null
}

export interface BookingDetails extends BookingListItem {
  siteContactName?: string | null
  siteContactPhone?: string | null
  accessRequirements?: string | null
  safetyNotes?: string | null
  latitude?: number | null
  longitude?: number | null
  completedAt?: string | null
  completionConfirmedAt?: string | null
  disputeReason?: string | null
}

export interface PaymentConfirmRequest {
  paymentGatewayReference: string
}

export interface BookingActionResponse {
  bookingId?: string
  status?: string
  statusAr?: string
  message?: string
}

export interface DisputeBookingRequest {
  reason: string
  evidencePhotoUrls: string[]
}

export interface BookingReviewForm {
  bookingId: string
  rating: number
  comment: string
}

export interface BookingReviewResult {
  id: string
  rating: number
  isPublished: boolean
  message: string
}

export interface BookingTimelineItem {
  key: string
  label: string
  labelAr: string
  date: string | null
  done: boolean
}

export interface BookingNextAction {
  label: string
  labelAr: string
  actionKey: string
}

export interface BookingAvailableActions {
  canAccept: boolean
  canReject: boolean
  canStart: boolean
  canComplete: boolean
  canConfirmCompletion: boolean
  canDispute: boolean
  canCancel: boolean
  canProviderCancel: boolean
  canPay: boolean
}

export interface BookingTracker {
  bookingId: string
  bookingNumber: string
  currentStatus: string
  currentStatusAr: string
  timeline: BookingTimelineItem[]
  nextAction: BookingNextAction
  availableActions: BookingAvailableActions
}

export interface BookingEscrow {
  bookingId: string
  grossAmount: number
  platformCommission: number
  providerPayout: number
  vatAmount: number
  status: string
  statusAr: string
  capturedAt: string | null
  releasedAt: string | null
  frozenAt: string | null
}

export interface CustomerTimeAdjustmentItem {
  id: string
  bookingId: string
  bookingNumber: string
  requestedAdditionalHrs: number
  additionalCostAmount: number
  status: string
  statusAr: string
  providerNote: string | null
  customerAcknowledgedAt: string | null
  createdAt: string
  canApprove: boolean
  canReject: boolean
  canPay: boolean
}

export interface TimeAdjustmentDecisionResponse {
  id: string
  bookingId: string
  bookingNumber: string
  requestedAdditionalHrs: number
  additionalCostAmount: number
  bookingEstimatedDurationHours: number
  bookingEstimatedTotal: number
  status: string
  statusAr: string
  customerAcknowledgedAt: string | null
  message: string
}

export interface TimeAdjustmentPaymentCheckoutResponse {
  timeAdjustmentRequestId: string
  bookingId: string
  bookingNumber: string
  amount: number
  currency: string
  paymentProvider: string
  checkoutUrl: string
  status: string
  paymentGatewayReference: string
}

export interface TimeAdjustmentPaymentConfirmResponse {
  timeAdjustmentRequestId: string
  bookingId: string
  bookingNumber: string
  timeAdjustmentStatus: string
  timeAdjustmentStatusAr: string
  escrowStatus: string
  escrowStatusAr: string
  bookingEstimatedDurationHours: number
  bookingEstimatedTotal: number
  message: string
}

export interface BookingCreateContext {
  serviceListingId: string
  serviceTitle: string
  providerCompany: string
  hourlyRate: number | null
  dailyRate: number | null
  minimumBookingHours: number
  availability: BookingCreateAvailability[]
  defaultAddress: BookingCreateAddress | null
  customerEligibility: BookingCustomerEligibility
}

export interface BookingCreateAvailability {
  id: string
  dayOfWeek: number
  dayName: string
  dayNameAr: string
  openTime: string
  closeTime: string
}

export interface BookingCreateAddress {
  id: string
  label: string | null
  governorate: string
  district: string
  street: string | null
  latitude: number | null
  longitude: number | null
  isDefault?: boolean
}

export interface BookingCustomerEligibility {
  canBook: boolean
  missingRequirements: string[]
}

export interface CreateBookingRequest {
  serviceListingId: string
  jobTitle: string
  jobDescription: string | null
  addressId: string | null
  governorate: string | null
  district: string | null
  street: string | null
  latitude: number | null
  longitude: number | null
  requestedStartDate: string
  requestedStartTime: string
  estimatedDurationHours: number
  siteContactName: string | null
  siteContactPhone: string | null
  accessRequirements: string | null
  safetyNotes: string | null
  acceptOutOfZoneSurcharge: boolean
}

export interface CreateBookingResponse {
  id: string
  bookingNumber: string
  status: string
  statusAr: string
  serviceTitle: string
  providerCompany: string
  requestedStartDate: string
  requestedStartTime: string
  estimatedDurationHours: number
  hourlyRateSnapshot: number
  estimatedTotal: number
  message: string
}

export interface BookingPaymentCheckoutRequest {
  paymentMethod: string
  successUrl: string
  cancelUrl: string
}

export interface BookingPaymentCheckoutResponse {
  bookingId: string
  amount: number
  currency: string
  paymentProvider: string
  checkoutUrl: string
  status: string
  paymentGatewayReference: string
}

export interface BookingPaymentConfirmResponse {
  bookingId: string
  bookingStatus: string
  bookingStatusAr: string
  escrowStatus: string
  escrowStatusAr: string
  message: string
}

export interface CancelBookingRequest {
  reason: string
}

export interface CancelBookingResponse {
  bookingId: string
  status: string
  statusAr: string
  refundPercentage: number
  message: string
}