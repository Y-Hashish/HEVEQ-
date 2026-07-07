import { inject } from '@angular/core'
import { CanActivateFn, Router } from '@angular/router'
import { TokenStorage } from '../services/token-storage'
import { UserRole } from '../models/auth.models'

export const roleGuard = (allowedRoles: UserRole[]): CanActivateFn => {
  return () => {
    const tokenStorage = inject(TokenStorage)
    const router = inject(Router)
    const role = tokenStorage.getRole()

    if (role && allowedRoles.includes(role)) {
      return true
    }

    router.navigate(['/'])
    return false
  }
}