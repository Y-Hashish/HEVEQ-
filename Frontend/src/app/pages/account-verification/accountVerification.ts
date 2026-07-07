import { CommonModule } from '@angular/common'
import { ChangeDetectorRef, Component, OnInit } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { finalize } from 'rxjs'
import {DocumentItem, DocumentType, DocumentTypeOption, DocumentVerificationStatus } from '../../core/models/documentModels'
import { DocumentsService } from '../../core/services/documentsService'
import { MediaUploadService } from '../../core/services/mediaUploadService'
import { TokenStorage } from '../../core/services/token-storage'

@Component({
  selector: 'app-account-verification',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './accountVerification.html',
  styleUrl: './accountVerification.css'
})
export class AccountVerification implements OnInit {
  documents: DocumentItem[] = []
  documentTypeOptions: DocumentTypeOption[] = []

  selectedDocumentType: DocumentType = DocumentType.NationalId
  expiryDate = ''
  selectedFile: File | null = null
  selectedFileName = ''

  isLoading = false
  isUploading = false
  errorMessage = ''
  successMessage = ''

  constructor(
    private mediaUploadService: MediaUploadService,
    private documentsService: DocumentsService,
    private tokenStorage: TokenStorage,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.setDocumentTypeOptions()
    this.loadDocuments()
  }

  get selectedDocumentDescription(): string {
    return this.documentTypeOptions.find(
      option => option.value === Number(this.selectedDocumentType)
    )?.description || ''
  }

  private setDocumentTypeOptions(): void {
    const role = this.tokenStorage.getRole()

    const commonOptions: DocumentTypeOption[] = [
      {
        value: DocumentType.NationalId,
        label: 'البطاقة الشخصية',
        description: 'صورة واضحة للبطاقة الشخصية من الجهتين إن أمكن',
        requiresExpiryDate: true
      },
      {
        value: DocumentType.Other,
        label: 'مستند آخر',
        description: 'أي مستند إضافي مطلوب للتحقق',
        requiresExpiryDate: false
      }
    ]

    const providerOptions: DocumentTypeOption[] = [
      {
        value: DocumentType.CommercialRegistration,
        label: 'السجل التجاري',
        description: 'صورة واضحة من السجل التجاري الخاص بالشركة',
        requiresExpiryDate: true
      },
      {
        value: DocumentType.TaxCard,
        label: 'البطاقة الضريبية',
        description: 'صورة البطاقة الضريبية الخاصة بالنشاط',
        requiresExpiryDate: true
      },
      {
        value: DocumentType.EquipmentLicense,
        label: 'رخصة المعدة',
        description: 'رخصة أو مستند ملكية للمعدة',
        requiresExpiryDate: true
      },
      {
        value: DocumentType.Insurance,
        label: 'التأمين',
        description: 'مستند التأمين إن وجد',
        requiresExpiryDate: true
      }
    ]

    this.documentTypeOptions =
      role === 'provider'
        ? [...commonOptions, ...providerOptions]
        : commonOptions

    this.selectedDocumentType = this.documentTypeOptions[0].value
  }

