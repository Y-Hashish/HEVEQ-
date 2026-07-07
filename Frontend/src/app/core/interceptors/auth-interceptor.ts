import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http'
import { inject } from '@angular/core'
import { catchError, switchMap, throwError } from 'rxjs'
import { TokenStorage } from '../services/token-storage'
import { Auth } from '../services/auth'

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenStorage = inject(TokenStorage)
  const authService = inject(Auth)

  const accessToken = tokenStorage.getAccessToken()

  const isAuthEndpoint =
    req.url.includes('/Auth/Login') ||
    req.url.includes('/Auth/Register') ||
    req.url.includes('/Auth/refresh-token') ||
    req.url.includes('/Auth/logout')

  const authReq = accessToken && !isAuthEndpoint
    ? req.clone({
        setHeaders: {
          Authorization: `Bearer ${accessToken}`
        }
      })
    : req

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isAuthEndpoint) {
        return throwError(() => error)
      }

      const refreshToken = tokenStorage.getRefreshToken()

      if (!refreshToken) {
        tokenStorage.clear()
        return throwError(() => error)
      }

      return authService.refreshToken(refreshToken).pipe(
        switchMap(response => {
          tokenStorage.saveAuth(response)

          const newAccessToken = response.accessToken

          const retryReq = req.clone({
            setHeaders: {
              Authorization: `Bearer ${newAccessToken}`
            }
          })

          return next(retryReq)
        }),
        catchError(refreshError => {
          tokenStorage.clear()
          return throwError(() => refreshError)
        })
      )
    })
  )
}