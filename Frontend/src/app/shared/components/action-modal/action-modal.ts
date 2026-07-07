import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-action-modal',
  imports: [CommonModule, FormsModule],
  templateUrl: './action-modal.html',
  styleUrl: './action-modal.css'
})
export class ActionModal {
  @Input() isOpen: boolean = false;
  @Input() title: string = 'تأكيد الإجراء';
  @Input() description: string = '';
  @Input() confirmText: string = 'تأكيد';
  @Input() cancelText: string = 'إلغاء';
  @Input() requireNote: boolean = false;
  @Input() noteLabel: string = 'ملاحظات / سبب';
  @Input() confirmColor: 'primary' | 'danger' | 'success' = 'primary';
  
  @Output() confirm = new EventEmitter<string | undefined>();
  @Output() close = new EventEmitter<void>();

  noteText: string = '';
  isSubmitted: boolean = false;

  onClose() {
    this.noteText = '';
    this.isSubmitted = false;
    this.isOpen = false;
    this.close.emit();
  }

  onConfirm() {
    this.isSubmitted = true;
    if (this.requireNote && !this.noteText.trim()) return;
    
    this.confirm.emit(this.requireNote ? this.noteText : undefined);
    this.onClose();
  }
}
