import { CommonModule } from '@angular/common'
import { HttpClient } from '@angular/common/http'
import { Component, OnInit } from '@angular/core'
import { RouterLink } from '@angular/router'
import { finalize } from 'rxjs'
import { API_BASE_URL } from '../../../core/constants/api.constants'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'

interface CustomerDashboardSummary {
  activeBookings: number
  pendingBookings: number
  completedBookings: number
  openDisputes: number
  marketplacePurchases: number
  unreadNotifications: number
  trustScore: number
  requiresAdditionalVerification: boolean
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {
  summary: CustomerDashboardSummary | null = null
  isLoading = false
  errorMessage = ''

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.load()
  }

  load(): void {
    this.isLoading = true
    this.errorMessage = ''

    this.http.get<CustomerDashboardSummary>(`${API_BASE_URL}/customer/dashboard/summary`)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: summary => (this.summary = summary),
        error: error => {
          this.summary = null
          this.errorMessage = getErrorMessage(error, 'تعذر تحميل لوحة التحكم')
        }
      })
  }
}
