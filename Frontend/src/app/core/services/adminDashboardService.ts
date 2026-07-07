import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { API_BASE_URL } from '../constants/api.constants';
import { DashboardSummary, PaginatedResponse, PendingAction } from '../models/admin.models';

@Injectable({
  providedIn: 'root'
})
export class AdminDashboardService {
  constructor(private http: HttpClient) {}

  getSummary() {
    return this.http.get<DashboardSummary>(`${API_BASE_URL}/admin/dashboard/summary`);
  }

  getPendingActions() {
    return this.http.get<PaginatedResponse<PendingAction>>(`${API_BASE_URL}/admin/pending-actions`);
  }
}
