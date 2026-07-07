import { CommonModule } from '@angular/common'
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { finalize, forkJoin, of, Subscription, switchMap } from 'rxjs'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'
import {
  CreateTicketRequest,
  TicketCategory,
  TicketDetails,
  TicketListItem
} from '../../../core/models/ticketModels'
import { NotificationsService } from '../../../core/services/notificationsService'
import { TicketsService } from '../../../core/services/ticketsService'
import { MediaUploadService } from '../../../core/services/mediaUploadService'

@Component({
  selector: 'app-support-tickets',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './supportTickets.html',
  styleUrl: './supportTickets.css'
})
export class SupportTickets implements OnInit, OnDestroy {
  tickets: TicketListItem[] = []
  selectedTicket: TicketDetails | null = null

  isLoading = false
  isDetailsLoading = false
  isCreating = false
  isSendingMessage = false

  errorMessage = ''
  successMessage = ''
  totalCount = 0

  newTicketForm: CreateTicketRequest = {
    subject: '',
    category: TicketCategory.General,
    message: '',
    bookingId: null,
    marketplaceOrderId: null
  }

  replyBody = ''
  selectedTicketFiles: File[] = []

  categoryOptions = [
    {
      value: TicketCategory.General,
      label: 'استفسار عام'
    },
    {
      value: TicketCategory.BookingIssue,
      label: 'مشكلة في حجز'
    },
    {
      value: TicketCategory.PaymentIssue,
      label: 'مشكلة في الدفع'
    },
    {
      value: TicketCategory.DocumentVerification,
      label: 'مراجعة مستندات'
    },
    {
      value: TicketCategory.MarketplaceOrder,
      label: 'طلب من السوق'
    },
    {
      value: TicketCategory.TechnicalIssue,
      label: 'مشكلة تقنية'
    },
    {
      value: TicketCategory.AccountVerification,
      label: 'توثيق الحساب'
    },
    {
      value: TicketCategory.Complaint,
      label: 'شكوى'
    }
  ]

  private latestNotificationSub?: Subscription

  constructor(
    private ticketsService: TicketsService,
    private notificationsService: NotificationsService,
    private mediaUploadService: MediaUploadService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadTickets()

    this.latestNotificationSub = this.notificationsService.latestNotification$.subscribe(notification => {
      if (
        notification.eventType?.includes('Ticket') ||
        notification.eventType?.includes('Message')
      ) {
        this.loadTickets()

        if (this.selectedTicket?.id && notification.referenceId === this.selectedTicket.id) {
          this.openTicket(this.selectedTicket.id)
        }
      }
    })
  }

  ngOnDestroy(): void {
    this.latestNotificationSub?.unsubscribe()
  }

