import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MenuItem } from '../models/models';

@Injectable({ providedIn: 'root' })
export class MenuService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.menuApi;

  getMenu(): Observable<MenuItem[]> {
    return this.http.get<MenuItem[]>(`${this.base}/Menu`);
  }

  getByCategory(category: string): Observable<MenuItem[]> {
    return this.http.get<MenuItem[]>(`${this.base}/Menu/category/${category}`);
  }
}
