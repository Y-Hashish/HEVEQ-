import { CommonModule } from '@angular/common'
import { Component, OnInit } from '@angular/core'
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule, NgForm } from '@angular/forms'
import { ActivatedRoute, Router, RouterLink } from '@angular/router'
import { HttpErrorResponse } from '@angular/common/http'
import { finalize, forkJoin, of, switchMap } from 'rxjs'

import { Category } from '../../../core/models/category.models'
import { CreateMarketplaceListingRequest, MarketplaceCondition, MarketplaceListingPhotoDto, MarketplaceTransactionMethod } from '../../../core/models/marketplace.models'
import { CategoriesService } from '../../../core/services/categories'
import { MarketplaceService } from '../../../core/services/marketplace'
import { ServiceListings } from '../../../core/services/service-listings'
import { OperatorsApi } from '../../../core/services/operators-api'
import { MediaUploadService } from '../../../core/services/mediaUploadService'
import { Toast } from '../../../core/services/toast'
import { extractErrorMessage } from '../../../core/models/api-error.models'
import { Operator } from '../../../core/models/operator.models'
import { ManageServiceListing, ServiceListingFormPayload } from '../../../core/models/service-listing.models'

const EMPTY_BASIC_INFO: ServiceListingFormPayload = {
  categoryId: 0,
  title: '',
  description: '',
  tags: null,
  equipmentModel: null,
  equipmentCapacity: null,
  equipmentCondition: null,
  yearOfManufacture: null,
  equipmentRegistrationNumber: null,
  hourlyRate: 0,
  dailyRate: null,
  minimumBookingHours: 1
}

@Component({
  selector: 'app-create-listing',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './create-listing.html',
  styleUrl: './create-listing.css'
})
export class CreateListing implements OnInit {
  // Tab control
  listingType: 'service' | 'marketplace' = 'service'

  // Shared Categories
  categories: Category[] = []

  // Marketplace flow fields
  form: FormGroup
  isSubmitting = false
  submitMessage = ''
  marketplaceEditId: string | null = null
  marketplaceExistingPhotos: MarketplaceListingPhotoDto[] = []
  selectedMarketplaceFiles: File[] = []
  submitError = ''

  // Service flow fields
  listingId: string | null = null
  isEditMode = false
  currentStep = 1
  isInitialLoading = false
  isSavingStep1 = false
  isAddingPhoto = false
  isLinkingOperator = false
  isAddingAvailability = false

  basicInfoModel: ServiceListingFormPayload = { ...EMPTY_BASIC_INFO }
  availabilityModel = {
    dayOfWeek: 0,
    openTime: '08:00',
    closeTime: '17:00'
  }

  availableOperators: Operator[] = []
  selectedServicePhotoFile: File | null = null
  newPhotoOrder = 1
  selectedOperatorId = ''
  manageData: ManageServiceListing | null = null

  steps = [
    { id: 1, label: 'المعلومات الأساسية' },
    { id: 2, label: 'الصور' },
    { id: 3, label: 'المشغلين' },
    { id: 4, label: 'التوفر' },
    { id: 5, label: 'المراجعة والإرسال' }
  ]

