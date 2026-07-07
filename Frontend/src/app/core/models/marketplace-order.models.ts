// Marketplace order contracts used by the customer purchase flow and provider sales flow.
export type DeliveryPreference = 'Pickup' | 'Delivery' | 'Either'

export interface CreateMarketplaceOrderRequest {
  listingId: string
  deliveryAddress?: string
  deliveryPreference?: DeliveryPreference
}

export interface CreateMarketplaceOrderResponse {
  id: string
  orderNumber: string
  listingTitle: string
  amount: number
  status: string
  statusAr: string
  message: string
}

export interface MarketplaceOrderListItem {
  id: string
  orderNumber: string
  listingId: string
  listingTitle: string
  buyerId?: string
  buyerName?: string
  sellerId?: string
  sellerName?: string
  amount: number
  deliveryAddress?: string | null
  deliveryPreference?: string | null
  trackingNumber?: string | null
  status: string
  statusAr: string
  sellerConfirmedAt?: string | null
  dispatchedAt?: string | null
  deliveredAt?: string | null
  confirmedByBuyerAt?: string | null
  cancellationReason?: string | null
  cancelledAt?: string | null
  cancellationInitiatedByRole?: string | null
  returnShippingCost?: number | null
  returnShippingAcceptedByBuyerAt?: string | null
  createdAt: string
}

export interface MarketplaceOrderDetails extends MarketplaceOrderListItem {
  buyerId: string
  buyerName: string
  sellerId: string
  sellerName: string
  viewerRole?: string
}

export interface MarketplaceEscrow {
  orderId: string
  grossAmount: number
  platformCommission: number
  providerPayout: number
  status: string
  statusAr: string
  capturedAt?: string | null
  releasedAt?: string | null
  frozenAt?: string | null
}

export interface MarketplaceOrderTrackingActions {
  canSellerConfirm: boolean
  canDispatch: boolean
  canMarkDelivered: boolean
  canComplete: boolean
  canCancel: boolean
  canDispute: boolean
}

export interface MarketplaceOrderTrackingTimelineItem {
  label: string
  labelAr: string
  date?: string | null
  done: boolean
}

export interface MarketplaceOrderTracking {
  id: string
  orderNumber: string
  listingTitle: string
  buyerName: string
  sellerName: string
  amount: number
  deliveryPreference?: string | null
  trackingNumber?: string | null
  status: string
  statusAr: string
  escrowStatus?: string | null
  timeline: MarketplaceOrderTrackingTimelineItem[]
  availableActions: MarketplaceOrderTrackingActions
}

export interface MarketplaceOrderActionResponse {
  orderId: string
  status: string
  statusAr: string
  message: string
}

export interface MarketplaceOrderPaymentCheckoutResponse {
  marketplaceOrderId: string
  orderNumber: string
  amount: number
  currency: string
  paymentProvider: string
  checkoutUrl: string
  status: string
  paymentGatewayReference: string
}

export interface MarketplaceOrderPaymentConfirmResponse {
  marketplaceOrderId: string
  orderStatus: string
  orderStatusAr: string
  escrowStatus: string
  escrowStatusAr: string
  message: string
}

export interface OpenMarketplaceOrderDisputeResponse {
  orderId: string
  status: string
  statusAr: string
  ticketId: string
  escrowStatus: string
  message: string
}
