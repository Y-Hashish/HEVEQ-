import { CommonModule } from '@angular/common'
import { Component, OnInit, ChangeDetectorRef } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { finalize, Observable } from 'rxjs'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'
import { Operator, OperatorFormPayload } from '../../../core/models/operator.models'
import { OperatorsApi } from '../../../core/services/operators-api'

@Component({
  selector: 'app-operators',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './operators.html',
  styleUrl: './operators.css'
})
export class Operators implements OnInit {
  operators: Operator[] = []
  isLoading = false
  isSaving = false
  errorMessage = ''
  successMessage = ''
  editingId: string | null = null

  form: OperatorFormPayload & { isActive: boolean } = this.emptyForm()

  constructor(
    private operatorsApi: OperatorsApi,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load()
  }

  load(): void {
    this.isLoading = true
    this.errorMessage = ''
    this.cdr.detectChanges()

    this.operatorsApi.getMine().pipe(
      finalize(() => {
        this.isLoading = false
        this.cdr.detectChanges()
      })
    ).subscribe({
      next: operators => {
        this.operators = operators ?? []
        this.cdr.detectChanges()
      },
      error: error => {
        this.operators = []
        this.errorMessage = getErrorMessage(error, 'تعذر تحميل المشغلين')
        this.cdr.detectChanges()
      }
    })
  }

  save(): void {
    if (!this.form.fullName.trim()) {
      this.errorMessage = 'اكتب اسم المشغل'
      return
    }

    this.isSaving = true
    this.errorMessage = ''
    this.successMessage = ''

    const payload: OperatorFormPayload = {
      fullName: this.form.fullName.trim(),
      yearsOfExperience: Number(this.form.yearsOfExperience || 0),
      specialization: this.clean(this.form.specialization),
      licenseType: this.clean(this.form.licenseType),
      licenseNumber: this.clean(this.form.licenseNumber),
      licenseExpiryDate: this.clean(this.form.licenseExpiryDate)
    }

    const request: Observable<string | void> = this.editingId
      ? this.operatorsApi.update(this.editingId, { ...payload, isActive: this.form.isActive })
      : this.operatorsApi.create(payload)

    request.pipe(
      finalize(() => {
        this.isSaving = false
        this.cdr.detectChanges()
      })
    ).subscribe({
      next: () => {
        this.successMessage = this.editingId ? 'تم تحديث المشغل' : 'تم إضافة المشغل'
        this.resetForm()
        this.load()
        this.cdr.detectChanges()
      },
      error: (error: any) => {
        this.errorMessage = getErrorMessage(error, 'تعذر حفظ المشغل')
        this.cdr.detectChanges()
      }
    })
  }

  edit(operator: Operator): void {
    this.editingId = operator.id
    this.form = {
      fullName: operator.fullName,
      yearsOfExperience: operator.yearsOfExperience ?? 0,
      specialization: operator.specialization,
      licenseType: operator.licenseType,
      licenseNumber: operator.licenseNumber,
      licenseExpiryDate: operator.licenseExpiryDate,
      isActive: operator.isActive
    }
  }

  remove(operator: Operator): void {
    if (!confirm(`هل تريد حذف المشغل ${operator.fullName}؟`)) return

    this.operatorsApi.delete(operator.id).subscribe({
      next: () => {
        this.successMessage = 'تم حذف المشغل'
        this.load()
        this.cdr.detectChanges()
      },
      error: error => {
        this.errorMessage = getErrorMessage(error, 'تعذر حذف المشغل')
        this.cdr.detectChanges()
      }
    })
  }

  resetForm(): void {
    this.editingId = null
    this.form = this.emptyForm()
  }

  trackById(index: number, item: Operator): string {
    return item.id
  }

  private emptyForm(): OperatorFormPayload & { isActive: boolean } {
    return {
      fullName: '',
      yearsOfExperience: 0,
      specialization: null,
      licenseType: null,
      licenseNumber: null,
      licenseExpiryDate: null,
      isActive: true
    }
  }

  private clean(value: string | null): string | null {
    return value && value.trim() ? value.trim() : null
  }
}
