import { CommonModule } from '@angular/common'
import { ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core'
import { ActivatedRoute } from '@angular/router'
import { FormsModule } from '@angular/forms'
import { finalize, forkJoin, of, switchMap } from 'rxjs'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'
import {
  BookingDetails,
  BookingEscrow,
  BookingListItem,
  BookingReviewForm,
  BookingTracker,
  CustomerTimeAdjustmentItem
} from '../../../core/models/bookingModels'
import { BookingsService } from '../../../core/services/bookingsService'
import { ReviewsService } from '../../../core/services/reviewsService'
import { MediaUploadService } from '../../../core/services/mediaUploadService'

@Component({
  selector: 'app-bookings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './bookings.html',
  styleUrl: './bookings.css'
})
export class Bookings implements OnInit {
  bookings: BookingListItem[] = []
  selectedBooking: BookingDetails | null = null

  isLoading = false
  isDetailsLoading = false
  isActionLoading = false
  isReviewSubmitting = false

  cancelReason = ''
  isCancelLoading = false

  errorMessage = ''
  successMessage = ''

  disputeReason = ''
  disputeEvidenceUrls = ''

  reviewMessage = ''
  reviewMessageType: 'success' | 'warning' = 'success'

  tracker: BookingTracker | null = null
  escrow: BookingEscrow | null = null
  isTrackerLoading = false

  selectedDisputeFiles: File[] = []

  timeAdjustments: CustomerTimeAdjustmentItem[] = []
  isTimeAdjustmentsLoading = false
  activeTimeAdjustmentId = ''

  reviewForm: BookingReviewForm = {
    bookingId: '',
    rating: 5,
    comment: ''
  }

  ratingOptions = [5, 4, 3, 2, 1]

  constructor(
  private bookingsService: BookingsService,
  private reviewsService: ReviewsService,
  private mediaUploadService: MediaUploadService,
  private cdr: ChangeDetectorRef,
  private ngZone: NgZone,
  private route: ActivatedRoute
) {}

  ngOnInit(): void {
    this.loadBookings()
  }

  loadBookings(clearMessages = true): void {
  this.ngZone.run(() => {
    this.isLoading = true
    this.errorMessage = ''

    if (clearMessages) {
      this.successMessage = ''
      this.reviewMessage = ''
    }

    this.cdr.detectChanges()
  })

  this.bookingsService
    .getMyBookings()
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
          const data: any = response
          this.bookings = Array.isArray(data)
            ? data
            : data.items ?? data.bookings ?? []

          if (!this.selectedBooking && this.bookings.length > 0) {
            const targetId = this.route.snapshot.queryParamMap.get('bookingId')
            const target = targetId && this.bookings.some(b => b.id === targetId) ? targetId : this.bookings[0].id
            this.openBooking(target)
          }

          this.cdr.detectChanges()
        })
      },
      error: error => {
        this.ngZone.run(() => {
          this.bookings = []
          this.errorMessage = getErrorMessage(error, 'تعذر تحميل الحجوزات')
          this.cdr.detectChanges()
        })
      }
    })
}

 openBooking(bookingId: string): void {
  this.ngZone.run(() => {
    this.isDetailsLoading = true
    this.isTrackerLoading = true
    this.errorMessage = ''
    this.successMessage = ''
    this.reviewMessage = ''
    this.timeAdjustments = []
    this.tracker = null
    this.escrow = null
    this.resetActionForms()
    this.cdr.detectChanges()
  })

  this.bookingsService
    .getBookingDetails(bookingId)
    .pipe(
      finalize(() => {
        this.ngZone.run(() => {
          this.isDetailsLoading = false
          this.cdr.detectChanges()
        })
      })
    )
    .subscribe({
      next: booking => {
        this.ngZone.run(() => {
          this.selectedBooking = booking
          this.reviewForm.bookingId = booking.id
          this.cdr.detectChanges()
        })

        this.loadTrackerAndEscrow(booking.id)
        this.loadTimeAdjustments(booking.id)
      },
      error: error => {
        this.ngZone.run(() => {
          this.selectedBooking = null
          this.tracker = null
          this.escrow = null
          this.isTrackerLoading = false
          this.errorMessage = getErrorMessage(error, 'تعذر تحميل تفاصيل الحجز')
          this.cdr.detectChanges()
        })
      }
    })
}

  confirmPayment(): void {
  if (!this.selectedBooking) {
    return
  }

  const bookingId = this.selectedBooking.id
  const origin = window.location.origin

  this.ngZone.run(() => {
    this.isActionLoading = true
    this.errorMessage = ''
    this.successMessage = ''
    this.cdr.detectChanges()
  })

  this.bookingsService
    .checkoutBookingPayment(bookingId, {
      paymentMethod: 'Card',
      successUrl: `${origin}/payment/success?type=booking&bookingId=${bookingId}&session_id={CHECKOUT_SESSION_ID}`,
      cancelUrl: `${origin}/payment/cancel?type=booking&bookingId=${bookingId}`
    })
    .pipe(
      finalize(() => {
        this.ngZone.run(() => {
          this.isActionLoading = false
          this.cdr.detectChanges()
        })
      })
    )
    .subscribe({
      next: response => {
        if (response.checkoutUrl) {
          window.location.href = response.checkoutUrl
          return
        }

        this.ngZone.run(() => {
          this.errorMessage = 'لم يتم إنشاء رابط الدفع'
          this.cdr.detectChanges()
        })
      },
      error: error => {
        this.ngZone.run(() => {
          this.errorMessage = getErrorMessage(error, 'تعذر إنشاء رابط الدفع')
          this.cdr.detectChanges()
        })
      }
    })
}

  confirmCompletion(): void {
    if (!this.selectedBooking) {
      return
    }

    this.isActionLoading = true
    this.errorMessage = ''
    this.successMessage = ''
    this.updateView()

    this.bookingsService
      .confirmCompletion(this.selectedBooking.id)
      .pipe(
        finalize(() => {
          this.isActionLoading = false
          this.updateView()
        })
      )
      .subscribe({
        next: response => {
          this.successMessage = response.message || 'تم تأكيد اكتمال الحجز بنجاح'
          this.refreshSelectedBooking()
          this.loadBookings()
        },
        error: error => {
          this.errorMessage = getErrorMessage(error, 'تعذر تأكيد اكتمال الحجز')
          this.updateView()
        }
      })
  }

  submitDispute(): void {
  if (!this.selectedBooking) {
    return
  }

  if (!this.disputeReason.trim()) {
    this.errorMessage = 'من فضلك اكتب سبب الشكوى'
    this.updateView()
    return
  }

  const bookingId = this.selectedBooking.id

  this.ngZone.run(() => {
    this.isActionLoading = true
    this.errorMessage = ''
    this.successMessage = ''
    this.cdr.detectChanges()
  })

  const uploadRequest = this.selectedDisputeFiles.length > 0
    ? forkJoin(
        this.selectedDisputeFiles.map(file =>
          this.mediaUploadService.uploadImage(file, 'booking-disputes', bookingId)
        )
      )
    : of([])

  uploadRequest
    .pipe(
      switchMap(uploadResults => {
        const evidencePhotoUrls = uploadResults.map(result => result.url)

        return this.bookingsService.disputeBooking(bookingId, {
          reason: this.disputeReason.trim(),
          evidencePhotoUrls
        })
      }),
      finalize(() => {
        this.ngZone.run(() => {
          this.isActionLoading = false
          this.cdr.detectChanges()
        })
      })
    )
    .subscribe({
      next: response => {
        this.ngZone.run(() => {
          this.successMessage = response.message || 'تم فتح شكوى على الحجز بنجاح'
          this.resetActionForms()
          this.cdr.detectChanges()
        })

        this.refreshSelectedBooking()
        this.loadBookings(false)
      },
      error: error => {
        this.ngZone.run(() => {
          this.errorMessage = getErrorMessage(error, 'تعذر فتح الشكوى')
          this.cdr.detectChanges()
        })
      }
    })
}

  submitReview(): void {
  if (!this.selectedBooking) {
    return
  }

  if (!this.canReview(this.selectedBooking)) {
    this.errorMessage = 'يمكن إضافة تقييم فقط بعد اكتمال الحجز'
    this.updateView()
    return
  }

  this.ngZone.run(() => {
    this.isReviewSubmitting = true
    this.errorMessage = ''
    this.successMessage = ''
    this.reviewMessage = ''
    this.cdr.detectChanges()
  })

  this.reviewsService
    .submitReview({
      bookingId: this.selectedBooking.id,
      marketplaceOrderId: null,
      rating: Number(this.reviewForm.rating),
      comment: this.reviewForm.comment.trim() || null
    })
    .pipe(
      finalize(() => {
        this.ngZone.run(() => {
          this.isReviewSubmitting = false
          this.cdr.detectChanges()
        })
      })
    )
    .subscribe({
      next: response => {
        this.ngZone.run(() => {
          this.reviewMessage = response.message || 'تم إرسال التقييم بنجاح'
          this.reviewMessageType = response.isPublished === false ? 'warning' : 'success'

          this.reviewForm.comment = ''
          this.reviewForm.rating = 5

          if (this.selectedBooking) {
            this.selectedBooking.hasReview = true
          }

          this.cdr.detectChanges()
        })

        this.loadBookings(false)
      },
      error: error => {
        this.ngZone.run(() => {
          this.errorMessage = getErrorMessage(error, 'تعذر إرسال التقييم')
          this.cdr.detectChanges()
        })
      }
    })
}

  private refreshSelectedBooking(): void {
    if (!this.selectedBooking) {
      return
    }

    const bookingId = this.selectedBooking.id
    this.openBooking(bookingId)
  }

  private resetActionForms(): void {
  this.disputeReason = ''
  this.disputeEvidenceUrls = ''
  this.selectedDisputeFiles = []
  this.cancelReason = ''

  this.reviewForm = {
    bookingId: this.selectedBooking?.id || '',
    rating: 5,
    comment: ''
  }
}

  getStatusText(status: any, statusAr?: string | null): string {
    if (statusAr) {
      return statusAr
    }

    

    if (typeof status === 'number') {
      switch (status) {
        case 1:
          return 'بانتظار رد المزود'
        case 2:
          return 'بانتظار الدفع'
        case 3:
          return 'نشط'
        case 4:
          return 'قيد التنفيذ'
        case 5:
          return 'بانتظار تأكيد العميل'
        case 6:
          return 'مكتمل'
        case 7:
          return 'مرفوض'
        case 8:
          return 'ملغي'
        case 9:
          return 'متنازع عليه'
        default:
          return 'غير معروف'
      }
    }

    switch (status) {
      case 'PendingProviderResponse':
        return 'بانتظار رد المزود'
      case 'ConfirmedPendingPayment':
        return 'بانتظار الدفع'
      case 'Active':
        return 'نشط'
      case 'InProgress':
        return 'قيد التنفيذ'
      case 'PendingCustomerConfirmation':
        return 'بانتظار تأكيد العميل'
      case 'Completed':
        return 'مكتمل'
      case 'Rejected':
        return 'مرفوض'
      case 'Cancelled':
        return 'ملغي'
      case 'Disputed':
        return 'متنازع عليه'
      default:
        return status || 'غير معروف'
    }
  }

  getStatusClass(status: any): string {
    const value = String(status)

    if (value === '6' || value === 'Completed') {
      return 'completed'
    }

    if (value === '9' || value === 'Disputed') {
      return 'disputed'
    }

    if (value === '8' || value === 'Cancelled' || value === '7' || value === 'Rejected') {
      return 'cancelled'
    }

    if (value === '2' || value === 'ConfirmedPendingPayment') {
      return 'payment'
    }

    if (value === '5' || value === 'PendingCustomerConfirmation') {
      return 'confirmation'
    }

    return 'active'
  }

  canPay(booking: BookingDetails | BookingListItem | null): boolean {
    if (!booking) {
      return false
    }

    return booking.status === 2 || booking.status === 'ConfirmedPendingPayment'
  }

  canConfirmCompletion(booking: BookingDetails | BookingListItem | null): boolean {
    if (!booking) {
      return false
    }

    return booking.status === 5 || booking.status === 'PendingCustomerConfirmation'
  }

  canDispute(booking: BookingDetails | BookingListItem | null): boolean {
    if (!booking) {
      return false
    }

    return booking.status === 5 || booking.status === 'PendingCustomerConfirmation'
  }

  canReview(booking: BookingDetails | BookingListItem | null): boolean {
    if (!booking) {
      return false
    }

    const isCompleted = booking.status === 6 || booking.status === 'Completed'

    return isCompleted && booking.hasReview !== true
  }

  getStars(rating: number): string {
    return '★'.repeat(Number(rating)) + '☆'.repeat(5 - Number(rating))
  }

  trackById(index: number, item: BookingListItem): string {
    return item.id
  }

  private updateView(): void {
  this.ngZone.run(() => {
    this.cdr.detectChanges()
  })
}

  private loadTrackerAndEscrow(bookingId: string): void {
  this.ngZone.run(() => {
    this.isTrackerLoading = true
    this.cdr.detectChanges()
  })

  forkJoin({
    tracker: this.bookingsService.getBookingTracker(bookingId),
    escrow: this.bookingsService.getBookingEscrow(bookingId)
  })
    .pipe(
      finalize(() => {
        this.ngZone.run(() => {
          this.isTrackerLoading = false
          this.cdr.detectChanges()
        })
      })
    )
    .subscribe({
      next: result => {
        this.ngZone.run(() => {
          this.tracker = result.tracker
          this.escrow = result.escrow
          this.cdr.detectChanges()
        })
      },
      error: error => {
        this.ngZone.run(() => {
          this.tracker = null
          this.escrow = null
          this.errorMessage = getErrorMessage(error, 'تعذر تحميل مسار الحجز أو بيانات الضمان')
          this.cdr.detectChanges()
        })
      }
    })
}

