import { Component } from '@angular/core'
import { CommonModule } from '@angular/common'
import { FormsModule } from '@angular/forms'
import { Router, RouterLink } from '@angular/router'
import { Auth } from '../../../core/services/auth'
import { TokenStorage } from '../../../core/services/token-storage'
import { finalize } from 'rxjs'
import { getErrorMessage } from '../../../core/helpers/errorMessageHelper'

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  email = ''
  password = ''
  errorMessage = ''
  isLoading = false
  showPassword = false

  constructor(
    private authService: Auth,
    private tokenStorage: TokenStorage,
    private router: Router
  ) {}

  submit(): void {
    this.isLoading = true
    this.errorMessage = ''

    this.authService.login({
      email: this.email,
      password: this.password
    }).subscribe({
      next: response => {
        if (!response.isAuthenticated) {
          this.isLoading = false
          this.errorMessage = response.message || 'بيانات الدخول غير صحيحة'
          return
        }

        this.authService.getMe().pipe(finalize(() => (this.isLoading = false))).subscribe({
          next: () => this.navigateAfterLogin(),
          error: () => this.navigateAfterLogin()
        })
      },
      error: error => {
        this.isLoading = false

        if (error.status === 403 && error.error?.requiresEmailConfirmation) {
          this.router.navigate(['/auth/confirm-email'], { queryParams: { email: this.email, pending: true } })
          return
        }

        this.errorMessage = getErrorMessage(error, 'حدث خطأ أثناء تسجيل الدخول')
      }
    })
  }

  private navigateAfterLogin(): void {
    const role = this.tokenStorage.getRole()

    if (role === 'customer') {
      this.router.navigate(['/'])
      return
    }

    if (role === 'provider') {
      this.router.navigate(['/provider-dashboard'])
      return
    }

    if (role === 'admin' || role === 'employee') {
      this.router.navigate(['/admin'])
      return
    }

    this.router.navigate(['/'])
  }
}
