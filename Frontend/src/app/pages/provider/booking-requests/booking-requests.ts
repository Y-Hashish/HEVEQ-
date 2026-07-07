import { CommonModule } from '@angular/common'
import { ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { finalize } from 'rxjs'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'
import {
  ProviderBookingRequestItem,
  ProviderOperatorItem
} from '../../../core/models/providerBookingModels'
import { ProviderBookingsService } from '../../../core/services/providerBookingsService'

@Component({
  selector: 'app-booking-requests',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './booking-requests.html',
  styleUrl: './booking-requests.css'
})
export class BookingRequests implements OnInit {
  requests: ProviderBookingRequestItem[] = []
  operators: ProviderOperatorItem[] = []

  selectedRequest: ProviderBookingRequestItem | null = null
  selectedOperatorId = ''
  rejectReason = ''

  isLoading = false
  isOperatorsLoading = false
  isActionLoading = false

  errorMessage = ''
  successMessage = ''

  constructor(
    private providerBookingsService: ProviderBookingsService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit(): void {
    this.loadRequests()
    this.loadOperators()
  }

  loadRequests(): void {
    this.ngZone.run(() => {
      this.isLoading = true
      this.errorMessage = ''
      this.successMessage = ''
      this.cdr.detectChanges()
    })

    this.providerBookingsService
      .getBookingRequests()
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
            this.requests = response.items ?? []

            if (!this.selectedRequest && this.requests.length > 0) {
              this.selectRequest(this.requests[0])
            }

            this.cdr.detectChanges()
          })
        },
        error: error => {
          this.ngZone.run(() => {
            this.requests = []
            this.errorMessage = getErrorMessage(error, 'تعذر تحميل طلبات الحجز')
            this.cdr.detectChanges()
          })
        }
      })
  }

  loadOperators(): void {
    this.isOperatorsLoading = true
    this.cdr.detectChanges()

    this.providerBookingsService
      .getOperators()
      .pipe(
        finalize(() => {
          this.ngZone.run(() => {
            this.isOperatorsLoading = false
            this.cdr.detectChanges()
          })
        })
      )
      .subscribe({
        next: operators => {
          this.ngZone.run(() => {
            this.operators = operators ?? []

            const firstActiveOperator = this.operators.find(x => x.isActive !== false)
            this.selectedOperatorId = firstActiveOperator?.id || ''

            this.cdr.detectChanges()
          })
        },
        error: () => {
          this.ngZone.run(() => {
            this.operators = []
            this.cdr.detectChanges()
          })
        }
      })
  }

  selectRequest(request: ProviderBookingRequestItem): void {
    this.selectedRequest = request
    this.rejectReason = ''
    this.errorMessage = ''
    this.successMessage = ''
    this.cdr.detectChanges()
  }

  acceptSelectedRequest(): void {
    if (!this.selectedRequest) {
      return
    }

    if (!this.selectedOperatorId) {
      this.errorMessage = 'من فضلك اختر المشغل المسؤول عن تنفيذ الحجز'
      this.cdr.detectChanges()
      return
    }

    this.ngZone.run(() => {
      this.isActionLoading = true
      this.errorMessage = ''
      this.successMessage = ''
      this.cdr.detectChanges()
    })

    this.providerBookingsService
      .acceptBooking(this.selectedRequest.id, {
        operatorId: this.selectedOperatorId
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
          this.ngZone.run(() => {
            this.successMessage = response.message || 'تم قبول الحجز بنجاح'
            this.selectedRequest = null
            this.cdr.detectChanges()
          })

          this.loadRequests()
        },
        error: error => {
          this.ngZone.run(() => {
            this.errorMessage = getErrorMessage(error, 'تعذر قبول الحجز')
            this.cdr.detectChanges()
          })
        }
      })
  }

  rejectSelectedRequest(): void {
    if (!this.selectedRequest) {
      return
    }

    if (!this.rejectReason.trim()) {
      this.errorMessage = 'من فضلك اكتب سبب رفض الحجز'
      this.cdr.detectChanges()
      return
    }

    this.ngZone.run(() => {
      this.isActionLoading = true
      this.errorMessage = ''
      this.successMessage = ''
      this.cdr.detectChanges()
    })

    this.providerBookingsService
      .rejectBooking(this.selectedRequest.id, {
        reason: this.rejectReason.trim()
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
          this.ngZone.run(() => {
            this.successMessage = response.message || 'تم رفض الحجز بنجاح'
            this.selectedRequest = null
            this.rejectReason = ''
            this.cdr.detectChanges()
          })

          this.loadRequests()
        },
        error: error => {
          this.ngZone.run(() => {
            this.errorMessage = getErrorMessage(error, 'تعذر رفض الحجز')
            this.cdr.detectChanges()
          })
        }
      })
  }

  trackById(index: number, item: ProviderBookingRequestItem): string {
    return item.id
  }
}