export interface RealtimeMessage {
  id: string
  conversationId: string
  senderId: string
  senderName?: string
  messageType?: string
  body?: string
  sentAt: string

  chatRoomId?: string
  content?: string
  createdAt?: string
}
