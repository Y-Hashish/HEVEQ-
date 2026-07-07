import { CommonModule } from '@angular/common'
import { Component, OnInit } from '@angular/core'
import { ActivatedRoute, Router } from '@angular/router'
import { HttpErrorResponse } from '@angular/common/http'
import { ServiceListings } from '../../../core/services/service-listings'
import { ConversationsService } from '../../../core/services/conversationsService'
import { TokenStorage } from '../../../core/services/token-storage'
import { Toast } from '../../../core/services/toast'
import { ReviewsService } from '../../../core/services/reviewsService'
import { ReviewItem } from '../../../core/models/reviewModels'
import { extractErrorMessage } from '../../../core/models/api-error.models'
import { PublicServiceListingDetail } from '../../../core/models/service-listing.models'

@Component({
  selector: 'app-service-details',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './service-details.html',
  styleUrl: './service-details.css'
})
export class ServiceDetails implements OnInit {
  listing: PublicServiceListingDetail | null = null
  isLoading = false
  notFound = false
  isStartingConversation = false
  reviews: ReviewItem[] = []
  reviewsAverage = 0
  reviewsTotal = 0
  reviewsLoading = false

  readonly dayNames = ['الأحد', 'الإثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت']

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private serviceListings: ServiceListings,
    private conversationsService: ConversationsService,
    private reviewsService: ReviewsService,
    private tokenStorage: TokenStorage,
    private toast: Toast
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')
    if (!id) {
      this.notFound = true
      return
    }

    this.isLoading = true

    this.serviceListings.getPublicById(id).subscribe({
      next: listing => {
        this.listing = listing
        this.isLoading = false
        this.loadServiceReviews(listing.id)
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading = false
        // The backend collapses "doesn't exist" and "not Approved yet" into
        // the same 404 on purpose, to avoid leaking unpublished listing
        // existence to anonymous callers — surfaced here identically.
        this.notFound = err.status === 404
        if (!this.notFound) {
          this.toast.error(extractErrorMessage(err))
        }
      }
    })
  }

  requestBooking(): void {
    if (!this.listing?.id) {
      this.toast.error('بيانات الخدمة غير مكتملة')
      return
    }

    this.router.navigate(['/create-booking', this.listing.id])
  }


  startProviderChat(): void {
    if (!this.listing?.id) {
      this.toast.error('بيانات الخدمة غير مكتملة')
      return
    }

    if (!this.tokenStorage.isLoggedIn()) {
      this.router.navigate(['/login'], { queryParams: { returnUrl: `/service-details/${this.listing.id}` } })
      return
    }

    this.isStartingConversation = true
    this.conversationsService.startConversation({ contextType: 'ServiceListing', referenceId: this.listing.id }).subscribe({
      next: response => {
        this.isStartingConversation = false
        this.router.navigate(['/messages'], { queryParams: { conversationId: response.id } })
      },
      error: (err: HttpErrorResponse) => {
        this.isStartingConversation = false
        this.toast.error(extractErrorMessage(err))
      }
    })
  }


  loadServiceReviews(serviceListingId: string): void {
    this.reviewsLoading = true
    this.reviewsService.getReviewsForServiceListing(serviceListingId).subscribe({
      next: res => {
        this.reviews = res.items ?? []
        this.reviewsAverage = res.averageRating ?? 0
        this.reviewsTotal = res.totalCount ?? 0
        this.reviewsLoading = false
      },
      error: () => {
        this.reviews = []
        this.reviewsAverage = 0
        this.reviewsTotal = 0
        this.reviewsLoading = false
      }
    })
  }

  ratingStars(rating: number | null): boolean[] {
    const rounded = Math.round(rating ?? 0)
    return Array.from({ length: 5 }, (_, i) => i < rounded)
  }

  // Stopgap only — equipmentCondition should really carry its own Arabic
  // label from the backend, same as status/statusAr (Task 2 general rule
  // #6: every enum returns as a string or with its Arabic pair). Flagging
  // for a small PublicServiceListingDetailDto addition; mapping here so the
  // page isn't broken in the meantime.
  conditionLabel(value: number | null): string {
    switch (value) {
      case 0:
        return 'ممتاز'
      case 1:
        return 'جيد'
      case 2:
        return 'متوسط'
      default:
        return '—'
    }
  }
}
