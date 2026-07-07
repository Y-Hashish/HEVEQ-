import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { API_BASE_URL } from '../constants/api.constants'
import {
  AddressActionResponse,
  AddressItem,
  CreateAddressRequest,
  CreateAddressResponse,
  UpdateAddressRequest
} from '../models/addressModels'

@Injectable({
  providedIn: 'root'
})
export class AddressesService {
  constructor(private http: HttpClient) {}

  getMyAddresses() {
    return this.http.get<AddressItem[]>(`${API_BASE_URL}/Address/my`)
  }

  createAddress(request: CreateAddressRequest) {
    return this.http.post<CreateAddressResponse>(
      `${API_BASE_URL}/Address/create`,
      request
    )
  }

  updateAddress(id: string, request: UpdateAddressRequest) {
    return this.http.put<void>(`${API_BASE_URL}/Address/${id}`, request)
  }

  deleteAddress(id: string) {
    return this.http.delete<AddressActionResponse | void>(
      `${API_BASE_URL}/Address/${id}`
    )
  }

  setDefaultAddress(id: string) {
    return this.http.patch<AddressActionResponse>(
      `${API_BASE_URL}/Address/${id}/set-default`,
      {}
    )
  }
}