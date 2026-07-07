// Backend contract assumed for the gap-fill endpoints (GET/POST/PUT
// /api/provider/operators) — see chat note: this CRUD didn't exist before;
// only the listing<->operator LINKING endpoints did. Confirm the exact
// shape against the real handler once it's added on the backend.

export interface Operator {
  id: string
  fullName: string
  yearsOfExperience: number
  specialization: string | null
  licenseType: string | null
  licenseNumber: string | null
  licenseExpiryDate: string | null // "yyyy-MM-dd"
  isActive: boolean
}

export interface OperatorFormPayload {
  fullName: string
  yearsOfExperience: number
  specialization: string | null
  licenseType: string | null
  licenseNumber: string | null
  licenseExpiryDate: string | null
}


export interface ProviderOperator {
  id: string
  providerProfileId: string
  fullName: string
  yearsOfExperience: number | null
  specialization: string | null
  licenseType: string | null
  licenseNumber: string | null
  licenseExpiryDate: string | null
  profilePhotoUrl: string | null
  isActive: boolean
  createdAt: string
}