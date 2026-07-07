import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-empty-state',
  imports: [CommonModule],
  templateUrl: './empty-state.html',
  styleUrl: './empty-state.css'
})
export class EmptyState {
  @Input() title: string = 'لا توجد بيانات';
  @Input() message: string = 'لم يتم العثور على أي بيانات لعرضها حالياً.';
}
