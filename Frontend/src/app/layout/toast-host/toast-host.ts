import { CommonModule } from '@angular/common'
import { Component } from '@angular/core'
import { Toast, ToastMessage } from '../../core/services/toast'

@Component({
  selector: 'app-toast-host',
  imports: [CommonModule],
  templateUrl: './toast-host.html',
  styleUrl: './toast-host.css'
})
export class ToastHost {
  constructor(public toast: Toast) {}

  trackById(_: number, item: ToastMessage): number {
    return item.id
  }
}