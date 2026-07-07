import { HttpClient, HttpParams } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { Observable } from 'rxjs'
import { API_BASE_URL } from '../constants/api.constants'
import { ProviderOperator } from '../models/operator.models'

// Backend: HEVEQ.Api/Controllers/ProviderOperatorsController.cs -> [Route("api/provider/operators")]
const BASE_URL = `${API_BASE_URL}/provider/operators`

@Injectable({
  providedIn: 'root'
})
export class Operators {
  constructor(private http: HttpClient) {}

  // GET /api/provider/operators?includeInactive=
  getMine(includeInactive = false): Observable<ProviderOperator[]> {
    const params = new HttpParams().set('includeInactive', includeInactive)
    return this.http.get<ProviderOperator[]>(BASE_URL, { params })
  }
}