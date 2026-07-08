import { CommonModule } from '@angular/common'
import { HttpClient, HttpParams } from '@angular/common/http'
import { Component, OnInit, ChangeDetectorRef } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { finalize, forkJoin } from 'rxjs'
import { API_BASE_URL } from '../../../core/constants/api.constants'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'

interface ServiceEarningsSummary {
  grossAmount: number
  platformCommission: number
  providerPayout: number
  heldAmount: number
  releasedAmount: number
  completedBookingsCount: number
}

interface MarketplaceEarningsSummary {
  grossSales: number
  platformCommission: number
  sellerPayout: number
  heldAmount: number
  releasedAmount: number
  completedOrdersCount: number
}

@Component({
  selector: 'app-earnings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './earnings.html',
  styleUrl: './earnings.css'
})
export class Earnings implements OnInit {
  from = ''
  to = ''
  serviceSummary: ServiceEarningsSummary | null = null
  marketplaceSummary: MarketplaceEarningsSummary | null = null
  isLoading = false
  errorMessage = ''

  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const now = new Date()
    const first = new Date(now.getFullYear(), 0, 1)
    this.from = this.toDateInput(first)
    this.to = this.toDateInput(now)
    this.load()
  }

  load(): void {
    this.isLoading = true
    this.errorMessage = ''
    this.cdr.detectChanges()

    const params = new HttpParams().set('from', this.from).set('to', this.to)

    forkJoin({
      service: this.http.get<ServiceEarningsSummary>(`${API_BASE_URL}/provider/earnings/service-summary`, { params }),
      marketplace: this.http.get<MarketplaceEarningsSummary>(`${API_BASE_URL}/provider/earnings/marketplace-summary`, { params })
    })
      .pipe(
        finalize(() => {
          this.isLoading = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: result => {
          this.serviceSummary = result.service
          this.marketplaceSummary = result.marketplace
          this.cdr.detectChanges()
        },
        error: error => {
          this.serviceSummary = null
          this.marketplaceSummary = null
          this.errorMessage = getErrorMessage(error, 'تعذر تحميل ملخص الأرباح')
          this.cdr.detectChanges()
        }
      })
  }

  money(value: number | null | undefined): string {
    return `${Number(value ?? 0).toFixed(2)} ج.م`
  }

  private toDateInput(date: Date): string {
    return date.toISOString().slice(0, 10)
  }
}
