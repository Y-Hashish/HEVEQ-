import { HttpClient, HttpParams } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { API_BASE_URL } from '../constants/api.constants'
import { CategoryDto, CategoryType } from '../models/category.models'

@Injectable({
  providedIn: 'root'
})
export class CategoriesService {
  constructor(private http: HttpClient) {}

  // GET /api/categories?type=Marketplace|Service (public)
  getCategories(type?: CategoryType) {
    let params = new HttpParams()

    if (type) {
      params = params.set('type', type)
    }

    return this.http.get<CategoryDto[]>(`${API_BASE_URL}/categories`, { params })
  }
}
