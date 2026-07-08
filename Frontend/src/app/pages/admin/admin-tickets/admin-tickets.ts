import { Component, OnInit, ViewChild, ElementRef, ChangeDetectionStrategy, DestroyRef, inject, ChangeDetectorRef, HostListener } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { AdminTicketsService } from '../../../core/services/adminTicketsService';
import { AdminTicket, AdminTicketDetails } from '../../../core/models/admin.models';
import { Loading } from '../../../shared/components/loading/loading';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { ActionModal } from '../../../shared/components/action-modal/action-modal';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { FormsModule } from '@angular/forms';
import { TokenStorage } from '../../../core/services/token-storage';
import { ActivatedRoute } from '@angular/router';

import { AdminUsersService } from '../../../core/services/adminUsersService';

@Component({
  selector: 'app-admin-tickets',
  imports: [CommonModule, FormsModule, Loading, EmptyState, ActionModal],
  templateUrl: './admin-tickets.html',
  styleUrl: './admin-tickets.css'
})
export class AdminTickets implements OnInit {
  tickets: AdminTicket[] = [];
  isLoading = true;
  
  // Filters
  filterStatus: string = 'Open'; // Default to Open/InProgress
  filterPriority: string = '';
  page = 1;
  pageSize = 15;
  totalCount = 0;
  isLoadingMore = false;

  // Selected Ticket View
  selectedTicket: AdminTicketDetails | null = null;
  isDetailsLoading = false;
  
  // Chat Reply
  replyContent = '';
  isReplying = false;

  // Dropdown overlay state
  isDropdownOpen = false;

  // Dispute decisions variables
  decisionType = '';
  decisionNote = '';
  customerAmount = 0;
  providerAmount = 0;
  selectedEmployeeId = '';
  availableEmployees: { id: string, displayName: string, isAvailable: boolean }[] = [];
  isDisputeModalOpen = false;
  isDocsModalOpen = false;
  isAiSummaryModalOpen = false;

  // Takeover modal state
  isTakeoverModal = false;

  // Current user details
  currentUserId = '';
  isAdminUser = false;

  // Modal
  isModalOpen = false;
  modalTitle = '';
  modalDesc = '';
  modalConfirmText = '';
  modalConfirmColor: 'primary' | 'danger' | 'success' = 'primary';
  requireNote = false;

  @ViewChild('chatContainer') private chatContainer!: ElementRef;
  private destroyRef = inject(DestroyRef);

