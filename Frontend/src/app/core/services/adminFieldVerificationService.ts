import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../constants/api.constants';
import { AdminFieldVerification, AdminFieldVerificationDetails, PaginatedResponse } from '../models/admin.models';

export interface DispatchEmployeeRequest {
  employeeId: string;
  scheduledDate: string;
}

export interface RecordVerificationDecisionRequest {
  decision: 'Approve' | 'Reject';
  adminNote: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminFieldVerificationService {
  constructor(private http: HttpClient) {}

  getVerifications(page: number = 1, pageSize: number = 10, status?: string) {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (status) params = params.set('status', status);

    return this.http.get<PaginatedResponse<AdminFieldVerification>>(`${API_BASE_URL}/admin/field-verifications`, { params });
  }

  getVerificationDetails(id: string) {
    return this.http.get<AdminFieldVerificationDetails>(`${API_BASE_URL}/admin/field-verifications/${id}`);
  }

  dispatchEmployee(id: string, request: DispatchEmployeeRequest) {
    return this.http.post(`${API_BASE_URL}/admin/field-verifications/${id}/dispatch`, request);
  }

  recordDecision(id: string, request: RecordVerificationDecisionRequest) {
    return this.http.post(`${API_BASE_URL}/admin/field-verifications/${id}/decision`, request);
  }
}
