import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../constants/api.constants';
import { AdminDocument, PaginatedResponse } from '../models/admin.models';

export interface RejectDocumentRequest {
  reason: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminDocumentsService {
  constructor(private http: HttpClient) {}

  getDocuments(page: number = 1, pageSize: number = 10, status?: string, documentType?: string, role?: string, userId?: string) {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (status) params = params.set('status', status);
    if (documentType) params = params.set('documentType', documentType);
    if (role) params = params.set('role', role);
    if (userId) params = params.set('userId', userId);

    return this.http.get<PaginatedResponse<AdminDocument>>(`${API_BASE_URL}/admin/documents`, { params });
  }

  approveDocument(id: string) {
    return this.http.post(`${API_BASE_URL}/admin/documents/${id}/approve`, {});
  }

  rejectDocument(id: string, reason: string) {
    return this.http.post(`${API_BASE_URL}/admin/documents/${id}/reject`, { reason: reason });
  }
}
