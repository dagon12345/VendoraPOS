import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RecordStockMovementRequest, StockMovement } from '../models/stock-movement.model';

@Injectable({ providedIn: 'root' })
export class StockMovementService {
  private readonly baseUrl = `${environment.apiUrl}/products`;

  constructor(private readonly http: HttpClient) {}

  getHistory(productId: string): Observable<StockMovement[]> {
    return this.http.get<StockMovement[]>(`${this.baseUrl}/${productId}/stock-movements`);
  }

  record(productId: string, request: RecordStockMovementRequest): Observable<StockMovement> {
    return this.http.post<StockMovement>(`${this.baseUrl}/${productId}/stock-movements`, request);
  }
}
