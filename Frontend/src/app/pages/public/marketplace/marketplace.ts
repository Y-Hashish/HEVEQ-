import { CommonModule } from '@angular/common'
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { RouterLink } from '@angular/router'
import { Subject, Subscription, debounceTime, distinctUntilChanged, finalize } from 'rxjs'
import { CategoryDto } from '../../../core/models/category.models'
import { MarketplaceCondition, MarketplaceListing } from '../../../core/models/marketplace.models'
import { CategoriesService } from '../../../core/services/categories'
import { MarketplaceService } from '../../../core/services/marketplace'

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

  searchTerm = ''
  condition: MarketplaceCondition | '' = ''
  governorate = ''
  categoryId: number | null = null
  minPrice: number | null = null
  maxPrice: number | null = null

  categories: CategoryDto[] = []

  // Mirrors HEVEQ.Domain.Enums.ProductCondition + ArabicLocalizer.ToArabic(ProductCondition)
  readonly conditions: { value: MarketplaceCondition | ''; label: string }[] = [
    { value: '', label: 'كل الحالات' },
    { value: 'New', label: 'جديد' },
    { value: 'Excellent', label: 'ممتاز' },
    { value: 'Good', label: 'جيد' },
    { value: 'Fair', label: 'مقبول' },
    { value: 'Used', label: 'مستعمل' }
  ]

  // Canonical governorate names from EgyptianGeofenceValidator (this is what's stored in Listing.Governorate)
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

  constructor(
    private marketplaceService: MarketplaceService,
    private categoriesService: CategoriesService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadListings()
    this.loadCategories()

    this.searchSub = this.searchSubject
      .pipe(debounceTime(400), distinctUntilChanged())
      .subscribe(() => {
        this.page = 1
        this.loadListings()
      })
  }

  ngOnDestroy(): void {
    this.searchSub?.unsubscribe()
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

  loadListings(): void {
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
    return Math.max(Math.ceil(this.totalCount / this.pageSize), 1)
  }

  get pageNumbers(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1)
  }

  goToPage(targetPage: number): void {
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

  // Backend doesn't localize MarketplaceTransactionMethod (no ArabicLocalizer entry for it),
  // so it's translated client-side. Values: Pickup | Delivery | Both
  transactionMethodLabel(method: string): string {
    const labels: Record<string, string> = {
      Pickup: 'استلام من موقع البائع',
      Delivery: 'يوجد توصيل',
      Both: 'استلام أو توصيل'
    }

    return labels[method] ?? method
  }

  trackById(_index: number, item: MarketplaceListing): string {
    return item.id
  }
}