import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pagination.html',
  styleUrl: './pagination.css'
})
export class Pagination {
  @Input() page: number = 1;
  @Input() pageSize: number = 10;
  @Input() totalCount: number = 0;
  
  @Output() pageChange = new EventEmitter<number>();

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  nextPage() {
    if (this.page < this.totalPages) {
      this.pageChange.emit(this.page + 1);
    }
  }

  prevPage() {
    if (this.page > 1) {
      this.pageChange.emit(this.page - 1);
    }
  }
}
