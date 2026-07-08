import { CommonModule } from '@angular/common'
import { ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { ActivatedRoute, Router, RouterLink } from '@angular/router'
import { finalize, firstValueFrom } from 'rxjs'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'
import { extractCoordinatesFromMapLink } from '../../../core/helpers/mapLinkHelper'
import {
  BookingCreateContext,
  CreateBookingRequest
} from '../../../core/models/bookingModels'
import { BookingsService } from '../../../core/services/bookingsService'
import { MapLinkResolverService } from '../../../core/services/mapLinkResolverService'

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
  availableTimeSlots: string[] = []
  scheduleWarning = ''
  zoneWarning = ''
  manualLocationUrl = ''
  minBookingDate = ''
  maxBookingDate = ''

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
    private mapLinkResolver: MapLinkResolverService,
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
            this.setNativeDateRange()
            this.refreshTimeSlots()

            if (response.defaultAddress) {
              this.useDefaultAddress = true
              this.form.addressId = response.defaultAddress.id
              this.refreshZoneWarning()
            } else {
              this.useDefaultAddress = false
              this.form.addressId = null
              this.refreshZoneWarning()
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

  async createBooking(): Promise<void> {
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
      this.errorMessage = 'من فضلك اختر يوم متاح من التقويم'
      this.updateView()
      return
    }

    if (!this.isSelectedDateAvailable()) {
      this.errorMessage = 'اليوم المحدد غير متاح لهذه الخدمة. اختر يوماً من الأيام المفعلة في التقويم'
      this.updateView()
      return
    }

    if (!this.form.requestedStartTime) {
      this.errorMessage = 'من فضلك اختر وقت بداية الحجز من الأوقات المتاحة'
      this.updateView()
      return
    }

    if (!this.isSelectedTimeAvailable()) {
      this.errorMessage = 'وقت البداية مع مدة الحجز يتجاوز مواعيد عمل المزود. اختر وقتاً يسمح بانتهاء الحجز قبل نهاية وقت العمل'
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
      if (!(await this.syncManualAddressCoordinates())) {
        return
      }

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

  onAddressModeChanged(): void {
    this.refreshZoneWarning()
    this.updateView()
  }

  onManualLocationChanged(): void {
    const coordinates = extractCoordinatesFromMapLink(this.manualLocationUrl)
    if (coordinates) {
      this.form.latitude = coordinates.latitude
      this.form.longitude = coordinates.longitude
      this.refreshZoneWarning()
    }
    this.updateView()
  }

  private async syncManualAddressCoordinates(): Promise<boolean> {
    const coordinates = await this.resolveCoordinatesFromMapLink(this.manualLocationUrl)
    if (!coordinates) {
      this.errorMessage = 'من فضلك أدخل رابط موقع يحتوي على الإحداثيات أو الصق الإحداثيات مباشرة مثل 30.123,31.456.'
      this.updateView()
      return false
    }

    this.form.latitude = coordinates.latitude
    this.form.longitude = coordinates.longitude
    this.refreshZoneWarning()
    return true
  }

  private async resolveCoordinatesFromMapLink(value: string): Promise<{ latitude: number; longitude: number } | null> {
    const direct = extractCoordinatesFromMapLink(value)
    if (direct) {
      return direct
    }

    if (!value?.trim()) {
      return null
    }

    try {
      const response = await firstValueFrom(this.mapLinkResolver.resolve(value.trim()))
      return { latitude: Number(response.latitude), longitude: Number(response.longitude) }
    } catch {
      return null
    }
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

  getAvailableDaysText(): string {
    if (!this.context?.availability?.length) {
      return 'لا توجد أيام متاحة'
    }

    return this.context.availability
      .map(item => item.dayNameAr || item.dayName || this.getArabicDayName(Number(item.dayOfWeek)))
      .join('، ')
  }

  onDateChanged(): void {
    if (!this.form.requestedStartDate) {
      this.availableTimeSlots = []
      this.form.requestedStartTime = ''
      this.scheduleWarning = ''
      this.updateView()
      return
    }

    if (!this.isSelectedDateAvailable()) {
      const selectedDate = this.form.requestedStartDate
      this.form.requestedStartDate = ''
      this.form.requestedStartTime = ''
      this.availableTimeSlots = []
      this.scheduleWarning = `التاريخ ${selectedDate} غير متاح لهذه الخدمة. اختر واحداً من الأيام المتاحة فقط: ${this.getAvailableDaysText()}`
      this.updateView()
      return
    }

    this.refreshTimeSlots()
    this.updateView()
  }



  onDurationChanged(): void {
    this.form.estimatedDurationHours = Number(this.form.estimatedDurationHours || 0)
    this.refreshTimeSlots()
    this.updateView()
  }

  onTimeChanged(): void {
    if (this.form.requestedStartTime && !this.isSelectedTimeAvailable()) {
      this.scheduleWarning = 'هذا الوقت لا يسمح بانتهاء الحجز داخل مواعيد عمل المزود.'
    } else {
      this.scheduleWarning = ''
    }
    this.updateView()
  }

  getSelectedDayAvailabilityText(): string {
    const availability = this.getAvailabilityForDate(this.form.requestedStartDate)
    if (!availability) {
      return 'اختر يوماً متاحاً لعرض الأوقات المناسبة'
    }

    const blocked = this.getReservedSlotsForDate(this.form.requestedStartDate)
    if (!blocked.length) {
      return `متاح من ${this.formatTime(availability.openTime)} إلى ${this.formatTime(availability.closeTime)}`
    }

    const blockedText = blocked
      .map(slot => `${this.formatTime(slot.startTime)}-${this.formatTime(slot.endTime)}`)
      .join('، ')

    return `متاح من ${this.formatTime(availability.openTime)} إلى ${this.formatTime(availability.closeTime)} | أوقات محجوزة: ${blockedText}`
  }

  getEstimatedOutOfZoneSurcharge(): number {
    const distance = this.getOutOfZoneDistanceKm()
    if (distance <= 0) {
      return 0
    }

    return Math.round(distance * Number(this.context?.outOfZoneSurchargePerKm ?? 25) * 100) / 100
  }

  getOutOfZoneDistanceKm(): number {
    const context = this.context
    if (!context?.providerBaseLatitude || !context.providerBaseLongitude || !context.serviceRadiusKm) {
      return 0
    }

    const target = this.getCurrentAddressCoordinates()
    if (!target) {
      return 0
    }

    const distance = this.calculateDistanceKm(
      Number(context.providerBaseLatitude),
      Number(context.providerBaseLongitude),
      target.latitude,
      target.longitude
    )

    return Math.max(0, Math.round((distance - Number(context.serviceRadiusKm)) * 100) / 100)
  }

  refreshZoneWarning(): void {
    const outOfZoneDistanceKm = this.getOutOfZoneDistanceKm()
    if (outOfZoneDistanceKm > 0) {
      this.zoneWarning = `العنوان خارج نطاق خدمة المزود بحوالي ${outOfZoneDistanceKm} كم. قد يتم تطبيق رسوم خارج النطاق بقيمة تقريبية ${this.getEstimatedOutOfZoneSurcharge()} جنيه.`
      return
    }

    this.zoneWarning = ''
  }

  private getCurrentAddressCoordinates(): { latitude: number; longitude: number } | null {
    if (this.useDefaultAddress && this.context?.defaultAddress?.latitude != null && this.context.defaultAddress.longitude != null) {
      return {
        latitude: Number(this.context.defaultAddress.latitude),
        longitude: Number(this.context.defaultAddress.longitude)
      }
    }

    if (this.form.latitude != null && this.form.longitude != null) {
      return {
        latitude: Number(this.form.latitude),
        longitude: Number(this.form.longitude)
      }
    }

    return null
  }

  private calculateDistanceKm(lat1: number, lon1: number, lat2: number, lon2: number): number {
    const earthRadiusKm = 6371
    const dLat = this.toRadians(lat2 - lat1)
    const dLon = this.toRadians(lon2 - lon1)
    const a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
      Math.cos(this.toRadians(lat1)) * Math.cos(this.toRadians(lat2)) *
      Math.sin(dLon / 2) * Math.sin(dLon / 2)
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a))
    return earthRadiusKm * c
  }

  private toRadians(value: number): number {
    return value * Math.PI / 180
  }

  private setNativeDateRange(): void {
    const today = new Date()
    today.setHours(0, 0, 0, 0)
    const max = new Date(today)
    max.setDate(today.getDate() + 60)

    this.minBookingDate = this.toDateIso(today)
    this.maxBookingDate = this.toDateIso(max)
  }

  private refreshTimeSlots(): void {
    this.scheduleWarning = ''
    this.availableTimeSlots = this.getAvailableTimeSlots()

    if (this.form.requestedStartDate && this.getAvailabilityForDate(this.form.requestedStartDate) && !this.availableTimeSlots.length && !this.scheduleWarning) {
      this.scheduleWarning = 'لا توجد أوقات متاحة لهذا اليوم بعد استبعاد الحجوزات الموجودة أو بسبب مدة الحجز المختارة.'
    }

    if (this.form.requestedStartTime && !this.availableTimeSlots.includes(this.normalizeTime(this.form.requestedStartTime))) {
      this.form.requestedStartTime = ''
      this.scheduleWarning = 'تم مسح وقت البداية لأن مدة الحجز لا تناسب مواعيد العمل في اليوم المحدد.'
    }
  }

  private getAvailableTimeSlots(): string[] {
    const availability = this.getAvailabilityForDate(this.form.requestedStartDate)
    if (!availability) {
      return []
    }

    const durationMinutes = Math.ceil(Number(this.form.estimatedDurationHours || 0) * 60)
    const openMinutes = this.timeToMinutes(availability.openTime)
    const closeMinutes = this.timeToMinutes(availability.closeTime)
    const latestStart = closeMinutes - durationMinutes

    if (durationMinutes <= 0 || latestStart < openMinutes) {
      this.scheduleWarning = 'مدة الحجز أطول من فترة العمل المتاحة في هذا اليوم.'
      return []
    }

    const slots: string[] = []
    for (let minutes = openMinutes; minutes <= latestStart; minutes += 30) {
      const endMinutes = minutes + durationMinutes
      if (!this.doesSlotOverlapExistingBooking(this.form.requestedStartDate, minutes, endMinutes)) {
        slots.push(this.minutesToTime(minutes))
      }
    }

    if (!slots.length && this.getReservedSlotsForDate(this.form.requestedStartDate).length) {
      this.scheduleWarning = 'كل الأوقات المناسبة لمدة الحجز في هذا اليوم متداخلة مع حجوزات مؤكدة بالفعل. اختر يوماً أو وقتاً آخر.'
    }

    return slots
  }

  private doesSlotOverlapExistingBooking(dateIso: string, slotStartMinutes: number, slotEndMinutes: number): boolean {
    return this.getReservedSlotsForDate(dateIso).some(slot => {
      const reservedStart = this.timeToMinutes(slot.startTime)
      const reservedEnd = this.timeToMinutes(slot.endTime)
      return slotStartMinutes < reservedEnd && slotEndMinutes > reservedStart
    })
  }

  private getReservedSlotsForDate(dateIso: string | null | undefined): Array<{ startTime: string; endTime: string; bookingNumber?: string }> {
    if (!dateIso || !this.context?.unavailableSlots?.length) {
      return []
    }

    return this.context.unavailableSlots.filter(slot => String(slot.date).substring(0, 10) === dateIso)
  }

  private isSelectedDateAvailable(): boolean {
    return !!this.getAvailabilityForDate(this.form.requestedStartDate)
  }

  private isSelectedTimeAvailable(): boolean {
    if (!this.form.requestedStartTime) {
      return false
    }

    return this.availableTimeSlots.includes(this.normalizeTime(this.form.requestedStartTime))
  }

  private getAvailabilityForDate(dateIso: string | null | undefined) {
    if (!dateIso || !this.context?.availability?.length) {
      return null
    }

    const date = this.parseLocalDate(dateIso)
    if (!date) {
      return null
    }

    const day = date.getDay()
    return this.context.availability.find(item => Number(item.dayOfWeek) === day) || null
  }

  private parseLocalDate(dateIso: string): Date | null {
    const parts = dateIso.split('-').map(x => Number(x))
    if (parts.length !== 3 || parts.some(x => Number.isNaN(x))) {
      return null
    }

    return new Date(parts[0], parts[1] - 1, parts[2])
  }

  private toDateIso(date: Date): string {
    const year = date.getFullYear()
    const month = String(date.getMonth() + 1).padStart(2, '0')
    const day = String(date.getDate()).padStart(2, '0')
    return `${year}-${month}-${day}`
  }

  private timeToMinutes(value: string): number {
    const [hours, minutes] = this.normalizeTime(value).split(':').map(x => Number(x))
    return hours * 60 + minutes
  }

  private minutesToTime(totalMinutes: number): string {
    const hours = Math.floor(totalMinutes / 60)
    const minutes = totalMinutes % 60
    return `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}`
  }

  private normalizeTime(value: string): string {
    const [hours = '00', minutes = '00'] = String(value).split(':')
    return `${hours.padStart(2, '0')}:${minutes.padStart(2, '0')}`
  }

  private formatTime(value: string): string {
    return this.normalizeTime(value)
  }

  private getArabicDayName(day: number): string {
    return ['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت'][day] || ''
  }

  private getArabicMonthName(month: number): string {
    return ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'][month] || ''
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