import { Component, OnInit, ChangeDetectionStrategy, DestroyRef, inject, ChangeDetectorRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AdminDashboardService } from '../../../core/services/adminDashboardService';
import { DashboardSummary, PendingAction } from '../../../core/models/admin.models';
import { Loading } from '../../../shared/components/loading/loading';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { RealtimeService } from '../../../core/services/realtimeService';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-admin-dashboard',
  imports: [CommonModule, RouterModule, Loading, EmptyState],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboard implements OnInit {
  summary: DashboardSummary | null = null;
  pendingActions: PendingAction[] = [];
  isLoadingSummary = true;
  isLoadingActions = true;
  summaryError = false;
  actionsError = false;
  
  private destroyRef = inject(DestroyRef);

  constructor(
    private dashboardService: AdminDashboardService,
    private realtimeService: RealtimeService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadData();
    this.setupRealtimeUpdates();
  }

  loadData() {
    this.loadSummary();
    this.loadPendingActions();
  }

  loadSummary() {
    this.isLoadingSummary = true;
    this.summaryError = false;
    this.dashboardService.getSummary()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
      next: (data) => {
        this.summary = data;
        this.isLoadingSummary = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.summaryError = true;
        this.isLoadingSummary = false;
        this.toastService.error('فشل في تحميل الإحصائيات');
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
      error: () => {
        this.actionsError = true;
        this.isLoadingActions = false;
        this.toastService.error('فشل في تحميل الإجراءات المعلقة');
        this.cdr.detectChanges();
      }
    });
  }
  
  setupRealtimeUpdates() {
    // If realtimeService exposes an observable for notifications, we could subscribe here
    // Currently acting as a placeholder for SignalR integration per the plan.
  }
}
