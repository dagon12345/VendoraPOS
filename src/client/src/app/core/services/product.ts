import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateProductRequest, Product, ProductAuditLog, UpdateProductRequest } from '../models/product.model';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly baseUrl = `${environment.apiUrl}/products`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(this.baseUrl);
  }

  getById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.baseUrl, request);
  }

  update(id: string, request: UpdateProductRequest): Observable<Product> {
    return this.http.put<Product>(`${this.baseUrl}/${id}`, request);
  }

  setActive(id: string, isActive: boolean): Observable<Product> {
    return this.http.post<Product>(`${this.baseUrl}/${id}/${isActive ? 'activate' : 'deactivate'}`, {});
  }

  getAuditLog(id: string): Observable<ProductAuditLog[]> {
    return this.http.get<ProductAuditLog[]>(`${this.baseUrl}/${id}/audit-log`);
  }
}
