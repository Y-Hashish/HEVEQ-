import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { API_BASE_URL } from '../constants/api.constants'
import {
  CustomerProfile,
  CustomerTrustHistoryItem,
  ProviderProfile,
  ProviderTrustHistoryItem,
  UpdateCustomerProfileRequest,
  UpdateProviderProfileRequest
} from '../models/profileModels'

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  constructor(private http: HttpClient) {}

  getCustomerProfile() {
    return this.http.get<CustomerProfile>(`${API_BASE_URL}/customer-profile/me`)
  }

  updateCustomerProfile(request: UpdateCustomerProfileRequest) {
    return this.http.put<CustomerProfile>(`${API_BASE_URL}/customer-profile/me`, request)
  }

  getCustomerTrustHistory() {
    return this.http.get<CustomerTrustHistoryItem[]>(
      `${API_BASE_URL}/customer-profile/me/trust-history`
    )
  }

  getProviderProfile() {
    return this.http.get<ProviderProfile>(`${API_BASE_URL}/provider/profile/me`)
  }

  updateProviderProfile(request: UpdateProviderProfileRequest) {
    return this.http.put<ProviderProfile>(`${API_BASE_URL}/provider/profile/me`, request)
  }

  getProviderTrustHistory() {
    return this.http.get<ProviderTrustHistoryItem[]>(
      `${API_BASE_URL}/provider/profile/me/trust-history`
    )
  }
}