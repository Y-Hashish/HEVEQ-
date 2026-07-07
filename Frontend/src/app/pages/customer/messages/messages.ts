import { CommonModule } from '@angular/common'
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { finalize, Subscription } from 'rxjs'
import { ActivatedRoute } from '@angular/router'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'
import {
  ConversationListItem,
  ConversationMessage,
  MessageType
} from '../../../core/models/conversationModels'
import { RealtimeMessage } from '../../../core/models/realtimeModels'
import { ConversationsService } from '../../../core/services/conversationsService'
import { RealtimeService } from '../../../core/services/realtimeService'

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './messages.html',
  styleUrl: './messages.css'
})
export class Messages implements OnInit, OnDestroy {
  conversations: ConversationListItem[] = []
  selectedConversation: ConversationListItem | null = null
  messages: ConversationMessage[] = []

  isLoading = false
  isMessagesLoading = false
  isSending = false

  errorMessage = ''
  successMessage = ''
  newMessageBody = ''

  private realtimeMessageSub?: Subscription
  private pendingConversationId: string | null = null

  constructor(
    private conversationsService: ConversationsService,
    private realtimeService: RealtimeService,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.pendingConversationId = this.route.snapshot.queryParamMap.get('conversationId')
    this.loadConversations()

    this.realtimeMessageSub = this.realtimeService.messageReceived$.subscribe(message => {
      this.handleRealtimeMessage(message)
    })
  }

  ngOnDestroy(): void {
    this.realtimeMessageSub?.unsubscribe()
  }

  loadConversations(): void {
    this.isLoading = true
    this.errorMessage = ''
    this.cdr.detectChanges()

    this.conversationsService
      .getMyConversations()
      .pipe(
        finalize(() => {
          this.isLoading = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: response => {
          this.conversations = response.items ?? []

          const requestedConversation = this.pendingConversationId
            ? this.conversations.find(item => item.id === this.pendingConversationId)
            : null

          if (requestedConversation) {
            this.pendingConversationId = null
            this.openConversation(requestedConversation)
          } else if (!this.selectedConversation && this.conversations.length > 0) {
            this.openConversation(this.conversations[0])
          }

          this.cdr.detectChanges()
        },
        error: error => {
          this.conversations = []
          this.errorMessage = getErrorMessage(error, 'تعذر تحميل المحادثات')
          this.cdr.detectChanges()
        }
      })
  }

  openConversation(conversation: ConversationListItem): void {
    this.selectedConversation = conversation
    this.messages = []
    this.errorMessage = ''
    this.successMessage = ''
    this.isMessagesLoading = true
    this.cdr.detectChanges()

    this.conversationsService
      .getMessages(conversation.id, 1, 50)
      .pipe(
        finalize(() => {
          this.isMessagesLoading = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: response => {
          this.messages = response.items ?? []

          this.markConversationRead(conversation.id)

          this.cdr.detectChanges()
        },
        error: error => {
          this.messages = []
          this.errorMessage = getErrorMessage(error, 'تعذر تحميل رسائل المحادثة')
          this.cdr.detectChanges()
        }
      })
  }

  sendMessage(): void {
    this.errorMessage = ''
    this.successMessage = ''

    if (!this.selectedConversation) {
      this.errorMessage = 'اختر محادثة أولًا'
      this.cdr.detectChanges()
      return
    }

    if (!this.newMessageBody.trim()) {
      this.errorMessage = 'اكتب الرسالة أولًا'
      this.cdr.detectChanges()
      return
    }

    if (this.selectedConversation.isLocked) {
      this.errorMessage = 'هذه المحادثة مغلقة ولا يمكن إرسال رسائل جديدة'
      this.cdr.detectChanges()
      return
    }

    const conversationId = this.selectedConversation.id
    const messageBody = this.newMessageBody.trim()

    this.isSending = true
    this.cdr.detectChanges()

    this.conversationsService
      .sendMessage(conversationId, {
        body: messageBody,
        messageType: MessageType.Text
      })
      .pipe(
        finalize(() => {
          this.isSending = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: () => {
          this.newMessageBody = ''
          this.loadMessagesAgain(conversationId)
          this.loadConversations()
          this.cdr.detectChanges()
        },
        error: error => {
          this.errorMessage = getErrorMessage(error, 'تعذر إرسال الرسالة')
          this.cdr.detectChanges()
        }
      })
  }

  private loadMessagesAgain(conversationId: string): void {
    if (!this.selectedConversation || this.selectedConversation.id !== conversationId) {
      return
    }

    this.conversationsService.getMessages(conversationId, 1, 50).subscribe({
      next: response => {
        this.messages = response.items ?? []
        this.cdr.detectChanges()
      },
      error: () => {}
    })
  }

  private markConversationRead(conversationId: string): void {
    this.conversationsService.markRead(conversationId).subscribe({
      next: response => {
        this.conversations = this.conversations.map(item => {
          if (item.id !== conversationId) {
            return item
          }

          return {
            ...item,
            unreadCount: response.unreadCount
          }
        })

        this.cdr.detectChanges()
      },
      error: () => {}
    })
  }

  private handleRealtimeMessage(message: RealtimeMessage): void {
    const conversationId = message.conversationId

    this.conversations = this.conversations.map(item => {
      if (item.id !== conversationId) {
        return item
      }

      return {
        ...item,
        lastMessagePreview: message.body || '',
        lastMessageAt: message.sentAt,
        unreadCount:
          this.selectedConversation?.id === conversationId
            ? item.unreadCount
            : item.unreadCount + 1
      }
    })

    if (this.selectedConversation?.id === conversationId) {
      this.messages = [
        ...this.messages,
        {
          id: message.id,
          senderName: message.senderName || 'مستخدم',
          messageType: message.messageType || 'Text',
          body: message.body ?? null,
          createdAt: message.sentAt
        }
      ]

      this.markConversationRead(conversationId)
    }

    this.cdr.detectChanges()
  }

  getConversationTitle(conversation: ConversationListItem): string {
    return conversation.title || conversation.otherPartyName || 'محادثة'
  }

  getContextText(contextType: string): string {
    switch (contextType) {
      case 'Booking':
        return 'حجز'
      case 'ServiceListing':
        return 'خدمة'
      case 'MarketplaceListing':
        return 'منتج من السوق'
      default:
        return contextType || 'عام'
    }
  }

  trackConversationById(index: number, item: ConversationListItem): string {
    return item.id
  }

  trackMessageById(index: number, item: ConversationMessage): string {
    return item.id
  }
}