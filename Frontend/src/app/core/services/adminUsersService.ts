import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../constants/api.constants';
import { PaginatedResponse } from '../models/admin.models';

export interface AdminUser {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  roles: string[];
  isActive: boolean;
  isAvailableForDispatch?: boolean;
  createdAt: string;
}

export interface UpdateUserStatusRequest {
  isActive: boolean;
  reason?: string;
}

export interface UpdateUserStatusResponse {
  id: string;
  isActive: boolean;
  statusText: string;
  statusAr: string;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminUsersService {
  constructor(private http: HttpClient) {}

  getUsers(page: number = 1, pageSize: number = 10, role?: string, isActive?: boolean) {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (role) {
      params = params.set('role', role);
    }
    if (isActive !== undefined) {
      params = params.set('isActive', isActive.toString());
    }

    // Backend route is api/AdminUsers/users
    return this.http.get<PaginatedResponse<AdminUser>>(`${API_BASE_URL}/AdminUsers/users`, { params });
  }

  updateStatus(id: string, request: UpdateUserStatusRequest) {
    return this.http.patch<UpdateUserStatusResponse>(`${API_BASE_URL}/AdminUsers/users/${id}/status`, request);
  }
}
