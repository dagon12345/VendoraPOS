import { Component, OnInit, computed, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../../core/services/product';
import { StockRealtimeService } from '../../../core/services/stock-realtime';
import { Product } from '../../../core/models/product.model';
import { LOW_STOCK_THRESHOLD, EXPIRING_SOON_DAYS, isExpired, isExpiringSoon, isLowStock } from '../../../core/constants/inventory-thresholds';

@Component({
  imports: [CommonModule, RouterLink],
  selector: 'app-stock-alerts',
  styleUrl: './stock-alerts.scss',
  templateUrl: './stock-alerts.html',
})
export class StockAlerts implements OnInit {
  readonly products = signal<Product[]>([]);
  readonly loading = signal(true);
  readonly loadError = signal<string | null>(null);

  readonly lowStockThreshold = LOW_STOCK_THRESHOLD;
  readonly expiringSoonDays = EXPIRING_SOON_DAYS;

  // Only active products need restocking/expiry attention - a deactivated product isn't being sold.
  readonly lowStockProducts = computed(() =>
    this.products()
      .filter((p) => p.isActive && isLowStock(p.quantityOnHand))
      .sort((a, b) => a.quantityOnHand - b.quantityOnHand),
  );

  readonly expiredProducts = computed(() =>
    this.products()
      .filter((p) => p.isActive && p.expiryDate && isExpired(p.expiryDate))
      .sort((a, b) => a.expiryDate!.localeCompare(b.expiryDate!)),
  );

  readonly expiringSoonProducts = computed(() =>
    this.products()
      .filter((p) => p.isActive && p.expiryDate && isExpiringSoon(p.expiryDate))
      .sort((a, b) => a.expiryDate!.localeCompare(b.expiryDate!)),
  );

  constructor(
    private readonly productService: ProductService,
    private readonly stockRealtime: StockRealtimeService,
  ) {
    // Live stock updates from other terminals - a sale elsewhere can push a product onto (or off)
    // this list without needing a reload.
    effect(() => {
      const change = this.stockRealtime.lastChange();
      if (!change) return;
      this.products.update((products) =>
        products.map((p) => (p.id === change.productId ? { ...p, quantityOnHand: change.quantityOnHand } : p)),
      );
    });
  }

  ngOnInit(): void {
    this.productService.getAll().subscribe({
      next: (products) => {
        this.products.set(products);
        this.loading.set(false);
      },
      error: () => {
        this.loadError.set('Could not reach the API. Is it running?');
        this.loading.set(false);
      },
    });
  }
}
