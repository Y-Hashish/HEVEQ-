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
    
    const obs$ = this.activeTab === 'services' 
      ? this.listingService.getPendingServiceListings(1, 50)
      : this.listingService.getPendingMarketplaceListings(1, 50);

    obs$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res) => {
        this.pendingListings = res.items || [];
        this.isLoading = false;
        if (openId) {
          this.viewDetails(openId);
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
