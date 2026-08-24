import { Routes } from '@angular/router';
import { ProductList } from './features/inventory/product-list/product-list';

export const routes: Routes = [
  { path: '', redirectTo: 'products', pathMatch: 'full' },
  { path: 'products', component: ProductList },
];
