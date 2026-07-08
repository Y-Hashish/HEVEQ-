import { CommonModule } from '@angular/common'
import { Component, OnInit, ChangeDetectorRef } from '@angular/core'
import { HttpErrorResponse } from '@angular/common/http'
import { ProviderDashboardApi } from '../../../core/services/provider-dashboard-api'
import { Toast } from '../../../core/services/toast'
import { extractErrorMessage } from '../../../core/models/api-error.models'
import { ProviderDashboardSummary } from '../../../core/models/provider-dashboard.models'

interface Widget {
  icon: string
  label: string
  value: string
  background: string
}

@Component({
  selector: 'app-provider-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './provider-dashboard.html',
  styleUrl: './provider-dashboard.css'
})
export class ProviderDashboard implements OnInit {
  summary: ProviderDashboardSummary | null = null
  isLoading = false
  widgets: Widget[] = []

  constructor(
    private dashboardApi: ProviderDashboardApi,
    private toast: Toast,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.isLoading = true
    this.cdr.detectChanges()

    this.dashboardApi.getSummary().subscribe({
      next: summary => {
        this.summary = summary
        this.widgets = this.buildWidgets(summary)
        this.isLoading = false
        this.cdr.detectChanges()
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading = false
        this.toast.error(extractErrorMessage(err))
        this.cdr.detectChanges()
      }
    })
  }

  private buildWidgets(summary: ProviderDashboardSummary): Widget[] {
    return [
      { icon: 'fa-solid fa-list-check', label: 'إجمالي القوائم', value: `${summary.totalServiceListings}`, background: '#e8f0fe' },
      { icon: 'fa-solid fa-circle-check', label: 'قوائم متاحة', value: `${summary.approvedServiceListings}`, background: '#e6f7ee' },
      { icon: 'fa-solid fa-hourglass-half', label: 'قيد المراجعة', value: `${summary.pendingServiceListings}`, background: '#fff4e5' },
      { icon: 'fa-solid fa-envelope-open-text', label: 'طلبات حجز معلقة', value: `${summary.pendingBookingRequests}`, background: '#fdecea' },
      { icon: 'fa-solid fa-person-digging', label: 'أعمال نشطة', value: `${summary.activeJobs}`, background: '#e8f0fe' },
      { icon: 'fa-solid fa-flag-checkered', label: 'أعمال مكتملة', value: `${summary.completedJobs}`, background: '#e6f7ee' },
      { icon: 'fa-solid fa-star', label: 'التقييم المتوسط', value: summary.averageRating.toFixed(1), background: '#fff4e5' },
      { icon: 'fa-solid fa-chart-line', label: 'معدل الاستجابة', value: `${Math.round(summary.responseRate * 100)}%`, background: '#e8f0fe' },
      { icon: 'fa-solid fa-shield-halved', label: `الثقة (${summary.trustLevel})`, value: summary.trustScore.toFixed(0), background: '#e6f7ee' },
      { icon: 'fa-solid fa-money-bill-wave', label: 'أرباح هذا الشهر', value: `${summary.earningsThisMonth.toLocaleString()} ج.م`, background: '#fff4e5' }
    ]
  }
}
