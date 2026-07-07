export enum DocumentType {
  NationalId = 0,
  CommercialRegistration = 1,
  TaxCard = 2,
  EquipmentLicense = 3,
  OperatorLicense = 4,
  Insurance = 5,
  Other = 99
}

export enum DocumentVerificationStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Expired = 3
}

export interface UploadDocumentRequest {
  documentType: DocumentType
  fileUrl: string
  expiryDate: string | null
  serviceListingId: string | null
  marketplaceListingId: string | null
  operatorId: string | null
}

export interface UploadDocumentResponse {
  id: string
  documentType: string
  fileUrl: string
  status: string
  statusAr: string
  uploadedAt: string
}

export interface DocumentItem {
  id: string
  userId: string | null
  serviceListingId: string | null
  marketplaceListingId: string | null
  operatorId: string | null
  documentType: DocumentType
  fileUrl: string
  status: DocumentVerificationStatus
  statusAr: string
  expiryDate: string | null
  failureReason: string | null
  uploadedAt: string
  verifiedAt: string | null
}

export interface DocumentTypeOption {
  value: DocumentType
  label: string
  description: string
  requiresExpiryDate: boolean
}