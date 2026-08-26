import { Component, EventEmitter, HostListener, Input, Output } from '@angular/core';

@Component({
  selector: 'app-confirm-dialog',
  styleUrl: './confirm-dialog.scss',
  templateUrl: './confirm-dialog.html',
})
export class ConfirmDialog {
  @Input() open = false;
  @Input() title = 'Are you sure?';
  @Input() message = '';
  @Input() confirmLabel = 'Confirm';
  @Input() confirmingLabel = 'Working…';
  @Input() cancelLabel = 'Cancel';
  @Input() confirming = false;
  @Input() danger = true;

  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.open && !this.confirming) {
      this.cancelled.emit();
    }
  }

  @HostListener('document:keydown.enter')
  onEnter(): void {
    if (this.open && !this.confirming) {
      this.confirmed.emit();
    }
  }

  onBackdropClick(): void {
    if (!this.confirming) {
      this.cancelled.emit();
    }
  }
}
