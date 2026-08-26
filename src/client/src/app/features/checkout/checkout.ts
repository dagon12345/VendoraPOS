import { Component, ElementRef, HostListener, OnInit, QueryList, ViewChild, ViewChildren, computed, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductService } from '../../core/services/product';
import { SaleService } from '../../core/services/sale';
import { ToastService } from '../../core/services/toast';
import { StockRealtimeService } from '../../core/services/stock-realtime';
import { Product } from '../../core/models/product.model';
import { PaymentMethod } from '../../core/models/sale.model';
import { BarcodeDirective } from '../../shared/barcode/barcode.directive';
import { ConfirmDialog } from '../../shared/confirm-dialog/confirm-dialog';

interface CartLine {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  availableStock: number;
}

@Component({
  imports: [CommonModule, BarcodeDirective, ConfirmDialog],
  selector: 'app-checkout',
  styleUrl: './checkout.scss',
  templateUrl: './checkout.html',
})
export class Checkout implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly saleService = inject(SaleService);
  private readonly toastService = inject(ToastService);
  private readonly stockRealtime = inject(StockRealtimeService);

  @ViewChild('searchInput') searchInputRef?: ElementRef<HTMLInputElement>;
  @ViewChild('amountTenderedInput') amountTenderedInputRef?: ElementRef<HTMLInputElement>;
  @ViewChildren('qtyInput') qtyInputRefs!: QueryList<ElementRef<HTMLInputElement>>;

  readonly products = signal<Product[]>([]);
  readonly loading = signal(true);
  readonly loadError = signal<string | null>(null);
  readonly submitError = signal<string | null>(null);
  readonly saving = signal(false);
  readonly confirmingSale = signal(false);

  readonly searchTerm = signal('');
  readonly cart = signal<CartLine[]>([]);
  readonly paymentMethod = signal<PaymentMethod>('Cash');
  readonly amountTendered = signal<number>(0);

  readonly pageSize = signal(10);
  readonly currentPage = signal(1);
  readonly pageSizeOptions = [10, 25, 50, 100];

  readonly activeProducts = computed(() => this.products().filter((p) => p.isActive));

  readonly filteredProducts = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    if (!term) {
      return this.activeProducts();
    }
    return this.activeProducts().filter(
      (p) => p.sku.toLowerCase().includes(term) || p.name.toLowerCase().includes(term) || p.barcode?.toLowerCase() === term,
    );
  });

  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.filteredProducts().length / this.pageSize())));

  readonly pagedProducts = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    return this.filteredProducts().slice(start, start + this.pageSize());
  });

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

  readonly totalAmount = computed(() => this.cart().reduce((sum, line) => sum + line.unitPrice * line.quantity, 0));

  readonly changeDue = computed(() =>
    this.paymentMethod() === 'Cash' ? Math.max(0, this.amountTendered() - this.totalAmount()) : null,
  );

  readonly canSubmit = computed(
    () => this.cart().length > 0 && (this.paymentMethod() !== 'Cash' || this.amountTendered() >= this.totalAmount()),
  );

  readonly confirmSaleMessage = computed(() => {
    const items = this.cart().reduce((sum, l) => sum + l.quantity, 0);
    const itemWord = items === 1 ? 'item' : 'items';
    return `Complete this sale of ${items} ${itemWord} for ${this.totalAmount().toFixed(2)} (${this.paymentMethod()})?`;
  });

  constructor() {
    // Live stock updates from other terminals (a sale/void/restock elsewhere) - patches this
    // screen's own product list in place, same shape as the local patch confirmSubmit already
    // does for its own sale. Harmless no-op if the product isn't in this screen's list yet.
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
        // Search box doesn't exist in the DOM until loading flips to false; defer focus to
        // after Angular renders it, so a barcode scanner can start typing immediately without
        // the cashier needing to click anything first.
        setTimeout(() => this.focusAndSelect(this.searchInputRef));
      },
      error: () => {
        this.loadError.set('Could not reach the API. Is it running?');
        this.loading.set(false);
      },
    });
  }

  /** Ctrl+F search, Ctrl+Q the top cart line's quantity (optional - mouse/tap works too),
   *  Ctrl+M Amount tendered (Cash only), Ctrl+Enter completes the sale. */
  @HostListener('document:keydown', ['$event'])
  onGlobalKeydown(event: KeyboardEvent): void {
    if (event.ctrlKey && event.key.toLowerCase() === 'f') {
      event.preventDefault();
      this.focusAndSelect(this.searchInputRef);
    } else if (event.ctrlKey && event.key.toLowerCase() === 'q') {
      event.preventDefault();
      this.focusAndSelect(this.qtyInputRefs?.first);
    } else if (event.ctrlKey && event.key.toLowerCase() === 'm') {
      event.preventDefault();
      this.focusAndSelect(this.amountTenderedInputRef);
    } else if (event.ctrlKey && event.key === 'Enter') {
      event.preventDefault();
      this.requestSubmit();
    }
  }

  private focusAndSelect(ref: ElementRef<HTMLInputElement> | undefined): void {
    ref?.nativeElement.focus();
    ref?.nativeElement.select();
  }

  /** A barcode scanner is just a keyboard that types fast and hits Enter - it has no idea what's
   *  focused. So: clicking ANYWHERE on the page while checked out - a button, a pagination link,
   *  a product card, blank space, headings, whitespace, anything - snaps focus back to search
   *  afterward. Each click is judged only by what was actually clicked, so genuine text fields
   *  (quantity, amount tendered, the payment-method select) are the one exception: clicking
   *  those still focuses them normally for deliberate manual editing, and clicking directly from
   *  one such field to another (e.g. quantity straight into amount tendered) is unaffected too,
   *  since that next click's target is itself a real field. Only once you click somewhere that
   *  isn't a field does focus return to search - so search is always the resting state. This
   *  pauses while the confirm-sale dialog is open so a stray scan can't accidentally confirm the
   *  sale.
   */
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (this.confirmingSale()) {
      return;
    }
    const target = event.target as HTMLElement;
    const isTextEntry = target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.tagName === 'SELECT';
    if (isTextEntry || target === this.searchInputRef?.nativeElement) {
      return;
    }
    setTimeout(() => this.focusAndSelect(this.searchInputRef));
  }

  /** Selects the field's contents on focus, so its current value (a default "0", or an existing
   *  quantity) is replaced by typing rather than needing to be deleted first - used for both
   *  Amount tendered and each cart line's quantity field. */
  selectOnFocus(input: HTMLInputElement): void {
    input.select();
  }

  /** Enter in quantity/Amount tendered confirms the value (blurring the field) and immediately
   *  returns focus to search, matching the click-based behavior elsewhere - search is always
   *  the resting state once you're done with a field. */
  onFieldEnterConfirm(input: HTMLInputElement): void {
    input.blur();
    this.focusAndSelect(this.searchInputRef);
  }

  dismissSubmitError(): void {
    this.submitError.set(null);
  }

  onSearchInput(term: string): void {
    this.searchTerm.set(term);
    this.currentPage.set(1);
  }

  /** Enter in the search box (typed, or from a barcode scanner acting as a keyboard) adds the
   *  matching product without touching the mouse - an exact barcode match wins if there is one,
   *  otherwise a single remaining filtered result is added. */
  onSearchEnter(): void {
    const term = this.searchTerm().trim().toLowerCase();
    if (!term) {
      return;
    }

    const results = this.filteredProducts();
    const exactBarcodeMatch = results.find((p) => p.barcode?.toLowerCase() === term);
    const product = exactBarcodeMatch ?? (results.length === 1 ? results[0] : null);

    if (product) {
      this.addToCart(product);
      this.onSearchInput('');
    }
  }

  clearSearch(): void {
    this.onSearchInput('');
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

  onAmountTenderedInput(value: string): void {
    this.amountTendered.set(Number(value) || 0);
  }

  addToCart(product: Product): void {
    this.cart.update((lines) => {
      const existing = lines.find((l) => l.productId === product.id);
      if (existing) {
        return lines.map((l) => (l.productId === product.id ? { ...l, quantity: l.quantity + 1 } : l));
      }
      // New items go on top so the most recently scanned/tapped product is immediately visible
      // without scrolling down a growing list.
      return [
        { productId: product.id, productName: product.name, unitPrice: product.price, quantity: 1, availableStock: product.quantityOnHand },
        ...lines,
      ];
    });
    this.toastService.show(`${product.name} added`);
  }

  /** A cleared/empty field or a non-positive number falls back to 1 rather than removing the
   *  line - clearing the box to type a new value shouldn't delete the product. Removal only
   *  happens via the explicit Remove button. */
  updateQuantity(productId: string, quantity: number): void {
    const safeQuantity = quantity > 0 ? quantity : 1;
    this.cart.update((lines) => lines.map((l) => (l.productId === productId ? { ...l, quantity: safeQuantity } : l)));
  }

  /** Angular's [value] binding skips re-rendering the input when the bound number hasn't
   *  actually changed (e.g. clearing the field back down to the same "1" it already defaults
   *  to) - so on blur, force the visible field to match the real underlying quantity, rather
   *  than leaving it looking empty while the value underneath is already correct. */
  syncQtyDisplay(input: HTMLInputElement, line: CartLine): void {
    const current = this.cart().find((l) => l.productId === line.productId);
    if (current) {
      input.value = String(current.quantity);
    }
  }

  removeFromCart(productId: string): void {
    this.cart.update((lines) => lines.filter((l) => l.productId !== productId));
  }

  onPaymentMethodChange(method: PaymentMethod): void {
    this.paymentMethod.set(method);
  }

  requestSubmit(): void {
    if (!this.canSubmit() || this.saving()) {
      return;
    }
    this.confirmingSale.set(true);
  }

  cancelSubmit(): void {
    if (this.saving()) {
      return;
    }
    this.confirmingSale.set(false);
  }

  confirmSubmit(): void {
    this.saving.set(true);
    this.submitError.set(null);

    this.saleService
      .create({
        lines: this.cart().map((l) => ({ productId: l.productId, quantity: l.quantity })),
        paymentMethod: this.paymentMethod(),
        amountTendered: this.paymentMethod() === 'Cash' ? this.amountTendered() : null,
      })
      .subscribe({
        next: (sale) => {
          // Reflect the new stock levels immediately from the sale response itself, rather than
          // waiting for a manual reload (or an extra round-trip) to see the decremented numbers.
          const soldQuantities = new Map(sale.lines.map((l) => [l.productId, l.quantity]));
          this.products.update((products) =>
            products.map((p) =>
              soldQuantities.has(p.id) ? { ...p, quantityOnHand: p.quantityOnHand - soldQuantities.get(p.id)! } : p,
            ),
          );

          this.cart.set([]);
          this.amountTendered.set(0);
          this.paymentMethod.set('Cash');
          this.saving.set(false);
          this.confirmingSale.set(false);
          // Stay on Checkout for the next customer instead of navigating away - the receipt is
          // still reachable any time from Sales.
          this.toastService.show(`Sale completed — total ${sale.totalAmount.toFixed(2)}`, 3000);
          // Re-focus search immediately so a barcode scanner (or typing) can start the next
          // transaction right away, with no click needed in between customers.
          setTimeout(() => this.focusAndSelect(this.searchInputRef));
        },
        error: (err) => {
          this.submitError.set(typeof err?.error === 'string' ? err.error : 'Could not complete this sale.');
          this.saving.set(false);
          this.confirmingSale.set(false);
        },
      });
  }
}
