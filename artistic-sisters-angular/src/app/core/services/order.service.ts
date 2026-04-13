import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PlaceOrderRequest, CommissionRequest, OrderResult, CustomerOrderDto, AdminOrderDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly BASE = 'http://localhost:5000/api/orders';

  constructor(private http: HttpClient) {}

  placeOrder(req: PlaceOrderRequest): Observable<OrderResult> {
    return this.http.post<OrderResult>(this.BASE, req);
  }

  placeCommission(req: CommissionRequest): Observable<OrderResult> {
    return this.http.post<OrderResult>(`${this.BASE}/commission`, req);
  }

  getCustomerOrders(customerId: string): Observable<CustomerOrderDto[]> {
    return this.http.get<CustomerOrderDto[]>(`${this.BASE}/customer/${customerId}`);
  }

  getOrderById(orderId: string): Observable<CustomerOrderDto> {
    return this.http.get<CustomerOrderDto>(`${this.BASE}/${orderId}`);
  }

  getAllOrders(): Observable<AdminOrderDto[]> {
    return this.http.get<AdminOrderDto[]>(`${this.BASE}/all`);
  }

  updateOrderStatus(orderId: string, status: number): Observable<any> {
    return this.http.patch(`${this.BASE}/${orderId}/status`, { status });
  }
}
