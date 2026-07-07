import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { API_BASE_URL } from '../constants/api.constants'
import {
  CreateMarketplaceOrderRequest,
  CreateMarketplaceOrderResponse,
  MarketplaceEscrow,
  MarketplaceOrderActionResponse,
  MarketplaceOrderDetails,
  MarketplaceOrderListItem,
  MarketplaceOrderPaymentCheckoutResponse,
  MarketplaceOrderPaymentConfirmResponse,
  MarketplaceOrderTracking,
  OpenMarketplaceOrderDisputeResponse
} from '../models/marketplace-order.models'

@Injectable({
  providedIn: 'root'
})
export class MarketplaceOrdersService {
  constructor(private http: HttpClient) {}

  createOrder(request: CreateMarketplaceOrderRequest) {
    return this.http.post<CreateMarketplaceOrderResponse>(`${API_BASE_URL}/marketplace-orders`, request)
  }

  getMyPurchases() {
    return this.http.get<MarketplaceOrderListItem[]>(`${API_BASE_URL}/marketplace-orders/my/purchases`)
  }

  getMySales() {
    return this.http.get<MarketplaceOrderListItem[]>(`${API_BASE_URL}/marketplace-orders/my/sales`)
  }

  getById(id: string) {
    return this.http.get<MarketplaceOrderDetails>(`${API_BASE_URL}/marketplace-orders/${id}`)
  }

  getTracking(id: string) {
    return this.http.get<MarketplaceOrderTracking>(`${API_BASE_URL}/marketplace-orders/${id}/tracking`)
  }

  getEscrow(id: string) {
    return this.http.get<MarketplaceEscrow>(`${API_BASE_URL}/marketplace-orders/${id}/escrow`)
  }

  checkoutPayment(id: string, request: { paymentMethod: string; successUrl: string; cancelUrl: string }) {
    return this.http.post<MarketplaceOrderPaymentCheckoutResponse>(`${API_BASE_URL}/marketplace-orders/${id}/payment/checkout`, request)
  }

  confirmPayment(id: string, request: { paymentGatewayReference: string | null }) {
    return this.http.post<MarketplaceOrderPaymentConfirmResponse>(`${API_BASE_URL}/marketplace-orders/${id}/payment/mock-confirm`, request)
  }

  sellerConfirm(id: string) {
    return this.http.post<MarketplaceOrderActionResponse>(`${API_BASE_URL}/marketplace-orders/${id}/seller-confirm`, {})
  }

  dispatch(id: string, trackingNumber: string | null) {
    return this.http.post<MarketplaceOrderActionResponse>(`${API_BASE_URL}/marketplace-orders/${id}/dispatch`, { trackingNumber })
  }

  deliver(id: string) {
    return this.http.post<MarketplaceOrderActionResponse>(`${API_BASE_URL}/marketplace-orders/${id}/deliver`, {})
  }

  complete(id: string) {
    return this.http.post<MarketplaceOrderActionResponse>(`${API_BASE_URL}/marketplace-orders/${id}/buyer-confirm-receipt`, {})
  }

  cancel(id: string, reason: string | null) {
    return this.http.post<MarketplaceOrderActionResponse>(`${API_BASE_URL}/marketplace-orders/${id}/cancel`, { reason })
  }

  dispute(id: string, reason: string, evidencePhotoUrls: string[] = []) {
    return this.http.post<OpenMarketplaceOrderDisputeResponse>(`${API_BASE_URL}/marketplace-orders/${id}/dispute`, {
      reason,
      evidencePhotoUrls
    })
  }
}
