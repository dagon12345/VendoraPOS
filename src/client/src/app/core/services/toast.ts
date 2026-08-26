import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  message: string;
}

/** Brief, auto-dismissing confirmation messages (e.g. "Product added"). Not for errors that need
 *  to stay visible and be dismissed deliberately - those use the existing .error-banner pattern. */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 0;
  readonly toasts = signal<Toast[]>([]);

  show(message: string, durationMs = 2000): void {
    const id = this.nextId++;
    this.toasts.update((list) => [...list, { id, message }]);
    setTimeout(() => this.dismiss(id), durationMs);
  }

  dismiss(id: number): void {
    this.toasts.update((list) => list.filter((t) => t.id !== id));
  }
}
