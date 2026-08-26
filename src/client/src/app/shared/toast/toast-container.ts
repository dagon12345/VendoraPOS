import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../core/services/toast';

@Component({
  selector: 'app-toast-container',
  imports: [CommonModule],
  styleUrl: './toast-container.scss',
  templateUrl: './toast-container.html',
})
export class ToastContainer {
  readonly toastService = inject(ToastService);
}
