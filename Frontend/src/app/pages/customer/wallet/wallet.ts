import { CommonModule } from '@angular/common'
import { Component, OnInit, ChangeDetectorRef } from '@angular/core'
import { forkJoin, finalize } from 'rxjs'
import { BookingsService } from '../../../core/services/bookingsService'
import { MarketplaceOrdersService } from '../../../core/services/marketplace-orders'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'

@Component({
  selector: 'app-wallet',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './wallet.html',
  styleUrl: './wallet.css'
})
export class Wallet implements OnInit {
  isLoading = false
  errorMessage = ''
  summary = {
    bookingEscrowTotal: 0,
    marketplaceOrdersTotal: 0,
    pendingPayments: 0,
    completedPayments: 0
  }

  constructor(
    private bookingsService: BookingsService,
    private marketplaceOrdersService: MarketplaceOrdersService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load()
  }

  load(): void {
    this.isLoading = true
    this.errorMessage = ''
    this.cdr.detectChanges()

    forkJoin({
      bookings: this.bookingsService.getMyBookings(),
      orders: this.marketplaceOrdersService.getMyPurchases()
    }).pipe(
      finalize(() => {
        this.isLoading = false
        this.cdr.detectChanges()
      })
    ).subscribe({
      next: result => {
        const bookingsData: any = result.bookings
        const bookings = Array.isArray(bookingsData) ? bookingsData : bookingsData.items ?? bookingsData.bookings ?? []
        const orders = result.orders ?? []

        this.summary.bookingEscrowTotal = bookings.reduce((sum: number, item: any) => sum + Number(item.estimatedTotal ?? item.totalAmount ?? 0), 0)
        this.summary.marketplaceOrdersTotal = orders.reduce((sum, item) => sum + Number(item.amount ?? 0), 0)
        this.summary.pendingPayments = bookings.filter((b: any) => String(b.status).includes('ConfirmedPendingPayment') || b.status === 2).length
        this.summary.completedPayments = bookings.filter((b: any) => String(b.status).includes('Completed') || b.status === 6).length + orders.filter(o => String(o.status).includes('Completed')).length
        this.cdr.detectChanges()
      },
      error: error => {
        this.errorMessage = getErrorMessage(error, 'تعذر تحميل بيانات المحفظة')
        this.cdr.detectChanges()
      }
    })
  }

  money(value: number): string {
    return `${Number(value || 0).toFixed(2)} ج.م`
  }
}
