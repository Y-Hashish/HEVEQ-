import { CommonModule } from '@angular/common'
import { ChangeDetectorRef, Component, OnInit } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { RouterLink } from '@angular/router'
import { finalize, firstValueFrom } from 'rxjs'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'
import { extractCoordinatesFromMapLink } from '../../../core/helpers/mapLinkHelper'
import {
  CustomerProfile,
  ProviderProfile
} from '../../../core/models/profileModels'
import { ProfileService } from '../../../core/services/profileService'
import { MapLinkResolverService } from '../../../core/services/mapLinkResolverService'
import { TokenStorage } from '../../../core/services/token-storage'

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class Profile implements OnInit {
  role = ''
  isLoading = false
  isSaving = false
  errorMessage = ''
  successMessage = ''

  customerProfile: CustomerProfile | null = null
  providerProfile: ProviderProfile | null = null

  form = {
    firstName: '',
    lastName: '',
    userName: '',
    email: '',
    phoneNumber: '',

    companyName: '',
    businessDescription: '',
    baseLocationUrl: '',
    baseLatitude: null as number | null,
    baseLongitude: null as number | null,
    serviceRadiusKm: 20,

    defaultAddressLabel: '',
    defaultAddressGovernorate: '',
    defaultAddressDistrict: '',
    defaultAddressStreet: ''
  }

  constructor(
    private profileService: ProfileService,
    private tokenStorage: TokenStorage,
    private mapLinkResolver: MapLinkResolverService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.role = this.tokenStorage.getRole() || ''
    this.loadProfile()
  }

  get isCustomer(): boolean {
    return this.role === 'customer'
  }

  get isProvider(): boolean {
    return this.role === 'provider'
  }

  loadProfile(): void {
    this.errorMessage = ''
    this.successMessage = ''
    this.isLoading = true
    this.cdr.detectChanges()

    if (this.isProvider) {
      this.loadProviderProfile()
      return
    }

    this.loadCustomerProfile()
  }

  private loadCustomerProfile(): void {
    this.profileService
      .getCustomerProfile()
      .pipe(
        finalize(() => {
          this.isLoading = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: profile => {
          this.customerProfile = profile
          this.providerProfile = null
          this.patchCustomerForm(profile)
          this.cdr.detectChanges()
        },
        error: error => {
          this.customerProfile = null
          this.errorMessage = getErrorMessage(error, 'تعذر تحميل بيانات الحساب')
          this.cdr.detectChanges()
        }
      })
  }

  private loadProviderProfile(): void {
    this.profileService
      .getProviderProfile()
      .pipe(
        finalize(() => {
          this.isLoading = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: profile => {
          this.providerProfile = profile
          this.customerProfile = null
          this.patchProviderForm(profile)
          this.cdr.detectChanges()
        },
        error: error => {
          this.providerProfile = null
          this.errorMessage = getErrorMessage(error, 'تعذر تحميل بيانات المزود')
          this.cdr.detectChanges()
        }
      })
  }

  private patchCustomerForm(profile: CustomerProfile): void {
    this.form.firstName = profile.firstName || ''
    this.form.lastName = profile.lastName || ''
    this.form.userName = profile.userName || ''
    this.form.email = profile.email || ''
    this.form.phoneNumber = profile.phoneNumber || ''

    this.form.defaultAddressLabel = profile.defaultAddress?.label || ''
    this.form.defaultAddressGovernorate = profile.defaultAddress?.governorate || ''
    this.form.defaultAddressDistrict = profile.defaultAddress?.district || ''
    this.form.defaultAddressStreet = profile.defaultAddress?.street || ''
  }

  private patchProviderForm(profile: ProviderProfile): void {
    this.form.firstName = profile.firstName || ''
    this.form.lastName = profile.lastName || ''
    this.form.userName = profile.userName || ''
    this.form.email = profile.email || ''
    this.form.phoneNumber = profile.phoneNumber || ''

    this.form.companyName = profile.companyName || ''
    this.form.businessDescription = profile.businessDescription || ''
    this.form.baseLocationUrl = profile.baseLatitude !== null && profile.baseLongitude !== null ? `${profile.baseLatitude},${profile.baseLongitude}` : ''
    this.form.baseLatitude = profile.baseLatitude
    this.form.baseLongitude = profile.baseLongitude
    this.form.serviceRadiusKm = profile.serviceRadiusKm || 20
  }

  onProviderLocationChanged(): void {
    const coordinates = extractCoordinatesFromMapLink(this.form.baseLocationUrl)
    if (coordinates) {
      this.form.baseLatitude = coordinates.latitude
      this.form.baseLongitude = coordinates.longitude
    }
    this.cdr.detectChanges()
  }

  private async syncProviderBaseCoordinates(): Promise<boolean> {
    if (!this.isProvider) {
      return true
    }

    const coordinates = await this.resolveCoordinatesFromMapLink(this.form.baseLocationUrl)
    if (!coordinates) {
      this.errorMessage = 'من فضلك أدخل رابط موقع صحيح يحتوي على إحداثيات مقر المزود من الخريطة أو الصق الإحداثيات مباشرة.'
      this.cdr.detectChanges()
      return false
    }

    this.form.baseLatitude = coordinates.latitude
    this.form.baseLongitude = coordinates.longitude
    return true
  }

  async saveProfile(): Promise<void> {
    this.errorMessage = ''
    this.successMessage = ''

    if (!this.form.firstName || !this.form.lastName || !this.form.email) {
      this.errorMessage = 'من فضلك املأ الاسم الأول واسم العائلة والبريد الإلكتروني'
      this.cdr.detectChanges()
      return
    }

    if (this.isProvider && !this.form.companyName) {
      this.errorMessage = 'من فضلك اكتب اسم الشركة'
      this.cdr.detectChanges()
      return
    }

    if (this.isProvider && !(await this.syncProviderBaseCoordinates())) {
      return
    }

    this.isSaving = true
    this.cdr.detectChanges()

    if (this.isProvider) {
      this.saveProviderProfile()
      return
    }

    this.saveCustomerProfile()
  }

  private saveCustomerProfile(): void {
    const hasDefaultAddress =
      this.form.defaultAddressGovernorate ||
      this.form.defaultAddressDistrict ||
      this.form.defaultAddressStreet ||
      this.form.defaultAddressLabel

    this.profileService
      .updateCustomerProfile({
        firstName: this.form.firstName,
        lastName: this.form.lastName,
        email: this.form.email,
        phoneNumber: this.form.phoneNumber || null,
        defaultAddress: hasDefaultAddress
          ? {
              label: this.form.defaultAddressLabel || null,
              governorate: this.form.defaultAddressGovernorate,
              district: this.form.defaultAddressDistrict,
              street: this.form.defaultAddressStreet || null
            }
          : null
      })
      .pipe(
        finalize(() => {
          this.isSaving = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: profile => {
          this.customerProfile = profile
          this.patchCustomerForm(profile)
          this.successMessage = 'تم تحديث بيانات الحساب بنجاح'
          this.cdr.detectChanges()
        },
        error: error => {
          this.errorMessage = getErrorMessage(error, 'تعذر تحديث بيانات الحساب')
          this.cdr.detectChanges()
        }
      })
  }

  private saveProviderProfile(): void {
    this.profileService
      .updateProviderProfile({
        firstName: this.form.firstName,
        lastName: this.form.lastName,
        userName: this.form.userName,
        email: this.form.email,
        phoneNumber: this.form.phoneNumber || null,
        companyName: this.form.companyName,
        businessDescription: this.form.businessDescription || null,
        baseLatitude: this.form.baseLatitude,
        baseLongitude: this.form.baseLongitude,
        serviceRadiusKm: Number(this.form.serviceRadiusKm)
      })
      .pipe(
        finalize(() => {
          this.isSaving = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: profile => {
          this.providerProfile = profile
          this.patchProviderForm(profile)
          this.successMessage = 'تم تحديث بيانات المزود بنجاح'
          this.cdr.detectChanges()
        },
        error: error => {
          this.errorMessage = getErrorMessage(error, 'تعذر تحديث بيانات المزود')
          this.cdr.detectChanges()
        }
      })
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

  formatNumber(value: number | null | undefined): string {
    if (value === null || value === undefined) {
      return '0'
    }

    return Number(value).toFixed(1)
  }

  formatPercent(value: number | null | undefined): string {
    if (value === null || value === undefined) {
      return '0%'
    }

    return `${Number(value).toFixed(0)}%`
  }
}