import { CommonModule } from '@angular/common'
import { ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { ActivatedRoute, Router, RouterLink } from '@angular/router'
import { finalize } from 'rxjs'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'
import {
  BookingCreateContext,
  CreateBookingRequest
} from '../../../core/models/bookingModels'
import { BookingsService } from '../../../core/services/bookingsService'

@Component({
  selector: 'app-create-booking',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './create-booking.html',
  styleUrl: './create-booking.css'
})
export class CreateBooking implements OnInit {
  serviceListingId = ''
  context: BookingCreateContext | null = null

  isLoading = false
  isCreating = false

  errorMessage = ''
  successMessage = ''

  useDefaultAddress = true

  form: CreateBookingRequest = {
    serviceListingId: '',
    jobTitle: '',
    jobDescription: null,
    addressId: null,
    governorate: null,
    district: null,
    street: null,
    latitude: null,
    longitude: null,
    requestedStartDate: '',
    requestedStartTime: '',
    estimatedDurationHours: 1,
    siteContactName: null,
    siteContactPhone: null,
    accessRequirements: null,
    safetyNotes: null,
    acceptOutOfZoneSurcharge: false
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private bookingsService: BookingsService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('serviceListingId')

    if (!id) {
      this.errorMessage = 'ServiceListingId غير موجود في الرابط'
      this.updateView()
      return
    }

    this.serviceListingId = id
    this.form.serviceListingId = id

    this.loadCreateContext()
  }

  loadCreateContext(): void {
    this.ngZone.run(() => {
      this.isLoading = true
      this.errorMessage = ''
      this.successMessage = ''
      this.cdr.detectChanges()
    })

    this.bookingsService
      .getBookingCreateContext(this.serviceListingId)
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
            this.context = response
            this.form.estimatedDurationHours = response.minimumBookingHours || 1

            if (response.defaultAddress) {
              this.useDefaultAddress = true
              this.form.addressId = response.defaultAddress.id
            } else {
              this.useDefaultAddress = false
              this.form.addressId = null
            }

            this.cdr.detectChanges()
          })
        },
        error: error => {
          this.ngZone.run(() => {
            this.context = null
            this.errorMessage = getErrorMessage(error, 'تعذر تحميل بيانات إنشاء الحجز')
            this.cdr.detectChanges()
          })
        }
      })
  }

  createBooking(): void {
    this.errorMessage = ''
    this.successMessage = ''

    if (!this.context) {
      this.errorMessage = 'بيانات الخدمة غير محملة'
      this.updateView()
      return
    }

    if (!this.context.customerEligibility.canBook) {
      this.errorMessage = 'لا يمكنك إنشاء حجز قبل استكمال متطلبات الحساب'
      this.updateView()
      return
    }

    if (!this.form.jobTitle.trim()) {
      this.errorMessage = 'من فضلك اكتب عنوان العمل'
      this.updateView()
      return
    }

    if (!this.form.requestedStartDate) {
      this.errorMessage = 'من فضلك اختر تاريخ بداية الحجز'
      this.updateView()
      return
    }

    if (!this.form.requestedStartTime) {
      this.errorMessage = 'من فضلك اختر وقت بداية الحجز'
      this.updateView()
      return
    }

    if (Number(this.form.estimatedDurationHours) < this.context.minimumBookingHours) {
      this.errorMessage = `أقل مدة حجز هي ${this.context.minimumBookingHours} ساعة`
      this.updateView()
      return
    }

    const request: CreateBookingRequest = {
      ...this.form,
      jobTitle: this.form.jobTitle.trim(),
      jobDescription: this.emptyToNull(this.form.jobDescription),
      siteContactName: this.emptyToNull(this.form.siteContactName),
      siteContactPhone: this.emptyToNull(this.form.siteContactPhone),
      accessRequirements: this.emptyToNull(this.form.accessRequirements),
      safetyNotes: this.emptyToNull(this.form.safetyNotes),
      estimatedDurationHours: Number(this.form.estimatedDurationHours)
    }

    if (this.useDefaultAddress && this.context.defaultAddress) {
      request.addressId = this.context.defaultAddress.id
      request.governorate = null
      request.district = null
      request.street = null
      request.latitude = null
      request.longitude = null
    } else {
      request.addressId = null
      request.governorate = this.emptyToNull(this.form.governorate)
      request.district = this.emptyToNull(this.form.district)
      request.street = this.emptyToNull(this.form.street)
      request.latitude = this.form.latitude === null || this.form.latitude === undefined ? null : Number(this.form.latitude)
      request.longitude = this.form.longitude === null || this.form.longitude === undefined ? null : Number(this.form.longitude)
    }

    this.ngZone.run(() => {
      this.isCreating = true
      this.errorMessage = ''
      this.successMessage = ''
      this.cdr.detectChanges()
    })

    this.bookingsService
      .createBooking(request)
      .pipe(
        finalize(() => {
          this.ngZone.run(() => {
            this.isCreating = false
            this.cdr.detectChanges()
          })
        })
      )
      .subscribe({
        next: response => {
          this.ngZone.run(() => {
            this.successMessage = response.message || 'تم إنشاء طلب الحجز بنجاح'
            this.cdr.detectChanges()
          })

          this.router.navigate(['/bookings'])
        },
        error: error => {
          this.ngZone.run(() => {
            this.errorMessage = getErrorMessage(error, 'تعذر إنشاء الحجز')
            this.cdr.detectChanges()
          })
        }
      })
  }

  getEstimatedTotal(): number {
    const rate = this.context?.hourlyRate || 0
    const hours = Number(this.form.estimatedDurationHours || 0)

    return rate * hours
  }

  getAvailabilityText(): string {
    if (!this.context?.availability?.length) {
      return 'لا توجد مواعيد متاحة مسجلة'
    }

    return this.context.availability
      .map(item => `${item.dayNameAr || item.dayName}: ${item.openTime} - ${item.closeTime}`)
      .join(' | ')
  }

  private emptyToNull(value: string | null): string | null {
    if (!value || !value.trim()) {
      return null
    }

    return value.trim()
  }

  private updateView(): void {
    this.ngZone.run(() => {
      this.cdr.detectChanges()
    })
  }
}