formatMoney(value: number | null | undefined): string {
  if (value === null || value === undefined) {
    return '0 جنيه'
  }

  return `${Number(value).toFixed(2)} جنيه`
}

getTimelineDate(itemDate: string | null): string {
  if (!itemDate) {
    return 'لم يتم بعد'
  }

  return itemDate
}

get visibleTimeline() {
  const timeline = this.tracker?.timeline ?? []

  return timeline.filter(item => {
    if (this.isRejectedBooking()) {
      return this.isTimelineDone(item) || this.isRejectionTimelineItem(item)
    }

    if (this.isCancelledBooking()) {
      return this.isTimelineDone(item) || this.isCancellationTimelineItem(item)
    }

    return !this.shouldHideOptionalFutureTimelineItem(item)
  })
}

private isRejectedBooking(): boolean {
  const status = String(
    this.tracker?.currentStatus ||
    this.selectedBooking?.status ||
    ''
  ).toLowerCase()

  const statusAr = String(
    this.tracker?.currentStatusAr ||
    this.selectedBooking?.statusAr ||
    ''
  )

  return (
    status.includes('rejected') ||
    status === '7' ||
    statusAr.includes('مرفوض') ||
    statusAr.includes('رفض')
  )
}

private isCancelledBooking(): boolean {
  const status = String(
    this.tracker?.currentStatus ||
    this.selectedBooking?.status ||
    ''
  ).toLowerCase()

  const statusAr = String(
    this.tracker?.currentStatusAr ||
    this.selectedBooking?.statusAr ||
    ''
  )

  return (
    status.includes('cancelled') ||
    status.includes('canceled') ||
    status === '8' ||
    status === '11' ||
    status === '16' ||
    statusAr.includes('ملغي') ||
    statusAr.includes('إلغاء')
  )
}

