import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { SaleService } from '../../../core/services/sale';
import { Sale } from '../../../core/models/sale.model';

@Component({
  imports: [CommonModule, RouterLink],
  selector: 'app-sale-history',
  styleUrl: './sale-history.scss',
  templateUrl: './sale-history.html',
})
export class SaleHistory implements OnInit {
  readonly sales = signal<Sale[]>([]);
  readonly loading = signal(true);
  readonly loadError = signal<string | null>(null);

  readonly pageSize = signal(10);
  readonly currentPage = signal(1);
  readonly pageSizeOptions = [10, 25, 50, 100];

  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.sales().length / this.pageSize())));

  readonly pagedSales = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    return this.sales().slice(start, start + this.pageSize());
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

  constructor(private readonly saleService: SaleService) {}

  ngOnInit(): void {
    this.saleService.getAll().subscribe({
      next: (sales) => {
        this.sales.set(sales);
        this.loading.set(false);
      },
      error: () => {
        this.loadError.set('Could not reach the API. Is it running?');
        this.loading.set(false);
      },
    });
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
}
