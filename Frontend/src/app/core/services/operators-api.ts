import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { API_BASE_URL } from '../constants/api.constants'
import { Operator, OperatorFormPayload } from '../models/operator.models'

/**
 * Confirmed against ProviderOperatorsController.cs:
 *   GET    /api/provider/operators
 *   POST   /api/provider/operators
 *   PUT    /api/provider/operators/{id}
 *   DELETE /api/provider/operators/{id}
 * Full CRUD exists on the backend — this service now covers all four.
 */
@Injectable({
  providedIn: 'root'
})
export class OperatorsApi {
  constructor(private http: HttpClient) {}

  getMine() {
    return this.http.get<Operator[]>(`${API_BASE_URL}/provider/operators`)
  }

  create(payload: OperatorFormPayload) {
    return this.http.post<string>(`${API_BASE_URL}/provider/operators`, payload)
  }

  update(id: string, payload: OperatorFormPayload & { isActive: boolean }) {
    return this.http.put<void>(`${API_BASE_URL}/provider/operators/${id}`, payload)
  }

  delete(id: string) {
    return this.http.delete<void>(`${API_BASE_URL}/provider/operators/${id}`)
  }
}