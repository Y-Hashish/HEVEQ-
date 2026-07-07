export interface DashboardSummary {
  totalUsers: number;
  activeUsers: number;
  totalProviders: number;
  pendingServiceListings: number;
  pendingMarketplaceListings: number;
  pendingDocuments: number;
  openTickets: number;
  disputedBookings: number;
  disputedMarketplaceOrders: number;
  frozenEscrowRecords: number;
  pendingFieldVerifications: number;
  aiHighRiskItems: number;
}

export interface PendingAction {
  type: string;
  title: string;
  referenceId: string;
  priority: string;
  createdAt: string;
  actionUrl: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page?: number;
  pageSize?: number;
}

export interface PendingListing {
  id: string;
  title: string;
  ownerName: string;
  categoryName: string;
  qualityScore: number;
  aiRiskScore?: number;
  aiRiskLevel: string;
  aiRiskFlags: string;
  status: string;
  statusAr: string;
  submittedAt: string;
  // Specific fields based on type
  photosCount?: number;
  price?: number;
}

export interface ReviewProvider {
  companyName: string;
  email: string;
  phoneNumber: string;
}

export interface ListingReviewDetails {
  id: string;
  title: string;
  description: string;
  categoryName: string;
  provider: ReviewProvider;
  photos: { url: string }[];
  qualityScore: number;
  aiRiskScore?: number;
  aiRiskLevel: string;
  aiRiskFlags: string;
  aiRecommendation: string;
  status: string;
  statusAr: string;
  // Optional depending on type
  pricing?: any;
  operators?: any[];
  documents?: { type: string, fileUrl: string }[];
}

export interface AccountVerification {
  userId: string;
  userName: string;
  email: string;
  role: string;
  verificationStatus: string;
  submittedAt: string;
  documentIds?: string[];
}

export interface AdminDocument {
  id: string;
  userId: string;
  userName: string;
  role: string;
  documentType: string;
  fileUrl: string;
  status: string;
  statusAr: string;
  statusText: string;
  uploadedAt: string;
  extractedText?: string;
  confidenceScore?: number;
  keyFieldsPresent?: boolean;
}

export interface AdminTicketMessage {
  id: string;
  senderId: string;
  senderName: string;
  senderRole: string; // "Admin", "Customer", etc.
  content: string;
  createdAt: string;
}

export interface AdminTicket {
  id: string;
  title: string;
  ticketNumber?: string;
  userId: string;
  userName: string;
  status: 'Open' | 'InProgress' | 'Resolved' | 'Closed';
  priority: 'Low' | 'Medium' | 'High' | 'Urgent';
  category: string;
  createdAt: string;
  updatedAt: string;
}

export interface EscrowSummary {
  id: string;
  grossAmount: number;
  platformCommission: number;
  providerPayout: number;
  status: string;
}

export interface TicketAttachment {
  id: string;
  fileUrl: string;
  fileName: string;
  fileType: string;
  createdAt: string;
}

export interface ProviderCompletionEvidence {
  id: string;
  providerNotes?: string;
  status: string;
  submittedAt?: string;
  photos: { id: string; photoUrl: string; caption?: string }[];
}

export interface FieldVisitPhoto {
  id: string;
  photoUrl: string;
  caption?: string;
}

export interface FieldVisit {
  id: string;
  dispatchedEmployeeId: string;
  dispatchedEmployeeName: string;
  dispatchInstructions?: string;
  visitStatus: string;
  visitStatusAr: string;
  employeeNotes?: string;
  fieldVerificationOutcome?: string;
  adminDecision: string;
  adminDecisionNote?: string;
  dispatchedAt: string;
  visitedAt?: string;
  photos: FieldVisitPhoto[];
}

export interface AdminTicketDetails extends AdminTicket {
  description: string;
  messages: AdminTicketMessage[];
  assignedToUserId?: string;
  assignedToUserName?: string;
  linkedBooking?: { id: string; bookingReference: string };
  linkedMarketplaceOrder?: { id: string; orderNumber: string };
  escrowSummary?: EscrowSummary;
  availableDisputeDecisions?: string[];
  customerAttachments?: TicketAttachment[];
  providerCompletionEvidence?: ProviderCompletionEvidence;
  fieldVisits?: FieldVisit[];
}

export interface AdminDispute {
  id: string;
  bookingId: string;
  customerName: string;
  providerName: string;
  status: 'Open' | 'UnderReview' | 'Resolved' | 'Disputed';
  reason: string;
  amount: number;
  currency: string;
  createdAt: string;
}

export interface AdminDisputeDetails extends AdminDispute {
  description: string;
  serviceTitle: string;
}

export interface AdminFieldVerification {
  id: string;
  listingId: string;
  providerName: string;
  status: 'Pending' | 'Dispatched' | 'InProgress' | 'Completed' | 'Verified' | 'Failed';
  scheduledDate?: string;
  employeeId?: string;
  employeeName?: string;
  createdAt: string;
}

export interface AdminFieldVerificationDetails extends AdminFieldVerification {
  listingTitle: string;
  locationDetails: string;
  employeeReport?: string;
  evidenceUrls?: string[];
}
