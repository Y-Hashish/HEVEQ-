import { Component, OnInit, DestroyRef, inject, ChangeDetectorRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { AdminListingReviewService } from '../../../core/services/adminListingReviewService';
import { ListingReviewDetails, PendingListing } from '../../../core/models/admin.models';
import { Loading } from '../../../shared/components/loading/loading';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { ActionModal } from '../../../shared/components/action-modal/action-modal';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ActivatedRoute } from '@angular/router';
import { extractErrorMessage } from '../../../core/models/api-error.models';

@Component({
  selector: 'app-admin-listing-review',
  imports: [CommonModule, Loading, EmptyState, ActionModal],
  templateUrl: './admin-listing-review.html',
  styleUrl: './admin-listing-review.css'
})
export class AdminListingReview implements OnInit {
  activeTab: 'services' | 'marketplace' = 'services';
  
  pendingListings: PendingListing[] = [];
  isLoading = true;
  isLoadingMore = false;
  page = 1;
  pageSize = 10;
  totalCount = 0;

  // Details View
  selectedDetails: ListingReviewDetails | null = null;
  isDetailsLoading = false;

  // Modal
  isModalOpen = false;
  modalTitle = '';
  modalDesc = '';
  modalConfirmText = '';
  modalConfirmColor: 'primary' | 'danger' | 'success' = 'primary';
  requireNote = false;
  pendingAction: 'approve' | 'reject' | null = null;

  private destroyRef = inject(DestroyRef);

  constructor(
    private listingService: AdminListingReviewService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    const type = this.route.snapshot.queryParamMap.get('type');
    const id = this.route.snapshot.queryParamMap.get('id');

    if (type === 'marketplace') {
      this.activeTab = 'marketplace';
    } else {
      this.activeTab = 'services';
    }

    this.loadListings(id || undefined);
  }

  switchTab(tab: 'services' | 'marketplace') {
    if (this.activeTab === tab) return;
    this.activeTab = tab;
    this.selectedDetails = null; // Reset details view
    this.loadListings();
  }

  loadListings(openId?: string) {
    this.isLoading = true;
    this.pendingListings = [];
    this.page = 1;
    
    const obs$ = this.activeTab === 'services' 
      ? this.listingService.getPendingServiceListings(this.page, this.pageSize)
      : this.listingService.getPendingMarketplaceListings(this.page, this.pageSize);

    obs$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        this.pendingListings = res.items || [];
        this.totalCount = res.totalCount || 0;
        this.isLoading = false;
        if (openId) {
          this.viewDetails(openId);
        } else if (this.pendingListings.length > 0) {
          this.viewDetails(this.pendingListings[0].id);
        }
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.toastService.error('فشل في تحميل الإدراجات');
        this.cdr.detectChanges();
      }
    });
  }

  loadMore() {
    if (this.isLoading || this.isLoadingMore || this.pendingListings.length >= this.totalCount) return;
    this.isLoadingMore = true;
    this.page++;
    
    const obs$ = this.activeTab === 'services' 
      ? this.listingService.getPendingServiceListings(this.page, this.pageSize)
      : this.listingService.getPendingMarketplaceListings(this.page, this.pageSize);

    obs$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        const newItems = res.items || [];
        this.pendingListings = [...this.pendingListings, ...newItems];
        this.totalCount = res.totalCount || 0;
        this.isLoadingMore = false;
        
        // Auto-select first item if details are empty
        if (!this.selectedDetails && !this.isDetailsLoading && this.pendingListings.length > 0) {
          this.viewDetails(this.pendingListings[0].id);
        }
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoadingMore = false;
        this.toastService.error('فشل في تحميل المزيد من الإدراجات');
        this.cdr.detectChanges();
      }
    });
  }

  onScroll(event: Event) {
    const element = event.target as HTMLElement;
    const threshold = 100; // Trigger load when within 100px of bottom
    const atBottom = element.scrollHeight - element.scrollTop <= element.clientHeight + threshold;
    if (atBottom) {
      this.loadMore();
    }
  }

  viewDetails(id: string) {
    this.isDetailsLoading = true;
    const obs$ = this.activeTab === 'services'
      ? this.listingService.getServiceListingDetails(id)
      : this.listingService.getMarketplaceListingDetails(id);

    obs$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        this.selectedDetails = res;
        this.isDetailsLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isDetailsLoading = false;
        this.toastService.error('فشل في تحميل التفاصيل');
        this.cdr.detectChanges();
      }
    });
  }

  closeDetails() {
    this.selectedDetails = null;
  }

  openApproveModal() {
    if (!this.selectedDetails) return;
    this.pendingAction = 'approve';
    this.modalTitle = 'تأكيد الموافقة';
    this.modalDesc = `هل أنت متأكد من الموافقة على ${this.selectedDetails.title} ونشره في المنصة؟`;
    this.modalConfirmText = 'نعم، موافقة';
    this.modalConfirmColor = 'success';
    this.requireNote = false;
    this.isModalOpen = true;
  }

  openRejectModal() {
    if (!this.selectedDetails) return;
    this.pendingAction = 'reject';
    this.modalTitle = 'رفض الإدراج';
    this.modalDesc = `سيتم رفض ${this.selectedDetails.title} ولن يظهر في المنصة. يرجى توضيح السبب.`;
    this.modalConfirmText = 'تأكيد الرفض';
    this.modalConfirmColor = 'danger';
    this.requireNote = true;
    this.isModalOpen = true;
  }

  formatAiText(value: string | null | undefined): string[] {
    if (!value) return []

    return value
      .replace(/[•]+/g, '\n• ')
      .split(/\r?\n|(?=\d+[\)\-.])|(?=•)/)
      .map(part => part.trim())
      .filter(Boolean)
  }

  handleModalConfirm(note?: string) {
    if (!this.selectedDetails || !this.pendingAction) return;

    const id = this.selectedDetails.id;
    let obs$;

    if (this.pendingAction === 'approve') {
      obs$ = this.activeTab === 'services' 
        ? this.listingService.approveServiceListing(id)
        : this.listingService.approveMarketplaceListing(id);
    } else {
      obs$ = this.activeTab === 'services'
        ? this.listingService.rejectServiceListing(id, note || '')
        : this.listingService.rejectMarketplaceListing(id, note || '');
    }

    obs$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.toastService.success(this.pendingAction === 'approve' ? 'تمت الموافقة بنجاح' : 'تم الرفض بنجاح');
        this.closeDetails();
        this.loadListings(); // Refresh list
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Action failed', err);
        this.toastService.error(extractErrorMessage(err) || 'حدث خطأ أثناء تنفيذ الإجراء');
        this.cdr.detectChanges();
      }
    });
  }
}