private getTimelineSearchText(item: { key?: string; label?: string; labelAr?: string }): string {
  return `${item.key ?? ''} ${item.label ?? ''} ${item.labelAr ?? ''}`.toLowerCase()
}

private isTimelineDone(item: { done: boolean; date: string | null }): boolean {
  return item.done === true || !!item.date
}

private isRejectionTimelineItem(item: { key?: string; label?: string; labelAr?: string }): boolean {
  const text = this.getTimelineSearchText(item)

  return (
    text.includes('reject') ||
    text.includes('rejected') ||
    text.includes('رفض') ||
    text.includes('مرفوض')
  )
}

private isCancellationTimelineItem(item: { key?: string; label?: string; labelAr?: string }): boolean {
  const text = this.getTimelineSearchText(item)

  return (
    text.includes('cancel') ||
    text.includes('cancelled') ||
    text.includes('canceled') ||
    text.includes('إلغاء') ||
    text.includes('ملغي')
  )
}

private shouldHideOptionalFutureTimelineItem(item: { key?: string; label?: string; labelAr?: string; done: boolean; date: string | null }): boolean {
  if (this.isTimelineDone(item)) {
    return false
  }

  const text = this.getTimelineSearchText(item)

  const optionalFutureSteps = [
    'reject',
    'rejected',
    'رفض',
    'مرفوض',
    'cancel',
    'cancelled',
    'canceled',
    'إلغاء',
    'ملغي',
    'dispute',
    'نزاع',
    'refund',
    'استرجاع'
  ]

  return optionalFutureSteps.some(word => text.includes(word))
}

