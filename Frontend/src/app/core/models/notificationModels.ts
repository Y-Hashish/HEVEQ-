export interface NotificationItem {
  id: string
  title: string
  message?: string
  body?: string
  createdAt?: string
  sentAt?: string
  read?: boolean
  isRead?: boolean
  eventType?: string
  referenceId?: string
  referenceType?: string
}
