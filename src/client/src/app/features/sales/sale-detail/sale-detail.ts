import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { SaleService } from '../../../core/services/sale';
import { Sale, SaleLine } from '../../../core/models/sale.model';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';

@Component({
  imports: [CommonModule, RouterLink, ConfirmDialog],
  selector: 'app-sale-detail',
  styleUrl: './sale-detail.scss',
  templateUrl: './sale-detail.html',
})
export class SaleDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly saleService = inject(SaleService);

  private saleId!: string;

  readonly sale = signal<Sale | null>(null);
  readonly loading = signal(true);
  readonly loadError = signal<string | null>(null);
  readonly actionError = signal<string | null>(null);
  readonly confirmingVoid = signal(false);
  readonly voiding = signal(false);
  readonly confirmingRestore = signal(false);
  readonly restoring = signal(false);

  // Partial return of a single line - a separate flow from the whole-sale Void above.
  readonly returningLine = signal<SaleLine | null>(null);
  readonly returnQuantity = signal(1);
  readonly returnReason = signal<string | null>(null);
  readonly returningLineSaving = signal(false);

  // Undoing a mistaken partial return - the line-level counterpart to Restore above.
  readonly restoringLine = signal<SaleLine | null>(null);
  readonly restoreLineQuantity = signal(1);
  readonly restoreLineReason = signal<string | null>(null);
  readonly restoringLineSaving = signal(false);

  ngOnInit(): void {
    this.saleId = this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.saleService.getById(this.saleId).subscribe({
      next: (sale) => {
        this.sale.set(sale);
        this.loading.set(false);
      },
      error: () => {
        this.loadError.set('Could not load this sale.');
        this.loading.set(false);
      },
    });
  }

  dismissActionError(): void {
    this.actionError.set(null);
  }

  requestVoid(): void {
    this.confirmingVoid.set(true);
  }

  cancelVoid(): void {
    if (this.voiding()) {
      return;
    }
    this.confirmingVoid.set(false);
  }

  confirmVoid(): void {
    this.voiding.set(true);
    this.actionError.set(null);
    this.saleService.void(this.saleId, {}).subscribe({
      next: (sale) => {
        this.sale.set(sale);
        this.voiding.set(false);
        this.confirmingVoid.set(false);
      },
      error: (err) => {
        this.actionError.set(typeof err?.error === 'string' ? err.error : 'Could not void this sale.');
        this.voiding.set(false);
        this.confirmingVoid.set(false);
      },
    });
  }

  requestRestore(): void {
    this.confirmingRestore.set(true);
  }

  cancelRestore(): void {
    if (this.restoring()) {
      return;
    }
    this.confirmingRestore.set(false);
  }

  /** Undoes a mistaken void - re-deducts stock and flips the sale back to active. Can fail if the
   *  stock is no longer available (e.g. sold to someone else since the void). */
  confirmRestore(): void {
    this.restoring.set(true);
    this.actionError.set(null);
    this.saleService.restore(this.saleId).subscribe({
      next: (sale) => {
        this.sale.set(sale);
        this.restoring.set(false);
        this.confirmingRestore.set(false);
      },
      error: (err) => {
        this.actionError.set(typeof err?.error === 'string' ? err.error : 'Could not restore this sale.');
        this.restoring.set(false);
        this.confirmingRestore.set(false);
      },
    });
  }

  /** A customer returning just one product out of a multi-item sale - voids only that line's
   *  quantity, leaving the rest of the transaction and its stock untouched. */
  requestVoidLine(line: SaleLine): void {
    this.returningLine.set(line);
    this.returnQuantity.set(1);
    this.returnReason.set(null);
  }

  cancelVoidLine(): void {
    if (this.returningLineSaving()) {
      return;
    }
    this.returningLine.set(null);
  }

  onReturnQuantityInput(value: string): void {
    const line = this.returningLine();
    const max = line ? line.activeQuantity : 1;
    const parsed = Math.floor(Number(value) || 1);
    this.returnQuantity.set(Math.min(Math.max(1, parsed), max));
  }

  onReturnReasonInput(value: string): void {
    this.returnReason.set(value || null);
  }

  confirmVoidLine(): void {
    const line = this.returningLine();
    if (!line) {
      return;
    }

    this.returningLineSaving.set(true);
    this.actionError.set(null);
    this.saleService
      .voidLine(this.saleId, { productId: line.productId, quantity: this.returnQuantity(), reason: this.returnReason() })
      .subscribe({
        next: (sale) => {
          this.sale.set(sale);
          this.returningLineSaving.set(false);
          this.returningLine.set(null);
        },
        error: (err) => {
          this.actionError.set(typeof err?.error === 'string' ? err.error : 'Could not return this item.');
          this.returningLineSaving.set(false);
          this.returningLine.set(null);
        },
      });
  }

  /** Undoes a mistaken return - re-deducts the stock and reduces that line's returned count. */
  requestRestoreLine(line: SaleLine): void {
    this.restoringLine.set(line);
    this.restoreLineQuantity.set(line.voidedQuantity);
    this.restoreLineReason.set(null);
  }

  cancelRestoreLine(): void {
    if (this.restoringLineSaving()) {
      return;
    }
    this.restoringLine.set(null);
  }

  onRestoreLineQuantityInput(value: string): void {
    const line = this.restoringLine();
    const max = line ? line.voidedQuantity : 1;
    const parsed = Math.floor(Number(value) || 1);
    this.restoreLineQuantity.set(Math.min(Math.max(1, parsed), max));
  }

  onRestoreLineReasonInput(value: string): void {
    this.restoreLineReason.set(value || null);
  }

  confirmRestoreLine(): void {
    const line = this.restoringLine();
    if (!line) {
      return;
    }

    this.restoringLineSaving.set(true);
    this.actionError.set(null);
    this.saleService
      .restoreLine(this.saleId, { productId: line.productId, quantity: this.restoreLineQuantity(), reason: this.restoreLineReason() })
      .subscribe({
        next: (sale) => {
          this.sale.set(sale);
          this.restoringLineSaving.set(false);
          this.restoringLine.set(null);
        },
        error: (err) => {
          this.actionError.set(typeof err?.error === 'string' ? err.error : 'Could not restore this return.');
          this.restoringLineSaving.set(false);
          this.restoringLine.set(null);
        },
      });
  }
}
