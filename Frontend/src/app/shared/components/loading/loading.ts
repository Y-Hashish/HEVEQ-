import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loading',
  imports: [CommonModule],
  templateUrl: './loading.html',
  styleUrl: './loading.css'
})
export class Loading {
  @Input() message: string = 'جاري التحميل...';
  @Input() fullScreen: boolean = false;
}
