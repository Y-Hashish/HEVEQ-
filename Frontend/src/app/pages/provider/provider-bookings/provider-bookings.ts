import { CommonModule } from '@angular/common'
import { ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { finalize } from 'rxjs'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'
import { ProviderBookingListItem, ProviderOperatorItem } from '../../../core/models/providerBookingModels'
import { ProviderBookingsService } from '../../../core/services/providerBookingsService'

@Component({
  selector: 'app-provider-bookings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './provider-bookings.html',
  styleUrl: './provider-bookings.css'
})
export class ProviderBookings implements OnInit {
  bookings: ProviderBookingListItem[] = []
  operators: ProviderOperatorItem[] = []
  selectedBooking: ProviderBookingListItem | null = null

  statusFilter = 'all'
  selectedOperatorId = ''
  rejectReason = ''

  isLoading = false
  isActionLoading = false
  errorMessage = ''
  successMessage = ''

  readonly filters = [
    { value: 'all', label: 'كل الحجوزات' },
    { value: 'pending', label: 'طلبات جديدة' },
    { value: 'payment', label: 'بانتظار دفع العميل' },
    { value: 'active', label: 'نشطة وقيد التنفيذ' },
    { value: 'completed', label: 'مكتملة أو مغلقة' },
    { value: 'cancelled', label: 'مرفوضة أو ملغاة' }
  ]

  constructor(
    private providerBookingsService: ProviderBookingsService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit(): void {
    this.loadBookings()
    this.loadOperators()
  }

  loadBookings(): void {
    this.ngZone.run(() => {
      this.isLoading = true
      this.errorMessage = ''
      this.successMessage = ''
      this.cdr.detectChanges()
    })

    this.providerBookingsService
      .getAllBookings()
      .pipe(finalize(() => this.ngZone.run(() => {
        this.isLoading = false
        this.cdr.detectChanges()
      })))
      .subscribe({
        next: response => this.ngZone.run(() => {
          this.bookings = response.items ?? []
          if (!this.selectedBooking && this.filteredBookings.length) {
            this.selectBooking(this.filteredBookings[0])
          }
          this.cdr.detectChanges()
        }),
        error: error => this.ngZone.run(() => {
          this.bookings = []
          this.errorMessage = getErrorMessage(error, 'تعذر تحميل حجوزات المزود')
          this.cdr.detectChanges()
        })
      })
  }

  loadOperators(): void {
    this.providerBookingsService.getOperators().subscribe({
      next: operators => {
        this.operators = (operators ?? []).filter(x => x.isActive !== false)
        this.selectedOperatorId = this.operators[0]?.id || ''
      },
      error: () => (this.operators = [])
    })
  }

  get filteredBookings(): ProviderBookingListItem[] {
    return this.bookings.filter(item => {
      if (this.statusFilter === 'all') return true
      if (this.statusFilter === 'pending') return item.status === 'PendingProviderResponse'
      if (this.statusFilter === 'payment') return item.status === 'ConfirmedPendingPayment'
      if (this.statusFilter === 'active') return ['Active', 'InProgress', 'PendingCustomerConfirmation'].includes(item.status)
      if (this.statusFilter === 'completed') return ['Completed', 'ResolvedReleased', 'ResolvedRefunded'].includes(item.status)
      if (this.statusFilter === 'cancelled') return ['Rejected', 'Cancelled', 'CancelledRefunded', 'ProviderUnresponsive'].includes(item.status)
      return true
    })
  }

  onFilterChanged(): void {
    const stillVisible = this.selectedBooking && this.filteredBookings.some(x => x.bookingId === this.selectedBooking?.bookingId)
    this.selectedBooking = stillVisible ? this.selectedBooking : (this.filteredBookings[0] ?? null)
    this.errorMessage = ''
    this.successMessage = ''
    this.cdr.detectChanges()
  }

  selectBooking(booking: ProviderBookingListItem): void {
    this.selectedBooking = booking
    this.rejectReason = ''
    this.errorMessage = ''
    this.successMessage = ''
    this.cdr.detectChanges()
  }

  acceptSelected(): void {
    if (!this.selectedBooking) return
    if (!this.selectedOperatorId) {
      this.errorMessage = 'من فضلك اختر المشغل المسؤول عن تنفيذ الحجز'
      this.cdr.detectChanges()
      return
    }

    this.runAction(() => this.providerBookingsService.acceptBooking(this.selectedBooking!.bookingId, { operatorId: this.selectedOperatorId }), 'تم قبول الحجز بنجاح')
  }

  rejectSelected(): void {
    if (!this.selectedBooking) return
    if (!this.rejectReason.trim()) {
      this.errorMessage = 'من فضلك اكتب سبب رفض الحجز'
      this.cdr.detectChanges()
      return
    }

    this.runAction(() => this.providerBookingsService.rejectBooking(this.selectedBooking!.bookingId, { reason: this.rejectReason.trim() }), 'تم رفض الحجز بنجاح')
  }

  private runAction(requestFactory: () => any, fallbackMessage: string): void {
    this.ngZone.run(() => {
      this.isActionLoading = true
      this.errorMessage = ''
      this.successMessage = ''
      this.cdr.detectChanges()
    })

    requestFactory()
      .pipe(finalize(() => this.ngZone.run(() => {
        this.isActionLoading = false
        this.cdr.detectChanges()
      })))
      .subscribe({
        next: (response: any) => this.ngZone.run(() => {
          this.successMessage = response?.message || fallbackMessage
          this.selectedBooking = null
          this.rejectReason = ''
          this.loadBookings()
        }),
        error: (error: any) => this.ngZone.run(() => {
          this.errorMessage = getErrorMessage(error, 'تعذر تنفيذ الإجراء')
          this.cdr.detectChanges()
        })
      })
  }

  trackById(_index: number, item: ProviderBookingListItem): string {
    return item.bookingId
  }
}
