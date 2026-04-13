import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PaymentStatus, SagaState } from '../models/models';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly BASE = 'http://localhost:5000/api/payment';
  constructor(private http: HttpClient) {}

  getByOrder(orderId: string): Observable<PaymentStatus> {
    return this.http.get<PaymentStatus>(`${this.BASE}/order/${orderId}`);
  }

  getSaga(orderId: string): Observable<SagaState> {
    return this.http.get<SagaState>(`${this.BASE}/saga/${orderId}`);
  }

  createRazorpayOrder(amount: number, orderId: string): Observable<any> {
    return this.http.post<any>(`${this.BASE}/razorpay/create-order`, { amount, orderId });
  }

  verifyRazorpayPayment(orderId: string, razorpayOrderId: string, razorpayPaymentId: string, signature: string): Observable<any> {
    return this.http.post<any>(`${this.BASE}/razorpay/verify`, {
      orderId,
      razorpayOrderId,
      razorpayPaymentId,
      razorpaySignature: signature
    });
  }
}
