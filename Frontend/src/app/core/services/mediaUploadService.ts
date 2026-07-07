import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { API_BASE_URL } from '../constants/api.constants'
import { ImageUploadResponse } from '../models/mediaModels'

@Injectable({
  providedIn: 'root'
})
export class MediaUploadService {
  constructor(private http: HttpClient) {}

  uploadImage(file: File, purpose = 'documents', referenceId: string | null = null) {
    const formData = new FormData()

    formData.append('file', file)
    formData.append('purpose', purpose)

    if (referenceId) {
      formData.append('referenceId', referenceId)
    }

    return this.http.post<ImageUploadResponse>(
      `${API_BASE_URL}/media/images`,
      formData
    )
  }
}