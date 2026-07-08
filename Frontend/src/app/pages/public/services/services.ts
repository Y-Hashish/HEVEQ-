import { CommonModule } from '@angular/common'
import { Component, OnInit } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { HttpErrorResponse } from '@angular/common/http'
import { ServiceListings } from '../../../core/services/service-listings'
import { CategoriesService } from '../../../core/services/categories'
import { Toast } from '../../../core/services/toast'
import { extractErrorMessage } from '../../../core/models/api-error.models'
import { Category } from '../../../core/models/category.models'
import { PublicServiceListing, PublicServiceListingFilters } from '../../../core/models/service-listing.models'
import { AiSearchService } from '../../../core/services/aiSearchService'

@Component({
  selector: 'app-services',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './services.html',
  styleUrl: './services.css'
})
export class Services implements OnInit {
  listings: PublicServiceListing[] = []
  categories: Category[] = []
  isLoading = false
  totalCount = 0
  isAiResultsMode = false
  aiResultsMessage = ''

  readonly governorates = [
    { value: 'Cairo', label: 'القاهرة' },
    { value: 'Alexandria', label: 'الإسكندرية' },
    { value: 'Giza', label: 'الجيزة' },
    { value: 'Qalyubia', label: 'القليوبية' },
    { value: 'Port Said', label: 'بورسعيد' },
    { value: 'Suez', label: 'السويس' },
    { value: 'Ismailia', label: 'الإسماعيلية' },
    { value: 'Dakahlia', label: 'الدقهلية' },
    { value: 'Gharbia', label: 'الغربية' },
    { value: 'Menoufia', label: 'المنوفية' },
    { value: 'Beheira', label: 'البحيرة' },
    { value: 'Kafr El Sheikh', label: 'كفر الشيخ' },
    { value: 'Damietta', label: 'دمياط' },
    { value: 'Sharqia', label: 'الشرقية' },
    { value: 'Faiyum', label: 'الفيوم' },
    { value: 'Beni Suef', label: 'بني سويف' },
    { value: 'Minya', label: 'المنيا' },
    { value: 'Asyut', label: 'أسيوط' },
    { value: 'Sohag', label: 'سوهاج' },
    { value: 'Qena', label: 'قنا' },
    { value: 'Luxor', label: 'الأقصر' },
    { value: 'Aswan', label: 'أسوان' },
    { value: 'Red Sea', label: 'البحر الأحمر' },
    { value: 'New Valley', label: 'الوادي الجديد' },
    { value: 'Matrouh', label: 'مطروح' },
    { value: 'North Sinai', label: 'شمال سيناء' },
    { value: 'South Sinai', label: 'جنوب سيناء' }
  ]

  filters: PublicServiceListingFilters = {
    search: '',
    categoryId: undefined,
    governorate: '',
    minRate: undefined,
    maxRate: undefined,
    page: 1,
    pageSize: 9
  }

  constructor(
    private serviceListings: ServiceListings,
    private categoriesService: CategoriesService,
    private toast: Toast,
    private router: Router,
    private route: ActivatedRoute,
    private aiSearchService: AiSearchService
  ) {}

  ngOnInit(): void {
    this.categoriesService.getCategories('Service').subscribe({
      next: categories => (this.categories = categories),
      error: (err: HttpErrorResponse) => this.toast.error(extractErrorMessage(err))
    })

    this.route.queryParamMap.subscribe(params => {
      if (params.get('ai') === '1') {
        this.loadAiResults()
        return
      }

      this.isAiResultsMode = false
      this.aiResultsMessage = ''
      this.loadListings()
    })
  }

  loadListings(): void {
    this.isLoading = true

    const requestFilters: PublicServiceListingFilters = {
      ...this.filters,
      governorate: this.resolveGovernorateValue(this.filters.governorate || '') || undefined
    }

    this.serviceListings.getPublicList(requestFilters).subscribe({
      next: result => {
        this.listings = result.items
        this.totalCount = result.totalCount
        this.isLoading = false
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading = false
        this.toast.error(extractErrorMessage(err))
      }
    })
  }

  loadAiResults(): void {
    const response = this.aiSearchService.readResults()
    this.isAiResultsMode = true
    this.isLoading = false
    this.filters.page = 1

    if (!response || !this.aiSearchService.isResultsReady(response)) {
      this.listings = []
      this.totalCount = 0
      this.aiResultsMessage = 'لا توجد نتائج بحث ذكي محفوظة. ابدأ بحثًا جديدًا من شريط البحث بالأعلى.'
      return
    }

    const serviceItems = (response.results ?? [])
      .filter(item => !!item.serviceListing)
      .map(item => {
        const listing = item.serviceListing!
        const distanceText = listing.distanceKm != null ? `يبعد ${listing.distanceKm.toFixed(1)} كم تقريبًا` : 'راجع تفاصيل التوفر والموقع'
        return {
          id: listing.id,
          title: listing.title,
          categoryName: 'ترشيح ذكي',
          providerCompany: listing.providerCompanyName,
          coverPhotoUrl: null,
          hourlyRate: listing.hourlyRate,
          dailyRate: listing.dailyRate ?? null,
          minimumBookingHours: 1,
          location: distanceText,
          serviceRadiusKm: 0,
          averageRating: Number(listing.providerAverageRating ?? 0),
          totalReviewsCount: 0,
          trustLevel: item.matchExplanation || 'مطابق لطلبك',
          availabilityText: item.matchExplanation || 'اضغط لعرض التفاصيل والحجز',
          status: 'Active',
          statusAr: 'نشط'
        } satisfies PublicServiceListing
      })

    this.listings = serviceItems
    this.totalCount = serviceItems.length
    this.aiResultsMessage = serviceItems.length
      ? `هذه النتائج مرشحة بواسطة البحث الذكي فقط بناءً على طلبك.`
      : 'لم يجد البحث الذكي خدمات مطابقة بدقة. جرّب توضيح نوع المعدة والموقع.'
  }

  private resolveGovernorateValue(input: string): string {
    const value = input.trim()
    if (!value) return ''

    const match = this.governorates.find(g =>
      g.value.toLowerCase() === value.toLowerCase() ||
      g.label === value ||
      g.label.includes(value)
    )

    return match?.value || value
  }

  search(): void {
    this.isAiResultsMode = false
    this.aiResultsMessage = ''
    this.filters.page = 1
    this.router.navigate([], { queryParams: {} })
    this.loadListings()
  }

  clearAiResults(): void {
    this.aiSearchService.clearResults()
    this.router.navigate(['/services'])
  }

  nextPage(): void {
    if (this.isAiResultsMode) return
    const page = this.filters.page ?? 1
    const pageSize = this.filters.pageSize ?? 9
    if (page * pageSize >= this.totalCount) return

    this.filters.page = page + 1
    this.loadListings()
  }

  previousPage(): void {
    if (this.isAiResultsMode) return
    const page = this.filters.page ?? 1
    if (page <= 1) return

    this.filters.page = page - 1
    this.loadListings()
  }

  viewListing(id: string): void {
    this.router.navigate(['/service-details', id])
  }
}
