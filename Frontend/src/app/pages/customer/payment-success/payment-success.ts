import { CommonModule } from '@angular/common'
import { ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core'
import { ActivatedRoute, RouterLink } from '@angular/router'
import { finalize } from 'rxjs'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'
import { BookingsService } from '../../../core/services/bookingsService'
import { MarketplaceOrdersService } from '../../../core/services/marketplace-orders'

@Component({
  selector: 'app-payment-success',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './payment-success.html',
  styleUrl: './payment-success.css'
})
export class PaymentSuccess implements OnInit {
  isLoading = true
  successMessage = ''
  errorMessage = ''
  returnLink = '/bookings'
  returnLabel = 'الرجوع إلى حجوزاتي'

  constructor(
    private route: ActivatedRoute,
    private bookingsService: BookingsService,
    private marketplaceOrdersService: MarketplaceOrdersService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit(): void {
  const type = this.route.snapshot.queryParamMap.get('type')
  const bookingId = this.route.snapshot.queryParamMap.get('bookingId')
  const timeAdjustmentId = this.route.snapshot.queryParamMap.get('timeAdjustmentId')
  const marketplaceOrderId = this.route.snapshot.queryParamMap.get('marketplaceOrderId')
  const sessionId = this.route.snapshot.queryParamMap.get('session_id')

  if (type === 'booking') {
    if (!bookingId) {
      this.isLoading = false
      this.errorMessage = 'بيانات دفع الحجز غير مكتملة'
      this.updateView()
      return
    }

    this.confirmBookingPayment(bookingId, sessionId)
    return
  }

  if (type === 'timeAdjustment') {
    if (!timeAdjustmentId) {
      this.isLoading = false
      this.errorMessage = 'بيانات دفع زيادة الوقت غير مكتملة'
      this.updateView()
      return
    }

    this.confirmTimeAdjustmentPayment(timeAdjustmentId, sessionId)
    return
  }

  if (type === 'marketplaceOrder') {
    this.returnLink = '/marketplace-orders'
    this.returnLabel = 'الرجوع إلى طلبات السوق'

    if (!marketplaceOrderId) {
      this.isLoading = false
      this.errorMessage = 'بيانات دفع طلب السوق غير مكتملة'
      this.updateView()
      return
    }

    this.confirmMarketplaceOrderPayment(marketplaceOrderId, sessionId)
    return
  }

  this.isLoading = false
  this.errorMessage = 'نوع الدفع غير معروف'
  this.updateView()
}

  private updateView(): void {
    this.ngZone.run(() => {
      this.cdr.detectChanges()
    })
  }

  private confirmBookingPayment(bookingId: string, sessionId: string | null): void {
  if (!sessionId) {
    this.ngZone.run(() => {
      this.isLoading = false
      this.errorMessage = 'رقم جلسة الدفع غير موجود في رابط الرجوع من Stripe'
      this.cdr.detectChanges()
    })

    return
  }

  this.bookingsService
    .confirmBookingPayment(bookingId, {
      paymentGatewayReference: sessionId
    })
    .pipe(
      finalize(() => {
        this.ngZone.run(() => {
          this.isLoading = false
          this.cdr.detectChanges()
        })
      })
    )
    .subscribe({
      next: response => {
        this.ngZone.run(() => {
          this.successMessage = response.message || 'تم الدفع بنجاح وتم تفعيل الحجز'
          this.cdr.detectChanges()
        })
      },
      error: error => {
        this.ngZone.run(() => {
          this.errorMessage = getErrorMessage(error, 'تم الرجوع من Stripe لكن تعذر تأكيد دفع الحجز داخل النظام')
          this.cdr.detectChanges()
        })
      }
    })
}

private confirmTimeAdjustmentPayment(timeAdjustmentId: string, sessionId: string | null): void {
  if (!sessionId) {
    this.ngZone.run(() => {
      this.isLoading = false
      this.errorMessage = 'رقم جلسة الدفع غير موجود في رابط الرجوع من Stripe'
      this.cdr.detectChanges()
    })

    return
  }

  this.bookingsService
    .confirmTimeAdjustmentPayment(timeAdjustmentId, {
      paymentGatewayReference: sessionId
    })
    .pipe(
      finalize(() => {
        this.ngZone.run(() => {
          this.isLoading = false
          this.cdr.detectChanges()
        })
      })
    )
    .subscribe({
      next: response => {
        this.ngZone.run(() => {
          this.successMessage = response.message || 'تم دفع قيمة زيادة الوقت بنجاح'
          this.cdr.detectChanges()
        })
      },
      error: error => {
        this.ngZone.run(() => {
          this.errorMessage = getErrorMessage(error, 'تم الرجوع من Stripe لكن تعذر تأكيد دفع زيادة الوقت داخل النظام')
          this.cdr.detectChanges()
        })
      }
    })
}

private confirmMarketplaceOrderPayment(orderId: string, sessionId: string | null): void {
  if (!sessionId) {
    this.ngZone.run(() => {
      this.isLoading = false
      this.errorMessage = 'رقم جلسة الدفع غير موجود في رابط الرجوع من Stripe'
      this.cdr.detectChanges()
    })

    return
  }

  this.marketplaceOrdersService
    .confirmPayment(orderId, {
      paymentGatewayReference: sessionId
    })
    .pipe(
      finalize(() => {
        this.ngZone.run(() => {
          this.isLoading = false
          this.cdr.detectChanges()
        })
      })
    )
    .subscribe({
      next: response => {
        this.ngZone.run(() => {
          this.successMessage = response.message || 'تم دفع طلب السوق بنجاح'
          this.cdr.detectChanges()
        })
      },
      error: error => {
        this.ngZone.run(() => {
          this.errorMessage = getErrorMessage(error, 'تم الرجوع من Stripe لكن تعذر تأكيد دفع طلب السوق داخل النظام')
          this.cdr.detectChanges()
        })
      }
    })
}
}