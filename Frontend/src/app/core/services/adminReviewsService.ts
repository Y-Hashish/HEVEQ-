import { HttpClient, HttpParams } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { API_BASE_URL } from '../constants/api.constants'
import { PaginatedResponse } from '../models/admin.models'

export interface AdminPendingReview {
  id: string
  reviewerName: string
  reviewedUserName: string
  rating: number
  comment?: string
  moderationStatus: string
  sourceType: string
  sourceTitle: string
  bookingId?: string
  marketplaceOrderId?: string
  createdAt: string
}

@Injectable({ providedIn: 'root' })
export class AdminReviewsService {
  constructor(private http: HttpClient) {}

  getPending(page = 1, pageSize = 10) {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize)

    return this.http.get<PaginatedResponse<AdminPendingReview>>(`${API_BASE_URL}/admin/reviews/pending`, { params })
  }

  approve(id: string) {
    return this.http.post<{ message: string }>(`${API_BASE_URL}/admin/reviews/${id}/approve`, {})
  }

  reject(id: string) {
    return this.http.post<{ message: string }>(`${API_BASE_URL}/admin/reviews/${id}/reject`, {})
  }
}
