import { CommonModule } from '@angular/common'
import { Component, OnInit, ChangeDetectorRef } from '@angular/core'
import { ActivatedRoute } from '@angular/router'
import { FormsModule } from '@angular/forms'
import { finalize, forkJoin } from 'rxjs'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'
import {
  MarketplaceEscrow,
  MarketplaceOrderDetails,
  MarketplaceOrderListItem,
  MarketplaceOrderTracking
} from '../../../core/models/marketplace-order.models'
import { MarketplaceOrdersService } from '../../../core/services/marketplace-orders'

@Component({
  selector: 'app-marketplace-sales',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './marketplace-sales.html',
  styleUrl: './marketplace-sales.css'
})
export class MarketplaceSales implements OnInit {
  orders: MarketplaceOrderListItem[] = []
  selectedOrder: MarketplaceOrderDetails | null = null
  tracking: MarketplaceOrderTracking | null = null
  escrow: MarketplaceEscrow | null = null

  isLoading = false
  isDetailsLoading = false
  isActionLoading = false
  errorMessage = ''
  successMessage = ''
  trackingNumber = ''
  cancelReason = ''

  constructor(
    private ordersService: MarketplaceOrdersService,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadOrders()
  }

  loadOrders(): void {
    this.isLoading = true
    this.errorMessage = ''
    this.cdr.detectChanges()

    this.ordersService.getMySales().pipe(
      finalize(() => {
        this.isLoading = false
        this.cdr.detectChanges()
      })
    ).subscribe({
      next: orders => {
        this.orders = orders ?? []
        if (!this.selectedOrder && this.orders.length) {
          const targetId = this.route.snapshot.queryParamMap.get('orderId')
          const target = targetId && this.orders.some(o => o.id === targetId) ? targetId : this.orders[0].id
          this.openOrder(target)
        }
        this.cdr.detectChanges()
      },
      error: error => {
        this.orders = []
        this.errorMessage = getErrorMessage(error, 'تعذر تحميل طلبات البيع')
        this.cdr.detectChanges()
      }
    })
  }

  openOrder(id: string): void {
    this.isDetailsLoading = true
    this.errorMessage = ''
    this.successMessage = ''
    this.cancelReason = ''
    this.cdr.detectChanges()

    forkJoin({
      details: this.ordersService.getById(id),
      tracking: this.ordersService.getTracking(id),
      escrow: this.ordersService.getEscrow(id)
    }).pipe(
      finalize(() => {
        this.isDetailsLoading = false
        this.cdr.detectChanges()
      })
    ).subscribe({
      next: result => {
        this.selectedOrder = result.details
        this.tracking = result.tracking
        this.escrow = result.escrow
        this.trackingNumber = result.details.trackingNumber ?? ''
        this.cdr.detectChanges()
      },
      error: error => {
        this.selectedOrder = null
        this.tracking = null
        this.escrow = null
        this.errorMessage = getErrorMessage(error, 'تعذر تحميل تفاصيل طلب البيع')
        this.cdr.detectChanges()
      }
    })
  }

  sellerConfirm(): void {
    if (!this.selectedOrder) return
    this.runAction(this.ordersService.sellerConfirm(this.selectedOrder.id), 'تم تأكيد الطلب من البائع')
  }

  dispatchOrder(): void {
    if (!this.selectedOrder) return
    this.runAction(this.ordersService.dispatch(this.selectedOrder.id, this.trackingNumber.trim() || null), 'تم تسجيل شحن الطلب')
  }

  markDelivered(): void {
    if (!this.selectedOrder) return
    this.runAction(this.ordersService.deliver(this.selectedOrder.id), 'تم تسجيل وصول الطلب للعميل')
  }

  cancelOrder(): void {
    if (!this.selectedOrder) return
    if (!this.cancelReason.trim()) {
      this.errorMessage = 'اكتب سبب الإلغاء أولاً'
      return
    }

    this.runAction(this.ordersService.cancel(this.selectedOrder.id, this.cancelReason.trim()), 'تم إلغاء الطلب')
  }

  canSellerConfirm(): boolean {
    return this.tracking?.availableActions?.canSellerConfirm === true || this.selectedOrder?.status === 'PaymentCaptured'
  }

  canDispatch(): boolean {
    return this.tracking?.availableActions?.canDispatch === true || this.selectedOrder?.status === 'SellerConfirmed'
  }

  canDeliver(): boolean {
    return this.tracking?.availableActions?.canMarkDelivered === true || this.selectedOrder?.status === 'Dispatched'
  }

  canCancel(): boolean {
    return this.tracking?.availableActions?.canCancel === true
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

  private runAction(request: any, defaultMessage: string): void {
    if (!this.selectedOrder) return
    const id = this.selectedOrder.id

    this.isActionLoading = true
    this.errorMessage = ''
    this.successMessage = ''
    this.cdr.detectChanges()

    request.pipe(
      finalize(() => {
        this.isActionLoading = false
        this.cdr.detectChanges()
      })
    ).subscribe({
      next: (response: any) => {
        this.successMessage = response?.message || defaultMessage
        this.openOrder(id)
        this.loadOrders()
        this.cdr.detectChanges()
      },
      error: (error: any) => {
        this.errorMessage = getErrorMessage(error, 'تعذر تنفيذ العملية')
        this.cdr.detectChanges()
      }
    })
  }
}
