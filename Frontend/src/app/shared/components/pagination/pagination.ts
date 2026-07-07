import { Component, Input, Output, EventEmitter } from '@angular/core'
import { CommonModule } from '@angular/common'

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pagination.html',
  styleUrl: './pagination.css'
})
export class Pagination {
  @Input() page = 1
  @Input() pageSize = 10
  @Input() totalCount = 0

  @Output() pageChange = new EventEmitter<number>()

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize))
  }

  get pages(): number[] {
    const total = this.totalPages
    const start = Math.max(1, this.page - 2)
    const end = Math.min(total, start + 4)
    const safeStart = Math.max(1, end - 4)
    const result: number[] = []
    for (let i = safeStart; i <= end; i += 1) result.push(i)
    return result
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages && page !== this.page) {
      this.pageChange.emit(page)
    }
  }

  nextPage() {
    this.goToPage(this.page + 1)
  }

  prevPage() {
    this.goToPage(this.page - 1)
  }
}
