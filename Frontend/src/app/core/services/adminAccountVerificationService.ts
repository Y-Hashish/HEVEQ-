import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../constants/api.constants';
import { AccountVerification, PaginatedResponse } from '../models/admin.models';

@Injectable({
  providedIn: 'root'
})
export class AdminAccountVerificationService {
  constructor(private http: HttpClient) {}

  getPendingVerifications(page: number = 1, pageSize: number = 10) {
    return this.http.get<PaginatedResponse<AccountVerification>>(`${API_BASE_URL}/admin/account-verifications?page=${page}&pageSize=${pageSize}`);
  }
}
