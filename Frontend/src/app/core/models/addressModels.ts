export interface AddressItem {
  id: string
  label: string
  governorate: string
  district: string
  street: string
  latitude: number | null
  longitude: number | null
  isDefault: boolean
}

export interface CreateAddressRequest {
  label: string
  governorate: string
  district: string
  street: string
  latitude: number
  longitude: number
  isDefault: boolean
}

export interface UpdateAddressRequest {
  label: string
  governorate: string
  district: string
  street: string
  latitude: number | null
  longitude: number | null
  isDefault: boolean
}

export interface CreateAddressResponse {
  id: string
  label: string
  isDefault: boolean
}

export interface AddressActionResponse {
  isSuccess?: boolean
  message?: string
  statusCode?: number
  notice?: string
}