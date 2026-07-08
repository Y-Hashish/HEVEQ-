import { Component, OnInit, OnDestroy, ChangeDetectorRef, NgZone } from '@angular/core'
import { CommonModule } from '@angular/common'
import { Subscription } from 'rxjs'
import { NotificationsService } from '../../../core/services/notificationsService'
import { NotificationItem } from '../../../core/models/notificationModels'
import { Router } from '@angular/router'
import { TokenStorage } from '../../../core/services/token-storage'

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications.html',
  styleUrl: './notifications.css'
})
export class Notifications implements OnInit, OnDestroy {
  notifications: NotificationItem[] = []
  unreadCount = 0
  isLoading = false
  errorMessage = ''

  private latestNotificationSub?: Subscription
  private unreadCountSub?: Subscription

  constructor(
    private notificationsService: NotificationsService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone,
    private router: Router,
    private tokenStorage: TokenStorage
  ) {}

  ngOnInit(): void {
    this.loadNotifications()

    this.latestNotificationSub = this.notificationsService.latestNotification$.subscribe({
      next: (notification) => {
        this.ngZone.run(() => {
          if (!this.notifications.some(n => n.id === notification.id)) {
            this.notifications = [notification, ...this.notifications]
            this.cdr.detectChanges()
          }
        })
      }
    })

    this.unreadCountSub = this.notificationsService.unreadCount$.subscribe({
      next: (count) => {
        this.ngZone.run(() => {
          this.unreadCount = count
          this.cdr.detectChanges()
        })
      }
    })
  }

  loadNotifications(): void {
    this.isLoading = true
    this.errorMessage = ''

    this.notificationsService.getMyNotifications(undefined, 1, 50).subscribe({
      next: (res) => {
        this.notifications = res.items || []
        this.notificationsService.setUnreadCount(res.unreadCount || 0)
        this.isLoading = false
        this.cdr.detectChanges()
      },
      error: () => {
        this.errorMessage = 'فشل في تحميل الإشعارات. يرجى المحاولة مرة أخرى.'
        this.isLoading = false
        this.cdr.detectChanges()
      }
    })
  }

  markAsRead(notification: NotificationItem): void {
    const isRead = notification.isRead || notification.read
    if (isRead) return

    this.notificationsService.markAsRead(notification.id).subscribe({
      next: () => {
        notification.read = true
        notification.isRead = true
        this.notificationsService.setUnreadCount(Math.max(0, this.unreadCount - 1))
        this.cdr.detectChanges()
      },
      error: () => {}
    })
  }

  getNotificationIcon(eventType?: string): string {
    if (!eventType) return 'fa-solid fa-bell'
    const type = eventType.toLowerCase()
    if (type.includes('ticket')) return 'fa-solid fa-ticket'
    if (type.includes('booking')) return 'fa-solid fa-calendar-days'
    if (type.includes('escrow') || type.includes('wallet') || type.includes('paid')) return 'fa-solid fa-money-bill-wave'
    if (type.includes('dispute')) return 'fa-solid fa-scale-balanced'
    if (type.includes('message')) return 'fa-solid fa-comments'
    if (type.includes('order') || type.includes('marketplace')) return 'fa-solid fa-box-open'
    if (type.includes('fieldverification')) return 'fa-solid fa-location-dot'
    if (type.includes('document')) return 'fa-solid fa-file-lines'
    return 'fa-solid fa-bell'
  }

  getNotificationMessage(notification: NotificationItem): string {
    return notification.body || notification.message || ''
  }

  isNotificationRead(notification: NotificationItem): boolean {
    return !!(notification.isRead || notification.read)
  }

  getNotificationDate(notification: NotificationItem): string {
    return notification.sentAt || notification.createdAt || ''
  }

  getNavigationCommands(notification: NotificationItem): { commands: any[]; queryParams?: any } | null {
    const id = notification.referenceId
    if (!id) return null

    const role = this.tokenStorage.getRole()
    const eventType = String(notification.eventType || '').toLowerCase()
    const refType = String(notification.referenceType || '').toLowerCase()
    const type = `${eventType} ${refType}`

    if (refType.includes('servicelisting')) {
      if (role === 'admin' || role === 'employee') return { commands: ['/admin/listing-review'], queryParams: { type: 'service', id } }
      if (eventType.includes('approved')) return { commands: ['/service-details', id] }
      return { commands: ['/equipment'] }
    }

    if (refType.includes('marketplacelisting')) {
      if (role === 'admin' || role === 'employee') return { commands: ['/admin/listing-review'], queryParams: { type: 'marketplace', id } }
      if (eventType.includes('approved')) return { commands: ['/product-details', id] }
      return { commands: ['/equipment'] }
    }

    if (refType.includes('booking') || type.includes('booking')) {
      if (role === 'provider') return { commands: ['/booking-requests'], queryParams: { bookingId: id } }
      if (role === 'admin' || role === 'employee') return { commands: ['/admin/disputes'], queryParams: { bookingId: id } }
      return { commands: ['/bookings'], queryParams: { bookingId: id } }
    }

    if (refType.includes('marketplaceorder') || type.includes('marketplaceorder') || type.includes('order')) {
      if (role === 'provider') return { commands: ['/marketplace-sales'], queryParams: { orderId: id } }
      if (role === 'admin' || role === 'employee') return { commands: ['/admin/disputes'], queryParams: { orderId: id } }
      return { commands: ['/marketplace-orders'], queryParams: { orderId: id } }
    }

    if (refType.includes('ticket') || type.includes('ticket')) {
      if (role === 'admin' || role === 'employee') return { commands: ['/admin/tickets'], queryParams: { ticketId: id } }
      return { commands: ['/support-tickets'], queryParams: { ticketId: id } }
    }

    if (refType.includes('fieldverification')) return { commands: ['/employee/field-visits'], queryParams: { visitId: id } }
    if (refType.includes('document')) return { commands: ['/admin/documents'], queryParams: { documentId: id } }
    if (refType.includes('message')) return { commands: ['/messages'], queryParams: { messageId: id } }
    if (refType.includes('escrow')) return role === 'provider' ? { commands: ['/earnings'] } : { commands: ['/wallet'] }

    return null
  }

  getNavigationLink(notification: NotificationItem): string | null {
    const nav = this.getNavigationCommands(notification)
    return nav ? nav.commands.join('/') : null
  }

  navigateToLink(notification: NotificationItem): void {
    this.markAsRead(notification)
    const nav = this.getNavigationCommands(notification)
    if (nav) this.router.navigate(nav.commands, { queryParams: nav.queryParams })
  }

  ngOnDestroy(): void {
    this.latestNotificationSub?.unsubscribe()
    this.unreadCountSub?.unsubscribe()
  }
}
