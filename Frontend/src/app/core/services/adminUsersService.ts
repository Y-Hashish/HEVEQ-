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

export interface CreateStaffRequest {
  firstName: string;
  lastName: string;
  userName: string;
  email: string;
  password: string;
  phoneNumber?: string;
  role: 'Admin' | 'Employee';
  department?: string;
  assignedGovernorate?: string;
  isAvailableForDispatch?: boolean;
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

  getUsers(page: number = 1, pageSize: number = 10, role?: string, isActive?: boolean, search?: string) {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (role) {
      params = params.set('role', role);
    }
    if (isActive !== undefined) {
      params = params.set('isActive', isActive.toString());
    }
    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }

    // Backend route is api/AdminUsers/users
    return this.http.get<PaginatedResponse<AdminUser>>(`${API_BASE_URL}/AdminUsers/users`, { params });
  }

  updateStatus(id: string, request: UpdateUserStatusRequest) {
    return this.http.patch<UpdateUserStatusResponse>(`${API_BASE_URL}/AdminUsers/users/${id}/status`, request);
  }

  createStaff(request: CreateStaffRequest) {
    if (request.role === 'Employee') {
      return this.http.post(`${API_BASE_URL}/admin/employees`, {
        firstName: request.firstName,
        lastName: request.lastName,
        userName: request.userName,
        email: request.email,
        password: request.password,
        phoneNumber: request.phoneNumber,
        department: request.department,
        assignedGovernorate: request.assignedGovernorate,
        isAvailableForDispatch: request.isAvailableForDispatch === true
      });
    }

    return this.http.post(`${API_BASE_URL}/AdminUsers/admins`, {
      firstName: request.firstName,
      lastName: request.lastName,
      userName: request.userName,
      email: request.email,
      password: request.password,
      phoneNumber: request.phoneNumber
    });
  }
}
