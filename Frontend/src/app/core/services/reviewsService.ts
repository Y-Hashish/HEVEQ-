import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { API_BASE_URL } from '../constants/api.constants'
import {
  SubmitReviewRequest,
  SubmitReviewResponse,
  UserReviewsResponse
} from '../models/reviewModels'

@Injectable({
  providedIn: 'root'
})
export class ReviewsService {
  constructor(private http: HttpClient) {}

  submitReview(request: SubmitReviewRequest) {
    return this.http.post<SubmitReviewResponse>(
      `${API_BASE_URL}/reviews`,
      request
    )
  }

  getReviewsForUser(userId: string) {
    return this.http.get<UserReviewsResponse>(
      `${API_BASE_URL}/reviews/for-user/${userId}`
    )
  }
}