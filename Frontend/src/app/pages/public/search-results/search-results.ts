import { CommonModule } from '@angular/common'
import { Component, OnInit } from '@angular/core'
import { ActivatedRoute, RouterLink } from '@angular/router'
import { forkJoin, finalize } from 'rxjs'
import { MarketplaceListing } from '../../../core/models/marketplace.models'
import { PublicServiceListing } from '../../../core/models/service-listing.models'
import { MarketplaceService } from '../../../core/services/marketplace'
import { ServiceListings } from '../../../core/services/service-listings'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'

@Component({
  selector: 'app-search-results',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './search-results.html',
  styleUrl: './search-results.css'
})
export class SearchResults implements OnInit {
  query = ''
  services: PublicServiceListing[] = []
  marketplace: MarketplaceListing[] = []
  isLoading = false
  errorMessage = ''

  constructor(
    private route: ActivatedRoute,
    private serviceListings: ServiceListings,
    private marketplaceService: MarketplaceService
  ) {}

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      this.query = params.get('q') || params.get('search') || ''
      this.search()
    })
  }

  search(): void {
    this.isLoading = true
    this.errorMessage = ''

    forkJoin({
      services: this.serviceListings.getPublicList({ search: this.query, page: 1, pageSize: 12 }),
      marketplace: this.marketplaceService.getListings({ search: this.query, page: 1, pageSize: 12 })
    }).pipe(finalize(() => (this.isLoading = false))).subscribe({
      next: result => {
        this.services = result.services.items ?? []
        this.marketplace = result.marketplace.items ?? []
      },
      error: error => {
        this.errorMessage = getErrorMessage(error, 'تعذر تنفيذ البحث')
      }
    })
  }
}
