import { CommonModule } from '@angular/common'
import { ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { finalize, forkJoin, of, switchMap } from 'rxjs'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'
import { ProviderActiveJobItem } from '../../../core/models/providerBookingModels'
import { MediaUploadService } from '../../../core/services/mediaUploadService'
import { ProviderBookingsService } from '../../../core/services/providerBookingsService'

@Component({
  selector: 'app-active-jobs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './active-jobs.html',
  styleUrl: './active-jobs.css'
})
export class ActiveJobs implements OnInit {
  jobs: ProviderActiveJobItem[] = []
  selectedJob: ProviderActiveJobItem | null = null

  isLoading = false
  isActionLoading = false
  isCompleting = false
  isTimeAdjustmentLoading = false

  providerCancelReason = ''
  isCancelLoading = false

  errorMessage = ''
  successMessage = ''

  providerNotes = ''
  selectedEvidenceFiles: File[] = []
  evidenceCaptions: string[] = []

  requestedAdditionalHrs: number | null = null
  providerNote = ''

  constructor(
    private providerBookingsService: ProviderBookingsService,
    private mediaUploadService: MediaUploadService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit(): void {
    this.loadActiveJobs()
  }

  loadActiveJobs(): void {
    this.ngZone.run(() => {
      this.isLoading = true
      this.errorMessage = ''
      this.successMessage = ''
      this.cdr.detectChanges()
    })

    this.providerBookingsService
      .getActiveJobs()
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

            this.jobs = Array.isArray(data)
              ? data
              : data.items ?? data.jobs ?? data.data ?? []

            if (!this.selectedJob && this.jobs.length > 0) {
              this.selectJob(this.jobs[0])
            }

            this.cdr.detectChanges()
          })
        },
        error: error => {
          this.ngZone.run(() => {
            this.jobs = []
            this.errorMessage = getErrorMessage(error, 'تعذر تحميل الأعمال النشطة')
            this.cdr.detectChanges()
          })
        }
      })
  }

  selectJob(job: ProviderActiveJobItem): void {
    this.selectedJob = job
    this.resetForms()
    this.errorMessage = ''
    this.successMessage = ''
    this.cdr.detectChanges()
  }

  startSelectedJob(): void {
    if (!this.selectedJob) {
      return
    }

    this.ngZone.run(() => {
      this.isActionLoading = true
      this.errorMessage = ''
      this.successMessage = ''
      this.cdr.detectChanges()
    })

    this.providerBookingsService
      .startBooking(this.selectedJob.id)
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
            this.successMessage = response.message || 'تم بدء تنفيذ الحجز بنجاح'
            this.cdr.detectChanges()
          })

          this.selectedJob = null
          this.loadActiveJobs()
        },
        error: error => {
          this.ngZone.run(() => {
            this.errorMessage = getErrorMessage(error, 'تعذر بدء تنفيذ الحجز')
            this.cdr.detectChanges()
          })
        }
      })
  }

  onEvidenceFilesSelected(event: Event): void {
    this.errorMessage = ''
    this.successMessage = ''

    const input = event.target as HTMLInputElement
    const files = Array.from(input.files ?? [])

    if (files.length === 0) {
      this.selectedEvidenceFiles = []
      this.evidenceCaptions = []
      this.cdr.detectChanges()
      return
    }

    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp']
    const maxSizeInMb = 10
    const maxSizeInBytes = maxSizeInMb * 1024 * 1024

    const invalidFile = files.find(file => !allowedTypes.includes(file.type))

    if (invalidFile) {
      this.errorMessage = 'مسموح فقط بصور JPG أو PNG أو WEBP'
      this.selectedEvidenceFiles = []
      this.evidenceCaptions = []
      this.cdr.detectChanges()
      return
    }

    const oversizedFile = files.find(file => file.size > maxSizeInBytes)

    if (oversizedFile) {
      this.errorMessage = `حجم كل صورة يجب ألا يتجاوز ${maxSizeInMb} ميجابايت`
      this.selectedEvidenceFiles = []
      this.evidenceCaptions = []
      this.cdr.detectChanges()
      return
    }

    this.selectedEvidenceFiles = files
    this.evidenceCaptions = files.map(() => '')
    this.cdr.detectChanges()
  }

  completeSelectedJob(): void {
    if (!this.selectedJob) {
      return
    }

    if (this.selectedEvidenceFiles.length === 0) {
      this.errorMessage = 'من فضلك اختر صورة إثبات واحدة على الأقل'
      this.cdr.detectChanges()
      return
    }

    this.ngZone.run(() => {
      this.isCompleting = true
      this.errorMessage = ''
      this.successMessage = ''
      this.cdr.detectChanges()
    })

    const bookingId = this.selectedJob.id

    const uploadRequests = this.selectedEvidenceFiles.map(file =>
      this.mediaUploadService.uploadImage(file, 'booking-completion', bookingId)
    )

    forkJoin(uploadRequests)
      .pipe(
        switchMap(uploadResults => {
          const photos = uploadResults.map((result, index) => ({
            photoUrl: result.url,
            caption: this.evidenceCaptions[index]?.trim() || null,
            displayOrder: index + 1
          }))

          return this.providerBookingsService.completeByProvider(bookingId, {
            providerNotes: this.providerNotes.trim() || null,
            photos
          })
        }),
        finalize(() => {
          this.ngZone.run(() => {
            this.isCompleting = false
            this.cdr.detectChanges()
          })
        })
      )
      .subscribe({
        next: response => {
          this.ngZone.run(() => {
            this.successMessage = response.message || 'تم إرسال إثبات اكتمال الخدمة بنجاح'
            this.resetForms()
            this.selectedJob = null
            this.cdr.detectChanges()
          })

          this.loadActiveJobs()
        },
        error: error => {
          this.ngZone.run(() => {
            this.errorMessage = getErrorMessage(error, 'تعذر إرسال إثبات اكتمال الخدمة')
            this.cdr.detectChanges()
          })
        }
      })
  }

  requestTimeAdjustment(): void {
    if (!this.selectedJob) {
      return
    }

    if (!this.requestedAdditionalHrs || this.requestedAdditionalHrs <= 0) {
      this.errorMessage = 'من فضلك اكتب عدد ساعات إضافية صحيح'
      this.cdr.detectChanges()
      return
    }

    if (!this.providerNote.trim()) {
      this.errorMessage = 'من فضلك اكتب سبب طلب زيادة الوقت'
      this.cdr.detectChanges()
      return
    }

    this.ngZone.run(() => {
      this.isTimeAdjustmentLoading = true
      this.errorMessage = ''
      this.successMessage = ''
      this.cdr.detectChanges()
    })

    this.providerBookingsService
      .createTimeAdjustment(this.selectedJob.id, {
        requestedAdditionalHrs: Number(this.requestedAdditionalHrs),
        providerNote: this.providerNote.trim()
      })
      .pipe(
        finalize(() => {
          this.ngZone.run(() => {
            this.isTimeAdjustmentLoading = false
            this.cdr.detectChanges()
          })
        })
      )
      .subscribe({
        next: response => {
          this.ngZone.run(() => {
            this.successMessage = response.message || 'تم إرسال طلب زيادة الوقت بنجاح'
            this.requestedAdditionalHrs = null
            this.providerNote = ''
            this.cdr.detectChanges()
          })

          this.selectedJob = null
          this.loadActiveJobs()
        },
        error: error => {
          this.ngZone.run(() => {
            this.errorMessage = getErrorMessage(error, 'تعذر إرسال طلب زيادة الوقت')
            this.cdr.detectChanges()
          })
        }
      })
  }

  resetForms(): void {
  this.providerNotes = ''
  this.selectedEvidenceFiles = []
  this.evidenceCaptions = []
  this.requestedAdditionalHrs = null
  this.providerNote = ''
  this.providerCancelReason = ''
}

  trackById(index: number, item: ProviderActiveJobItem): string {
    return item.id
  }
  canProviderCancel(job: ProviderActiveJobItem | null): boolean {
  if (!job) {
    return false
  }

  return job.status === 'Active' || job.status === '3'
}

cancelSelectedJob(): void {
  if (!this.selectedJob) {
    return
  }

  if (!this.providerCancelReason.trim()) {
    this.errorMessage = 'من فضلك اكتب سبب إلغاء الحجز'
    this.cdr.detectChanges()
    return
  }

  const bookingId = this.selectedJob.id

  this.ngZone.run(() => {
    this.isCancelLoading = true
    this.errorMessage = ''
    this.successMessage = ''
    this.cdr.detectChanges()
  })

  this.providerBookingsService
    .cancelBooking(bookingId, {
      reason: this.providerCancelReason.trim()
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

          this.providerCancelReason = ''
          this.selectedJob = null
          this.cdr.detectChanges()
        })

        this.loadActiveJobs()
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