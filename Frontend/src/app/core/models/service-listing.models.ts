// NOTE: assumes the backend's default System.Text.Json camelCase naming policy
// (ASP.NET Core's Web API default). If your Program.cs overrides
// JsonSerializerOptions.PropertyNamingPolicy, these field names need to match
// whatever's actually configured — worth a quick Swagger check before relying
// on this against the real API.

export type ServiceListingStatus =
  | 'Draft'
  | 'PendingReview'
  | 'Active'
  | 'Rejected'
  | 'Suspended'
  | 'Inactive'

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

// ---------- Provider-scoped list (GET /api/provider/service-listings) ----------

export interface ProviderServiceListing {
  id: string
  title: string
  categoryName: string
  coverPhotoUrl: string | null
  hourlyRate: number
  status: ServiceListingStatus
  statusAr: string
  qualityScore: number | null
  photosCount: number
  operatorsCount: number
  availabilityCount: number
  createdAt: string
}

export interface ProviderServiceListingsResult {
  items: ProviderServiceListing[]
  totalCount: number
}

// ---------- Public list/detail (GET /api/public/service-listings) ----------

export interface PublicServiceListing {
  id: string
  title: string
  categoryName: string
  providerCompany: string
  coverPhotoUrl: string | null
  hourlyRate: number
  dailyRate: number | null
  minimumBookingHours: number
  location: string | null
  serviceRadiusKm: number
  averageRating: number
  totalReviewsCount: number
  trustLevel: string
  availabilityText: string
  status: ServiceListingStatus
  statusAr: string
}

export interface PublicServiceListingFilters {
  search?: string
  categoryId?: number
  governorate?: string
  minRate?: number
  maxRate?: number
  page?: number
  pageSize?: number
}

export interface ServiceListingPhoto {
  id: string
  listingId: string
  photoUrl: string
  displayOrder: number
}

export interface ServiceListingAvailability {
  id: string
  listingId?: string
  dayOfWeek: number // 0 = Sunday .. 6 = Saturday
  openTime: string // "HH:mm:ss" — confirm exact TimeOnly serialization format against Swagger
  closeTime: string
}

export interface ProviderPanel {
  companyName: string
  averageRating: number
  completedBookingsCount: number
  trustScore: number
  trustLevel: string
  governorate?: string | null
}

export interface OperatorSummary {
  id?: string
  fullName: string
  yearsOfExperience?: number
  specialization?: string | null
  licenseType?: string | null
  rating?: number | null
}

export interface PublicServiceListingDetail {
  id: string
  title: string
  description: string
  categoryName: string
  equipmentModel: string | null
  equipmentCapacity: string | null
  equipmentCondition: number | null
  yearOfManufacture: number | null
  hourlyRate: number
  dailyRate: number | null
  minimumBookingHours: number
  photos: Array<ServiceListingPhoto | string>
  availability: ServiceListingAvailability[]
  provider: ProviderPanel
  operators: OperatorSummary[]
  canRequestBooking: boolean
}

// ---------- Manage (owner-only) (GET /api/provider/service-listings/{id}/manage) ----------

export interface ServiceListingOperatorLink {
  operatorId: string
  fullName: string
  yearsOfExperience: number
  specialization: string | null
  licenseType: string | null
}

export interface BlackoutDate {
  id: string
  listingId: string
  operatorId: string | null
  date: string // "yyyy-MM-dd"
  reason: string | null
}

export interface ManageServiceListing {
  id: string
  title: string
  description: string
  categoryId: number
  tags: string | null
  equipmentModel: string | null
  equipmentCapacity: string | null
  equipmentCondition: number | null
  yearOfManufacture: number | null
  equipmentRegistrationNumber: string | null
  hourlyRate: number
  dailyRate: number | null
  minimumBookingHours: number
  status: ServiceListingStatus
  statusAr: string
  adminRejectionNote: string | null
  qualityScore: number | null
  photos: ServiceListingPhoto[]
  operators: ServiceListingOperatorLink[]
  availability: ServiceListingAvailability[]
  blackoutDates: BlackoutDate[]
  canSubmitForReview: boolean
  missingRequirements: string[]
}

// ---------- Create / Update / Submit ----------

export interface ServiceListingFormPayload {
  categoryId: number
  title: string
  description: string
  tags: string | null
  equipmentModel: string | null
  equipmentCapacity: string | null
  equipmentCondition: number | null
  yearOfManufacture: number | null
  equipmentRegistrationNumber: string | null
  hourlyRate: number
  dailyRate: number | null
  minimumBookingHours: number
}

export interface CreateServiceListingResult {
  id: string
  status: ServiceListingStatus
  statusAr: string
  nextStep: string
  message: string
}

export interface SubmitForReviewResult {
  id: string
  status: ServiceListingStatus
  statusAr: string
  message: string
}

// ---------- Photos ----------

export interface AddPhotoPayload {
  photoUrl: string
  displayOrder: number
}

// ---------- Operators (linking, not roster management) ----------

export interface LinkOperatorPayload {
  operatorId: string
}

// ---------- Availability ----------

export interface AvailabilityPayload {
  dayOfWeek: number
  openTime: string // "HH:mm" is fine to SEND; TimeOnly model binding accepts it
  closeTime: string
}

// ---------- Blackout dates ----------

export interface AddBlackoutDatePayload {
  date: string // "yyyy-MM-dd"
  reason: string | null
  operatorId: string | null
}

// ---------- Provider raw detail (GET /api/service-listings/{id}) ----------
// Confirmed against GetServiceListingByIdQueryHandler.cs — this is the ONLY
// endpoint that returns pricing/equipment fields for an existing listing.
// ManageServiceListingDto deliberately does not include them (see comments
// on ManageServiceListing above this file).
export interface ServiceListingDetail {
  id: string
  categoryId: number
  title: string
  description: string
  tags: string | null
  equipmentModel: string | null
  equipmentCapacity: string | null
  equipmentCondition: number | null
  yearOfManufacture: number | null
  equipmentRegistrationNumber: string | null
  hourlyRate: number
  dailyRate: number | null
  minimumBookingHours: number
  status: number // raw enum int here — this DTO does NOT stringify status
}