  constructor(
    private ticketsService: AdminTicketsService,
    private usersService: AdminUsersService,
    private tokenStorage: TokenStorage,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute
  ) {}

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    this.isDropdownOpen = false;
  }

  toggleDropdown(event?: Event) {
    if (event) {
      event.stopPropagation();
    }
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  closeDropdown() {
    this.isDropdownOpen = false;
  }

  ngOnInit() {
    const user = this.tokenStorage.getCurrentUser();
    this.currentUserId = user?.id || '';
    const role = this.tokenStorage.getRole();
    this.isAdminUser = role === 'admin';
    this.loadTickets();
  }

  loadTickets() {
    this.isLoading = true;
    this.page = 1;
    this.cdr.detectChanges();
    this.ticketsService.getTickets(this.page, this.pageSize, this.filterStatus || undefined, this.filterPriority || undefined)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
      next: (res) => {
        this.tickets = res.items || [];
        this.totalCount = res.totalCount || 0;
        this.isLoading = false;
        const targetId = this.route.snapshot.queryParamMap.get('ticketId');
        if (targetId) {
          if (!this.selectedTicket) {
            this.viewTicket(targetId);
          }
        } else if (this.tickets.length > 0 && !this.selectedTicket) {
          this.viewTicket(this.tickets[0].id);
        }
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.toastService.error('فشل في تحميل التذاكر');
        this.cdr.detectChanges();
      }
    });
  }

  onFilterChange() {
    this.page = 1;
    this.selectedTicket = null;
    this.loadTickets();
  }

  onListScroll(event: Event) {
    const element = event.target as HTMLElement;
    const atBottom = element.scrollHeight - element.scrollTop <= element.clientHeight + 100;
    
    if (atBottom && !this.isLoadingMore && this.tickets.length < this.totalCount) {
      this.loadMoreTickets();
    }
  }

  loadMoreTickets() {
    this.isLoadingMore = true;
    this.page++;
    this.cdr.detectChanges();

    this.ticketsService.getTickets(this.page, this.pageSize, this.filterStatus || undefined, this.filterPriority || undefined)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.tickets = [...this.tickets, ...(res.items || [])];
          this.isLoadingMore = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.isLoadingMore = false;
          this.page--;
          this.toastService.error('فشل في تحميل المزيد من التذاكر');
          this.cdr.detectChanges();
        }
      });
  }

  viewTicket(id: string) {
    this.isDetailsLoading = true;
    this.decisionType = '';
    this.decisionNote = '';
    this.customerAmount = 0;
    this.providerAmount = 0;
    this.selectedEmployeeId = '';
    this.cdr.detectChanges();

    this.ticketsService.getTicketDetails(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
      next: (res) => {
        this.selectedTicket = res;
        this.isDetailsLoading = false;
        this.scrollToBottom();
        this.cdr.detectChanges();

        if (res.availableDisputeDecisions?.includes('SendFieldVerification')) {
          this.loadEmployees();
        }
      },
      error: () => {
        this.isDetailsLoading = false;
        this.toastService.error('فشل في تحميل تفاصيل التذكرة');
        this.cdr.detectChanges();
      }
    });
  }

  closeTicket() {
    this.selectedTicket = null;
    this.replyContent = '';
  }

  sendReply() {
    if (!this.selectedTicket || !this.replyContent.trim()) return;
    
    this.isReplying = true;
    this.ticketsService.addMessage(this.selectedTicket.id, { content: this.replyContent })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
      next: (msg) => {
        this.selectedTicket?.messages.push(msg);
        this.replyContent = '';
        this.isReplying = false;
        this.scrollToBottom();
        this.cdr.detectChanges();
      },
      error: () => {
        this.isReplying = false;
        this.toastService.error('فشل في إرسال الرد');
        this.cdr.detectChanges();
      }
    });
  }

  scrollToBottom(): void {
    setTimeout(() => {
      try {
        if (this.chatContainer) {
          this.chatContainer.nativeElement.scrollTop = this.chatContainer.nativeElement.scrollHeight;
        }
      } catch(err) { }
    }, 100);
  }

  openResolveModal() {
    if (!this.selectedTicket) return;

    if (this.selectedTicket.linkedBooking || this.selectedTicket.linkedMarketplaceOrder) {
      this.isDisputeModalOpen = true;
      this.decisionType = '';
      this.decisionNote = '';
      this.customerAmount = 0;
      this.providerAmount = 0;
      this.selectedEmployeeId = '';
      this.cdr.detectChanges();
    } else {
      this.modalTitle = 'حل التذكرة وإغلاقها';
      this.modalDesc = `هل تم حل مشكلة "${this.selectedTicket.title}" بالكامل؟ يرجى ترك ملاحظة ختامية للمستخدم.`;
      this.modalConfirmText = 'حل التذكرة';
      this.modalConfirmColor = 'success';
      this.requireNote = true;
      this.isTakeoverModal = false;
      this.isModalOpen = true;
      this.cdr.detectChanges();
    }
  }

  closeDisputeModal() {
    this.isDisputeModalOpen = false;
    this.decisionType = '';
    this.decisionNote = '';
    this.customerAmount = 0;
    this.providerAmount = 0;
    this.selectedEmployeeId = '';
    this.cdr.detectChanges();
  }

  openDocsModal() {
    this.isDocsModalOpen = true;
    this.cdr.detectChanges();
  }

  closeDocsModal() {
    this.isDocsModalOpen = false;
    this.cdr.detectChanges();
  }

  hasAiSummary(): boolean {
    return !!(this.selectedTicket && (
      this.selectedTicket.aiSummary ||
      this.selectedTicket.aiIdentifiedIssue ||
      this.selectedTicket.aiClaimedImpact ||
      this.selectedTicket.aiEscalationPriority !== null && this.selectedTicket.aiEscalationPriority !== undefined
    ));
  }

  openAiSummaryModal() {
    if (!this.hasAiSummary()) return;
    this.isAiSummaryModalOpen = true;
    this.cdr.detectChanges();
  }

  closeAiSummaryModal() {
    this.isAiSummaryModalOpen = false;
    this.cdr.detectChanges();
  }

  handleModalConfirm(note?: string) {
    if (!this.selectedTicket) return;

    if (this.isTakeoverModal) {
      if (!note || !note.trim()) {
        this.toastService.error('الرجاء كتابة سبب الاستحواذ');
        return;
      }
      this.ticketsService.takeoverTicket(this.selectedTicket.id, note)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.isModalOpen = false;
            this.isTakeoverModal = false;
            this.toastService.success('تم الاستحواذ على التذكرة بنجاح');
            this.viewTicket(this.selectedTicket!.id);
            this.loadTickets();
          },
          error: (err: any) => {
            console.error('Takeover failed', err);
            this.toastService.error(err.error?.message || 'فشل في الاستحواذ على التذكرة');
            this.cdr.detectChanges();
          }
        });
      return;
    }

    this.ticketsService.resolveTicket(this.selectedTicket.id, { resolutionNote: note || '' })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
      next: () => {
        this.selectedTicket!.status = 'Resolved'; // optimistic update
        this.isModalOpen = false;
        this.toastService.success('تم حل التذكرة بنجاح');
        this.loadTickets(); // refresh list
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Resolve failed', err);
        this.toastService.error('فشل في حل التذكرة');
        this.cdr.detectChanges();
      }
    });
  }

  claimTicket() {
    if (!this.selectedTicket) return;
    this.ticketsService.claimTicket(this.selectedTicket.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success('تم استلام التذكرة وتعيينها لك بنجاح');
          this.viewTicket(this.selectedTicket!.id);
          this.loadTickets();
        },
        error: (err: any) => {
          this.toastService.error(err.error?.message || 'فشل في استلام التذكرة');
        }
      });
  }

  openTakeoverModal() {
    if (!this.selectedTicket) return;
    this.modalTitle = 'الاستحواذ على التذكرة';
    this.modalDesc = `التذكرة معينة حالياً للمسؤول "${this.selectedTicket.assignedToUserName}". هل أنت متأكد من رغبتك بالاستحواذ عليها وتعيينها لنفسك؟`;
    this.modalConfirmText = 'تأكيد الاستحواذ';
    this.modalConfirmColor = 'danger';
    this.requireNote = true;
    this.isTakeoverModal = true;
    this.isModalOpen = true;
  }

  loadEmployees() {
    this.usersService.getUsers(1, 100, 'Employee', true)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.availableEmployees = (res.items || []).map(u => ({
            id: u.id,
            displayName: `${u.firstName} ${u.lastName}`.trim(),
            isAvailable: u.isActive && u.isAvailableForDispatch !== false
          })).filter(emp => emp.isAvailable);
          this.cdr.detectChanges();
        },
        error: (err) => console.error('Failed to load employees', err)
      });
  }

  onDecisionTypeChange() {
    this.customerAmount = 0;
    this.providerAmount = 0;
    this.selectedEmployeeId = '';
    this.cdr.detectChanges();
  }

  getDecisionLabelAr(dec: string): string {
    switch (dec) {
      case 'ReleaseToProvider': return 'صرف كامل المبلغ للمزود (Release)';
      case 'RefundCustomer': return 'إعادة كامل المبلغ للعميل (Refund)';
      case 'ReleaseToSeller': return 'صرف كامل المبلغ للبائع (Release)';
      case 'RefundBuyer': return 'إعادة كامل المبلغ للمشتري (Refund)';
      case 'PartialSettlement': return 'تسوية جزئية للمبلغ (Partial Settlement)';
      case 'SendFieldVerification': return 'إرسال فحص ميداني (Field Verification)';
      case 'EscalateToAdmin': return 'تصعيد التذكرة للإدارة (Escalate)';
      default: return dec;
    }
  }

  isDecisionValid(): boolean {
    if (!this.selectedTicket || !this.decisionType) return false;
    if (!this.decisionNote || !this.decisionNote.trim()) return false;

    if (this.decisionType === 'PartialSettlement') {
      if (this.customerAmount < 0 || this.providerAmount < 0) return false;
      const total = this.customerAmount + this.providerAmount;
      const maxAmount = this.selectedTicket.escrowSummary?.grossAmount || 0;
      if (total <= 0 || total > maxAmount) return false;
    }

    if (this.decisionType === 'SendFieldVerification') {
      if (!this.selectedEmployeeId) return false;
    }

    return true;
  }

  submitDisputeDecision() {
    if (!this.selectedTicket || !this.isDecisionValid()) return;

    const payload = {
      decisionType: this.decisionType,
      decisionNote: this.decisionNote,
      customerAmount: this.customerAmount,
      providerAmount: this.providerAmount,
      employeeId: this.decisionType === 'SendFieldVerification' ? this.selectedEmployeeId : undefined
    };

    const action$ = this.decisionType === 'SendFieldVerification'
      ? this.ticketsService.createFieldVisit(this.selectedTicket.id, {
          employeeUserId: this.selectedEmployeeId,
          dispatchInstructions: this.decisionNote
        })
      : this.ticketsService.submitDisputeDecision(this.selectedTicket.id, payload);

    action$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          const successMessage = this.decisionType === 'SendFieldVerification'
            ? 'تم إنشاء زيارة ميدانية وربطها بالتذكرة بنجاح'
            : 'تم تطبيق قرار تسوية النزاع بنجاح';
          this.toastService.success(successMessage);
          this.isDisputeModalOpen = false;
          this.viewTicket(this.selectedTicket!.id);
          this.loadTickets();
        },
        error: (err: any) => {
          this.toastService.error(err.error?.message || 'فشل في تطبيق القرار');
        }
      });
  }
}
