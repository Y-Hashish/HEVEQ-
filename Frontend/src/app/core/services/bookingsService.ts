import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { API_BASE_URL } from '../constants/api.constants'
import {
  BookingActionResponse,
  BookingDetails,
  BookingEscrow,
  BookingListItem,
  BookingListResponse,
  BookingTracker,
  DisputeBookingRequest,
  PaymentConfirmRequest,
  CustomerTimeAdjustmentItem,
  TimeAdjustmentDecisionResponse,
  TimeAdjustmentPaymentConfirmResponse,
  TimeAdjustmentPaymentCheckoutResponse,
  BookingCreateContext,
  BookingPaymentCheckoutRequest,
  BookingPaymentCheckoutResponse,
  BookingPaymentConfirmResponse,
  CreateBookingRequest,
  CreateBookingResponse,
  CancelBookingRequest,
  CancelBookingResponse
} from '../models/bookingModels'

@Injectable({
  providedIn: 'root'
})
export class BookingsService {
  constructor(private http: HttpClient) {}

  getMyBookings() {
    return this.http.get<BookingListResponse | BookingListItem[]>(
      `${API_BASE_URL}/Bookings/my`
    )
  }

  getBookingDetails(id: string) {
    return this.http.get<BookingDetails>(
      `${API_BASE_URL}/Bookings/${id}`
    )
  }

  getBookingTracker(id: string) {
  return this.http.get<BookingTracker>(
    `${API_BASE_URL}/Bookings/${id}/tracker`
  )
}

getBookingEscrow(id: string) {
  return this.http.get<BookingEscrow>(
    `${API_BASE_URL}/Bookings/${id}/escrow`
  )
}

getTimeAdjustments(bookingId: string) {
  return this.http.get<CustomerTimeAdjustmentItem[]>(
    `${API_BASE_URL}/Bookings/${bookingId}/time-adjustments`
  )
}

getBookingCreateContext(serviceListingId: string) {
  return this.http.get<BookingCreateContext>(
    `${API_BASE_URL}/Bookings/create-context/${serviceListingId}`
  )
}

createBooking(request: CreateBookingRequest) {
  return this.http.post<CreateBookingResponse>(
    `${API_BASE_URL}/Bookings`,
    request
  )
}

checkoutBookingPayment(bookingId: string, request: BookingPaymentCheckoutRequest) {
  return this.http.post<BookingPaymentCheckoutResponse>(
    `${API_BASE_URL}/Bookings/${bookingId}/payment/checkout`,
    request
  )
}

checkoutTimeAdjustmentPayment(id: string, request: BookingPaymentCheckoutRequest) {
  return this.http.post<TimeAdjustmentPaymentCheckoutResponse>(
    `${API_BASE_URL}/Bookings/time-adjustments/${id}/payment/checkout`,
    request
  )
}

confirmBookingPayment(bookingId: string, request: PaymentConfirmRequest) {
  return this.http.post<BookingPaymentConfirmResponse>(
    `${API_BASE_URL}/Bookings/${bookingId}/payment/mock-confirm`,
    request
  )
}

approveTimeAdjustment(id: string) {
  return this.http.post<TimeAdjustmentDecisionResponse>(
    `${API_BASE_URL}/Bookings/time-adjustments/${id}/approve`,
    {}
  )
}

rejectTimeAdjustment(id: string) {
  return this.http.post<TimeAdjustmentDecisionResponse>(
    `${API_BASE_URL}/Bookings/time-adjustments/${id}/reject`,
    {}
  )
}

confirmTimeAdjustmentPayment(id: string, request: PaymentConfirmRequest) {
  return this.http.post<TimeAdjustmentPaymentConfirmResponse>(
    `${API_BASE_URL}/Bookings/time-adjustments/${id}/payment/mock-confirm`,
    request
  )
}

  confirmMockPayment(bookingId: string, request: PaymentConfirmRequest) {
    return this.http.post<BookingActionResponse>(
      `${API_BASE_URL}/Bookings/${bookingId}/payment/mock-confirm`,
      request
    )
  }

  confirmCompletion(bookingId: string) {
    return this.http.post<BookingActionResponse>(
      `${API_BASE_URL}/Bookings/${bookingId}/confirm-completion`,
      {}
    )
  }

  disputeBooking(bookingId: string, request: DisputeBookingRequest) {
    return this.http.post<BookingActionResponse>(
      `${API_BASE_URL}/Bookings/${bookingId}/dispute`,
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