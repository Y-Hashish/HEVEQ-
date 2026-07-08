import { CommonModule } from '@angular/common'
import { ChangeDetectorRef, Component, OnInit } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { finalize, firstValueFrom } from 'rxjs'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'
import { extractCoordinatesFromMapLink } from '../../../core/helpers/mapLinkHelper'
import { AddressItem } from '../../../core/models/addressModels'
import { AddressesService } from '../../../core/services/addressesService'
import { MapLinkResolverService } from '../../../core/services/mapLinkResolverService'

@Component({
  selector: 'app-saved-addresses',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './saved-addresses.html',
  styleUrl: './saved-addresses.css'
})
export class SavedAddresses implements OnInit {
  addresses: AddressItem[] = []

  isLoading = false
  isSaving = false
  errorMessage = ''
  successMessage = ''

  isEditMode = false
  editingAddressId: string | null = null

  form = {
    label: '',
    governorate: '',
    district: '',
    street: '',
    locationUrl: '',
    latitude: null as number | null,
    longitude: null as number | null,
    isDefault: false
  }

  constructor(
    private addressesService: AddressesService,
    private mapLinkResolver: MapLinkResolverService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadAddresses()
  }

  loadAddresses(): void {
    this.isLoading = true
    this.errorMessage = ''
    this.cdr.detectChanges()

    this.addressesService
      .getMyAddresses()
      .pipe(
        finalize(() => {
          this.isLoading = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: addresses => {
          this.addresses = addresses ?? []
          this.cdr.detectChanges()
        },
        error: error => {
          this.addresses = []
          this.errorMessage = getErrorMessage(error, 'تعذر تحميل العناوين')
          this.cdr.detectChanges()
        }
      })
  }

  async saveAddress(): Promise<void> {
    this.errorMessage = ''
    this.successMessage = ''

    if (!this.form.label || !this.form.governorate || !this.form.district || !this.form.street) {
      this.errorMessage = 'من فضلك املأ اسم العنوان والمحافظة والمنطقة والشارع'
      this.cdr.detectChanges()
      return
    }

    if (!(await this.syncCoordinatesFromLocationUrl())) {
      return
    }

    this.isSaving = true
    this.cdr.detectChanges()

    if (this.isEditMode && this.editingAddressId) {
      this.updateAddress()
      return
    }

    this.createAddress()
  }

  private createAddress(): void {
    this.addressesService
      .createAddress({
        label: this.form.label,
        governorate: this.form.governorate,
        district: this.form.district,
        street: this.form.street,
        latitude: Number(this.form.latitude),
        longitude: Number(this.form.longitude),
        isDefault: this.form.isDefault
      })
      .pipe(
        finalize(() => {
          this.isSaving = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: () => {
          this.successMessage = 'تم إضافة العنوان بنجاح'
          this.resetForm()
          this.loadAddresses()
        },
        error: error => {
          this.errorMessage = getErrorMessage(error, 'تعذر إضافة العنوان')
          this.cdr.detectChanges()
        }
      })
  }

  private updateAddress(): void {
    if (!this.editingAddressId) {
      return
    }

    this.addressesService
      .updateAddress(this.editingAddressId, {
        label: this.form.label,
        governorate: this.form.governorate,
        district: this.form.district,
        street: this.form.street,
        latitude: this.form.latitude,
        longitude: this.form.longitude,
        isDefault: this.form.isDefault
      })
      .pipe(
        finalize(() => {
          this.isSaving = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: () => {
          this.successMessage = 'تم تحديث العنوان بنجاح'
          this.resetForm()
          this.loadAddresses()
        },
        error: error => {
          this.errorMessage = getErrorMessage(error, 'تعذر تحديث العنوان')
          this.cdr.detectChanges()
        }
      })
  }

  editAddress(address: AddressItem): void {
    this.isEditMode = true
    this.editingAddressId = address.id

    this.form = {
      label: address.label,
      governorate: address.governorate,
      district: address.district,
      street: address.street,
      locationUrl: address.latitude !== null && address.longitude !== null ? `${address.latitude},${address.longitude}` : '',
      latitude: address.latitude,
      longitude: address.longitude,
      isDefault: address.isDefault
    }

    this.successMessage = ''
    this.errorMessage = ''
    this.cdr.detectChanges()
  }

  setDefault(address: AddressItem): void {
    if (address.isDefault) {
      return
    }

    this.addressesService.setDefaultAddress(address.id).subscribe({
      next: () => {
        this.successMessage = 'تم تعيين العنوان كعنوان افتراضي'
        this.loadAddresses()
      },
      error: error => {
        this.errorMessage = getErrorMessage(error, 'تعذر تعيين العنوان الافتراضي')
        this.cdr.detectChanges()
      }
    })
  }

  deleteAddress(address: AddressItem): void {
    const confirmed = confirm('هل أنت متأكد من حذف هذا العنوان؟')

    if (!confirmed) {
      return
    }

    this.addressesService.deleteAddress(address.id).subscribe({
      next: () => {
        this.successMessage = 'تم حذف العنوان بنجاح'
        this.addresses = this.addresses.filter(item => item.id !== address.id)

        if (this.editingAddressId === address.id) {
          this.resetForm()
        }

        this.cdr.detectChanges()
      },
      error: error => {
        this.errorMessage = getErrorMessage(error, 'تعذر حذف العنوان')
        this.cdr.detectChanges()
      }
    })
  }

  resetForm(): void {
    this.isEditMode = false
    this.editingAddressId = null

    this.form = {
      label: '',
      governorate: '',
      district: '',
      street: '',
      locationUrl: '',
      latitude: null,
      longitude: null,
      isDefault: false
    }

    this.cdr.detectChanges()
  }

  private async syncCoordinatesFromLocationUrl(): Promise<boolean> {
    const coordinates = await this.resolveCoordinatesFromMapLink(this.form.locationUrl)

    if (!coordinates) {
      this.errorMessage = 'من فضلك أدخل رابط موقع يحتوي على الإحداثيات أو الصق الإحداثيات مباشرة مثل 30.123,31.456.'
      this.cdr.detectChanges()
      return false
    }

    this.form.latitude = coordinates.latitude
    this.form.longitude = coordinates.longitude
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

  trackById(index: number, item: AddressItem): string {
    return item.id
  }
}