import { CommonModule } from '@angular/common'
import { Component, OnInit } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { ActivatedRoute, RouterLink } from '@angular/router'
import { finalize } from 'rxjs'
import { Auth } from '../../../core/services/auth'

@Component({
  selector: 'app-email-confirmation',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './email-confirmation.html',
  styleUrl: './email-confirmation.css'
})
export class EmailConfirmation implements OnInit {
  email = ''
  userId = ''
  token = ''

  isConfirming = false
  isResending = false
  hasToken = false
  confirmed = false
  errorMessage = ''
  successMessage = ''

  constructor(
    private route: ActivatedRoute,
    private authService: Auth
  ) {}

  ngOnInit(): void {
    const query = this.route.snapshot.queryParamMap
    this.email = query.get('email') ?? ''
    this.userId = query.get('userId') ?? ''
    this.token = query.get('token') ?? ''
    this.hasToken = !!this.userId && !!this.token

    if (this.hasToken) {
      this.confirmEmail()
      return
    }

    this.successMessage = 'تم إنشاء الحساب. افتح بريدك الإلكتروني واضغط على رابط Confirm Email لتفعيل الحساب.'
  }

  confirmEmail(): void {
    if (!this.userId || !this.token) {
      this.errorMessage = 'رابط تفعيل البريد غير مكتمل.'
      return
    }

    this.isConfirming = true
    this.errorMessage = ''
    this.successMessage = ''

    this.authService
      .confirmEmail({ userId: this.userId, token: this.token })
      .pipe(finalize(() => (this.isConfirming = false)))
      .subscribe({
        next: response => {
          if (response.succeeded || response.isEmailConfirmed) {
            this.confirmed = true
            this.email = response.email || this.email
            this.successMessage = response.message || 'تم تفعيل البريد الإلكتروني بنجاح. يمكنك تسجيل الدخول الآن.'
            return
          }

          this.errorMessage = response.message || 'تعذر تفعيل البريد الإلكتروني.'
        },
        error: error => {
          this.errorMessage =
            error.error?.message ||
            error.error?.title ||
            (typeof error.error === 'string' ? error.error : null) ||
            'تعذر تفعيل البريد الإلكتروني. جرب إعادة إرسال رسالة التفعيل.'
        }
      })
  }

  resend(): void {
    if (!this.email.trim()) {
      this.errorMessage = 'اكتب البريد الإلكتروني أولاً لإعادة إرسال رسالة التفعيل.'
      return
    }

    this.isResending = true
    this.errorMessage = ''
    this.successMessage = ''

    this.authService
      .resendConfirmationEmail(this.email.trim())
      .pipe(finalize(() => (this.isResending = false)))
      .subscribe({
        next: response => {
          if (response.succeeded || response.emailConfirmationSent) {
            this.successMessage = response.message || 'تم إرسال رسالة التفعيل مرة أخرى.'
            return
          }

          this.errorMessage = response.message || 'تعذر إعادة إرسال رسالة التفعيل.'
        },
        error: error => {
          this.errorMessage =
            error.error?.message ||
            error.error?.title ||
            (typeof error.error === 'string' ? error.error : null) ||
            'تعذر إعادة إرسال رسالة التفعيل.'
        }
      })
  }
}
