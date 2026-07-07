import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { API_BASE_URL } from '../constants/api.constants'
import { FieldVisit } from '../models/admin.models'

export interface SubmitFieldVisitEvidenceRequest {
  employeeNotes: string
  outcome: 'JobConfirmed' | 'JobNotConfirmed' | 'PartiallyConfirmed' | 'Inconclusive' | string
  photoUrls: string[]
}

export interface UpdateFieldVisitStatusRequest {
  status: 'Dispatched' | 'OnSite' | 'Completed' | 'FailedAccess' | string
}

export interface FieldVisitActionResponse {
  isSuccess: boolean
  statusCode: number
  message: string
}

@Injectable({ providedIn: 'root' })
export class EmployeeFieldVisitsService {
  constructor(private http: HttpClient) {}

  getMyVisits() {
    return this.http.get<FieldVisit[]>(`${API_BASE_URL}/employee/field-visits/my`)
  }

  getVisitDetails(id: string) {
    return this.http.get<FieldVisit>(`${API_BASE_URL}/employee/field-visits/${id}`)
  }

  updateStatus(id: string, status: UpdateFieldVisitStatusRequest['status']) {
    return this.http.patch<FieldVisitActionResponse>(`${API_BASE_URL}/employee/field-visits/${id}/status`, { status })
  }

  submitEvidence(id: string, request: SubmitFieldVisitEvidenceRequest) {
    return this.http.post<FieldVisitActionResponse>(`${API_BASE_URL}/employee/field-visits/${id}/evidence`, request)
  }
}
