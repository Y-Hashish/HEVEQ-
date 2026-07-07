import { CommonModule } from '@angular/common'
import { Component, DestroyRef, OnInit, inject, ChangeDetectorRef } from '@angular/core'
import { AdminPendingReview, AdminReviewsService } from '../../../core/services/adminReviewsService'
import { Loading } from '../../../shared/components/loading/loading'
import { EmptyState } from '../../../shared/components/empty-state/empty-state'
import { Pagination } from '../../../shared/components/pagination/pagination'
import { ToastService } from '../../../shared/components/toast/toast.service'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'

@Component({
  selector: 'app-admin-reviews',
  standalone: true,
  imports: [CommonModule, Loading, EmptyState, Pagination],
  templateUrl: './admin-reviews.html',
  styleUrl: './admin-reviews.css'
})
export class AdminReviews implements OnInit {
  reviews: AdminPendingReview[] = []
  isLoading = true
  page = 1
  pageSize = 10
  totalCount = 0
  actionId = ''

  private destroyRef = inject(DestroyRef)

  constructor(
    private reviewsService: AdminReviewsService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadReviews()
  }

  loadReviews(): void {
    this.isLoading = true
    this.reviewsService.getPending(this.page, this.pageSize)
      .pipe()
      .subscribe({
        next: res => {
          this.reviews = res.items || []
          this.totalCount = res.totalCount || 0
          this.isLoading = false
          this.cdr.detectChanges()
        },
        error: err => {
          this.isLoading = false
          this.toastService.error(getErrorMessage(err, 'فشل تحميل التقييمات المعلقة'))
          this.cdr.detectChanges()
        }
      })
  }

  onPageChange(page: number): void {
    this.page = page
    this.loadReviews()
  }

  approve(review: AdminPendingReview): void {
    this.actionId = review.id
    this.reviewsService.approve(review.id)
      .subscribe({
        next: res => {
          this.actionId = ''
          this.toastService.success(res.message || 'تم قبول التقييم')
          this.loadReviews()
        },
        error: err => {
          this.actionId = ''
          this.toastService.error(getErrorMessage(err, 'فشل قبول التقييم'))
          this.cdr.detectChanges()
        }
      })
  }

  reject(review: AdminPendingReview): void {
    this.actionId = review.id
    this.reviewsService.reject(review.id)
      .subscribe({
        next: res => {
          this.actionId = ''
          this.toastService.success(res.message || 'تم رفض التقييم')
          this.loadReviews()
        },
        error: err => {
          this.actionId = ''
          this.toastService.error(getErrorMessage(err, 'فشل رفض التقييم'))
          this.cdr.detectChanges()
        }
      })
  }

  getStars(rating: number): string {
    return '★'.repeat(rating || 0) + '☆'.repeat(Math.max(0, 5 - (rating || 0)))
  }

  getSourceLabel(sourceType: string): string {
    return sourceType === 'Booking' ? 'حجز خدمة' : 'طلب سوق'
  }
}
