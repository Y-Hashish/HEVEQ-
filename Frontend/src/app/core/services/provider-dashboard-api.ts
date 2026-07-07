import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { Observable } from 'rxjs'
import { API_BASE_URL } from '../constants/api.constants'
import { ProviderDashboardSummary } from '../models/provider-dashboard.models'

// Backend: HEVEQ.Api/Controllers/ProviderDashboardController.cs -> [Route("api/provider/dashboard")]
const BASE_URL = `${API_BASE_URL}/provider/dashboard`

@Injectable({
  providedIn: 'root'
})
export class ProviderDashboardApi {
  constructor(private http: HttpClient) {}

  // GET /api/provider/dashboard/summary -> GetProviderDashboardSummaryQuery
  getSummary(): Observable<ProviderDashboardSummary> {
    return this.http.get<ProviderDashboardSummary>(`${BASE_URL}/summary`)
  }
}