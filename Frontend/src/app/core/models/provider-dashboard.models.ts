// Mirrors HEVEQ.Application.Features.ProviderDashboard.Queries.GetProviderDashboardSummary.ProviderDashboardSummaryDto
export interface ProviderDashboardSummary {
    totalServiceListings: number
    approvedServiceListings: number
    pendingServiceListings: number
    pendingBookingRequests: number
    activeJobs: number
    completedJobs: number
    averageRating: number
    responseRate: number
    trustScore: number
    trustLevel: string
    earningsThisMonth: number
  }