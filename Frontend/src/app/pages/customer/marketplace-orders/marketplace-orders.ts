import { CommonModule } from '@angular/common'
import { Component, OnInit } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { catchError, finalize, forkJoin, of, switchMap } from 'rxjs'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'
import {
  MarketplaceEscrow,
  MarketplaceOrderDetails,
  MarketplaceOrderListItem,
  MarketplaceOrderTracking
} from '../../../core/models/marketplace-order.models'
import { MarketplaceOrdersService } from '../../../core/services/marketplace-orders'
import { MediaUploadService } from '../../../core/services/mediaUploadService'

@Component({
  selector: 'app-marketplace-orders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './marketplace-orders.html',
  styleUrl: './marketplace-orders.css'
})
export class MarketplaceOrders implements OnInit {
  orders: MarketplaceOrderListItem[] = []
  selectedOrder: MarketplaceOrderDetails | null = null
  tracking: MarketplaceOrderTracking | null = null
  escrow: MarketplaceEscrow | null = null

  isLoading = false
  isDetailsLoading = false
  isActionLoading = false
  errorMessage = ''
  successMessage = ''

  disputeReason = ''
  cancelReason = ''
  selectedDisputeFiles: File[] = []

  constructor(
    private ordersService: MarketplaceOrdersService,
    private mediaUploadService: MediaUploadService
  ) {}

  ngOnInit(): void {
    this.loadOrders()
  }

