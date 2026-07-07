import { Component, OnInit, OnDestroy, ChangeDetectorRef, NgZone } from '@angular/core'
import { CommonModule } from '@angular/common'
import { Subscription } from 'rxjs'
import { NotificationsService } from '../../../core/services/notificationsService'
import { NotificationItem } from '../../../core/models/notificationModels'

import { Router } from '@angular/router'

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
    private router: Router
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
      error: (err) => {
        console.error('Failed to load notifications', err)
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
        // Decrement local unread count
        const currentCount = this.unreadCount
        this.notificationsService.setUnreadCount(Math.max(0, currentCount - 1))
        this.cdr.detectChanges()
      },
      error: (err) => console.error('Failed to mark notification as read', err)
    })
  }

  getNotificationIcon(eventType?: string): string {
    if (!eventType) return '🔔'
    const type = eventType.toLowerCase()
    if (type.includes('ticket')) return '🎫'
    if (type.includes('booking')) return '📅'
    if (type.includes('escrow') || type.includes('wallet') || type.includes('paid')) return '💰'
    if (type.includes('dispute')) return '⚖️'
    if (type.includes('message')) return '💬'
    if (type.includes('order') || type.includes('marketplace')) return '📦'
    return '🔔'
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

  getNavigationLink(notification: NotificationItem): string | null {
    if (!notification.referenceId) return null
    const type = (notification.referenceType || notification.eventType || '').toLowerCase()
    if (type.includes('ticket')) return `/support-tickets`
    if (type.includes('booking')) return `/bookings`
    if (type.includes('message')) return `/messages`
    if (type.includes('order') || type.includes('marketplace')) return `/marketplace`
    return null
  }

  navigateToLink(notification: NotificationItem): void {
    const link = this.getNavigationLink(notification)
    if (link) {
      this.router.navigate([link])
    }
  }

  ngOnDestroy(): void {
    this.latestNotificationSub?.unsubscribe()
    this.unreadCountSub?.unsubscribe()
  }
}
