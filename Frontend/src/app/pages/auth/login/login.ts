import { Component } from '@angular/core'
import { CommonModule } from '@angular/common'
import { FormsModule } from '@angular/forms'
import { Router, RouterLink } from '@angular/router'
import { Auth } from '../../../core/services/auth'
import { TokenStorage } from '../../../core/services/token-storage'

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
        this.isLoading = false

        if (!response.isAuthenticated) {
          this.errorMessage = response.message || 'بيانات الدخول غير صحيحة'
          return
        }

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
      },
      error: error => {
        this.isLoading = false

        if (error.status === 403 && error.error?.requiresEmailConfirmation) {
          this.router.navigate(['/auth/confirm-email'], { queryParams: { email: this.email, pending: true } })
          return
        }

        this.errorMessage =
          error.error?.message ||
          error.error?.title ||
          (typeof error.error === 'string' ? error.error : null) ||
          'حدث خطأ أثناء تسجيل الدخول'
      }
    })
  }
}