  loadOrders(): void {
    this.isLoading = true
    this.errorMessage = ''

    this.ordersService
      .getMyPurchases()
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: orders => {
          this.orders = orders ?? []
          if (!this.selectedOrder && this.orders.length) {
            this.openOrder(this.orders[0].id)
          }
        },
        error: error => {
          this.orders = []
          this.errorMessage = getErrorMessage(error, 'تعذر تحميل طلبات السوق')
        }
      })
  }

  openOrder(id: string): void {
    this.isDetailsLoading = true
    this.errorMessage = ''
    this.successMessage = ''
    this.disputeReason = ''
    this.cancelReason = ''
    this.selectedDisputeFiles = []

    forkJoin({
      details: this.ordersService.getById(id),
      tracking: this.ordersService.getTracking(id),
      escrow: this.ordersService.getEscrow(id).pipe(catchError(() => of(null)))
    })
      .pipe(finalize(() => (this.isDetailsLoading = false)))
      .subscribe({
        next: result => {
          this.selectedOrder = result.details
          this.tracking = result.tracking
          this.escrow = result.escrow
        },
        error: error => {
          this.selectedOrder = null
          this.tracking = null
          this.escrow = null
          this.errorMessage = getErrorMessage(error, 'تعذر تحميل تفاصيل الطلب')
        }
      })
  }

  paySelectedOrder(): void {
    if (!this.selectedOrder) return

    const id = this.selectedOrder.id
    const origin = window.location.origin

    this.isActionLoading = true
    this.errorMessage = ''
    this.successMessage = ''

    this.ordersService
      .checkoutPayment(id, {
        paymentMethod: 'Card',
        successUrl: `${origin}/payment/success?type=marketplaceOrder&marketplaceOrderId=${id}&session_id={CHECKOUT_SESSION_ID}`,
        cancelUrl: `${origin}/payment/cancel?type=marketplaceOrder&marketplaceOrderId=${id}`
      })
      .pipe(finalize(() => (this.isActionLoading = false)))
      .subscribe({
        next: response => {
          if (response.checkoutUrl) {
            window.location.href = response.checkoutUrl
            return
          }
          this.errorMessage = 'لم يتم إنشاء رابط الدفع'
        },
        error: error => {
          this.errorMessage = getErrorMessage(error, 'تعذر إنشاء رابط دفع الطلب')
        }
      })
  }

  confirmMockPayment(): void {
    if (!this.selectedOrder) return
    this.runAction(
      this.ordersService.confirmPayment(this.selectedOrder.id, {
        paymentGatewayReference: `MARKETPLACE-FRONT-${Date.now()}`
      }),
      'تم تأكيد دفع الطلب بنجاح'
    )
  }

  completeOrder(): void {
    if (!this.selectedOrder) return
    this.runAction(this.ordersService.complete(this.selectedOrder.id), 'تم تأكيد استلام الطلب وإكماله وتم الإفراج عن مستحقات البائع')
  }

  cancelOrder(): void {
    if (!this.selectedOrder) return
    if (!this.cancelReason.trim()) {
      this.errorMessage = 'اكتب سبب الإلغاء أولاً'
      return
    }

    this.runAction(this.ordersService.cancel(this.selectedOrder.id, this.cancelReason.trim()), 'تم إلغاء الطلب')
  }

  submitDispute(): void {
    if (!this.selectedOrder) return
    if (!this.disputeReason.trim()) {
      this.errorMessage = 'اكتب سبب النزاع أولاً'
      return
    }

    const orderId = this.selectedOrder.id
    const uploadRequest = this.selectedDisputeFiles.length
      ? forkJoin(this.selectedDisputeFiles.map(file => this.mediaUploadService.uploadImage(file, 'marketplace-disputes', orderId)))
      : of([])

    this.isActionLoading = true
    this.errorMessage = ''
    this.successMessage = ''

    uploadRequest
      .pipe(
        switchMap(results => this.ordersService.dispute(orderId, this.disputeReason.trim(), results.map(r => r.url))),
        finalize(() => (this.isActionLoading = false))
      )
      .subscribe({
        next: response => {
          this.successMessage = response.message || 'تم فتح النزاع وربطه بتذكرة دعم'
          this.disputeReason = ''
          this.selectedDisputeFiles = []
          this.openOrder(orderId)
          this.loadOrders()
        },
        error: error => {
          this.errorMessage = getErrorMessage(error, 'تعذر فتح النزاع')
        }
      })
  }

  onDisputeFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement
    const files = Array.from(input.files ?? [])
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp']
    const maxSize = 10 * 1024 * 1024

    const invalid = files.find(file => !allowedTypes.includes(file.type) || file.size > maxSize)
    if (invalid) {
      this.errorMessage = 'مسموح بصور JPG أو PNG أو WEBP فقط وبحد أقصى 10 ميجابايت للصورة'
      this.selectedDisputeFiles = []
      return
    }

    this.selectedDisputeFiles = files
  }

  canPay(): boolean {
    return this.hasAction('pay') || this.selectedOrder?.status === 'PendingPayment'
  }

  canComplete(): boolean {
    return this.tracking?.availableActions?.canComplete === true || this.selectedOrder?.status === 'Delivered'
  }

  canCancel(): boolean {
    return this.tracking?.availableActions?.canCancel === true
  }

  canDispute(): boolean {
    return this.tracking?.availableActions?.canDispute === true || this.selectedOrder?.status === 'Delivered'
  }


  getStatusClass(status: string | null | undefined): string {
    const normalized = String(status ?? '').toLowerCase()

    if (normalized.includes('completed') || normalized.includes('delivered')) return 'completed'
    if (normalized.includes('disputed') || normalized.includes('refunded')) return 'disputed'
    if (normalized.includes('cancelled') || normalized.includes('canceled')) return 'cancelled'
    if (normalized.includes('pendingpayment') || normalized.includes('pending_payment') || normalized.includes('payment')) return 'payment'
    if (normalized.includes('sellerconfirmed') || normalized.includes('confirmed')) return 'confirmation'
    if (normalized.includes('dispatch') || normalized.includes('captured')) return 'active'

    return 'active'
  }

  getTimelineItemState(step: any, index: number): string {
    if (step?.done) return 'done'
    if (index === 0) return 'current'
    return 'pending'
  }

  getTimelineHint(step: any, index: number): string {
    if (step?.done) return 'تم تنفيذ هذه الخطوة'
    if (index === 0) return 'الخطوة الحالية'
    return 'لم يتم بعد'
  }

  statusText(order: MarketplaceOrderListItem | MarketplaceOrderDetails | null): string {
    return order?.statusAr || order?.status || 'غير معروف'
  }

  money(value: number | null | undefined): string {
    return `${Number(value ?? 0).toFixed(2)} ج.م`
  }

  trackById(index: number, item: MarketplaceOrderListItem): string {
    return item.id
  }

  private hasAction(action: string): boolean {
    const actions = this.tracking?.availableActions
    if (!actions) return false

    if (action === 'pay') {
      const status = String(this.selectedOrder?.status ?? '').toLowerCase()
      return status.includes('pendingpayment') || status.includes('pending_payment')
    }

    return false
  }

  private runAction(request: any, defaultMessage: string): void {
    if (!this.selectedOrder) return
    const id = this.selectedOrder.id

    this.isActionLoading = true
    this.errorMessage = ''
    this.successMessage = ''

    request.pipe(finalize(() => (this.isActionLoading = false))).subscribe({
      next: (response: any) => {
        this.successMessage = response?.message || defaultMessage
        this.openOrder(id)
        this.loadOrders()
      },
      error: (error: any) => {
        this.errorMessage = getErrorMessage(error, 'تعذر تنفيذ العملية')
      }
    })
  }
}
