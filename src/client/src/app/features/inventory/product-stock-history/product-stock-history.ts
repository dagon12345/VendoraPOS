import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ProductService } from '../../../core/services/product';
import { StockMovementService } from '../../../core/services/stock-movement';
import { Product, ProductAuditLog } from '../../../core/models/product.model';
import { StockMovement, StockMovementReason } from '../../../core/models/stock-movement.model';

@Component({
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  selector: 'app-product-stock-history',
  styleUrl: './product-stock-history.scss',
  templateUrl: './product-stock-history.html',
})
export class ProductStockHistory implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly productService = inject(ProductService);
  private readonly movementService = inject(StockMovementService);

  private productId!: string;

  readonly product = signal<Product | null>(null);
  readonly movements = signal<StockMovement[]>([]);
  readonly auditLog = signal<ProductAuditLog[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly saving = signal(false);
  readonly selectedReason = signal<StockMovementReason>('Restock');

  readonly reasons: StockMovementReason[] = ['Restock', 'Adjustment', 'Waste'];

  // Restock/Waste: user enters a plain magnitude, direction is implied by the reason.
  // Adjustment: user enters a signed value directly, since a correction can go either way.
  readonly isSignedInput = computed(() => this.selectedReason() === 'Adjustment');

  readonly quantityLabel = computed(() => {
    switch (this.selectedReason()) {
      case 'Restock':
        return 'Quantity received';
      case 'Waste':
        return 'Quantity lost/wasted';
      default:
        return 'Quantity change (+/-)';
    }
  });

  readonly quantityHint = computed(() => {
    switch (this.selectedReason()) {
      case 'Restock':
        return 'Always adds to stock — enter how many units arrived.';
      case 'Waste':
        return 'Always removes from stock — enter how many units were lost, expired, or damaged.';
      default:
        return 'Use a positive number to increase stock, negative to decrease (e.g. after a stocktake).';
    }
  });

  readonly form = this.fb.nonNullable.group({
    quantity: this.fb.nonNullable.control(0, [Validators.required]),
    reason: this.fb.nonNullable.control<StockMovementReason>('Restock', [Validators.required]),
    note: this.fb.control<string | null>(null),
  });

  constructor() {
    this.form.controls.reason.valueChanges.subscribe((reason) => this.selectedReason.set(reason));
  }

  ngOnInit(): void {
    this.productId = this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.productService.getById(this.productId).subscribe({
      next: (product) => {
        this.product.set(product);
        forkJoin({
          movements: this.movementService.getHistory(this.productId),
          auditLog: this.productService.getAuditLog(this.productId),
        }).subscribe({
          next: ({ movements, auditLog }) => {
            this.movements.set(movements);
            this.auditLog.set(auditLog);
            this.loading.set(false);
          },
          error: () => {
            this.error.set('Could not load this product\'s history.');
            this.loading.set(false);
          },
        });
      },
      error: () => {
        this.error.set('Could not load this product.');
        this.loading.set(false);
      },
    });
  }

  private computeDelta(reason: StockMovementReason, quantity: number): number {
    if (reason === 'Restock') return Math.abs(quantity);
    if (reason === 'Waste') return -Math.abs(quantity);
    return quantity;
  }

  submit(): void {
    const value = this.form.getRawValue();
    const delta = this.computeDelta(value.reason, value.quantity);

    if (this.form.invalid || delta === 0) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    this.movementService.record(this.productId, { quantityDelta: delta, reason: value.reason, note: value.note }).subscribe({
      next: (movement) => {
        this.movements.update((list) => [movement, ...list]);
        this.product.update((p) => (p ? { ...p, quantityOnHand: p.quantityOnHand + movement.quantityDelta } : p));
        this.form.reset({ quantity: 0, reason: 'Restock', note: null });
        this.selectedReason.set('Restock');
        this.saving.set(false);
      },
      error: (err) => {
        this.error.set(
          typeof err?.error === 'string' ? err.error : 'Could not record this movement (check that stock would not go negative).',
        );
        this.saving.set(false);
      },
    });
  }

  /** Pre-fills the form with the exact opposite of a past movement, as a correcting Adjustment. */
  reverse(movement: StockMovement): void {
    this.form.setValue({
      quantity: -movement.quantityDelta,
      reason: 'Adjustment',
      note: `Correction: reversing ${movement.reason} of ${movement.quantityDelta} from ${new Date(movement.createdAtUtc).toLocaleString()}`,
    });
    this.selectedReason.set('Adjustment');
  }
}
