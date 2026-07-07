import { HttpClient, HttpParams } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { API_BASE_URL } from '../constants/api.constants'
import {
  AddBlackoutDatePayload,
  AddPhotoPayload,
  AvailabilityPayload,
  CreateServiceListingResult,
  ManageServiceListing,
  PagedResult,
  ProviderServiceListingsResult,
  PublicServiceListingDetail,
  PublicServiceListing,
  PublicServiceListingFilters,
  ServiceListingDetail,
  ServiceListingFormPayload,
  ServiceListingStatus,
  SubmitForReviewResult
} from '../models/service-listing.models'

@Injectable({
  providedIn: 'root'
})
export class ServiceListings {
  constructor(private http: HttpClient) {}

  // ---------- Public (unauthenticated) ----------

  getPublicList(filters: PublicServiceListingFilters) {
    let params = new HttpParams()

    if (filters.search) params = params.set('search', filters.search)
    if (filters.categoryId != null) params = params.set('categoryId', filters.categoryId)
    if (filters.governorate) params = params.set('governorate', filters.governorate)
    if (filters.minRate != null) params = params.set('minRate', filters.minRate)
    if (filters.maxRate != null) params = params.set('maxRate', filters.maxRate)
    params = params.set('page', filters.page ?? 1)
    params = params.set('pageSize', filters.pageSize ?? 10)

    return this.http.get<PagedResult<PublicServiceListing>>(
      `${API_BASE_URL}/public/service-listings`,
      { params }
    )
  }

  getPublicById(id: string) {
    return this.http.get<PublicServiceListingDetail>(`${API_BASE_URL}/public/service-listings/${id}`)
  }

  // ---------- Provider-scoped (authenticated) ----------

  getMine(status?: ServiceListingStatus) {
    const params = status ? new HttpParams().set('status', status) : undefined
    return this.http.get<ProviderServiceListingsResult>(`${API_BASE_URL}/provider/service-listings`, { params })
  }

  getManage(id: string) {
    return this.http.get<ManageServiceListing>(`${API_BASE_URL}/provider/service-listings/${id}/manage`)
  }

  // GET /api/service-listings/{id} -> GetServiceListingByIdQueryHandler.
  // Returns pricing/equipment fields that getManage() does not — needed to
  // prefill the edit form correctly instead of guessing/blanking them.
  getById(id: string) {
    return this.http.get<ServiceListingDetail>(`${API_BASE_URL}/service-listings/${id}`)
  }

  create(payload: ServiceListingFormPayload) {
    return this.http.post<CreateServiceListingResult>(`${API_BASE_URL}/service-listings`, payload)
  }

  update(id: string, payload: ServiceListingFormPayload) {
    return this.http.put<void>(`${API_BASE_URL}/service-listings/${id}`, payload)
  }

  delete(id: string) {
    return this.http.delete<void>(`${API_BASE_URL}/service-listings/${id}`)
  }

  submitForReview(id: string) {
    return this.http.post<SubmitForReviewResult>(`${API_BASE_URL}/service-listings/${id}/submit-for-review`, {})
  }

  // ---------- Photos ----------

  addPhoto(listingId: string, payload: AddPhotoPayload) {
    return this.http.post<string>(`${API_BASE_URL}/service-listings/${listingId}/photos`, payload)
  }

  deletePhoto(listingId: string, photoId: string) {
    return this.http.delete<void>(`${API_BASE_URL}/service-listings/${listingId}/photos/${photoId}`)
  }

  // ---------- Operators (linking only — see Operators roster gap-fill for CRUD) ----------

  linkOperator(listingId: string, operatorId: string) {
    // Backend: ServiceListingsController.LinkOperator(Guid id, [FromBody] Guid
    // operatorId) — expects a bare GUID string as the whole body, not { operatorId }.
    return this.http.post<{ message: string }>(
      `${API_BASE_URL}/service-listings/${listingId}/operators`,
      JSON.stringify(operatorId),
      { headers: { 'Content-Type': 'application/json' } }
    )
  }

  unlinkOperator(listingId: string, operatorId: string) {
    return this.http.delete<void>(`${API_BASE_URL}/service-listings/${listingId}/operators/${operatorId}`)
  }

  // ---------- Availability ----------

  addAvailability(listingId: string, payload: AvailabilityPayload) {
    return this.http.post<string>(`${API_BASE_URL}/service-listings/${listingId}/availability`, payload)
  }

  updateAvailability(listingId: string, availabilityId: string, payload: AvailabilityPayload) {
    return this.http.put<void>(`${API_BASE_URL}/service-listings/${listingId}/availability/${availabilityId}`, payload)
  }

  deleteAvailability(listingId: string, availabilityId: string) {
    return this.http.delete<void>(`${API_BASE_URL}/service-listings/${listingId}/availability/${availabilityId}`)
  }

  // ---------- Blackout dates ----------

  addBlackoutDate(listingId: string, payload: AddBlackoutDatePayload) {
    return this.http.post<string>(`${API_BASE_URL}/service-listings/${listingId}/blackout-dates`, payload)
  }

  deleteBlackoutDate(listingId: string, blackoutDateId: string) {
    return this.http.delete<void>(`${API_BASE_URL}/service-listings/${listingId}/blackout-dates/${blackoutDateId}`)
  }
}