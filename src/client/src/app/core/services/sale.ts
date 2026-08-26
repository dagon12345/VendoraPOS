import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateSaleRequest, RestoreLineRequest, Sale, VoidLineRequest, VoidSaleRequest } from '../models/sale.model';

@Injectable({ providedIn: 'root' })
export class SaleService {
  private readonly baseUrl = `${environment.apiUrl}/sales`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<Sale[]> {
    return this.http.get<Sale[]>(this.baseUrl);
  }

  getById(id: string): Observable<Sale> {
    return this.http.get<Sale>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateSaleRequest): Observable<Sale> {
    return this.http.post<Sale>(this.baseUrl, request);
  }

  void(id: string, request: VoidSaleRequest): Observable<Sale> {
    return this.http.post<Sale>(`${this.baseUrl}/${id}/void`, request);
  }

  restore(id: string): Observable<Sale> {
    return this.http.post<Sale>(`${this.baseUrl}/${id}/restore`, {});
  }

  voidLine(id: string, request: VoidLineRequest): Observable<Sale> {
    return this.http.post<Sale>(`${this.baseUrl}/${id}/void-line`, request);
  }

  restoreLine(id: string, request: RestoreLineRequest): Observable<Sale> {
    return this.http.post<Sale>(`${this.baseUrl}/${id}/restore-line`, request);
  }
}
