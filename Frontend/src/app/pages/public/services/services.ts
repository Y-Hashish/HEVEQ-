import { CommonModule } from '@angular/common'
import { Component, OnInit } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { Router } from '@angular/router'
import { HttpErrorResponse } from '@angular/common/http'
import { ServiceListings } from '../../../core/services/service-listings'
import { CategoriesService } from '../../../core/services/categories'
import { Toast } from '../../../core/services/toast'
import { extractErrorMessage } from '../../../core/models/api-error.models'
import { Category } from '../../../core/models/category.models'
import { PublicServiceListing, PublicServiceListingFilters } from '../../../core/models/service-listing.models'

@Component({
  selector: 'app-services',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './services.html',
  styleUrl: './services.css'
})
export class Services implements OnInit {
  listings: PublicServiceListing[] = []
  categories: Category[] = []
  isLoading = false
  totalCount = 0

  filters: PublicServiceListingFilters = {
    search: '',
    categoryId: undefined,
    governorate: '',
    minRate: undefined,
    maxRate: undefined,
    page: 1,
    pageSize: 9
  }

  constructor(
    private serviceListings: ServiceListings,
    private categoriesService: CategoriesService,
    private toast: Toast,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.categoriesService.getCategories('Service').subscribe({
      next: categories => (this.categories = categories),
      error: (err: HttpErrorResponse) => this.toast.error(extractErrorMessage(err))
    })

    this.loadListings()
  }

  loadListings(): void {
    this.isLoading = true

    this.serviceListings.getPublicList(this.filters).subscribe({
      next: result => {
        this.listings = result.items
        this.totalCount = result.totalCount
        this.isLoading = false
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading = false
        this.toast.error(extractErrorMessage(err))
      }
    })
  }

  search(): void {
    this.filters.page = 1
    this.loadListings()
  }

  nextPage(): void {
    const page = this.filters.page ?? 1
    const pageSize = this.filters.pageSize ?? 9
    if (page * pageSize >= this.totalCount) return

    this.filters.page = page + 1
    this.loadListings()
  }

  previousPage(): void {
    const page = this.filters.page ?? 1
    if (page <= 1) return

    this.filters.page = page - 1
    this.loadListings()
  }

  viewListing(id: string): void {
    this.router.navigate(['/service-details', id])
  }
}
