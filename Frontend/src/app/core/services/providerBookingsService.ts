import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { API_BASE_URL } from '../constants/api.constants'
import {
  AcceptBookingRequest,
  BookingProviderActionResponse,
  CompleteBookingByProviderRequest,
  CreateTimeAdjustmentRequest,
  ProviderActiveJobsResponse,
  ProviderBookingRequestsResponse,
  ProviderOperatorItem,
  RejectBookingRequest
} from '../models/providerBookingModels'

import {
  CancelBookingRequest,
  CancelBookingResponse
} from '../models/bookingModels'

@Injectable({
  providedIn: 'root'
})
export class ProviderBookingsService {
  constructor(private http: HttpClient) {}

  getBookingRequests() {
    return this.http.get<ProviderBookingRequestsResponse>(
      `${API_BASE_URL}/provider/bookings/requests`
    )
  }

  getActiveJobs() {
    return this.http.get<ProviderActiveJobsResponse>(
      `${API_BASE_URL}/provider/bookings/active-jobs`
    )
  }

  getOperators() {
    return this.http.get<ProviderOperatorItem[]>(
      `${API_BASE_URL}/provider/operators`
    )
  }

  acceptBooking(bookingId: string, request: AcceptBookingRequest) {
    return this.http.post<BookingProviderActionResponse>(
      `${API_BASE_URL}/Bookings/${bookingId}/accept`,
      request
    )
  }

  rejectBooking(bookingId: string, request: RejectBookingRequest) {
    return this.http.post<BookingProviderActionResponse>(
      `${API_BASE_URL}/Bookings/${bookingId}/reject`,
      request
    )
  }

  startBooking(bookingId: string) {
    return this.http.post<BookingProviderActionResponse>(
      `${API_BASE_URL}/Bookings/${bookingId}/start`,
      {}
    )
  }

  completeByProvider(bookingId: string, request: CompleteBookingByProviderRequest) {
    return this.http.post<BookingProviderActionResponse>(
      `${API_BASE_URL}/Bookings/${bookingId}/complete-by-provider`,
      request
    )
  }

  createTimeAdjustment(bookingId: string, request: CreateTimeAdjustmentRequest) {
    return this.http.post<BookingProviderActionResponse>(
      `${API_BASE_URL}/Bookings/${bookingId}/time-adjustments`,
      request
    )
  }

  cancelBooking(bookingId: string, request: CancelBookingRequest) {
  return this.http.post<CancelBookingResponse>(
    `${API_BASE_URL}/Bookings/${bookingId}/cancel`,
    request
  )
}
}