get currentTimelineIndex(): number {
  const items = this.visibleTimeline

  if (!items.length) {
    return -1
  }

  if (this.isRejectedBooking() || this.isCancelledBooking()) {
    return items.length - 1
  }

  const firstPendingIndex = items.findIndex(item => !this.isTimelineDone(item))

  if (firstPendingIndex !== -1) {
    return firstPendingIndex
  }

  return items.length - 1
}

getTimelineItemState(item: { done: boolean; date: string | null; key?: string; label?: string; labelAr?: string }, index: number): 'done' | 'current' | 'pending' | 'rejected' | 'cancelled' {
  if (this.isRejectedBooking() && this.isRejectionTimelineItem(item)) {
    return 'rejected'
  }

  if (this.isCancelledBooking() && this.isCancellationTimelineItem(item)) {
    return 'cancelled'
  }

  if (this.isTimelineDone(item)) {
    return 'done'
  }

  if (index === this.currentTimelineIndex && !this.isRejectedBooking() && !this.isCancelledBooking()) {
    return 'current'
  }

  return 'pending'
}

getTimelineLabel(item: { label?: string; labelAr?: string; key?: string; date: string | null; done: boolean }): string {
  if (this.isRejectedBooking() && this.isRejectionTimelineItem(item)) {
    return 'تم رفض الحجز'
  }

  if (this.isCancelledBooking() && this.isCancellationTimelineItem(item)) {
    return 'تم إلغاء الحجز'
  }

  return item.labelAr || item.label || 'مرحلة'
}

