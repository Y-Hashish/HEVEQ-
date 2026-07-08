import { CommonModule } from '@angular/common'
import { Component, OnInit, ChangeDetectorRef } from '@angular/core'
import { Router } from '@angular/router'
import { HttpErrorResponse } from '@angular/common/http'
import { ServiceListings } from '../../../core/services/service-listings'
import { MarketplaceService } from '../../../core/services/marketplace'
import { Toast } from '../../../core/services/toast'
import { extractErrorMessage } from '../../../core/models/api-error.models'
import { ProviderServiceListing, ServiceListingStatus } from '../../../core/models/service-listing.models'
import { MarketplaceListing } from '../../../core/models/marketplace.models'

@Component({
  selector: 'app-equipment',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './equipment.html',
  styleUrl: './equipment.css'
})
export class Equipment implements OnInit {
  listings: ProviderServiceListing[] = []
  marketplaceListings: MarketplaceListing[] = []
  activeTab: 'services' | 'marketplace' = 'services'
  isLoading = false
  activeFilter: ServiceListingStatus | null = null

  readonly statusFilters: { label: string; value: ServiceListingStatus | null }[] = [
    { label: 'الكل', value: null },
    { label: 'مسودة', value: 'Draft' },
    { label: 'قيد المراجعة', value: 'PendingReview' },
    { label: 'متاح', value: 'Active' },
    { label: 'مرفوض', value: 'Rejected' },
    { label: 'غير نشط', value: 'Inactive' }
  ]

  constructor(
    private serviceListings: ServiceListings,
    private marketplaceService: MarketplaceService,
    private toast: Toast,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load()
  }

  load(): void {
    this.isLoading = true
    this.cdr.detectChanges()

    if (this.activeTab === 'services') {
      this.serviceListings.getMine(this.activeFilter ?? undefined).subscribe({
        next: result => {
          this.listings = this.activeFilter
             ? result.items.filter(l => l.status === this.activeFilter)
             : result.items
          this.isLoading = false
          this.cdr.detectChanges()
        },
        error: (err: HttpErrorResponse) => {
          this.isLoading = false
          this.toast.error(extractErrorMessage(err))
          this.cdr.detectChanges()
        }
      })
      return
    }

    this.marketplaceService.getMine().subscribe({
      next: result => {
        this.marketplaceListings = result.items ?? []
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

  setTab(tab: 'services' | 'marketplace'): void {
    this.activeTab = tab
    this.load()
  }

  setFilter(status: ServiceListingStatus | null): void {
    this.activeFilter = status
    this.load()
  }

  statusBadgeClass(status: ServiceListingStatus): string {
    switch (status) {
      case 'Active':
        return 'badge-success'
      case 'PendingReview':
        return 'badge-warning'
      case 'Rejected':
      case 'Suspended':
        return 'badge-danger'
      default:
        return 'badge-info'
    }
  }

  goToCreate(): void {
    this.router.navigate(['/create-listing'])
  }

  goToEdit(id: string): void {
    this.router.navigate(['/create-listing', id])
  }

  goToMarketplaceDetails(id: string): void {
    this.router.navigate(['/product-details', id])
  }

  goToEditMarketplace(id: string): void {
    this.router.navigate(['/create-listing'], { queryParams: { type: 'marketplace', marketplaceId: id } })
  }

  submitForReview(listing: ProviderServiceListing): void {
    this.serviceListings.submitForReview(listing.id).subscribe({
      next: result => {
        this.toast.success(result.message)
        this.load()
        this.cdr.detectChanges()
      },
      error: (err: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(err))
        this.cdr.detectChanges()
      }
    })
  }

  deleteListing(listing: ProviderServiceListing): void {
    const confirmed = window.confirm(`هل تريد حذف "${listing.title}"؟`)
    if (!confirmed) return

    this.serviceListings.delete(listing.id).subscribe({
      next: () => {
        this.toast.success('تم حذف المعدة بنجاح')
        this.load()
        this.cdr.detectChanges()
      },
      error: (err: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(err))
        this.cdr.detectChanges()
      }
    })
  }

  deleteMarketplaceListing(listing: MarketplaceListing): void {
    const confirmed = window.confirm(`هل تريد حذف "${listing.title}"؟`)
    if (!confirmed) return

    this.marketplaceService.deleteListing(listing.id).subscribe({
      next: () => {
        this.toast.success('تم حذف الإعلان بنجاح')
        this.load()
        this.cdr.detectChanges()
      },
      error: (err: HttpErrorResponse) => {
        this.toast.error(extractErrorMessage(err))
        this.cdr.detectChanges()
      }
    })
  }
}