import { Component, OnInit, ChangeDetectionStrategy, DestroyRef, inject, ChangeDetectorRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { AdminDisputesService } from '../../../core/services/adminDisputesService';
import { AdminDispute, AdminDisputeDetails } from '../../../core/models/admin.models';
import { Loading } from '../../../shared/components/loading/loading';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { Pagination } from '../../../shared/components/pagination/pagination';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-disputes',
  imports: [CommonModule, FormsModule, Loading, EmptyState, Pagination],
  templateUrl: './admin-disputes.html',
  styleUrl: './admin-disputes.css'
})
export class AdminDisputes implements OnInit {
  disputes: AdminDispute[] = [];
  isLoading = true;
  
  // Filters
  filterStatus: string = 'Open';
  page = 1;
  pageSize = 10;
  totalCount = 0;

  // Selected Dispute
  selectedDispute: AdminDisputeDetails | null = null;
  isDetailsLoading = false;

  // Resolution Form
  resolutionType: 'RefundCustomer' | 'ReleaseToProvider' | 'PartialRefund' | '' = '';
  refundAmount: number | null = null;
  adminNote: string = '';
  isResolving = false;

  private destroyRef = inject(DestroyRef);

  constructor(
    private disputesService: AdminDisputesService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadDisputes();
  }

  loadDisputes() {
    this.isLoading = true;
    this.disputesService.getDisputes(this.page, this.pageSize, this.filterStatus || undefined)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
      next: (res) => {
        let items = res.items || [];
        
        // Map disputes to structure expected by component template
        items = items.map(d => ({
          ...d,
          currency: 'ج.م',
          reason: (d as any).type === 'Booking' ? 'نزاع على إتمام الحجز' : 'نزاع على طلب المتجر'
        }));

        this.disputes = items;
        this.totalCount = res.totalCount || 0;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.toastService.error('فشل في تحميل النزاعات');
        this.cdr.detectChanges();
      }
    });
  }

  onFilterChange() {
    this.page = 1;
    this.selectedDispute = null;
    this.loadDisputes();
  }

  onPageChange(newPage: number) {
    this.page = newPage;
    this.loadDisputes();
  }

  viewDispute(id: string) {
    const dispute = this.disputes.find(d => d.id === id || (d as any).Id === id);
    if (!dispute) return;

    this.isDetailsLoading = true;
    
    // Construct selectedDispute in memory using list values
    this.selectedDispute = {
      id: dispute.id,
      bookingId: (dispute as any).referenceNumber || dispute.id,
      customerName: dispute.customerName,
      providerName: dispute.providerName,
      status: dispute.status,
      amount: dispute.amount,
      currency: dispute.currency || 'ج.م',
      createdAt: dispute.createdAt,
      reason: dispute.reason || ((dispute as any).type === 'Booking' ? 'نزاع على إتمام الحجز' : 'نزاع على طلب المتجر'),
      description: (dispute as any).type === 'Booking' 
        ? 'لقد قام العميل بفتح نزاع بخصوص هذا الحجز. يرجى مراجعة تذكرة الدعم الفني المرتبطة لمشاهدة المرفقات والمراسلات بين الأطراف.' 
        : 'لقد تم فتح نزاع على هذا الطلب من المتجر. يرجى التنسيق مع الدعم الفني لمراجعة تفاصيل التذكرة وحلها.',
      serviceTitle: (dispute as any).type === 'Booking' ? 'خدمة حجز معدة ثقيلة' : 'طلب شراء معدات/قطع غيار'
    };

    // Store dispute type for resolving
    (this as any).selectedDisputeType = (dispute as any).type || 'Booking';

    this.isDetailsLoading = false;
    this.resolutionType = '';
    this.refundAmount = null;
    this.adminNote = '';
    this.cdr.detectChanges();
  }

  closeDispute() {
    this.selectedDispute = null;
  }

  resolveDispute() {
    if (!this.selectedDispute || !this.resolutionType || !this.adminNote) return;
    
    // Validation
    if (this.resolutionType === 'PartialRefund') {
      if (!this.refundAmount || this.refundAmount <= 0 || this.refundAmount >= this.selectedDispute.amount) {
        this.toastService.error('يرجى إدخال مبلغ استرداد جزئي صحيح.');
        return;
      }
    }

    const id = this.selectedDispute.id;
    const type = (this as any).selectedDisputeType || 'Booking';
    const note = this.adminNote;

    let obs$;

    if (type === 'Booking') {
      if (this.resolutionType === 'RefundCustomer') {
        obs$ = this.disputesService.refundBookingToCustomer(id, note);
      } else if (this.resolutionType === 'ReleaseToProvider') {
        obs$ = this.disputesService.releaseBookingToProvider(id, note);
      } else { // PartialRefund
        const customerAmt = this.refundAmount!;
        const providerAmt = this.selectedDispute.amount - customerAmt;
        obs$ = this.disputesService.partialSettleBooking(id, customerAmt, providerAmt, note);
      }
    } else { // MarketplaceOrder
      if (this.resolutionType === 'RefundCustomer') {
        obs$ = this.disputesService.refundMarketplaceToBuyer(id, note);
      } else if (this.resolutionType === 'ReleaseToProvider') {
        obs$ = this.disputesService.releaseMarketplaceToSeller(id, note);
      } else {
        this.toastService.error('الاسترداد الجزئي غير مدعوم لطلبات المتجر حالياً.');
        return;
      }
    }

    this.isResolving = true;
    obs$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isResolving = false;
        this.selectedDispute!.status = 'Resolved';
        this.toastService.success('تم حل النزاع بنجاح');
        this.loadDisputes(); // refresh list
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isResolving = false;
        console.error('Resolve dispute failed', err);
        this.toastService.error('فشل في حل النزاع. حاول مرة أخرى.');
        this.cdr.detectChanges();
      }
    });
  }
}
