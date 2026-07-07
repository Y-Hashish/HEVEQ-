import { CommonModule } from '@angular/common'
import { ChangeDetectorRef, Component, OnInit } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { ActivatedRoute, Router, RouterLink } from '@angular/router'
import { finalize } from 'rxjs'
import { DeliveryPreference } from '../../../core/models/marketplace-order.models'
import { MarketplaceListingDetails } from '../../../core/models/marketplace.models'
import { MarketplaceOrdersService } from '../../../core/services/marketplace-orders'
import { ConversationsService } from '../../../core/services/conversationsService'
import { MarketplaceService } from '../../../core/services/marketplace'
import { TokenStorage } from '../../../core/services/token-storage'

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './product-details.html',
  styleUrl: './product-details.css'
})
export class ProductDetails implements OnInit {
  listingId = ''
  listing: MarketplaceListingDetails | null = null

  isLoading = false
  errorMessage = ''
  notFound = false

  activePhotoIndex = 0

  isOwnListing = false

  // Buy-now flow state
  showBuyForm = false
  deliveryPreference: DeliveryPreference = 'Either'
  deliveryAddress = ''
  isPlacingOrder = false
  isStartingConversation = false
  orderError = ''
  chatError = ''
  orderResult: { orderNumber: string; message: string; statusAr: string } | null = null

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private marketplaceService: MarketplaceService,
    private ordersService: MarketplaceOrdersService,
    private conversationsService: ConversationsService,
    private tokenStorage: TokenStorage,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.listingId = this.route.snapshot.paramMap.get('id') ?? ''

    if (!this.listingId) {
      this.notFound = true
      return
    }

    this.loadListing()
  }

  loadListing(): void {
    this.isLoading = true
    this.errorMessage = ''
    this.notFound = false
    this.cdr.detectChanges()

    this.marketplaceService
      .getById(this.listingId)
      .pipe(
        finalize(() => {
          this.isLoading = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: listing => {
          this.listing = listing
          this.activePhotoIndex = 0

          const currentUserId = this.tokenStorage.getCurrentUser()?.id
          this.isOwnListing = !!currentUserId && currentUserId === listing.seller.id
        },
        error: error => {
          this.listing = null

          if (error.status === 404) {
            this.notFound = true
          } else {
            this.errorMessage =
              error.error?.message || error.error?.Message || 'تعذر تحميل بيانات السلعة، برجاء المحاولة لاحقًا'
          }
        }
      })
  }

  get sortedPhotos() {
    return [...(this.listing?.photos ?? [])].sort((a, b) => a.displayOrder - b.displayOrder)
  }

  get activePhotoUrl(): string | null {
    const photos = this.sortedPhotos
    return photos.length > 0 ? photos[this.activePhotoIndex]?.photoUrl ?? photos[0].photoUrl : null
  }

  selectPhoto(index: number): void {
    this.activePhotoIndex = index
  }

  ratingStars(rating: number | null): boolean[] {
    const rounded = Math.round(rating ?? 0)
    return Array.from({ length: 5 }, (_, i) => i < rounded)
  }

  startBuyNow(): void {
    if (!this.tokenStorage.isLoggedIn()) {
      this.router.navigate(['/login'], { queryParams: { returnUrl: `/product-details/${this.listingId}` } })
      return
    }

    this.orderError = ''
    this.orderResult = null
    this.showBuyForm = true
  }

  cancelBuyNow(): void {
    this.showBuyForm = false
    this.orderError = ''
  }

  confirmPurchase(): void {
    if (!this.listing) {
      return
    }

    this.isPlacingOrder = true
    this.orderError = ''
    this.cdr.detectChanges()

    this.ordersService
      .createOrder({
        listingId: this.listing.id,
        deliveryAddress: this.deliveryAddress.trim() || undefined,
        deliveryPreference: this.deliveryPreference
      })
      .pipe(
        finalize(() => {
          this.isPlacingOrder = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: response => {
          this.orderResult = {
            orderNumber: response.orderNumber,
            message: response.message,
            statusAr: response.statusAr
          }
          this.showBuyForm = false

          if (this.listing) {
            this.listing.canBuyNow = false
          }

          this.router.navigate(['/marketplace-orders', response.id, 'payment'])
        },
        error: error => {
          this.orderError =
            error.error?.message || error.error?.Message || 'تعذر إتمام عملية الشراء، برجاء المحاولة لاحقًا'
        }
      })
  }


  startSellerChat(): void {
    if (!this.listing) {
      return
    }

    if (!this.tokenStorage.isLoggedIn()) {
      this.router.navigate(['/login'], { queryParams: { returnUrl: `/product-details/${this.listingId}` } })
      return
    }

    this.isStartingConversation = true
    this.chatError = ''
    this.cdr.detectChanges()

    this.conversationsService
      .startConversation({ contextType: 'MarketplaceListing', referenceId: this.listing.id })
      .pipe(
        finalize(() => {
          this.isStartingConversation = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: response => {
          this.router.navigate(['/messages'], { queryParams: { conversationId: response.id } })
        },
        error: error => {
          this.chatError = error.error?.message || error.error?.Message || 'تعذر بدء المحادثة مع البائع'
        }
      })
  }

  get riskLevelBadgeClass(): string {
    const level = (this.listing?.managementInfo?.aiRiskLevel ?? '').toLowerCase()

    if (level.includes('high')) {
      return 'badge-danger'
    }

    if (level.includes('medium')) {
      return 'badge-warning'
    }

    if (level.includes('low')) {
      return 'badge-success'
    }

    return 'badge-info'
  }
}