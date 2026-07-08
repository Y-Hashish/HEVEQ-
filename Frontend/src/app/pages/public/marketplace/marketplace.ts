import { CommonModule } from '@angular/common'
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { ActivatedRoute, Router, RouterLink } from '@angular/router'
import { Subject, Subscription, debounceTime, distinctUntilChanged, finalize } from 'rxjs'
import { CategoryDto } from '../../../core/models/category.models'
import { MarketplaceCondition, MarketplaceListing } from '../../../core/models/marketplace.models'
import { CategoriesService } from '../../../core/services/categories'
import { MarketplaceService } from '../../../core/services/marketplace'
import { AiSearchService } from '../../../core/services/aiSearchService'

@Component({
  selector: 'app-marketplace',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './marketplace.html',
  styleUrl: './marketplace.css'
})
export class Marketplace implements OnInit, OnDestroy {
  listings: MarketplaceListing[] = []
  totalCount = 0

  page = 1
  pageSize = 9

  isLoading = false
  errorMessage = ''
  isAiResultsMode = false
  aiResultsMessage = ''

  searchTerm = ''
  condition: MarketplaceCondition | '' = ''
  governorate = ''
  categoryId: number | null = null
  minPrice: number | null = null
  maxPrice: number | null = null

  categories: CategoryDto[] = []

  readonly conditions: { value: MarketplaceCondition | ''; label: string }[] = [
    { value: '', label: 'كل الحالات' },
    { value: 'New', label: 'جديد' },
    { value: 'Excellent', label: 'ممتاز' },
    { value: 'Good', label: 'جيد' },
    { value: 'Fair', label: 'مقبول' },
    { value: 'Used', label: 'مستعمل' }
  ]

  readonly governorates: { value: string; label: string }[] = [
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
    { value: 'Aswan', label: 'أسوان' },
    { value: 'Luxor', label: 'الأقصر' },
    { value: 'Red Sea', label: 'البحر الأحمر' },
    { value: 'New Valley', label: 'الوادي الجديد' },
    { value: 'Matrouh', label: 'مطروح' },
    { value: 'North Sinai', label: 'شمال سيناء' },
    { value: 'South Sinai', label: 'جنوب سيناء' }
  ]

  private readonly searchSubject = new Subject<string>()
  private searchSub?: Subscription
  private routeSub?: Subscription

  constructor(
    private marketplaceService: MarketplaceService,
    private categoriesService: CategoriesService,
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private aiSearchService: AiSearchService
  ) {}

  ngOnInit(): void {
    this.loadCategories()

    this.searchSub = this.searchSubject
      .pipe(debounceTime(400), distinctUntilChanged())
      .subscribe(() => {
        if (this.isAiResultsMode) return
        this.page = 1
        this.loadListings()
      })

    this.routeSub = this.route.queryParamMap.subscribe(params => {
      if (params.get('ai') === '1') {
        this.loadAiResults()
        return
      }

      this.isAiResultsMode = false
      this.aiResultsMessage = ''
      this.loadListings()
    })
  }

  ngOnDestroy(): void {
    this.searchSub?.unsubscribe()
    this.routeSub?.unsubscribe()
  }

  onSearchChange(): void {
    this.searchSubject.next(this.searchTerm)
  }

  onFilterChange(): void {
    this.page = 1
    this.loadListings()
  }

  resetFilters(): void {
    this.searchTerm = ''
    this.condition = ''
    this.governorate = ''
    this.categoryId = null
    this.minPrice = null
    this.maxPrice = null
    this.page = 1
    this.loadListings()
  }

  clearAiResults(): void {
    this.aiSearchService.clearResults()
    this.router.navigate(['/marketplace'])
  }

  loadCategories(): void {
    this.categoriesService.getCategories('Marketplace').subscribe({
      next: categories => {
        this.categories = categories ?? []
        this.cdr.detectChanges()
      },
      error: () => {
        this.categories = []
      }
    })
  }

