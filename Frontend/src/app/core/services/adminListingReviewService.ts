import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
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
    return this.http.get<ListingReviewDetails>(`${API_BASE_URL}/admin/service-listings/${id}/review-details`);
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
    return this.http.get<ListingReviewDetails>(`${API_BASE_URL}/admin/marketplace-listings/${id}/review-details`);
  }

  approveMarketplaceListing(id: string) {
    return this.http.post<ListingReviewResult>(`${API_BASE_URL}/admin/marketplace-listings/${id}/approve`, {});
  }

  rejectMarketplaceListing(id: string, reason: string) {
    return this.http.post<ListingReviewResult>(`${API_BASE_URL}/admin/marketplace-listings/${id}/reject`, { adminRejectionNote: reason });
  }
}
