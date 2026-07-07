import { Component, OnInit, ChangeDetectionStrategy, DestroyRef, inject, ChangeDetectorRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { AdminUser, AdminUsersService, UpdateUserStatusRequest, CreateStaffRequest } from '../../../core/services/adminUsersService';
import { Loading } from '../../../shared/components/loading/loading';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { ActionModal } from '../../../shared/components/action-modal/action-modal';
import { Pagination } from '../../../shared/components/pagination/pagination';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-users',
  imports: [CommonModule, FormsModule, Loading, EmptyState, ActionModal, Pagination],
  templateUrl: './admin-users.html',
  styleUrl: './admin-users.css'
})
export class AdminUsers implements OnInit {
  users: AdminUser[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 10;
  
  filterRole: string = '';
  filterStatus: string = ''; // '' | 'true' | 'false'
  searchTerm = ''

  isLoading = true;
  error = false;
  isCreateModalOpen = false;
  isCreatingStaff = false;
  showNewPassword = false;
  createForm: CreateStaffRequest = this.getEmptyCreateForm();

  // Modal State
  isModalOpen = false;
  selectedUser: AdminUser | null = null;
  modalTitle = '';
  modalDesc = '';
  modalConfirmText = '';
  modalConfirmColor: 'primary' | 'danger' | 'success' = 'primary';
  requireNote = false;
  pendingAction: 'activate' | 'suspend' | null = null;
  private destroyRef = inject(DestroyRef);

  constructor(
    private usersService: AdminUsersService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.isLoading = true;
    this.error = false;
    
    let isActiveParam: boolean | undefined = undefined;
    if (this.filterStatus === 'true') isActiveParam = true;
    if (this.filterStatus === 'false') isActiveParam = false;

    this.usersService.getUsers(this.page, this.pageSize, this.filterRole || undefined, isActiveParam, this.searchTerm)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
      next: (res) => {
        this.users = res.items || [];
        this.totalCount = res.totalCount || 0;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.error = true;
        this.isLoading = false;
        this.toastService.error('حدث خطأ أثناء جلب المستخدمين');
        this.cdr.detectChanges();
      }
    });
  }

  onFilterChange() {
    this.page = 1;
    this.loadUsers();
  }

  onSearchChange() {
    this.page = 1;
    this.loadUsers();
  }

  openCreateModal(role: 'Admin' | 'Employee' = 'Employee') {
    this.createForm = this.getEmptyCreateForm(role);
    this.isCreateModalOpen = true;
  }

  closeCreateModal() {
    if (this.isCreatingStaff) return;
    this.isCreateModalOpen = false;
    this.showNewPassword = false;
  }

  getEmptyCreateForm(role: 'Admin' | 'Employee' = 'Employee'): CreateStaffRequest {
    return {
      firstName: '',
      lastName: '',
      userName: '',
      email: '',
      password: '',
      phoneNumber: '',
      role,
      department: role === 'Employee' ? 'Support' : undefined,
      assignedGovernorate: '',
      isAvailableForDispatch: false
    };
  }

  submitCreateStaff() {
    if (!this.createForm.firstName || !this.createForm.lastName || !this.createForm.userName || !this.createForm.email || !this.createForm.password) {
      this.toastService.error('من فضلك املأ بيانات الحساب المطلوبة');
      return;
    }

    if (this.createForm.role === 'Employee' && !this.createForm.department) {
      this.toastService.error('اختر قسم الموظف');
      return;
    }

    this.isCreatingStaff = true;
    this.usersService.createStaff(this.createForm)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isCreatingStaff = false;
          this.isCreateModalOpen = false;
          this.toastService.success('تم إنشاء الحساب بنجاح');
          this.loadUsers();
          this.cdr.detectChanges();
        },
        error: err => {
          this.isCreatingStaff = false;
          this.toastService.error(err.error?.message || 'فشل إنشاء الحساب');
          this.cdr.detectChanges();
        }
      });
  }

  onPageChange(newPage: number) {
    this.page = newPage;
    this.loadUsers();
  }

  openSuspendModal(user: AdminUser) {
    this.selectedUser = user;
    this.pendingAction = 'suspend';
    this.modalTitle = 'إيقاف حساب مستخدم';
    this.modalDesc = `هل أنت متأكد من إيقاف حساب ${user.firstName} ${user.lastName}؟ لن يتمكن من تسجيل الدخول.`;
    this.modalConfirmText = 'إيقاف الحساب';
    this.modalConfirmColor = 'danger';
    this.requireNote = true; // Reason required for suspension
    this.isModalOpen = true;
  }

  openActivateModal(user: AdminUser) {
    this.selectedUser = user;
    this.pendingAction = 'activate';
    this.modalTitle = 'تفعيل حساب مستخدم';
    this.modalDesc = `هل أنت متأكد من إعادة تفعيل حساب ${user.firstName} ${user.lastName}؟`;
    this.modalConfirmText = 'تفعيل الحساب';
    this.modalConfirmColor = 'success';
    this.requireNote = false; // Optional/Not required for activation
    this.isModalOpen = true;
  }

  handleModalConfirm(note?: string) {
    if (!this.selectedUser || !this.pendingAction) return;

    const request: UpdateUserStatusRequest = {
      isActive: this.pendingAction === 'activate',
      reason: note
    };

    const targetUser = this.selectedUser; // save ref before resetting modal

    // Optimistic update could go here, but let's wait for API
    this.usersService.updateStatus(targetUser.id, request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
      next: (res) => {
        targetUser.isActive = res.isActive;
        this.toastService.success('تم تحديث حالة المستخدم بنجاح');
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to update user status', err);
        this.toastService.error('فشل في تحديث حالة المستخدم');
        this.cdr.detectChanges();
      }
    });
  }
}
