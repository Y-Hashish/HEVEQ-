export interface ProviderBookingRequestsResponse {
  items: ProviderBookingRequestItem[]
  totalCount: number
}

export interface ProviderBookingRequestItem {
  id: string
  bookingNumber: string
  customerName: string
  serviceTitle: string
  jobTitle: string
  location: string
  requestedStartDate: string
  requestedStartTime: string
  estimatedDurationHours: number
  estimatedTotal: number
  status: string
  statusAr: string
  canAccept: boolean
  canReject: boolean
}

export interface ProviderActiveJobsResponse {
  items: ProviderActiveJobItem[]
  totalCount?: number
}

export interface ProviderActiveJobItem {
  id: string
  bookingNumber: string
  serviceTitle: string
  jobTitle?: string | null
  customerName: string
  operatorName: string
  scheduledStart: string
  scheduledEnd: string
  status: string
  statusAr: string
  canStart: boolean
  canComplete: boolean
  location?: string | null
}

export interface ProviderOperatorItem {
  id: string
  fullName: string
  phoneNumber?: string | null
  isActive?: boolean
}

export interface AcceptBookingRequest {
  operatorId: string
}

export interface RejectBookingRequest {
  reason: string
}

export interface CompletionEvidencePhoto {
  photoUrl: string
  caption: string | null
  displayOrder: number
}

export interface CompleteBookingByProviderRequest {
  providerNotes: string | null
  photos: CompletionEvidencePhoto[]
}

export interface CreateTimeAdjustmentRequest {
  requestedAdditionalHrs: number
  providerNote: string
}

export interface BookingProviderActionResponse {
  bookingId?: string
  status?: string
  statusAr?: string
  evidenceFormId?: string
  message?: string
}