  loadTickets(): void {
    this.isLoading = true
    this.errorMessage = ''
    this.cdr.detectChanges()

    this.ticketsService
      .getMyTickets()
      .pipe(
        finalize(() => {
          this.isLoading = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: response => {
          this.tickets = response.items ?? []
          this.totalCount = response.totalCount ?? this.tickets.length
          this.cdr.detectChanges()
        },
        error: error => {
          this.tickets = []
          this.totalCount = 0
          this.errorMessage = getErrorMessage(error, 'تعذر تحميل تذاكر الدعم')
          this.cdr.detectChanges()
        }
      })
  }

  createTicket(): void {
    this.errorMessage = ''
    this.successMessage = ''

    if (!this.newTicketForm.subject || !this.newTicketForm.message) {
      this.errorMessage = 'من فضلك اكتب عنوان التذكرة ووصف المشكلة'
      this.cdr.detectChanges()
      return
    }

    this.isCreating = true
    this.cdr.detectChanges()

    const uploadRequest = this.selectedTicketFiles.length > 0
      ? forkJoin(this.selectedTicketFiles.map(file => this.mediaUploadService.uploadImage(file, 'ticket-attachments')))
      : of([])

    uploadRequest
      .pipe(
        switchMap(uploadResults => this.ticketsService.createTicket({
          subject: this.newTicketForm.subject,
          category: Number(this.newTicketForm.category),
          message: this.newTicketForm.message,
          bookingId: this.emptyToNull(this.newTicketForm.bookingId),
          marketplaceOrderId: this.emptyToNull(this.newTicketForm.marketplaceOrderId),
          attachments: uploadResults.map(result => ({
            fileUrl: result.url,
            fileName: result.publicId || result.url.split('/').pop() || 'attachment',
            fileType: 0
          }))
        })),
        finalize(() => {
          this.isCreating = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: response => {
          this.successMessage = response.message || 'تم إنشاء تذكرة الدعم بنجاح'
          this.resetNewTicketForm()
          this.selectedTicketFiles = []
          this.loadTickets()

          if (response.id) {
            this.openTicket(response.id)
          }

          this.cdr.detectChanges()
        },
        error: error => {
          this.errorMessage = getErrorMessage(error, 'تعذر إنشاء تذكرة الدعم')
          this.cdr.detectChanges()
        }
      })
  }

  openTicket(ticketId: string): void {
    this.isDetailsLoading = true
    this.errorMessage = ''
    this.cdr.detectChanges()

    this.ticketsService
      .getTicketDetails(ticketId)
      .pipe(
        finalize(() => {
          this.isDetailsLoading = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: ticket => {
          this.selectedTicket = {
            ...ticket,
            messages: ticket.messages ?? []
          }

          this.cdr.detectChanges()
        },
        error: error => {
          this.selectedTicket = null
          this.errorMessage = getErrorMessage(error, 'تعذر تحميل تفاصيل التذكرة')
          this.cdr.detectChanges()
        }
      })
  }

  sendReply(): void {
    this.errorMessage = ''
    this.successMessage = ''

    if (!this.selectedTicket) {
      this.errorMessage = 'اختر تذكرة أولًا'
      this.cdr.detectChanges()
      return
    }

    if (!this.replyBody.trim()) {
      this.errorMessage = 'اكتب رسالة المتابعة أولًا'
      this.cdr.detectChanges()
      return
    }

    const ticketId = this.selectedTicket.id

    this.isSendingMessage = true
    this.cdr.detectChanges()

    this.ticketsService
      .addMessage(ticketId, {
        body: this.replyBody.trim()
      })
      .pipe(
        finalize(() => {
          this.isSendingMessage = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: () => {
          this.replyBody = ''
          this.successMessage = 'تم إرسال الرسالة بنجاح'
          this.openTicket(ticketId)
          this.cdr.detectChanges()
        },
        error: error => {
          this.errorMessage = getErrorMessage(error, 'تعذر إرسال الرسالة')
          this.cdr.detectChanges()
        }
      })
  }


  onTicketFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement
    const files = Array.from(input.files ?? [])
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp']
    const maxSize = 10 * 1024 * 1024

    if (files.length > 5) {
      this.errorMessage = 'الحد الأقصى 5 صور للتذكرة الواحدة'
      this.selectedTicketFiles = []
      this.cdr.detectChanges()
      return
    }

    const invalid = files.find(file => !allowedTypes.includes(file.type) || file.size > maxSize)
    if (invalid) {
      this.errorMessage = 'مسموح بصور JPG أو PNG أو WEBP فقط وبحد أقصى 10 ميجابايت للصورة'
      this.selectedTicketFiles = []
      this.cdr.detectChanges()
      return
    }

    this.selectedTicketFiles = files
    this.errorMessage = ''
    this.cdr.detectChanges()
  }

  resetNewTicketForm(): void {
    this.newTicketForm = {
      subject: '',
      category: TicketCategory.General,
      message: '',
      bookingId: null,
      marketplaceOrderId: null
    }
    this.selectedTicketFiles = []
  }

  getStatusText(status: string, statusAr?: string | null): string {
    if (statusAr) {
      return statusAr
    }

    switch (status) {
      case 'Open':
        return 'مفتوحة'
      case 'InProgress':
        return 'قيد المعالجة'
      case 'PendingCustomerReply':
        return 'بانتظار ردك'
      case 'PendingProviderReply':
        return 'بانتظار رد المزود'
      case 'PendingFieldVerification':
        return 'بانتظار التحقق الميداني'
      case 'Resolved':
        return 'محلولة'
      case 'Closed':
        return 'مغلقة'
      case 'Reopened':
        return 'معاد فتحها'
      default:
        return status || 'غير معروف'
    }
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Resolved':
      case 'Closed':
        return 'closed'
      case 'InProgress':
      case 'PendingFieldVerification':
        return 'in-progress'
      case 'PendingCustomerReply':
      case 'PendingProviderReply':
        return 'pending'
      default:
        return 'open'
    }
  }

  isTicketClosed(ticket: TicketDetails | null): boolean {
    if (!ticket) {
      return true
    }

    return ticket.status === 'Resolved' || ticket.status === 'Closed'
  }

  trackById(index: number, item: TicketListItem): string {
    return item.id
  }

  trackMessageById(index: number, item: any): string {
    return item.id
  }

  private emptyToNull(value: string | null): string | null {
    if (!value || !value.trim()) {
      return null
    }

    return value.trim()
  }
}