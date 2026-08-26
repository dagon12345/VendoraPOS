import { Component, OnInit, computed, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../../core/services/product';
import { StockRealtimeService } from '../../../core/services/stock-realtime';
import { Product } from '../../../core/models/product.model';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';
import { isExpired, isExpiringSoon, isLowStock } from '../../../core/constants/inventory-thresholds';

@Component({
  imports: [CommonModule, RouterLink, ConfirmDialog],
  selector: 'app-product-list',
  styleUrl: './product-list.scss',
  templateUrl: './product-list.html',
})
export class ProductList implements OnInit {
  readonly products = signal<Product[]>([]);
  readonly loading = signal(true);
  // loadError blocks the page (nothing to show without data); actionError is a
  // dismissible banner over an already-loaded list (e.g. a failed activate/deactivate).
  readonly loadError = signal<string | null>(null);
  readonly actionError = signal<string | null>(null);
  readonly togglingId = signal<string | null>(null);
  readonly productPendingDeactivate = signal<Product | null>(null);
  readonly deactivating = signal(false);

  readonly searchTerm = signal('');
  readonly pageSize = signal(10);
  readonly currentPage = signal(1);
  readonly pageSizeOptions = [10, 25, 50, 100];

  readonly filteredProducts = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    if (!term) {
      return this.products();
    }
    return this.products().filter(
      (p) => p.sku.toLowerCase().includes(term) || p.name.toLowerCase().includes(term),
    );
  });

  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.filteredProducts().length / this.pageSize())));

  readonly pagedProducts = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    return this.filteredProducts().slice(start, start + this.pageSize());
  });

  /** Page numbers to render, collapsing long runs into '...' so this stays usable with many pages. */
  readonly pageNumbers = computed<(number | '...')[]>(() => {
    const total = this.totalPages();
    const current = this.currentPage();
    const pages: (number | '...')[] = [];

    for (let i = 1; i <= total; i++) {
      if (i === 1 || i === total || Math.abs(i - current) <= 1) {
        pages.push(i);
      } else if (pages[pages.length - 1] !== '...') {
        pages.push('...');
      }
    }
    return pages;
  });

  constructor(
    private readonly productService: ProductService,
    private readonly stockRealtime: StockRealtimeService,
  ) {
    // Live stock updates from other terminals (a sale/void/restock elsewhere) - patches this
    // screen's own product list in place. Harmless no-op if the product isn't currently loaded.
    effect(() => {
      const change = this.stockRealtime.lastChange();
      if (!change) return;
      this.products.update((products) =>
        products.map((p) => (p.id === change.productId ? { ...p, quantityOnHand: change.quantityOnHand } : p)),
      );
    });
  }

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.loadError.set(null);
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

  readonly isLowStock = isLowStock;
  readonly isExpired = isExpired;
  readonly isExpiringSoon = isExpiringSoon;

  dismissActionError(): void {
    this.actionError.set(null);
  }

  onSearchChange(term: string): void {
    this.searchTerm.set(term);
    this.currentPage.set(1);
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
  }

  goToPage(page: number): void {
    this.currentPage.set(Math.min(Math.max(1, page), this.totalPages()));
  }

  previousPage(): void {
    this.goToPage(this.currentPage() - 1);
  }

  nextPage(): void {
    this.goToPage(this.currentPage() + 1);
  }

  /** Activating needs no confirmation; deactivating does, since it hides the product from sale. */
  requestToggleActive(product: Product): void {
    if (product.isActive) {
      this.productPendingDeactivate.set(product);
    } else {
      this.setActive(product, true);
    }
  }

  cancelDeactivate(): void {
    if (this.deactivating()) {
      return;
    }
    this.productPendingDeactivate.set(null);
  }

  confirmDeactivate(): void {
    const product = this.productPendingDeactivate();
    if (!product) {
      return;
    }
    this.deactivating.set(true);
    this.setActive(product, false, () => {
      this.deactivating.set(false);
      this.productPendingDeactivate.set(null);
    });
  }

  private setActive(product: Product, isActive: boolean, onDone?: () => void): void {
    this.togglingId.set(product.id);
    this.productService.setActive(product.id, isActive).subscribe({
      next: (updated) => {
        this.products.update((products) => products.map((p) => (p.id === updated.id ? updated : p)));
        this.togglingId.set(null);
        onDone?.();
      },
      error: (err) => {
        this.actionError.set(typeof err?.error === 'string' ? err.error : 'Could not update this product.');
        this.togglingId.set(null);
        onDone?.();
      },
    });
  }
}
