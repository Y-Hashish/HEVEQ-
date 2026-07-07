import { Injectable } from '@angular/core'
import { BehaviorSubject } from 'rxjs'
import { AuthResponse, CurrentUserResponse, UserRole } from '../models/auth.models'

const AUTH_KEY = 'sharegear_auth'
const CURRENT_USER_KEY = 'sharegear_current_user'

@Injectable({
  providedIn: 'root'
})
export class TokenStorage {
  private authSubject = new BehaviorSubject<AuthResponse | null>(this.getAuthFromStorage())
  private currentUserSubject = new BehaviorSubject<CurrentUserResponse | null>(this.getCurrentUserFromStorage())

  authState$ = this.authSubject.asObservable()
  currentUserState$ = this.currentUserSubject.asObservable()

  saveAuth(auth: AuthResponse): void {
    this.clearOldKeys()
    localStorage.setItem(AUTH_KEY, JSON.stringify(auth))
    this.authSubject.next(auth)
  }

  updateTokens(accessToken: string, refreshToken: string): void {
    const currentAuth = this.getAuth()

    if (!currentAuth) {
      return
    }

    const updatedAuth: AuthResponse = {
      ...currentAuth,
      accessToken,
      refreshToken,
      isAuthenticated: true
    }

    localStorage.setItem(AUTH_KEY, JSON.stringify(updatedAuth))
    this.authSubject.next(updatedAuth)
  }

  saveCurrentUser(user: CurrentUserResponse): void {
    localStorage.setItem(CURRENT_USER_KEY, JSON.stringify(user))
    this.currentUserSubject.next(user)
  }

  getAuth(): AuthResponse | null {
    return this.authSubject.value
  }

  getCurrentUser(): CurrentUserResponse | null {
    return this.currentUserSubject.value
  }

  private getAuthFromStorage(): AuthResponse | null {
    const value = localStorage.getItem(AUTH_KEY)

    if (!value) {
      return null
    }

    try {
      return JSON.parse(value) as AuthResponse
    } catch {
      localStorage.removeItem(AUTH_KEY)
      return null
    }
  }

  private getCurrentUserFromStorage(): CurrentUserResponse | null {
    const value = localStorage.getItem(CURRENT_USER_KEY)

    if (!value) {
      return null
    }

    try {
      return JSON.parse(value) as CurrentUserResponse
    } catch {
      localStorage.removeItem(CURRENT_USER_KEY)
      return null
    }
  }

  getAccessToken(): string | null {
    return this.getAuth()?.accessToken ?? null
  }

  getRefreshToken(): string | null {
    return this.getAuth()?.refreshToken ?? null
  }

  getDisplayName(): string {
    return (
      this.getCurrentUser()?.displayName ||
      this.getAuth()?.displayName ||
      this.getAuth()?.userName ||
      ''
    )
  }

  getEmail(): string {
    return this.getCurrentUser()?.email || this.getAuth()?.email || ''
  }

  getRole(): UserRole | null {
    const role =
      this.getCurrentUser()?.role ||
      this.getAuth()?.roles?.[0]

    if (!role) {
      return null
    }

    return role.toLowerCase() as UserRole
  }

  isLoggedIn(): boolean {
    return !!this.getAccessToken()
  }

  clear(): void {
    localStorage.removeItem(AUTH_KEY)
    localStorage.removeItem(CURRENT_USER_KEY)
    this.clearOldKeys()
    this.authSubject.next(null)
    this.currentUserSubject.next(null)
  }

  private clearOldKeys(): void {
    localStorage.removeItem('gp_access_token')
    localStorage.removeItem('gp_refresh_token')
    localStorage.removeItem('role')
    localStorage.removeItem('token')
    localStorage.removeItem('userRole')
    localStorage.removeItem('isLoggedIn')
  }
}