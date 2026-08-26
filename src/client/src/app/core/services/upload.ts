import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface UploadedImage {
  url: string;
}

@Injectable({ providedIn: 'root' })
export class UploadService {
  private readonly baseUrl = `${environment.apiUrl}/uploads`;

  constructor(private readonly http: HttpClient) {}

  uploadProductImage(file: File): Observable<UploadedImage> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<UploadedImage>(`${this.baseUrl}/product-image`, formData);
  }
}
