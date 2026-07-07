export interface PagedResult<T> {
  items: T[]
  totalCount: number
}

// Mirrors HEVEQ.Application.Features.MarketPlace.DTOs.MarketPlaceListingDTO
export interface MarketplaceListing {
  id: string
  title: string
  price: number
  condition: string
  conditionAr: string
  location: string | null
  coverPhotoUrl: string | null
  sellerName: string
  categoryName: string
  transactionMethod: string
  averageRating: number | null
  totalReviewsCount: number | null
  status: string
  statusAr: string
}

// Mirrors HEVEQ.Application.Features.MarketPlace.DTOs.MarketplaceListingPhotoDto
export interface MarketplaceListingPhotoDto {
  id: string
  photoUrl: string
  displayOrder: number
}

// Mirrors HEVEQ.Application.Features.MarketPlaceListings.DTOs.ListingSellerDto
export interface ListingSellerDto {
  id: string
  displayName: string
  averageRating: number | null
  totalReviewsCount: number | null
}

// Mirrors HEVEQ.Application.Features.MarketPlaceListings.DTOs.ListingManagementInfoDto
// Only populated for the listing owner or an admin
export interface ListingManagementInfoDto {
  aiRiskScore: number | null
  aiRiskLevel: string | null
  aiRiskFlags: string | null
  adminRejectionNote: string | null
}

// Mirrors HEVEQ.Application.Features.MarketPlace.DTOs.MarketplaceListingDetailsDto
export interface MarketplaceListingDetails {
  id: string
  title: string
  description: string
  price: number
  categoryId: number
  condition: string
  conditionAr: string
  yearOfManufacture: number | null
  specifications: string | null
  isNegotiable: boolean
  transactionMethod: MarketplaceTransactionMethod
  governorate: string | null
  district: string | null
  status?: string
  statusAr?: string
  videoUrl?: string | null
  photos: MarketplaceListingPhotoDto[]
  seller: ListingSellerDto
  canBuyNow: boolean
  managementInfo: ListingManagementInfoDto | null
}
export type MarketplaceCondition = 'New' | 'Excellent' | 'Good' | 'Fair' | 'Used'

// Mirrors HEVEQ.Domain.Enums.DeliveryPreference
export type MarketplaceTransactionMethod = 'Pickup' | 'Delivery' | 'Either'

export interface CreateMarketplaceListingResponse {
  id: string
  status: string
  statusAr: string
  nextStep: string
}

// Mirrors GetMarketPlaceListingsQuery (public, keyword filters + paging)
export interface MarketplaceListingsQuery {
  search?: string
  condition?: MarketplaceCondition
  governorate?: string
  minPrice?: number
  maxPrice?: number
  categoryId?: number
  page: number
  pageSize: number
}

export interface CreateMarketplaceListingRequest {
  categoryId: number
  title: string
  condition: MarketplaceCondition
  yearOfManufacture: number
  description: string
  specifications: string
  price: number
  isNegotiable: boolean
  transactionMethod: MarketplaceTransactionMethod
  governorate: string
  district: string
}