  loadDocuments(): void {
    this.isLoading = true
    this.errorMessage = ''
    this.cdr.detectChanges()

    this.documentsService
      .getMyDocuments()
      .pipe(
        finalize(() => {
          this.isLoading = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: documents => {
          this.documents = documents ?? []
          this.cdr.detectChanges()
        },
        error: error => {
          this.documents = []
          this.errorMessage = this.getErrorMessage(error, 'تعذر تحميل المستندات')
          this.cdr.detectChanges()
        }
      })
  }

  onFileSelected(event: Event): void {
    this.errorMessage = ''
    this.successMessage = ''

    const input = event.target as HTMLInputElement
    const file = input.files?.[0]

    if (!file) {
      this.selectedFile = null
      this.selectedFileName = ''
      return
    }

    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp']

    if (!allowedTypes.includes(file.type)) {
      this.selectedFile = null
      this.selectedFileName = ''
      this.errorMessage = 'مسموح فقط بصور JPG أو PNG أو WEBP'
      this.cdr.detectChanges()
      return
    }

    const maxSizeInMb = 10
    const maxSizeInBytes = maxSizeInMb * 1024 * 1024

    if (file.size > maxSizeInBytes) {
      this.selectedFile = null
      this.selectedFileName = ''
      this.errorMessage = `حجم الصورة يجب ألا يتجاوز ${maxSizeInMb} ميجابايت`
      this.cdr.detectChanges()
      return
    }

    this.selectedFile = file
    this.selectedFileName = file.name
    this.cdr.detectChanges()
  }

  uploadDocument(): void {
    this.errorMessage = ''
    this.successMessage = ''

    if (!this.selectedFile) {
      this.errorMessage = 'من فضلك اختر صورة المستند'
      this.cdr.detectChanges()
      return
    }

    const selectedOption = this.documentTypeOptions.find(
      option => option.value === Number(this.selectedDocumentType)
    )

    if (selectedOption?.requiresExpiryDate && !this.expiryDate) {
      this.errorMessage = 'من فضلك أدخل تاريخ انتهاء المستند'
      this.cdr.detectChanges()
      return
    }

    this.isUploading = true
    this.cdr.detectChanges()

    this.mediaUploadService
      .uploadImage(this.selectedFile, 'documents')
      .subscribe({
        next: uploadResult => {
          if (!uploadResult.url) {
            this.isUploading = false
            this.errorMessage = 'تم رفع الصورة لكن لم يتم إرجاع رابط صالح'
            this.cdr.detectChanges()
            return
          }

          this.saveDocumentMetadata(uploadResult.url)
        },
        error: error => {
          this.isUploading = false
          this.errorMessage = this.getErrorMessage(error, 'تعذر رفع الصورة')
          this.cdr.detectChanges()
        }
      })
  }

  private saveDocumentMetadata(fileUrl: string): void {
    this.documentsService
      .uploadDocument({
        documentType: Number(this.selectedDocumentType),
        fileUrl,
        expiryDate: this.expiryDate || null,
        serviceListingId: null,
        marketplaceListingId: null,
        operatorId: null
      })
      .pipe(
        finalize(() => {
          this.isUploading = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: response => {
          this.successMessage = response.statusAr
            ? `تم رفع المستند بنجاح وحالته: ${response.statusAr}`
            : 'تم رفع المستند بنجاح'

          this.selectedFile = null
          this.selectedFileName = ''
          this.expiryDate = ''

          this.loadDocuments()
          this.cdr.detectChanges()
        },
        error: error => {
          this.errorMessage = this.getErrorMessage(
            error,
            'تم رفع الصورة لكن تعذر حفظ بيانات المستند'
          )

          this.cdr.detectChanges()
        }
      })
  }

  getDocumentName(type: DocumentType): string {
    const option = this.documentTypeOptions.find(item => item.value === type)

    if (option) {
      return option.label
    }

    switch (type) {
      case DocumentType.NationalId:
        return 'البطاقة الشخصية'
      case DocumentType.CommercialRegistration:
        return 'السجل التجاري'
      case DocumentType.TaxCard:
        return 'البطاقة الضريبية'
      case DocumentType.EquipmentLicense:
        return 'رخصة المعدة'
      case DocumentType.OperatorLicense:
        return 'رخصة المشغل'
      case DocumentType.Insurance:
        return 'التأمين'
      default:
        return 'مستند آخر'
    }
  }

  getStatusText(status: DocumentVerificationStatus, statusAr: string): string {
    if (statusAr) {
      return statusAr
    }

    switch (status) {
      case DocumentVerificationStatus.Pending:
        return 'قيد المراجعة'
      case DocumentVerificationStatus.Approved:
        return 'تم القبول'
      case DocumentVerificationStatus.Rejected:
        return 'مرفوض'
      case DocumentVerificationStatus.Expired:
        return 'منتهي'
      default:
        return 'غير معروف'
    }
  }

  getStatusClass(status: DocumentVerificationStatus): string {
    switch (status) {
      case DocumentVerificationStatus.Approved:
        return 'approved'
      case DocumentVerificationStatus.Rejected:
        return 'rejected'
      case DocumentVerificationStatus.Expired:
        return 'expired'
      default:
        return 'pending'
    }
  }

  trackById(index: number, item: DocumentItem): string {
    return item.id
  }

  private getErrorMessage(error: any, fallback: string): string {
    const response = error?.error

    if (!response) {
      return fallback
    }

    if (typeof response === 'string') {
      return response
    }

    if (response.message) {
      return response.message
    }

    if (response.Message) {
      return response.Message
    }

    if (response.errors) {
      const errors = response.errors

      if (Array.isArray(errors)) {
        return errors.join('، ')
      }

      if (typeof errors === 'object') {
        return Object.values(errors)
          .flat()
          .join('، ')
      }
    }

    return fallback
  }
}