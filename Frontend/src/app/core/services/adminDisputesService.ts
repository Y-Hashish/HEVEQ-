import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../constants/api.constants';
import { AdminDispute, PaginatedResponse } from '../models/admin.models';

@Injectable({
  providedIn: 'root'
})
export class AdminDisputesService {
  constructor(private http: HttpClient) {}

  getDisputes(page: number = 1, pageSize: number = 10, status?: string) {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (status) {
      params = params.set('status', status);
    }

    return this.http.get<PaginatedResponse<AdminDispute>>(`${API_BASE_URL}/admin/disputes`, { params });
  }

  // Booking dispute resolutions
  releaseBookingToProvider(bookingId: string, decisionNote: string) {
    return this.http.post(`${API_BASE_URL}/admin/disputes/bookings/${bookingId}/release-to-provider`, { decisionNote });
  }

  refundBookingToCustomer(bookingId: string, decisionNote: string) {
    return this.http.post(`${API_BASE_URL}/admin/disputes/bookings/${bookingId}/refund-customer`, { decisionNote });
  }

  partialSettleBooking(bookingId: string, customerAmount: number, providerAmount: number, decisionNote: string) {
    return this.http.post(`${API_BASE_URL}/admin/disputes/bookings/${bookingId}/partial-settlement`, {
      customerAmount,
      providerAmount,
      decisionNote
    });
  }

  // Marketplace dispute resolutions
  releaseMarketplaceToSeller(orderId: string, decisionNote: string) {
    return this.http.post(`${API_BASE_URL}/admin/disputes/marketplace-orders/${orderId}/release-to-seller`, { decisionNote });
  }

  refundMarketplaceToBuyer(orderId: string, decisionNote: string) {
    return this.http.post(`${API_BASE_URL}/admin/disputes/marketplace-orders/${orderId}/refund-buyer`, { decisionNote });
  }
}
