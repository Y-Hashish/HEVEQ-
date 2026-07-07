import { CommonModule } from '@angular/common'
import { Component, DestroyRef, OnInit, inject, ChangeDetectorRef } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { takeUntilDestroyed } from '@angular/core/rxjs-interop'
import { EmployeeFieldVisitsService } from '../../../core/services/employeeFieldVisitsService'
import { FieldVisit } from '../../../core/models/admin.models'
import { Loading } from '../../../shared/components/loading/loading'
import { EmptyState } from '../../../shared/components/empty-state/empty-state'
import { ToastService } from '../../../shared/components/toast/toast.service'

@Component({
  selector: 'app-employee-field-visits',
  standalone: true,
  imports: [CommonModule, FormsModule, Loading, EmptyState],
  templateUrl: './field-visits.html',
  styleUrl: './field-visits.css'
})
export class EmployeeFieldVisits implements OnInit {
  visits: FieldVisit[] = []
  selectedVisit: FieldVisit | null = null
  isLoading = true
  isDetailsLoading = false
  isSavingStatus = false
  isSubmittingEvidence = false

  statusFilter = ''
  nextStatus = ''
  employeeNotes = ''
  outcome = 'JobConfirmed'
  photoUrlsText = ''

  private destroyRef = inject(DestroyRef)

  constructor(
    private fieldVisitsService: EmployeeFieldVisitsService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadVisits()
  }

  get filteredVisits(): FieldVisit[] {
    if (!this.statusFilter) return this.visits
    return this.visits.filter(v => v.visitStatus === this.statusFilter)
  }

  loadVisits(): void {
    this.isLoading = true
    this.fieldVisitsService.getMyVisits()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          this.visits = res || []
          this.isLoading = false
          if (this.selectedVisit) {
            const updated = this.visits.find(v => v.id === this.selectedVisit?.id)
            if (updated) this.selectedVisit = updated
          }
          this.cdr.detectChanges()
        },
        error: err => {
          this.isLoading = false
          this.toastService.error(err.error?.message || 'فشل في تحميل الزيارات الميدانية')
          this.cdr.detectChanges()
        }
      })
  }

  openVisit(visit: FieldVisit): void {
    this.isDetailsLoading = true
    this.fieldVisitsService.getVisitDetails(visit.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          this.selectedVisit = res
          this.nextStatus = res.visitStatus
          this.employeeNotes = res.employeeNotes || ''
          this.outcome = res.fieldVerificationOutcome || 'JobConfirmed'
          this.photoUrlsText = (res.photos || []).map(p => p.photoUrl).join('\n')
          this.isDetailsLoading = false
          this.cdr.detectChanges()
        },
        error: err => {
          this.isDetailsLoading = false
          this.toastService.error(err.error?.message || 'فشل في تحميل تفاصيل الزيارة')
          this.cdr.detectChanges()
        }
      })
  }

  closeDetails(): void {
    this.selectedVisit = null
    this.nextStatus = ''
    this.employeeNotes = ''
    this.outcome = 'JobConfirmed'
    this.photoUrlsText = ''
  }

  updateStatus(): void {
    if (!this.selectedVisit || !this.nextStatus) return

    this.isSavingStatus = true
    this.fieldVisitsService.updateStatus(this.selectedVisit.id, this.nextStatus)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success('تم تحديث حالة الزيارة')
          this.isSavingStatus = false
          this.openVisit(this.selectedVisit!)
          this.loadVisits()
        },
        error: err => {
          this.isSavingStatus = false
          this.toastService.error(err.error?.message || 'فشل في تحديث حالة الزيارة')
          this.cdr.detectChanges()
        }
      })
  }

  submitEvidence(): void {
    if (!this.selectedVisit) return
    if (!this.employeeNotes.trim()) {
      this.toastService.error('اكتب ملاحظات الزيارة أولا')
      return
    }

    const photoUrls = this.photoUrlsText
      .split('\n')
      .map(url => url.trim())
      .filter(Boolean)

    this.isSubmittingEvidence = true
    this.fieldVisitsService.submitEvidence(this.selectedVisit.id, {
      employeeNotes: this.employeeNotes.trim(),
      outcome: this.outcome,
      photoUrls
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success('تم رفع تقرير وإثباتات الزيارة')
          this.isSubmittingEvidence = false
          this.openVisit(this.selectedVisit!)
          this.loadVisits()
        },
        error: err => {
          this.isSubmittingEvidence = false
          this.toastService.error(err.error?.message || 'فشل في رفع إثباتات الزيارة')
          this.cdr.detectChanges()
        }
      })
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'Dispatched': return 'تم الإرسال'
      case 'OnSite': return 'في الموقع'
      case 'Completed': return 'تم الانتهاء'
      case 'FailedAccess': return 'فشل الدخول'
      default: return status || 'غير محدد'
    }
  }

  getOutcomeLabel(outcome?: string): string {
    switch (outcome) {
      case 'JobConfirmed': return 'تم تأكيد إتمام العمل'
      case 'JobNotConfirmed': return 'لم يتم تأكيد إتمام العمل'
      case 'PartiallyConfirmed': return 'تم التأكيد جزئيا'
      case 'Inconclusive': return 'غير حاسم'
      default: return outcome || 'لم يسجل بعد'
    }
  }
}
