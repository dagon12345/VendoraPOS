import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../../core/services/product';
import { Product } from '../../../core/models/product.model';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';

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

  constructor(private readonly productService: ProductService) {}

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

  dismissActionError(): void {
    this.actionError.set(null);
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
