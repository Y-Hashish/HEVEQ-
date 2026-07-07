import { CommonModule } from '@angular/common'
import { Component, HostListener, OnDestroy, OnInit } from '@angular/core'
import { NavigationEnd, Router, RouterLink } from '@angular/router'
import { filter, Subscription } from 'rxjs'
import { TokenStorage } from '../../core/services/token-storage'
import { Auth } from '../../core/services/auth'
import { NotificationsService } from '../../core/services/notificationsService'

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class Navbar implements OnInit, OnDestroy {
  isLoggedIn = false
  userName = ''
  role = ''
  isDropdownOpen = false
  currentUrl = ''
  themeIcon = '🌙'
  unreadNotifications = 0

  private authSub?: Subscription
  private routerSub?: Subscription
  private unreadSub?: Subscription

  constructor(
    private tokenStorage: TokenStorage,
    private authService: Auth,
    private router: Router,
    private notificationsService: NotificationsService
  ) {}

  ngOnInit(): void {
    this.currentUrl = this.router.url

    this.authSub = this.tokenStorage.authState$.subscribe(auth => {
      this.isLoggedIn = !!auth?.accessToken
      this.userName = auth?.userName || 'User'
      this.role = auth?.roles?.[0]?.toLowerCase() || ''
      this.isDropdownOpen = false
    })

    this.unreadSub = this.notificationsService.unreadCount$.subscribe(count => {
      this.unreadNotifications = count || 0
    })

    this.routerSub = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(event => {
        this.currentUrl = (event as NavigationEnd).urlAfterRedirects
        this.isDropdownOpen = false
      })

    const savedTheme = localStorage.getItem('theme') || 'light'
    document.documentElement.setAttribute('data-theme', savedTheme)
    this.themeIcon = savedTheme === 'dark' ? '☀️' : '🌙'
  }

  ngOnDestroy(): void {
    this.authSub?.unsubscribe()
    this.routerSub?.unsubscribe()
    this.unreadSub?.unsubscribe()
  }

  get isAuth(): boolean {
    return this.isLoggedIn
  }

  get isMarketplaceActive(): boolean {
    return this.currentUrl.startsWith('/marketplace')
  }

  get isServicesActive(): boolean {
    return this.currentUrl.startsWith('/services')
  }

  get isInternalDashboardPage(): boolean {
    const internalRoutes = [
      '/dashboard',
      '/bookings',
      '/wallet',
      '/messages',
      '/profile',
      '/saved-addresses',
      '/settings',
      '/provider-dashboard',
      '/active-jobs',
      '/equipment',
      '/booking-requests',
      '/earnings',
      '/create-listing',
      '/admin',
      '/admin-users',
      '/admin-tickets',
      '/employee/field-visits',
      '/support-tickets'
    ]

    return internalRoutes.some(route => this.currentUrl.startsWith(route))
  }

  get showNavLinks(): boolean {
    if (!this.isLoggedIn) {
      return true
    }

    if (this.role === 'customer') {
      return true
    }

    if (
      ['provider', 'admin', 'employee'].includes(this.role) &&
      this.isInternalDashboardPage
    ) {
      return false
    }

    return true
  }

  get showMobileButton(): boolean {
    return this.isLoggedIn && this.isInternalDashboardPage
  }

  get userInitials(): string {
    if (!this.userName) {
      return 'US'
    }

    const parts = this.userName.trim().split(' ')

    if (parts.length === 1) {
      return parts[0].substring(0, 2).toUpperCase()
    }

    return `${parts[0][0]}${parts[1][0]}`.toUpperCase()
  }

  get dashboardLink(): string {
    if (this.role === 'customer') {
      return '/dashboard'
    }

    if (this.role === 'provider') {
      return '/provider-dashboard'
    }

    if (this.role === 'admin' || this.role === 'employee') {
      return '/admin'
    }

    return '/'
  }

  get dashboardLabel(): string {
    if (this.role === 'admin' || this.role === 'employee') {
      return 'مركز القيادة'
    }

    if (this.role === 'provider') {
      return 'لوحة المزود'
    }

    return 'لوحة التحكم'
  }

  toggleDropdown(event?: Event): void {
    event?.stopPropagation()
    this.isDropdownOpen = !this.isDropdownOpen
  }

  @HostListener('document:click')
  closeDropdown(): void {
    this.isDropdownOpen = false
  }

  toggleTheme(): void {
    const currentTheme = localStorage.getItem('theme') || 'light'
    const nextTheme = currentTheme === 'dark' ? 'light' : 'dark'

    localStorage.setItem('theme', nextTheme)
    document.documentElement.setAttribute('data-theme', nextTheme)

    this.themeIcon = nextTheme === 'dark' ? '☀️' : '🌙'
  }

  toggleMobileMenu(): void {
    document.body.classList.toggle('sidebar-open')
  }

  logout(): void {
    this.authService.logout()
    this.isDropdownOpen = false
    this.router.navigate(['/login'])
  }
}