getTimelineHint(item: { done: boolean; date: string | null; key?: string; label?: string; labelAr?: string }, index: number): string {
  if (item.date) {
    return ''
  }

  const state = this.getTimelineItemState(item, index)

  if (state === 'done') {
    return 'تم تنفيذ هذه المرحلة'
  }

  if (state === 'rejected') {
    return 'تم رفض الطلب'
  }

  if (state === 'cancelled') {
    return 'تم إلغاء الطلب'
  }

  if (state === 'current') {
    return this.tracker?.nextAction?.labelAr || 'جاري تنفيذ هذه المرحلة الآن...'
  }

  return 'بانتظار الوصول إلى هذه المرحلة'
}

onDisputeFilesSelected(event: Event): void {
  this.errorMessage = ''
  this.successMessage = ''

  const input = event.target as HTMLInputElement
  const files = Array.from(input.files ?? [])

  if (files.length === 0) {
    this.selectedDisputeFiles = []
    this.updateView()
    return
  }

  const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp']
  const maxSizeInMb = 10
  const maxSizeInBytes = maxSizeInMb * 1024 * 1024

  const invalidFile = files.find(file => !allowedTypes.includes(file.type))

  if (invalidFile) {
    this.errorMessage = 'مسموح فقط بصور JPG أو PNG أو WEBP'
    this.selectedDisputeFiles = []
    this.updateView()
    return
  }

  const oversizedFile = files.find(file => file.size > maxSizeInBytes)

  if (oversizedFile) {
    this.errorMessage = `حجم كل صورة يجب ألا يتجاوز ${maxSizeInMb} ميجابايت`
    this.selectedDisputeFiles = []
    this.updateView()
    return
  }

  this.selectedDisputeFiles = files
  this.updateView()
}

