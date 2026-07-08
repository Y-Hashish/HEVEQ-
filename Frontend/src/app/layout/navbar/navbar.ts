import { CommonModule } from '@angular/common'
import { Component, HostListener, OnDestroy, OnInit } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { NavigationEnd, Router, RouterLink } from '@angular/router'
import { filter, Subscription } from 'rxjs'
import { TokenStorage } from '../../core/services/token-storage'
import { Auth } from '../../core/services/auth'
import { NotificationsService } from '../../core/services/notificationsService'
import { AiConversationTurn, AiSearchResponse, AiSearchService } from '../../core/services/aiSearchService'
import { getErrorMessage } from '../../core/helpers/errorMessageHelper'

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class Navbar implements OnInit, OnDestroy {
  isLoggedIn = false
  userName = ''
  role = ''
  isDropdownOpen = false
  currentUrl = ''
  themeIcon = 'fa-solid fa-moon'
  unreadNotifications = 0

  aiQuery = ''
  aiChatInput = ''
  aiChatOpen = false
  aiLoading = false
  aiError = ''
  aiSessionId = this.createSessionId()
  aiConversation: AiConversationTurn[] = []

  private authSub?: Subscription
  private routerSub?: Subscription
  private unreadSub?: Subscription

  constructor(
    private tokenStorage: TokenStorage,
    private authService: Auth,
    private router: Router,
    private notificationsService: NotificationsService,
    private aiSearchService: AiSearchService
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
    this.themeIcon = savedTheme === 'dark' ? 'fa-solid fa-sun' : 'fa-solid fa-moon'
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

  get showAiSearch(): boolean {
    return !this.currentUrl.startsWith('/login') &&
      !this.currentUrl.startsWith('/register') &&
      !this.currentUrl.startsWith('/auth/confirm-email')
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

  get showAccountVerificationLink(): boolean {
    return this.role === 'customer' || this.role === 'provider'
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

    this.themeIcon = nextTheme === 'dark' ? 'fa-solid fa-sun' : 'fa-solid fa-moon'
  }

  toggleMobileMenu(): void {
    document.body.classList.toggle('sidebar-open')
  }

  submitAiSearch(): void {
    const query = this.aiQuery.trim()
    if (!query || this.aiLoading) return

    this.aiSessionId = this.createSessionId()
    this.aiConversation = []
    this.aiError = ''
    this.aiLoading = true

    this.aiSearchService.search({
      rawQuery: query,
      conversationHistory: [],
      sessionId: this.aiSessionId
    }).subscribe({
      next: response => {
        this.aiLoading = false
        this.handleAiResponse(response, query)
      },
      error: error => {
        this.aiLoading = false
        this.aiError = getErrorMessage(error, 'تعذر تنفيذ البحث الذكي، حاول مرة أخرى')
        this.aiChatOpen = true
      }
    })
  }

  sendAiChatMessage(): void {
    const message = this.aiChatInput.trim()
    if (!message || this.aiLoading) return

    const history = [...this.aiConversation]
    this.aiChatInput = ''
    this.aiError = ''
    this.aiLoading = true

    this.aiSearchService.search({
      rawQuery: message,
      conversationHistory: history,
      sessionId: this.aiSessionId
    }).subscribe({
      next: response => {
        this.aiLoading = false
        this.handleAiResponse(response, message)
      },
      error: error => {
        this.aiLoading = false
        this.aiError = getErrorMessage(error, 'تعذر متابعة المحادثة، حاول مرة أخرى')
      }
    })
  }

  closeAiChat(): void {
    this.aiChatOpen = false
    this.aiError = ''
  }

  private handleAiResponse(response: AiSearchResponse, userMessage: string): void {
    this.aiConversation = [
      ...this.aiConversation,
      { role: 'user', content: userMessage }
    ]

    if (this.aiSearchService.needsClarification(response)) {
      const message = response.clarification?.message || 'ممكن توضح طلبك أكثر؟'
      this.aiConversation = [
        ...this.aiConversation,
        { role: 'assistant', content: message }
      ]
      this.aiChatOpen = true
      return
    }

    if (this.aiSearchService.isResultsReady(response)) {
      this.aiSearchService.saveResults(response)
      const hasResults = (response.results?.length ?? 0) > 0
      this.aiConversation = [
        ...this.aiConversation,
        {
          role: 'assistant',
          content: hasResults
            ? 'تم العثور على نتائج مناسبة، جاري فتح صفحة النتائج.'
            : 'لم أجد نتائج مطابقة بدقة، يمكنك تجربة وصف مختلف.'
        }
      ]

      const isMarketplace = this.aiSearchService.isMarketplaceTarget(response.intent?.target)
      this.aiChatOpen = false
      this.router.navigate([isMarketplace ? '/marketplace' : '/services'], {
        queryParams: { ai: '1', sessionId: this.aiSessionId }
      })
      return
    }

    this.aiError = 'تعذر فهم رد البحث الذكي، حاول مرة أخرى.'
    this.aiChatOpen = true
  }

  private createSessionId(): string {
    if (typeof crypto !== 'undefined' && 'randomUUID' in crypto) {
      return crypto.randomUUID()
    }

    return `ai-${Date.now()}-${Math.random().toString(16).slice(2)}`
  }

  logout(): void {
    this.authService.logout()
    this.isDropdownOpen = false
    this.router.navigate(['/login'])
  }
}
