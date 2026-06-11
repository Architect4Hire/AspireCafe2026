import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Payment, PaymentSubmit, TipSuggestion } from '../models/models';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.paymentsApi;

  getTipSuggestions(subtotal: number): Observable<TipSuggestion> {
    return this.http.get<TipSuggestion>(`${this.base}/Payments/tip-suggestions?subtotal=${subtotal}`);
  }

  process(payment: PaymentSubmit): Observable<Payment> {
    return this.http.post<Payment>(`${this.base}/Payments`, payment);
  }
}
