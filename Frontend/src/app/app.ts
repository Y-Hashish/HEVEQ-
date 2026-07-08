import { ApplicationRef, Component, OnDestroy, OnInit, signal } from '@angular/core'
import { RouterOutlet } from '@angular/router'
import { Subscription } from 'rxjs'
import { Footer } from './layout/footer/footer'
import { Navbar } from './layout/navbar/navbar'
import { Sidebar } from './layout/sidebar/sidebar'
import { Auth } from './core/services/auth'
import { TokenStorage } from './core/services/token-storage'
import { RealtimeService } from './core/services/realtimeService'
import { NotificationsService } from './core/services/notificationsService'
import { ToastComponent } from './shared/components/toast/toast.component'

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Footer, Navbar, Sidebar, ToastComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit, OnDestroy {
  protected readonly title = signal('HEVEQ')
  private authSub?: Subscription
  private uiTickTimer?: ReturnType<typeof setInterval>

  constructor(
    private authService: Auth,
    private tokenStorage: TokenStorage,
    private realtimeService: RealtimeService,
    private notificationsService: NotificationsService,
    private appRef: ApplicationRef
  ) {}

  ngOnInit(): void {
    this.startShortUiRefreshLoop()
    this.authSub = this.tokenStorage.authState$.subscribe(auth => {
      if (auth?.accessToken) {
        this.realtimeService.connect()
        this.notificationsService.loadUnreadCount()
      } else {
        this.realtimeService.disconnect()
        this.notificationsService.setUnreadCount(0)
      }
    })

    if (!this.tokenStorage.isLoggedIn()) {
      return
    }

    this.authService.getMe().subscribe({
      next: () => {
        this.realtimeService.connect()
        this.notificationsService.loadUnreadCount()
      },
      error: () => {
        this.tokenStorage.clear()
        this.realtimeService.disconnect()
      }
    })
  }

  ngOnDestroy(): void {
    this.authSub?.unsubscribe()

    if (this.uiTickTimer) {
      clearInterval(this.uiTickTimer)
    }
  }

  private startShortUiRefreshLoop(): void {
    let ticks = 0

    this.uiTickTimer = setInterval(() => {
      ticks += 1

      try {
        this.appRef.tick()
      } catch {
        // Angular may already be checking the view. Ignore this safe refresh.
      }

      if (ticks >= 20 && this.uiTickTimer) {
        clearInterval(this.uiTickTimer)
        this.uiTickTimer = undefined
      }
    }, 250)
  }
}
