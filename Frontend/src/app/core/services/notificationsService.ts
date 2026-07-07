import { Injectable } from '@angular/core'
import { HttpClient, HttpParams } from '@angular/common/http'
import { Observable, Subject, BehaviorSubject } from 'rxjs'
import { NotificationItem } from '../models/notificationModels'
import { API_BASE_URL } from '../constants/api.constants'

export interface PaginatedNotifications {
  items: NotificationItem[]
  unreadCount: number
  totalCount: number
}

@Injectable({
  providedIn: 'root'
})
export class NotificationsService {
  private latestNotificationSubject = new Subject<NotificationItem>()
  latestNotification$ = this.latestNotificationSubject.asObservable()

  private unreadCountSubject = new BehaviorSubject<number>(0)
  unreadCount$ = this.unreadCountSubject.asObservable()

  constructor(private http: HttpClient) {}

  getMyNotifications(isRead?: boolean, page: number = 1, pageSize: number = 20): Observable<PaginatedNotifications> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString())

    if (isRead !== undefined) {
      params = params.set('isRead', isRead.toString())
    }

    return this.http.get<PaginatedNotifications>(`${API_BASE_URL}/notifications/my`, { params })
  }

  loadUnreadCount(): void {
    this.http.get<PaginatedNotifications>(`${API_BASE_URL}/notifications/my?pageSize=1`).subscribe({
      next: (res) => {
        if (res && typeof res.unreadCount === 'number') {
          this.unreadCountSubject.next(res.unreadCount)
        }
      },
      error: (err) => console.error('Failed to load unread count', err)
    })
  }

  markAsRead(id: string): Observable<any> {
    return this.http.patch<any>(`${API_BASE_URL}/notifications/${id}/read`, {})
  }

  pushRealtimeNotification(notification: NotificationItem): void {
    console.log('NotificationsService.pushRealtimeNotification called', notification)
    this.latestNotificationSubject.next(notification)
    // Increment unread count locally when a new notification arrives
    this.unreadCountSubject.next(this.unreadCountSubject.value + 1)
  }

  setUnreadCount(count: number): void {
    this.unreadCountSubject.next(count)
  }
}
