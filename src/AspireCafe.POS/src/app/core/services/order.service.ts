import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Order, OrderSubmit } from '../models/models';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.ordersApi;

  submit(order: OrderSubmit): Observable<Order> {
    return this.http.post<Order>(`${this.base}/Orders`, order);
  }

  getActive(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.base}/Orders/active`);
  }

  getByTable(tableNumber: number): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.base}/Orders/table/${tableNumber}`);
  }

  updateStatus(orderId: string, status: string): Observable<Order> {
    return this.http.patch<Order>(`${this.base}/Orders/${orderId}/status`, { status });
  }
}