private loadTimeAdjustments(bookingId: string): void {
  this.ngZone.run(() => {
    this.isTimeAdjustmentsLoading = true
    this.cdr.detectChanges()
  })

  this.bookingsService
    .getTimeAdjustments(bookingId)
    .pipe(
      finalize(() => {
        this.ngZone.run(() => {
          this.isTimeAdjustmentsLoading = false
          this.cdr.detectChanges()
        })
      })
    )
    .subscribe({
      next: response => {
        this.ngZone.run(() => {
          this.timeAdjustments = response ?? []
          this.cdr.detectChanges()
        })
      },
      error: error => {
        this.ngZone.run(() => {
          this.timeAdjustments = []
          this.errorMessage = getErrorMessage(error, 'تعذر تحميل طلبات زيادة الوقت')
          this.cdr.detectChanges()
        })
      }
    })
}

approveTimeAdjustment(adjustment: CustomerTimeAdjustmentItem): void {
  this.ngZone.run(() => {
    this.activeTimeAdjustmentId = adjustment.id
    this.errorMessage = ''
    this.successMessage = ''
    this.cdr.detectChanges()
  })

  this.bookingsService
    .approveTimeAdjustment(adjustment.id)
    .pipe(
      finalize(() => {
        this.ngZone.run(() => {
          this.activeTimeAdjustmentId = ''
          this.cdr.detectChanges()
        })
      })
    )
    .subscribe({
      next: response => {
        this.ngZone.run(() => {
          this.successMessage = response.message || 'تمت الموافقة على زيادة الوقت، برجاء دفع قيمة الزيادة'
          this.cdr.detectChanges()
        })

        if (this.selectedBooking) {
          this.loadTimeAdjustments(this.selectedBooking.id)
        }
      },
      error: error => {
        this.ngZone.run(() => {
          this.errorMessage = getErrorMessage(error, 'تعذر الموافقة على زيادة الوقت')
          this.cdr.detectChanges()
        })
      }
    })
}

rejectTimeAdjustment(adjustment: CustomerTimeAdjustmentItem): void {
  this.ngZone.run(() => {
    this.activeTimeAdjustmentId = adjustment.id
    this.errorMessage = ''
    this.successMessage = ''
    this.cdr.detectChanges()
  })

  this.bookingsService
    .rejectTimeAdjustment(adjustment.id)
    .pipe(
      finalize(() => {
        this.ngZone.run(() => {
          this.activeTimeAdjustmentId = ''
          this.cdr.detectChanges()
        })
      })
    )
    .subscribe({
      next: response => {
        this.ngZone.run(() => {
          this.successMessage = response.message || 'تم رفض طلب زيادة الوقت'
          this.cdr.detectChanges()
        })

        if (this.selectedBooking) {
          this.loadTimeAdjustments(this.selectedBooking.id)
        }
      },
      error: error => {
        this.ngZone.run(() => {
          this.errorMessage = getErrorMessage(error, 'تعذر رفض طلب زيادة الوقت')
          this.cdr.detectChanges()
        })
      }
    })
}

