export interface SubmitReviewRequest {
  bookingId: string | null
  marketplaceOrderId: string | null
  rating: number
  comment: string | null
}

export interface SubmitReviewResponse {
  id: string
  rating: number
  isPublished: boolean
  message: string
}

export interface ReviewItem {
  id: string
  reviewerName: string
  rating: number
  comment: string | null
  createdAt: string
}

export interface UserReviewsResponse {
  items: ReviewItem[]
  averageRating: number
  totalCount: number
}

export type ReviewTargetType = 'booking' | 'marketplaceOrder'