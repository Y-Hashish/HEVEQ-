import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { Observable } from 'rxjs'
import { API_BASE_URL } from '../constants/api.constants'
import { ExtractedCoordinates } from '../helpers/mapLinkHelper'

export interface ResolveMapLinkResponse extends ExtractedCoordinates {
  resolvedUrl?: string | null
}

@Injectable({ providedIn: 'root' })
export class MapLinkResolverService {
  constructor(private http: HttpClient) {}

  resolve(url: string): Observable<ResolveMapLinkResponse> {
    return this.http.post<ResolveMapLinkResponse>(`${API_BASE_URL}/map-links/resolve`, { url })
  }
}
