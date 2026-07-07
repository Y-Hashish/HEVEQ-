import { Component, OnInit, DestroyRef, inject, ChangeDetectorRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AdminDashboardService } from '../../../core/services/adminDashboardService';
import { DashboardSummary, PendingAction, AdminTicket, FieldVisit } from '../../../core/models/admin.models';
import { Loading } from '../../../shared/components/loading/loading';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { TokenStorage } from '../../../core/services/token-storage';
import { EmployeeFieldVisitsService } from '../../../core/services/employeeFieldVisitsService';
import { AdminTicketsService } from '../../../core/services/adminTicketsService';
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper';

@Component({
  selector: 'app-admin-dashboard',
  imports: [CommonModule, RouterModule, Loading, EmptyState],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboard implements OnInit {
  summary: DashboardSummary | null = null;
  pendingActions: PendingAction[] = [];
  employeeFieldVisits: FieldVisit[] = [];
  employeeTickets: AdminTicket[] = [];

  isLoadingSummary = true;
  isLoadingActions = true;
  summaryError = false;
  actionsError = false;

  employeeMode: 'admin' | 'field' | 'support' = 'admin';

  private destroyRef = inject(DestroyRef);

  constructor(
    private dashboardService: AdminDashboardService,
    private fieldVisitsService: EmployeeFieldVisitsService,
    private adminTicketsService: AdminTicketsService,
    private tokenStorage: TokenStorage,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.resolveMode();
    this.loadData();
  }

  get isAdminMode(): boolean {
    return this.employeeMode === 'admin';
  }

  get isFieldEmployeeMode(): boolean {
    return this.employeeMode === 'field';
  }

  get isSupportEmployeeMode(): boolean {
    return this.employeeMode === 'support';
  }

  private resolveMode(): void {
    const role = this.tokenStorage.getRole();
    if (role !== 'employee') {
      this.employeeMode = 'admin';
      return;
    }

    const currentUser: any = this.tokenStorage.getCurrentUser();
    const department = String(currentUser?.employeeDepartment || currentUser?.department || '').toLowerCase();
    const isDispatchable = currentUser?.isAvailableForDispatch === true;

    this.employeeMode = isDispatchable || department.includes('field') || department.includes('verification') || department.includes('ميد')
      ? 'field'
      : 'support';
  }

  loadData() {
    if (this.isFieldEmployeeMode) {
      this.loadEmployeeFieldVisits();
      return;
    }

    if (this.isSupportEmployeeMode) {
      this.loadEmployeeTickets();
      return;
    }

    this.isLoadingActions = false;
    this.loadSummary();
  }

  loadSummary() {
    this.isLoadingSummary = true;
    this.summaryError = false;
    this.dashboardService.getSummary()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.summary = this.normalizeSummary(data);
          this.isLoadingSummary = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.summaryError = true;
          this.isLoadingSummary = false;
          this.toastService.error(getErrorMessage(err, 'فشل في تحميل الإحصائيات'));
          this.cdr.detectChanges();
        }
      });
  }

  loadPendingActions() {
    this.isLoadingActions = true;
    this.actionsError = false;
    this.dashboardService.getPendingActions()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.pendingActions = res.items || [];
          this.isLoadingActions = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.actionsError = true;
          this.isLoadingActions = false;
          this.toastService.error(getErrorMessage(err, 'فشل في تحميل الإجراءات المعلقة'));
          this.cdr.detectChanges();
        }
      });
  }

  loadEmployeeFieldVisits(): void {
    this.isLoadingSummary = true;
    this.isLoadingActions = true;
    this.fieldVisitsService.getMyVisits()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (visits) => {
          this.employeeFieldVisits = visits || [];
          this.isLoadingSummary = false;
          this.isLoadingActions = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.employeeFieldVisits = [];
          this.isLoadingSummary = false;
          this.isLoadingActions = false;
          this.toastService.error(getErrorMessage(err, 'فشل في تحميل مهام الزيارات الميدانية'));
          this.cdr.detectChanges();
        }
      });
  }

  loadEmployeeTickets(): void {
    this.isLoadingSummary = true;
    this.isLoadingActions = true;
    this.adminTicketsService.getTickets(1, 20)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.employeeTickets = res.items || [];
          this.isLoadingSummary = false;
          this.isLoadingActions = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.employeeTickets = [];
          this.isLoadingSummary = false;
          this.isLoadingActions = false;
          this.toastService.error(getErrorMessage(err, 'فشل في تحميل تذاكر الدعم'));
          this.cdr.detectChanges();
        }
      });
  }

  private normalizeSummary(data: DashboardSummary): DashboardSummary {
    return {
      totalUsers: Number(data?.totalUsers ?? 0),
      activeUsers: Number(data?.activeUsers ?? 0),
      totalProviders: Number(data?.totalProviders ?? 0),
      pendingServiceListings: Number(data?.pendingServiceListings ?? 0),
      pendingMarketplaceListings: Number(data?.pendingMarketplaceListings ?? 0),
      pendingDocuments: Number(data?.pendingDocuments ?? 0),
      openTickets: Number(data?.openTickets ?? 0),
      disputedBookings: Number(data?.disputedBookings ?? 0),
      disputedMarketplaceOrders: Number(data?.disputedMarketplaceOrders ?? 0),
      frozenEscrowRecords: Number(data?.frozenEscrowRecords ?? 0),
      pendingFieldVerifications: Number(data?.pendingFieldVerifications ?? 0),
      aiHighRiskItems: Number(data?.aiHighRiskItems ?? 0)
    };
  }

  get totalPendingListings(): number {
    return Number(this.summary?.pendingServiceListings ?? 0) + Number(this.summary?.pendingMarketplaceListings ?? 0);
  }

  get totalOpenDisputes(): number {
    return Number(this.summary?.disputedBookings ?? 0) + Number(this.summary?.disputedMarketplaceOrders ?? 0);
  }

  get pendingFieldVisitsCount(): number {
    return Number(this.summary?.pendingFieldVerifications ?? 0);
  }


  getActionRoute(url: string | null | undefined): string {
    const value = url || '/admin';
    return value.split('?')[0] || '/admin';
  }

  getActionQueryParams(url: string | null | undefined): Record<string, string> {
    const value = url || '';
    const query = value.includes('?') ? value.substring(value.indexOf('?') + 1) : '';
    if (!query) return {};

    return query.split('&').reduce((acc, pair) => {
      const [key, rawValue] = pair.split('=');
      if (key) acc[decodeURIComponent(key)] = decodeURIComponent(rawValue || '');
      return acc;
    }, {} as Record<string, string>);
  }

  getFieldVisitStatusLabel(status: string): string {
    switch (status) {
      case 'Dispatched': return 'تم الإرسال';
      case 'OnSite': return 'في الموقع';
      case 'Completed': return 'تم الانتهاء';
      case 'FailedAccess': return 'فشل الدخول';
      default: return status || 'غير محدد';
    }
  }
}
