export enum TicketCategory {
  General = 0,
  BookingIssue = 1,
  PaymentIssue = 2,
  DocumentVerification = 3,
  MarketplaceOrder = 4,
  TechnicalIssue = 5,
  AccountVerification = 6,
  Complaint = 7,
  CompletionDispute = 8
}

export interface MyTicketsResponse {
  items: TicketListItem[]
  totalCount: number
}

export interface TicketListItem {
  id: string
  ticketNumber: string
  subject: string
  status: string
  statusAr: string | null
  createdAt: string
  updatedAt?: string | null
}

export interface TicketAttachmentItem {
  id: string
  fileUrl: string
  fileName: string | null
  fileType?: string | number | null
  createdAt?: string | null
}

export interface TicketMessageItem {
  id: string
  ticketId?: string | null
  senderId?: string | null
  senderName: string | null
  body: string
  messageType?: string | number | null
  isInternal?: boolean
  createdAt: string
  attachments?: TicketAttachmentItem[]
}

export interface TicketDetails {
  id: string
  ticketNumber: string
  subject: string
  status: string
  statusAr: string | null
  createdAt?: string | null
  updatedAt?: string | null
  messages: TicketMessageItem[]
}

export interface CreateTicketAttachmentInput {
  fileUrl: string
  fileName: string
  fileType: number | string
}

export interface CreateTicketRequest {
  subject: string
  category: TicketCategory
  message: string
  bookingId: string | null
  marketplaceOrderId: string | null
  attachments?: CreateTicketAttachmentInput[]
}

export interface AddTicketMessageRequest {
  body: string
}

export interface CreateTicketResponse {
  id: string
  ticketNumber: string
  status: string
  statusAr: string | null
  message: string
}

export interface AddTicketMessageResponse {
  id: string
  message: string
}