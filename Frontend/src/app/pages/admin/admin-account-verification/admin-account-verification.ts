import { Component, OnInit, ChangeDetectionStrategy, DestroyRef, inject, ChangeDetectorRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AdminAccountVerificationService } from '../../../core/services/adminAccountVerificationService';
import { AccountVerification } from '../../../core/models/admin.models';
import { Loading } from '../../../shared/components/loading/loading';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-admin-account-verification',
  imports: [CommonModule, RouterModule, Loading, EmptyState],
  templateUrl: './admin-account-verification.html',
  styleUrl: './admin-account-verification.css'
})
export class AdminAccountVerification implements OnInit {
  verifications: AccountVerification[] = [];
  isLoading = true;

  private destroyRef = inject(DestroyRef);

  constructor(
    private verificationService: AdminAccountVerificationService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadVerifications();
  }

  loadVerifications() {
    this.isLoading = true;
    this.verificationService.getPendingVerifications()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
      next: (res) => {
        this.verifications = (res.items || []).map((doc: any) => ({
          userId: doc.user?.id,
          userName: doc.user?.displayName,
          email: doc.user?.email || '',
          role: doc.user?.role,
          verificationStatus: doc.status,
          submittedAt: doc.uploadedAt,
          documentIds: [doc.documentId]
        }));
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.toastService.error('فشل في تحميل طلبات التوثيق');
        this.cdr.detectChanges();
      }
    });
  }
}
