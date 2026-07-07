import { CommonModule } from '@angular/common'
import { Component, OnInit } from '@angular/core'
import { ActivatedRoute, Router, RouterLink } from '@angular/router'
import { catchError, finalize, forkJoin, of } from 'rxjs'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'
import { MarketplaceEscrow, MarketplaceOrderDetails } from '../../../core/models/marketplace-order.models'
import { MarketplaceOrdersService } from '../../../core/services/marketplace-orders'

@Component({
  selector: 'app-marketplace-order-payment',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './marketplace-order-payment.html',
  styleUrl: './marketplace-order-payment.css'
})
export class MarketplaceOrderPayment implements OnInit {
  orderId = ''
  order: MarketplaceOrderDetails | null = null
  escrow: MarketplaceEscrow | null = null

  isLoading = false
  isPaying = false
  isMockConfirming = false
  errorMessage = ''
  successMessage = ''

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private ordersService: MarketplaceOrdersService
  ) {}

  ngOnInit(): void {
    this.orderId = this.route.snapshot.paramMap.get('id') ?? ''

    if (!this.orderId) {
      this.errorMessage = 'رقم الطلب غير موجود'
      return
    }

    this.loadOrder()
  }

  loadOrder(): void {
    this.isLoading = true
    this.errorMessage = ''
    this.successMessage = ''

    forkJoin({
      order: this.ordersService.getById(this.orderId),
      escrow: this.ordersService.getEscrow(this.orderId).pipe(catchError(() => of(null)))
    })
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: result => {
          this.order = result.order
          this.escrow = result.escrow
        },
        error: error => {
          this.order = null
          this.escrow = null
          this.errorMessage = getErrorMessage(error, 'تعذر تحميل بيانات دفع طلب السوق')
        }
      })
  }

  payNow(): void {
    if (!this.order) return

    const origin = window.location.origin
    this.isPaying = true
    this.errorMessage = ''
    this.successMessage = ''

    this.ordersService
      .checkoutPayment(this.order.id, {
        paymentMethod: 'Card',
        successUrl: `${origin}/payment/success?type=marketplaceOrder&marketplaceOrderId=${this.order.id}&session_id={CHECKOUT_SESSION_ID}`,
        cancelUrl: `${origin}/payment/cancel?type=marketplaceOrder&marketplaceOrderId=${this.order.id}`
      })
      .pipe(finalize(() => (this.isPaying = false)))
      .subscribe({
        next: response => {
          if (response.checkoutUrl) {
            window.location.href = response.checkoutUrl
            return
          }
          this.errorMessage = 'لم يتم إنشاء رابط الدفع'
        },
        error: error => {
          this.errorMessage = getErrorMessage(error, 'تعذر إنشاء رابط الدفع')
        }
      })
  }

  confirmMockPayment(): void {
    if (!this.order) return

    this.isMockConfirming = true
    this.errorMessage = ''
    this.successMessage = ''

    this.ordersService
      .confirmPayment(this.order.id, { paymentGatewayReference: `MARKETPLACE-DEMO-${Date.now()}` })
      .pipe(finalize(() => (this.isMockConfirming = false)))
      .subscribe({
        next: response => {
          this.successMessage = response.message || 'تم تأكيد الدفع التجريبي بنجاح'
          this.loadOrder()
          setTimeout(() => this.router.navigate(['/marketplace-orders']), 1200)
        },
        error: error => {
          this.errorMessage = getErrorMessage(error, 'تعذر تأكيد الدفع التجريبي')
        }
      })
  }

  money(value: number | null | undefined): string {
    return `${Number(value ?? 0).toFixed(2)} ج.م`
  }
}
