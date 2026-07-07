import { Component, OnInit, ChangeDetectionStrategy, DestroyRef, inject, ChangeDetectorRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { AdminDocumentsService } from '../../../core/services/adminDocumentsService';
import { AdminDocument } from '../../../core/models/admin.models';
import { Loading } from '../../../shared/components/loading/loading';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { ActionModal } from '../../../shared/components/action-modal/action-modal';
import { Pagination } from '../../../shared/components/pagination/pagination';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-documents',
  imports: [CommonModule, FormsModule, Loading, EmptyState, ActionModal, Pagination],
  templateUrl: './admin-documents.html',
  styleUrl: './admin-documents.css'
})
export class AdminDocuments implements OnInit {
  documents: AdminDocument[] = [];
  isLoading = true;
  
  // Filters
  filterStatus: string = 'Pending';
  filterRole: string = '';
  filterType: string = '';
  filterUserId: string = '';
  page = 1;
  pageSize = 10;
  totalCount = 0;

  // Selected Document View
  selectedDoc: AdminDocument | null = null;

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
    private docService: AdminDocumentsService,
    private route: ActivatedRoute,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      if (params['role']) {
        this.filterRole = params['role'];
      }
      if (params['userId']) {
        this.filterUserId = params['userId'];
      }
      this.loadDocuments();
    });
  }

  loadDocuments() {
    this.isLoading = true;
    this.docService.getDocuments(
      this.page, 
      this.pageSize, 
      this.filterStatus || undefined, 
      this.filterType || undefined, 
      this.filterRole || undefined,
      this.filterUserId || undefined
    ).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (res: any) => {
        this.documents = (res.items || []).map((doc: any) => ({
          id: doc.documentId,
          userId: doc.user?.id,
          userName: doc.user?.displayName || 'Unknown',
          role: doc.user?.role || 'Unknown',
          documentType: doc.documentType,
          fileUrl: doc.fileUrl,
          status: doc.status,
          statusAr: doc.statusAr,
          uploadedAt: doc.uploadedAt,
          extractedText: doc.extractedText,
          confidenceScore: doc.confidenceScore,
          keyFieldsPresent: doc.keyFieldsPresent,
          failureReason: doc.failureReason,
          adminNote: doc.adminNote
        }));
        this.totalCount = res.totalCount || 0;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.toastService.error('فشل في تحميل المستندات');
        this.cdr.detectChanges();
      }
    });
  }

  onFilterChange() {
    this.page = 1;
    this.selectedDoc = null;
    this.loadDocuments();
  }

  onPageChange(newPage: number) {
    this.page = newPage;
    this.loadDocuments();
  }

  selectDocument(doc: AdminDocument) {
    this.selectedDoc = doc;
  }

  closeDocument() {
    this.selectedDoc = null;
  }

  openApproveModal() {
    if (!this.selectedDoc) return;
    this.pendingAction = 'approve';
    this.modalTitle = 'اعتماد المستند';
    this.modalDesc = `هل أنت متأكد من اعتماد مستند ${this.selectedDoc.documentType} الخاص بـ ${this.selectedDoc.userName}؟`;
    this.modalConfirmText = 'اعتماد';
    this.modalConfirmColor = 'success';
    this.requireNote = false;
    this.isModalOpen = true;
  }

  openRejectModal() {
    if (!this.selectedDoc) return;
    this.pendingAction = 'reject';
    this.modalTitle = 'رفض المستند';
    this.modalDesc = `سيتم رفض المستند وطلب إعادة الرفع من المستخدم. يرجى توضيح سبب الرفض.`;
    this.modalConfirmText = 'تأكيد الرفض';
    this.modalConfirmColor = 'danger';
    this.requireNote = true;
    this.isModalOpen = true;
  }

  handleModalConfirm(note?: string) {
    if (!this.selectedDoc || !this.pendingAction) return;

    const id = this.selectedDoc.id;
    const obs$ = this.pendingAction === 'approve'
      ? this.docService.approveDocument(id)
      : this.docService.rejectDocument(id, note || '');

    obs$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.toastService.success(this.pendingAction === 'approve' ? 'تم الاعتماد بنجاح' : 'تم الرفض بنجاح');
        this.closeDocument();
        this.loadDocuments();
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Action failed', err);
        this.toastService.error('حدث خطأ أثناء تنفيذ الإجراء');
        this.cdr.detectChanges();
      }
    });
  }
}
