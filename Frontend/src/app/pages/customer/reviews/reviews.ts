import { CommonModule } from '@angular/common'
import { ChangeDetectorRef, Component } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { finalize } from 'rxjs'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'
import {
  ReviewItem,
  ReviewTargetType,
  UserReviewsResponse
} from '../../../core/models/reviewModels'
import { ReviewsService } from '../../../core/services/reviewsService'

@Component({
  selector: 'app-reviews',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reviews.html',
  styleUrl: './reviews.css'
})
export class Reviews {
  targetType: ReviewTargetType = 'booking'
  bookingId = ''
  marketplaceOrderId = ''
  rating = 5
  comment = ''

  userIdToView = ''
  userReviews: ReviewItem[] = []
  reviewsSummary: UserReviewsResponse | null = null

  isSubmitting = false
  isLoadingReviews = false

  errorMessage = ''
  successMessage = ''

  ratingOptions = [5, 4, 3, 2, 1]

  constructor(
    private reviewsService: ReviewsService,
    private cdr: ChangeDetectorRef
  ) {}

  submitReview(): void {
    this.errorMessage = ''
    this.successMessage = ''

    if (this.targetType === 'booking' && !this.bookingId.trim()) {
      this.errorMessage = 'من فضلك اكتب رقم الحجز'
      this.cdr.detectChanges()
      return
    }

    if (this.targetType === 'marketplaceOrder' && !this.marketplaceOrderId.trim()) {
      this.errorMessage = 'من فضلك اكتب رقم طلب السوق'
      this.cdr.detectChanges()
      return
    }

    if (this.rating < 1 || this.rating > 5) {
      this.errorMessage = 'التقييم يجب أن يكون من 1 إلى 5'
      this.cdr.detectChanges()
      return
    }

    this.isSubmitting = true
    this.cdr.detectChanges()

    this.reviewsService
      .submitReview({
        bookingId: this.targetType === 'booking' ? this.bookingId.trim() : null,
        marketplaceOrderId:
          this.targetType === 'marketplaceOrder'
            ? this.marketplaceOrderId.trim()
            : null,
        rating: Number(this.rating),
        comment: this.comment.trim() || null
      })
      .pipe(
        finalize(() => {
          this.isSubmitting = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: response => {
          this.successMessage = response.message || 'تم إرسال التقييم بنجاح'
          this.resetSubmitForm()
          this.cdr.detectChanges()
        },
        error: error => {
          this.errorMessage = getErrorMessage(error, 'تعذر إرسال التقييم')
          this.cdr.detectChanges()
        }
      })
  }

  loadUserReviews(): void {
    this.errorMessage = ''
    this.successMessage = ''

    if (!this.userIdToView.trim()) {
      this.errorMessage = 'من فضلك اكتب UserId الخاص بالمزود أو البائع'
      this.cdr.detectChanges()
      return
    }

    this.isLoadingReviews = true
    this.cdr.detectChanges()

    this.reviewsService
      .getReviewsForUser(this.userIdToView.trim())
      .pipe(
        finalize(() => {
          this.isLoadingReviews = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: response => {
          this.reviewsSummary = response
          this.userReviews = response.items ?? []
          this.cdr.detectChanges()
        },
        error: error => {
          this.reviewsSummary = null
          this.userReviews = []
          this.errorMessage = getErrorMessage(error, 'تعذر تحميل التقييمات')
          this.cdr.detectChanges()
        }
      })
  }

  setTargetType(type: ReviewTargetType): void {
    this.targetType = type
    this.errorMessage = ''
    this.successMessage = ''
    this.cdr.detectChanges()
  }

  resetSubmitForm(): void {
    this.bookingId = ''
    this.marketplaceOrderId = ''
    this.rating = 5
    this.comment = ''
  }

  getStars(rating: number): string {
    return '★'.repeat(rating) + '☆'.repeat(5 - rating)
  }

  trackById(index: number, item: ReviewItem): string {
    return item.id
  }
}