export enum MessageType {
  Text = 0,
  Attachment = 1,
  System = 2
}

export interface ConversationListItem {
  id: string
  title: string
  contextType: string
  referenceId: string | null
  otherPartyName: string
  lastMessagePreview: string | null
  lastMessageAt: string | null
  unreadCount: number
  isLocked: boolean
}

export interface ConversationListResponse {
  items: ConversationListItem[]
  totalCount: number
}

export interface ConversationMessage {
  id: string
  senderName: string
  messageType: string
  body: string | null
  createdAt: string
}

export interface ConversationMessagesResponse {
  conversationId: string
  items: ConversationMessage[]
  totalCount: number
}

export interface StartConversationRequest {
  contextType: 'Booking' | 'ServiceListing' | 'MarketplaceListing'
  referenceId: string
}

export interface StartConversationResponse {
  id: string
  message: string
}

export interface SendConversationMessageRequest {
  body: string
  messageType: MessageType
}

export interface SendConversationMessageResponse {
  id: string
  message: string
}

export interface MarkConversationReadResponse {
  conversationId: string
  unreadCount: number
  message: string
}