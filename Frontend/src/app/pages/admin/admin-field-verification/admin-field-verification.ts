import { Component, OnInit, ChangeDetectionStrategy, DestroyRef, inject, ChangeDetectorRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { AdminFieldVerificationService } from '../../../core/services/adminFieldVerificationService';
import { AdminUsersService, AdminUser } from '../../../core/services/adminUsersService';
import { AdminFieldVerification, AdminFieldVerificationDetails } from '../../../core/models/admin.models';
import { Loading } from '../../../shared/components/loading/loading';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { Pagination } from '../../../shared/components/pagination/pagination';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-field-verification',
  imports: [CommonModule, FormsModule, Loading, EmptyState, Pagination],
  templateUrl: './admin-field-verification.html',
  styleUrl: './admin-field-verification.css'
})
export class AdminFieldVerificationComponent implements OnInit {
  verifications: AdminFieldVerification[] = [];
  isLoading = true;
  
  // Filters
  filterStatus: string = '';
  page = 1;
  pageSize = 10;
  totalCount = 0;

  // Selected Item
  selectedItem: AdminFieldVerificationDetails | null = null;
  isDetailsLoading = false;

  // Dispatch Form
  availableEmployees: AdminUser[] = [];
  selectedEmployeeId = '';
  scheduledDate = '';
  isDispatching = false;

  // Decision Form
  decisionType: 'Approve' | 'Reject' | '' = '';
  adminNote = '';
  isRecording = false;

  Math = Math;

  private destroyRef = inject(DestroyRef);

  constructor(
    private fieldService: AdminFieldVerificationService,
    private usersService: AdminUsersService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadVerifications();
    this.loadEmployees();
  }

  loadVerifications() {
    this.isLoading = true;
    this.fieldService.getVerifications(this.page, this.pageSize, this.filterStatus || undefined)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
      next: (res) => {
        this.verifications = res.items || [];
        this.totalCount = res.totalCount || 0;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.toastService.error('فشل في تحميل التحققات الميدانية');
        this.cdr.detectChanges();
      }
    });
  }

  loadEmployees() {
    // Load employees using AdminUsersService
    this.usersService.getUsers(1, 100, 'Employee', true)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
      next: (res) => {
        this.availableEmployees = (res.items || []).filter(u => u.isActive && u.isAvailableForDispatch !== false);
        this.cdr.detectChanges();
      }
    });
  }

  onFilterChange() {
    this.page = 1;
    this.selectedItem = null;
    this.loadVerifications();
  }

  onPageChange(newPage: number) {
    this.page = newPage;
    this.loadVerifications();
  }

  viewDetails(id: string) {
    this.isDetailsLoading = true;
    this.fieldService.getVerificationDetails(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
      next: (res) => {
        this.selectedItem = res;
        this.isDetailsLoading = false;
        
        // Reset forms
        this.selectedEmployeeId = '';
        this.scheduledDate = '';
        this.decisionType = '';
        this.adminNote = '';
        this.cdr.detectChanges();
      },
      error: () => {
        this.isDetailsLoading = false;
        this.toastService.error('فشل في تحميل تفاصيل التحقق');
        this.cdr.detectChanges();
      }
    });
  }

  closeDetails() {
    this.selectedItem = null;
  }

  dispatchEmployee() {
    if (!this.selectedItem || !this.selectedEmployeeId || !this.scheduledDate) return;

    this.isDispatching = true;
    this.fieldService.dispatchEmployee(this.selectedItem.id, {
      employeeId: this.selectedEmployeeId,
      scheduledDate: this.scheduledDate
    })
    .pipe(takeUntilDestroyed(this.destroyRef))
    .subscribe({
      next: () => {
        this.isDispatching = false;
        this.selectedItem!.status = 'Dispatched';
        this.toastService.success('تم التكليف بنجاح');
        this.loadVerifications();
        this.cdr.detectChanges();
      },
      error: () => {
        this.isDispatching = false;
        this.toastService.error('حدث خطأ أثناء التكليف.');
        this.cdr.detectChanges();
      }
    });
  }

  recordDecision() {
    if (!this.selectedItem || !this.decisionType || !this.adminNote) return;

    this.isRecording = true;
    this.fieldService.recordDecision(this.selectedItem.id, {
      decision: this.decisionType,
      adminNote: this.adminNote
    })
    .pipe(takeUntilDestroyed(this.destroyRef))
    .subscribe({
      next: () => {
        this.isRecording = false;
        this.selectedItem!.status = this.decisionType === 'Approve' ? 'Verified' : 'Failed';
        this.toastService.success('تم تسجيل القرار بنجاح');
        this.loadVerifications();
        this.cdr.detectChanges();
      },
      error: () => {
        this.isRecording = false;
        this.toastService.error('حدث خطأ أثناء حفظ القرار.');
        this.cdr.detectChanges();
      }
    });
  }
}
