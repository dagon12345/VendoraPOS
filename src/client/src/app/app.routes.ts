import { Routes } from '@angular/router';
import { ProductList } from './features/inventory/product-list/product-list';
import { ProductForm } from './features/inventory/product-form/product-form';
import { ProductStockHistory } from './features/inventory/product-stock-history/product-stock-history';
import { StockAlerts } from './features/inventory/stock-alerts/stock-alerts';
import { Help } from './features/help/help';
import { Checkout } from './features/checkout/checkout';
import { SaleHistory } from './features/sales/sale-history/sale-history';
import { SaleDetail } from './features/sales/sale-detail/sale-detail';

export const routes: Routes = [
  { path: '', redirectTo: 'products', pathMatch: 'full' },
  { path: 'products', component: ProductList },
  { path: 'products/alerts', component: StockAlerts },
  { path: 'products/new', component: ProductForm },
  { path: 'products/:id/edit', component: ProductForm },
  { path: 'products/:id/history', component: ProductStockHistory },
  { path: 'checkout', component: Checkout },
  { path: 'sales', component: SaleHistory },
  { path: 'sales/:id', component: SaleDetail },
  { path: 'help', component: Help },
];
