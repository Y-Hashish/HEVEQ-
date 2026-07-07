export interface ProfileAddress {
  id: string
  label: string | null
  governorate: string
  district: string
  street: string | null
  latitude: number | null
  longitude: number | null
  isDefault: boolean
}

export interface CustomerProfile {
  userId: string
  displayName: string
  firstName: string
  lastName: string
  userName: string
  email: string | null
  phoneNumber: string | null
  isPhoneVerified: boolean
  defaultAddress: ProfileAddress | null

  id: string
  trustScore: number
  cancellationRate: number | null
  disputeFrequencyScore: number | null
  paymentFailureCount: number
  reviewAuthenticityScore: number | null
  requiresAdditionalVerification: boolean
  totalBookings: number
  trustScoreLastComputedAt: string | null
  createdAt: string
  updatedAt: string
}

export interface UpdateCustomerProfileRequest {
  firstName: string
  lastName: string
  email: string
  phoneNumber: string | null
  defaultAddress: {
    label: string | null
    governorate: string
    district: string
    street: string | null
  } | null
}

export interface CustomerTrustHistoryItem {
  id: string
  trustScore: number
  triggerEvent: string | null
  recordedAt: string
}

export interface ProviderProfile {
  userId: string
  firstName: string
  lastName: string
  userName: string
  email: string | null
  phoneNumber: string | null

  id: string
  companyName: string
  businessDescription: string | null
  baseLatitude: number | null
  baseLongitude: number | null
  serviceRadiusKm: number

  averageRating: number
  totalReviewsCount: number
  completedBookingsCount: number
  responseRate: number
  trustScore: number
  trustLevel: number
  onboardingTier: number
  searchRankingModifier: number
  trustScoreLastComputedAt: string | null
  createdAt: string
  updatedAt: string
}

export interface UpdateProviderProfileRequest {
  firstName: string
  lastName: string
  userName: string
  email: string
  phoneNumber: string | null
  companyName: string
  businessDescription: string | null
  baseLatitude: number | null
  baseLongitude: number | null
  serviceRadiusKm: number
}

export interface ProviderTrustHistoryItem {
  id: string
  trustScore: number
  trustLevel: number
  componentRating: number | null
  componentCompletion: number | null
  componentResponse: number | null
  componentDocs: number | null
  componentIncident: number | null
  triggerEvent: string | null
  recordedAt: string
}