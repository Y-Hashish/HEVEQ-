import { Injectable } from '@angular/core'
import { BehaviorSubject } from 'rxjs'

export type ToastType = 'success' | 'danger' | 'info'

export interface ToastMessage {
  id: number
  type: ToastType
  text: string
}

@Injectable({
  providedIn: 'root'
})
export class Toast {
  private nextId = 1
  private messagesSubject = new BehaviorSubject<ToastMessage[]>([])

  messages$ = this.messagesSubject.asObservable()

  success(text: string, durationMs = 4000): void {
    this.push('success', text, durationMs)
  }

  error(text: string, durationMs = 5000): void {
    this.push('danger', text, durationMs)
  }

  info(text: string, durationMs = 4000): void {
    this.push('info', text, durationMs)
  }

  dismiss(id: number): void {
    this.messagesSubject.next(this.messagesSubject.value.filter((m) => m.id !== id))
  }

  private push(type: ToastType, text: string, durationMs: number): void {
    const id = this.nextId++
    this.messagesSubject.next([...this.messagesSubject.value, { id, type, text }])
    setTimeout(() => this.dismiss(id), durationMs)
  }
}