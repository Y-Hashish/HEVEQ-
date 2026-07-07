import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map } from 'rxjs';
import { API_BASE_URL } from '../constants/api.constants';
import { ListingReviewDetails, PaginatedResponse, PendingListing } from '../models/admin.models';

export interface RejectListingRequest {
  adminId?: string;
  reason: string;
}

export interface ListingReviewResult {
  id: string;
  status: string;
  statusText?: string;
  statusAr?: string;
  message?: string;
  adminRejectionNote?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminListingReviewService {
  constructor(private http: HttpClient) {}

  // Service Listings
  getPendingServiceListings(page: number = 1, pageSize: number = 10) {
    return this.http.get<PaginatedResponse<PendingListing>>(`${API_BASE_URL}/admin/service-listings/pending?page=${page}&pageSize=${pageSize}`);
  }

  getServiceListingDetails(id: string) {
    return this.http.get<ListingReviewDetails>(`${API_BASE_URL}/admin/service-listings/${id}/review-details`)
      .pipe(map(details => this.normalizeDetails(details)));
  }

  approveServiceListing(id: string) {
    return this.http.post<ListingReviewResult>(`${API_BASE_URL}/admin/service-listings/${id}/approve`, {});
  }

  rejectServiceListing(id: string, reason: string) {
    return this.http.post<ListingReviewResult>(`${API_BASE_URL}/admin/service-listings/${id}/reject`, { adminRejectionNote: reason });
  }

  // Marketplace Listings
  getPendingMarketplaceListings(page: number = 1, pageSize: number = 10) {
    return this.http.get<PaginatedResponse<PendingListing>>(`${API_BASE_URL}/admin/marketplace-listings/pending?page=${page}&pageSize=${pageSize}`);
  }

  getMarketplaceListingDetails(id: string) {
    return this.http.get<any>(`${API_BASE_URL}/admin/marketplace-listings/${id}/review-details`)
      .pipe(map(details => this.normalizeDetails(details)));
  }

  approveMarketplaceListing(id: string) {
    return this.http.post<ListingReviewResult>(`${API_BASE_URL}/admin/marketplace-listings/${id}/approve`, {});
  }

  rejectMarketplaceListing(id: string, reason: string) {
    return this.http.post<ListingReviewResult>(`${API_BASE_URL}/admin/marketplace-listings/${id}/reject`, { adminRejectionNote: reason });
  }

  private normalizeDetails(details: any): ListingReviewDetails {
    const sellerOrProvider = details.provider || details.seller || {};
    const companyName = sellerOrProvider.companyName
      || sellerOrProvider.displayName
      || sellerOrProvider.name
      || details.providerName
      || details.sellerName
      || 'مزود غير محدد';

    return {
      ...details,
      provider: {
        companyName,
        email: sellerOrProvider.email || details.providerEmail || details.sellerEmail || 'غير متوفر',
        phoneNumber: sellerOrProvider.phoneNumber || details.providerPhoneNumber || details.sellerPhoneNumber || 'غير متوفر'
      },
      aiRecommendation: details.aiRecommendation || details.aiRecommendationAr || 'لا توجد توصية متاحة',
      aiRiskFlags: details.aiRiskFlags || ''
    } as ListingReviewDetails;
  }
}
