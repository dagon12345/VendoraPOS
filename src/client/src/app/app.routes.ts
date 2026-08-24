import { Routes } from '@angular/router';
import { ProductList } from './features/inventory/product-list/product-list';
import { ProductForm } from './features/inventory/product-form/product-form';
import { ProductStockHistory } from './features/inventory/product-stock-history/product-stock-history';
import { Help } from './features/help/help';

export const routes: Routes = [
  { path: '', redirectTo: 'products', pathMatch: 'full' },
  { path: 'products', component: ProductList },
  { path: 'products/new', component: ProductForm },
  { path: 'products/:id/edit', component: ProductForm },
  { path: 'products/:id/history', component: ProductStockHistory },
  { path: 'help', component: Help },
];