  loadAiResults(): void {
    const response = this.aiSearchService.readResults()
    this.isAiResultsMode = true
    this.isLoading = false
    this.errorMessage = ''
    this.page = 1

    if (!response || !this.aiSearchService.isResultsReady(response)) {
      this.listings = []
      this.totalCount = 0
      this.aiResultsMessage = 'لا توجد نتائج بحث ذكي محفوظة. ابدأ بحثًا جديدًا من شريط البحث بالأعلى.'
      this.cdr.detectChanges()
      return
    }

    const marketItems = (response.results ?? [])
      .filter(item => !!item.marketplaceListing)
      .map(item => {
        const listing = item.marketplaceListing!
        return {
          id: listing.id,
          title: listing.title,
          price: listing.price,
          condition: listing.condition,
          conditionAr: this.conditionLabel(listing.condition),
          location: listing.distanceKm != null ? `يبعد ${listing.distanceKm.toFixed(1)} كم تقريبًا` : 'راجع تفاصيل البائع',
          coverPhotoUrl: null,
          sellerName: listing.sellerCompanyName,
          categoryName: 'ترشيح ذكي',
          transactionMethod: 'Pickup',
          averageRating: Number(listing.sellerAverageRating ?? 0),
          totalReviewsCount: 0,
          status: 'Active',
          statusAr: item.matchExplanation || 'مطابق لطلبك'
        } satisfies MarketplaceListing
      })

    this.listings = marketItems
    this.totalCount = marketItems.length
    this.aiResultsMessage = marketItems.length
      ? 'هذه النتائج مرشحة بواسطة البحث الذكي فقط بناءً على طلبك.'
      : 'لم يجد البحث الذكي منتجات مطابقة بدقة. جرّب توضيح نوع المنتج أو قطعة الغيار.'
    this.cdr.detectChanges()
  }

  loadListings(): void {
    if (this.isAiResultsMode) return

    this.isLoading = true
    this.errorMessage = ''
    this.cdr.detectChanges()

    this.marketplaceService
      .getListings({
        search: this.searchTerm.trim() || undefined,
        condition: this.condition || undefined,
        governorate: this.governorate || undefined,
        categoryId: this.categoryId ?? undefined,
        minPrice: this.minPrice ?? undefined,
        maxPrice: this.maxPrice ?? undefined,
        page: this.page,
        pageSize: this.pageSize
      })
      .pipe(
        finalize(() => {
          this.isLoading = false
          this.cdr.detectChanges()
        })
      )
      .subscribe({
        next: response => {
          this.listings = response.items ?? []
          this.totalCount = response.totalCount ?? 0
        },
        error: error => {
          this.listings = []
          this.totalCount = 0
          this.errorMessage =
            error.error?.message || error.error?.Message || 'تعذر تحميل بيانات سوق المعدات، برجاء المحاولة لاحقًا'
        }
      })
  }

  get totalPages(): number {
    if (this.isAiResultsMode) return 1
    return Math.max(Math.ceil(this.totalCount / this.pageSize), 1)
  }

  get pageNumbers(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1)
  }

  goToPage(targetPage: number): void {
    if (this.isAiResultsMode) return
    if (targetPage < 1 || targetPage > this.totalPages || targetPage === this.page) {
      return
    }

    this.page = targetPage
    this.loadListings()
  }

  ratingStars(rating: number | null): boolean[] {
    const rounded = Math.round(rating ?? 0)
    return Array.from({ length: 5 }, (_, i) => i < rounded)
  }

  transactionMethodLabel(method: string): string {
    const labels: Record<string, string> = {
      Pickup: 'استلام من موقع البائع',
      Delivery: 'يوجد توصيل',
      Both: 'استلام أو توصيل',
      Either: 'استلام أو توصيل'
    }

    return labels[method] ?? method
  }

  conditionLabel(condition: string): string {
    const found = this.conditions.find(item => item.value === condition)
    return found?.label ?? condition
  }

  trackById(_index: number, item: MarketplaceListing): string {
    return item.id
  }
}
