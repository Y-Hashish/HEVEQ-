import { HttpClient } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { tap } from 'rxjs'
import { API_BASE_URL } from '../constants/api.constants'
import { AuthResponse, CurrentUserResponse, LoginRequest, RegisterRequest } from '../models/auth.models'
import { TokenStorage } from './token-storage'

@Injectable({
  providedIn: 'root'
})
export class Auth {
  constructor(
    private http: HttpClient,
    private tokenStorage: TokenStorage
  ) {}

  login(request: LoginRequest) {
    return this.http
      .post<AuthResponse>(`${API_BASE_URL}/Auth/Login`, request)
      .pipe(
        tap(response => {
          if (response.isAuthenticated) {
            this.tokenStorage.saveAuth(response)
          }
        })
      )
  }

  register(request: RegisterRequest) {
    return this.http
      .post<AuthResponse>(`${API_BASE_URL}/Auth/Register`, request)
      .pipe(
        tap(response => {
          if (response.isAuthenticated) {
            this.tokenStorage.saveAuth(response)
          }
        })
      )
  }


  confirmEmail(request: { userId: string; token: string }) {
    return this.http.post<AuthResponse>(`${API_BASE_URL}/Auth/confirm-email`, request)
  }

  resendConfirmationEmail(email: string) {
    return this.http.post<AuthResponse>(`${API_BASE_URL}/Auth/resend-confirmation-email`, { email })
  }

  refreshToken(refreshToken: string) {
    return this.http.post<AuthResponse>(`${API_BASE_URL}/Auth/refresh-token`, {
      refreshToken
    })
  }

  getMe() {
    return this.http
      .get<CurrentUserResponse>(`${API_BASE_URL}/Auth/me`)
      .pipe(
        tap(user => {
          this.tokenStorage.saveCurrentUser(user)
        })
      )
  }
  logout(): void {
    const refreshToken = this.tokenStorage.getRefreshToken()

    this.tokenStorage.clear()
    if (!refreshToken)
      return

    this.http.post(`${API_BASE_URL}/Auth/logout`, { refreshToken }).subscribe({
      next: () => {},
      error: () => {}
    })
  }
}