import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { API_BASE_URL } from '../constants/api.constants'
import {
  DocumentItem,
  UploadDocumentRequest,
  UploadDocumentResponse
} from '../models/documentModels'

@Injectable({
  providedIn: 'root'
})
export class DocumentsService {
  constructor(private http: HttpClient) {}

  getMyDocuments() {
    return this.http.get<DocumentItem[]>(`${API_BASE_URL}/documents/my`)
  }

  uploadDocument(request: UploadDocumentRequest) {
    return this.http.post<UploadDocumentResponse>(
      `${API_BASE_URL}/documents`,
      request
    )
  }
}