confirmTimeAdjustmentPayment(adjustment: CustomerTimeAdjustmentItem): void {
  this.ngZone.run(() => {
    this.activeTimeAdjustmentId = adjustment.id
    this.errorMessage = ''
    this.successMessage = ''
    this.cdr.detectChanges()
  })

  this.bookingsService
    .confirmTimeAdjustmentPayment(adjustment.id, {
      paymentGatewayReference: `TIME-ADJ-FRONT-${Date.now()}`
    })
    .pipe(
      finalize(() => {
        this.ngZone.run(() => {
          this.activeTimeAdjustmentId = ''
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

        if (this.selectedBooking) {
          this.openBooking(this.selectedBooking.id)
        }
      },
      error: error => {
        this.ngZone.run(() => {
          this.errorMessage = getErrorMessage(error, 'تعذر تأكيد دفع زيادة الوقت')
          this.cdr.detectChanges()
        })
      }
    })
}

payTimeAdjustment(adjustment: CustomerTimeAdjustmentItem): void {
  if (!this.selectedBooking) {
    return
  }

  const origin = window.location.origin

  this.ngZone.run(() => {
    this.activeTimeAdjustmentId = adjustment.id
    this.errorMessage = ''
    this.successMessage = ''
    this.cdr.detectChanges()
  })

  this.bookingsService
    .checkoutTimeAdjustmentPayment(adjustment.id, {
      paymentMethod: 'Card',
      successUrl: `${origin}/payment/success?type=timeAdjustment&bookingId=${adjustment.bookingId}&timeAdjustmentId=${adjustment.id}&session_id={CHECKOUT_SESSION_ID}`,
      cancelUrl: `${origin}/payment/cancel?type=timeAdjustment&bookingId=${adjustment.bookingId}&timeAdjustmentId=${adjustment.id}`
    })
    .pipe(
      finalize(() => {
        this.ngZone.run(() => {
          this.activeTimeAdjustmentId = ''
          this.cdr.detectChanges()
        })
      })
    )
    .subscribe({
      next: response => {
        if (response.checkoutUrl) {
          window.location.href = response.checkoutUrl
          return
        }

        this.ngZone.run(() => {
          this.errorMessage = 'لم يتم إنشاء رابط دفع زيادة الوقت'
          this.cdr.detectChanges()
        })
      },
      error: error => {
        this.ngZone.run(() => {
          this.errorMessage = getErrorMessage(error, 'تعذر إنشاء رابط دفع زيادة الوقت')
          this.cdr.detectChanges()
        })
      }
    })
}

canCancel(booking: BookingDetails | BookingListItem | null): boolean {
  if (!booking) {
    return false
  }

  return (
    booking.status === 1 ||
    booking.status === 2 ||
    booking.status === 3 ||
    booking.status === 'PendingProviderResponse' ||
    booking.status === 'ConfirmedPendingPayment' ||
    booking.status === 'Active'
  )
}

cancelSelectedBooking(): void {
  if (!this.selectedBooking) {
    return
  }

  if (!this.cancelReason.trim()) {
    this.errorMessage = 'من فضلك اكتب سبب إلغاء الحجز'
    this.updateView()
    return
  }

  const bookingId = this.selectedBooking.id

  this.ngZone.run(() => {
    this.isCancelLoading = true
    this.errorMessage = ''
    this.successMessage = ''
    this.cdr.detectChanges()
  })

  this.bookingsService
    .cancelBooking(bookingId, {
      reason: this.cancelReason.trim()
    })
    .pipe(
      finalize(() => {
        this.ngZone.run(() => {
          this.isCancelLoading = false
          this.cdr.detectChanges()
        })
      })
    )
    .subscribe({
      next: response => {
        this.ngZone.run(() => {
          this.successMessage =
            response.refundPercentage > 0
              ? `تم إلغاء الحجز بنجاح. نسبة الاسترداد: ${response.refundPercentage}%`
              : response.message || 'تم إلغاء الحجز بنجاح'

          this.cancelReason = ''

          if (this.selectedBooking) {
            this.selectedBooking.status = response.status
            this.selectedBooking.statusAr = response.statusAr
          }

          this.cdr.detectChanges()
        })

        this.openBooking(bookingId)
        this.loadBookings(false)
      },
      error: error => {
        this.ngZone.run(() => {
          this.errorMessage = getErrorMessage(error, 'تعذر إلغاء الحجز')
          this.cdr.detectChanges()
        })
      }
    })
}
}