  readonly dayNames = ['الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت']

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    public router: Router,
    private serviceListings: ServiceListings,
    private categoriesService: CategoriesService,
    private marketplaceService: MarketplaceService,
    private operatorsApi: OperatorsApi,
    private mediaUploadService: MediaUploadService,
    private toast: Toast
  ) {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(150)]],
      categoryId: [null, [Validators.required]],
      condition: ['New', Validators.required],
      yearOfManufacture: [new Date().getFullYear(), [Validators.required, Validators.min(1900)]],
      description: ['', [Validators.required, Validators.maxLength(2000)]],
      specifications: [''],
      price: [null, [Validators.required, Validators.min(0)]],
      isNegotiable: [false],
      transactionMethod: ['Pickup', Validators.required],
      governorate: ['', Validators.required],
      district: ['', Validators.required],
    })
  }

  ngOnInit(): void {
    const marketplaceId = this.route.snapshot.queryParamMap.get('marketplaceId')
    const typeParam = this.route.snapshot.queryParamMap.get('type')
    const idParam = this.route.snapshot.paramMap.get('id')

    if (marketplaceId || typeParam === 'marketplace') {
      this.listingType = 'marketplace'
      this.loadMarketplaceCategories()

      if (marketplaceId) {
        this.marketplaceEditId = marketplaceId
        this.loadMarketplaceEditData(marketplaceId)
      }
    } else if (idParam) {
      this.listingId = idParam
      this.isEditMode = true
      this.listingType = 'service'
      this.loadInitialEditData(idParam)
      this.loadServiceCategories()
    } else {
      this.switchListingType(this.listingType)
    }

    this.operatorsApi.getMine().subscribe({
      next: operators => {
        this.availableOperators = operators.filter(o => o.isActive)
      },
      error: (err: HttpErrorResponse) => this.toast.error(extractErrorMessage(err))
    })
  }

  switchListingType(type: 'service' | 'marketplace'): void {
    if (this.isEditMode) return
    this.listingType = type
    this.categories = []

    if (type === 'service') {
      this.loadServiceCategories()
    } else {
      this.loadMarketplaceCategories()
    }
  }

  loadServiceCategories(): void {
    this.categoriesService.getCategories('Service').subscribe({
      next: categories => {
        this.categories = categories
      },
      error: (err: HttpErrorResponse) => this.toast.error(extractErrorMessage(err))
    })
  }

  loadMarketplaceCategories(): void {
    this.categoriesService.getCategories('Marketplace').subscribe({
      next: categories => {
        this.categories = categories
        if (categories.length > 0) {
          this.form.patchValue({ categoryId: categories[0].id })
        }
      },
      error: () => {
        this.submitError = 'تعذر تحميل الفئات حالياً.'
      }
    })
  }


  private loadMarketplaceEditData(id: string): void {
    this.isSubmitting = true
    this.submitError = ''

    this.marketplaceService
      .getById(id)
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: listing => {
          this.form.patchValue({
            title: listing.title,
            categoryId: listing.categoryId ?? this.form.value.categoryId,
            condition: listing.condition || 'New',
            yearOfManufacture: listing.yearOfManufacture ?? new Date().getFullYear(),
            description: listing.description,
            specifications: listing.specifications || '',
            price: listing.price,
            isNegotiable: listing.isNegotiable ?? false,
            transactionMethod: listing.transactionMethod || 'Pickup',
            governorate: listing.governorate || '',
            district: listing.district || ''
          })
          this.marketplaceExistingPhotos = [...(listing.photos ?? [])].sort((a, b) => a.displayOrder - b.displayOrder)
        },
        error: () => {
          this.submitError = 'تعذر تحميل بيانات إعلان السوق للتعديل.'
        }
      })
  }

  // ---------- Service Wizard Methods ----------

  private loadInitialEditData(id: string): void {
    this.isInitialLoading = true

    forkJoin({
      detail: this.serviceListings.getById(id),
      manage: this.serviceListings.getManage(id)
    }).subscribe({
      next: ({ detail, manage }) => {
        this.basicInfoModel = {
          categoryId: detail.categoryId,
          title: detail.title,
          description: detail.description,
          tags: detail.tags,
          equipmentModel: detail.equipmentModel,
          equipmentCapacity: detail.equipmentCapacity,
          equipmentCondition: detail.equipmentCondition,
          yearOfManufacture: detail.yearOfManufacture,
          equipmentRegistrationNumber: detail.equipmentRegistrationNumber,
          hourlyRate: detail.hourlyRate,
          dailyRate: detail.dailyRate,
          minimumBookingHours: detail.minimumBookingHours
        }
        this.manageData = manage
        this.isInitialLoading = false
      },
      error: (err: HttpErrorResponse) => {
        this.isInitialLoading = false
        this.toast.error(extractErrorMessage(err))
      }
    })
  }

  private refreshManageData(): void {
    if (!this.listingId) return

    this.serviceListings.getManage(this.listingId).subscribe({
      next: data => (this.manageData = data),
      error: (err: HttpErrorResponse) => this.toast.error(extractErrorMessage(err))
    })
  }

  goToStep(step: number): void {
    if (step > 1 && !this.listingId) {
      this.toast.error('احفظ المعلومات الأساسية أولاً')
      return
    }
    this.currentStep = step
  }

  saveBasicInfo(form: NgForm): void {
    if (form.invalid) return

    this.isSavingStep1 = true

    if (this.listingId) {
      this.serviceListings.update(this.listingId, this.basicInfoModel).subscribe({
        next: () => {
          this.isSavingStep1 = false
          this.toast.success('تم تحديث المعلومات الأساسية')
          this.refreshManageData()
        },
        error: (err: HttpErrorResponse) => {
          this.isSavingStep1 = false
          this.toast.error(extractErrorMessage(err))
        }
      })
    } else {
      this.serviceListings.create(this.basicInfoModel).subscribe({
        next: result => {
          this.isSavingStep1 = false
          this.listingId = result.id
          this.toast.success(result.message)
          this.refreshManageData()
          this.currentStep = 2
        },
        error: (err: HttpErrorResponse) => {
          this.isSavingStep1 = false
          this.toast.error(extractErrorMessage(err))
        }
      })
    }
  }

  addPhoto(): void {
    if (!this.listingId || !this.selectedServicePhotoFile) return

    this.isAddingPhoto = true

    this.mediaUploadService
      .uploadImage(this.selectedServicePhotoFile, 'service-listings', this.listingId)
      .pipe(
        switchMap(upload => this.serviceListings.addPhoto(this.listingId!, { photoUrl: upload.url, displayOrder: this.newPhotoOrder })),
        finalize(() => (this.isAddingPhoto = false))
      )
      .subscribe({
        next: () => {
          this.selectedServicePhotoFile = null
          this.newPhotoOrder = (this.manageData?.photos.length ?? 0) + 1
          this.toast.success('تم رفع الصورة وإضافتها')
          this.refreshManageData()
        },
        error: (err: HttpErrorResponse) => {
          this.toast.error(extractErrorMessage(err))
        }
      })
  }

  onServicePhotoSelected(event: Event): void {
    const input = event.target as HTMLInputElement
    const file = input.files?.[0] ?? null

    if (!file) {
      this.selectedServicePhotoFile = null
      return
    }

    if (!this.validateImageFile(file)) {
      this.selectedServicePhotoFile = null
      input.value = ''
      return
    }

    this.selectedServicePhotoFile = file
  }

  deletePhoto(photoId: string): void {
    if (!this.listingId) return

    this.serviceListings.deletePhoto(this.listingId, photoId).subscribe({
      next: () => {
        this.toast.success('تم حذف الصورة')
        this.refreshManageData()
      },
      error: (err: HttpErrorResponse) => this.toast.error(extractErrorMessage(err))
    })
  }

  linkOperator(): void {
    if (!this.listingId || !this.selectedOperatorId) return

    this.isLinkingOperator = true

    this.serviceListings.linkOperator(this.listingId, this.selectedOperatorId).subscribe({
      next: result => {
        this.isLinkingOperator = false
        this.selectedOperatorId = ''
        this.toast.success(result.message)
        this.refreshManageData()
      },
      error: (err: HttpErrorResponse) => {
        this.isLinkingOperator = false
        this.toast.error(extractErrorMessage(err))
      }
    })
  }

  unlinkOperator(operatorId: string): void {
    if (!this.listingId) return

    this.serviceListings.unlinkOperator(this.listingId, operatorId).subscribe({
      next: () => {
        this.toast.success('تم إلغاء ربط المشغل')
        this.refreshManageData()
      },
      error: (err: HttpErrorResponse) => this.toast.error(extractErrorMessage(err))
    })
  }

  addAvailability(): void {
    if (!this.listingId) return

    this.isAddingAvailability = true

    this.serviceListings.addAvailability(this.listingId, this.availabilityModel).subscribe({
      next: () => {
        this.isAddingAvailability = false
        this.toast.success('تمت إضافة الموعد')
        this.refreshManageData()
      },
      error: (err: HttpErrorResponse) => {
        this.isAddingAvailability = false
        this.toast.error(extractErrorMessage(err))
      }
    })
  }

  deleteAvailability(availabilityId: string): void {
    if (!this.listingId) return

    this.serviceListings.deleteAvailability(this.listingId, availabilityId).subscribe({
      next: () => {
        this.toast.success('تم حذف الموعد')
        this.refreshManageData()
      },
      error: (err: HttpErrorResponse) => this.toast.error(extractErrorMessage(err))
    })
  }

  submitForReview(): void {
    if (!this.listingId) return

    this.serviceListings.submitForReview(this.listingId).subscribe({
      next: result => {
        this.toast.success(result.message)
        this.router.navigate(['/equipment'])
      },
      error: (err: HttpErrorResponse) => this.toast.error(extractErrorMessage(err))
    })
  }

  // ---------- Marketplace Submit Method ----------

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched()
      this.submitError = 'يرجى تعبئة جميع الحقول المطلوبة.'
      return
    }

    const totalPhotosAfterSave = this.marketplaceExistingPhotos.length + this.selectedMarketplaceFiles.length
    if (totalPhotosAfterSave < 3) {
      this.submitError = 'يجب رفع 3 صور على الأقل قبل نشر إعلان البيع حتى لا يظل كمسودة.'
      return
    }

    if (totalPhotosAfterSave > 12) {
      this.submitError = 'الحد الأقصى لصور إعلان البيع هو 12 صورة.'
      return
    }

    const payload: CreateMarketplaceListingRequest = {
      categoryId: Number(this.form.value.categoryId),
      title: this.form.value.title.trim(),
      condition: this.form.value.condition as MarketplaceCondition,
      yearOfManufacture: Number(this.form.value.yearOfManufacture),
      description: this.form.value.description.trim(),
      specifications: this.form.value.specifications?.trim() || '',
      price: Number(this.form.value.price),
      isNegotiable: Boolean(this.form.value.isNegotiable),
      transactionMethod: this.form.value.transactionMethod as MarketplaceTransactionMethod,
      governorate: this.form.value.governorate.trim(),
      district: this.form.value.district.trim(),
    }

    this.isSubmitting = true
    this.submitMessage = ''
    this.submitError = ''

    const saveRequest = this.marketplaceEditId
      ? this.marketplaceService.updateListing(this.marketplaceEditId, payload).pipe(switchMap(() => of({ id: this.marketplaceEditId!, status: 'PendingReview', statusAr: 'قيد المراجعة', nextStep: '' })))
      : this.marketplaceService.createListing(payload)

    saveRequest
      .pipe(
        switchMap(response => this.uploadAndAttachMarketplacePhotos(response.id).pipe(switchMap(() => of(response)))),
        finalize(() => (this.isSubmitting = false))
      )
      .subscribe({
        next: response => {
          this.submitMessage = `تم حفظ العرض بنجاح. الحالة: ${response.statusAr || response.status}. تم رفع الصور وربطها بالإعلان.`
          this.selectedMarketplaceFiles = []
          setTimeout(() => this.router.navigate(['/equipment']), 1000)
        },
        error: (err: HttpErrorResponse) => {
          this.submitError = extractErrorMessage(err) || 'تعذر حفظ العرض. يرجى التحقق من البيانات والمحاولة مرة أخرى.'
        },
      })
  }

  onMarketplaceFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement
    const files = Array.from(input.files ?? [])

    if (!files.length) {
      this.selectedMarketplaceFiles = []
      return
    }

    const invalid = files.find(file => !this.validateImageFile(file, false))
    if (invalid) {
      this.submitError = 'مسموح بصور JPG أو PNG أو WEBP فقط وبحد أقصى 10 ميجابايت للصورة'
      this.selectedMarketplaceFiles = []
      input.value = ''
      return
    }

    const nextTotal = this.marketplaceExistingPhotos.length + files.length
    if (nextTotal > 12) {
      this.submitError = 'الحد الأقصى لصور إعلان البيع هو 12 صورة.'
      this.selectedMarketplaceFiles = []
      input.value = ''
      return
    }

    this.submitError = ''
    this.selectedMarketplaceFiles = files
  }

  removeSelectedMarketplaceFile(index: number): void {
    this.selectedMarketplaceFiles = this.selectedMarketplaceFiles.filter((_, i) => i !== index)
  }

  deleteMarketplacePhoto(photoId: string): void {
    if (!this.marketplaceEditId) return

    this.marketplaceService.deletePhoto(this.marketplaceEditId, photoId).subscribe({
      next: () => {
        this.toast.success('تم حذف الصورة')
        this.marketplaceExistingPhotos = this.marketplaceExistingPhotos.filter(photo => photo.id !== photoId)
      },
      error: (err: HttpErrorResponse) => this.toast.error(extractErrorMessage(err))
    })
  }

  private uploadAndAttachMarketplacePhotos(listingId: string) {
    if (!this.selectedMarketplaceFiles.length) {
      return of([])
    }

    const startOrder = this.marketplaceExistingPhotos.length + 1
    return forkJoin(
      this.selectedMarketplaceFiles.map((file, index) =>
        this.mediaUploadService
          .uploadImage(file, 'marketplace-listings', listingId)
          .pipe(
            switchMap(upload => this.marketplaceService.addPhoto(listingId, {
              photoUrl: upload.url,
              displayOrder: startOrder + index
            }))
          )
      )
    )
  }

  private validateImageFile(file: File, showToast = true): boolean {
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp']
    const maxSize = 10 * 1024 * 1024
    const isValid = allowedTypes.includes(file.type) && file.size <= maxSize

    if (!isValid && showToast) {
      this.toast.error('مسموح بصور JPG أو PNG أو WEBP فقط وبحد أقصى 10 ميجابايت للصورة')
    }

    return isValid
  }

}
