import { Component, OnInit, OnDestroy, HostListener, ElementRef, inject } from '@angular/core'
import { Router, NavigationEnd, RouterLink, RouterLinkActive } from '@angular/router'
import { CommonModule } from '@angular/common'
import { filter, Subscription } from 'rxjs'
import { TokenStorage } from '../../core/services/token-storage'

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class Sidebar implements OnInit, OnDestroy {
  currentUrl = ''
  isCollapsed = false
  private routerSub?: Subscription
  private elementRef = inject(ElementRef)

  constructor(
    private router: Router,
    private tokenStorage: TokenStorage
  ) {
    this.currentUrl = this.router.url
    this.isCollapsed = localStorage.getItem('sharegear_sidebar_collapsed') === 'true'
  }

  ngOnInit() {
    this.updateBodyClass()

    this.routerSub = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(event => {
        this.currentUrl = (event as NavigationEnd).urlAfterRedirects
        
        // Auto-collapse sidebar on route changes for small screens
        if (window.innerWidth <= 1024) {
          this.isCollapsed = true
          localStorage.setItem('sharegear_sidebar_collapsed', 'true')
        }
        
        this.updateBodyClass()
      })
  }

  ngOnDestroy() {
    this.routerSub?.unsubscribe()
    // Clean up body class when component is destroyed
    document.body.classList.remove('has-sidebar')
  }

  get role() {
    return this.tokenStorage.getRole()
  }

  get showSidebar(): boolean {
    const url = this.currentUrl

    if (!this.role) {
      return false
    }

    if (this.role === 'customer') {
      return ['/dashboard', '/bookings', '/marketplace-orders', '/support-tickets', '/notifications', '/wallet', '/messages', '/profile', '/settings'].some(x => url.startsWith(x))
    }

    if (this.role === 'provider') {
      return ['/provider-dashboard', '/booking-requests', '/active-jobs', '/equipment', '/marketplace-sales', '/earnings', '/operators', '/create-listing', '/messages'].some(x => url.startsWith(x))
    }

    if (this.role === 'admin' || this.role === 'employee') {
      return url.startsWith('/admin') || url.startsWith('/employee/field-visits')
    }

    return false
  }

  toggleCollapse() {
    this.isCollapsed = !this.isCollapsed
    localStorage.setItem('sharegear_sidebar_collapsed', String(this.isCollapsed))
    this.updateBodyClass()
  }

  updateBodyClass() {
    if (this.showSidebar && !this.isCollapsed) {
      document.body.classList.add('has-sidebar')
    } else {
      document.body.classList.remove('has-sidebar')
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const targetElement = event.target as HTMLElement
    
    // Catch mobile menu hamburger clicks
    const clickedMobileToggle = targetElement.closest('.mobile-menu-btn')
    if (clickedMobileToggle) {
      // Force expanding the sidebar when opening mobile menu
      this.isCollapsed = false
      localStorage.setItem('sharegear_sidebar_collapsed', 'false')
      this.updateBodyClass()
      return
    }

    // Only collapse on outside click if the sidebar is visible and expanded
    if (!this.showSidebar || this.isCollapsed) {
      return
    }

    // Check if the click target is inside the sidebar component
    const clickedInside = this.elementRef.nativeElement.contains(event.target)

    // Check if clicked target is a button that opens/toggles the sidebar
    const clickedToggle = targetElement.closest('.sidebar-expand-trigger') || 
                          targetElement.closest('.sidebar-collapse-btn')

    if (!clickedInside && !clickedToggle) {
      this.isCollapsed = true
      localStorage.setItem('sharegear_sidebar_collapsed', 'true')
      this.updateBodyClass()
    }
  }
}