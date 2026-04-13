import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { OrderService } from '../../core/services/order.service';
import { AuthService } from '../../core/services/auth.service';
import { CustomerOrderDto, OrderStatus, OrderType } from '../../core/models/models';

@Component({
  selector: 'app-customer-dashboard',
  imports: [CommonModule],
  template: `
    <div class="dashboard-header">
      <div class="container text-center">
        <h1 class="fade-up">My Dashboard</h1>
        <div class="divider"></div>
        <p class="fade-up-2">Track your orders and past commissions</p>
      </div>
    </div>
    
    <section class="section">
      <div class="container">
        <div *ngIf="loading" class="text-center p-4">
          <p class="text-muted">Loading your orders...</p>
        </div>

        <div *ngIf="!loading && orders.length === 0" class="empty-state">
          <div class="empty-icon">🎨</div>
          <h3>No Orders Yet</h3>
          <p>Looks like you haven't placed any orders or commissions yet.</p>
          <a routerLink="/portfolio" class="btn btn-primary mt-3">Explore Artwork</a>
        </div>

        <div *ngIf="!loading && orders.length > 0" class="orders-grid">
          <div class="order-card" *ngFor="let order of orders">
            <div class="order-header">
              <div class="order-id">
                <span class="label">ORDER ID</span>
                <strong>{{ order.id | slice:0:8 | uppercase }}</strong>
              </div>
              <div class="order-status" [ngClass]="getStatusClass(order.status)">
                {{ getStatusText(order.status) }}
              </div>
            </div>

            <div class="order-body">
              <div class="info-row">
                <span class="info-label">Type</span>
                <span class="info-val">{{ getTypeText(order.type) }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">Placed On</span>
                <span class="info-val">{{ order.placedAt | date:'mediumDate' }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">Total Amount</span>
                <span class="info-val price">₹{{ order.totalAmount }}</span>
              </div>
              
              <div class="divider-sm"></div>

              <div class="items-list">
                <div class="item" *ngFor="let item of order.items">
                  <span class="item-qty">{{ item.quantity }}x</span>
                  <span class="item-title">{{ item.artworkTitle }}</span>
                </div>
                <div class="item" *ngIf="order.type === 1 && order.artworkType">
                  <span class="item-title">Custom: {{ order.artworkType }} ({{ order.medium }})</span>
                </div>
              </div>
            </div>

            <div class="order-footer">
               <button (click)="trackOrder(order.id)" class="btn btn-outline-primary btn-full">
                  📍 Track Order
               </button>
            </div>
          </div>
        </div>
      </div>
    </section>
  `,
  styles: [`
    .dashboard-header { background: linear-gradient(160deg, #f7ede8, #fcf4f0); padding: 120px 24px 64px; }
    .divider { margin: 16px auto; width: 60px; height: 3px; background: var(--pink); border-radius: 2px; }
    .divider-sm { height: 1px; background: var(--border); margin: 16px 0; }
    
    .empty-state { text-align: center; padding: 60px 20px; background: white; border-radius: 16px; border: 1px dashed var(--border); }
    .empty-icon { font-size: 64px; margin-bottom: 20px; opacity: 0.8; }
    
    .orders-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 24px; }
    
    .order-card {
      background: white; border-radius: 16px; box-shadow: 0 10px 30px rgba(0,0,0,0.04);
      overflow: hidden; display: flex; flex-direction: column; transition: transform 0.3s ease;
      border: 1px solid rgba(0,0,0,0.05);
    }
    .order-card:hover { transform: translateY(-4px); box-shadow: 0 16px 40px rgba(0,0,0,0.08); }
    
    .order-header {
      padding: 20px 24px; display: flex; justify-content: space-between; align-items: center;
      border-bottom: 1px solid var(--border); background: #fafafb;
    }
    .order-id { display: flex; flex-direction: column; }
    .label { font-size: 0.75rem; color: var(--text-muted); font-weight: 600; letter-spacing: 0.05em; }
    
    .order-status {
      padding: 6px 12px; border-radius: 30px; font-size: 0.8rem; font-weight: 600; text-transform: uppercase;
    }
    .status-pending { background: #fff3cd; color: #856404; }
    .status-active { background: #cce5ff; color: #004085; }
    .status-ready { background: #d4edda; color: #155724; }
    .status-out { background: #e2e3e5; color: #383d41; }
    
    .order-body { padding: 24px; flex-grow: 1; }
    .info-row { display: flex; justify-content: space-between; margin-bottom: 12px; font-size: 0.95rem; }
    .info-label { color: var(--text-muted); }
    .info-val { font-weight: 500; color: var(--text-heading); }
    .price { font-weight: 700; color: var(--pink); font-size: 1.1rem; }
    
    .items-list { display: flex; flex-direction: column; gap: 8px; }
    .item { display: flex; gap: 12px; font-size: 0.9rem; align-items: baseline; }
    .item-qty { font-weight: 600; color: var(--text-muted); }
    
    .order-footer { padding: 20px 24px; background: #fafafb; border-top: 1px solid var(--border); }
    .btn-full { width: 100%; display: block; text-align: center; }
    .btn-outline-primary {
      background: transparent; border: 2px solid var(--pink); color: var(--pink);
      padding: 10px 24px; border-radius: 8px; font-weight: 600; transition: all 0.3s;
      text-decoration: none;
    }
    .btn-outline-primary:hover { background: var(--pink); color: white; }
  `]
})
export class CustomerDashboardComponent implements OnInit {
  orders: CustomerOrderDto[] = [];
  loading = true;

  constructor(
    private orderService: OrderService, 
    private authService: AuthService,
    private router: Router
  ) {}

  trackOrder(orderId: string) {
    this.router.navigate(['/track', orderId]);
  }

  ngOnInit() {
    const user = this.authService.currentUser;
    if (user?.customerId) {
      this.orderService.getCustomerOrders(user.customerId).subscribe({
        next: (data) => {
          this.orders = data;
          this.loading = false;
        },
        error: (err) => {
          console.error('Error fetching orders', err);
          this.loading = false;
        }
      });
    } else {
      this.loading = false;
    }
  }

  getStatusText(status: OrderStatus): string {
    switch (status) {
      case OrderStatus.Pending: return 'Pending';
      case OrderStatus.Confirmed: return 'Confirmed';
      case OrderStatus.InProduction: return 'In Production';
      case OrderStatus.ReadyForDelivery: return 'Ready To Ship';
      case OrderStatus.OutForDelivery: return 'Out for Delivery';
      case OrderStatus.Delivered: return 'Delivered';
      case OrderStatus.Cancelled: return 'Cancelled';
      default: return 'Unknown';
    }
  }

  getStatusClass(status: OrderStatus): string {
    switch (status) {
      case OrderStatus.Pending: return 'status-pending';
      case OrderStatus.Confirmed:
      case OrderStatus.InProduction: return 'status-active';
      case OrderStatus.ReadyForDelivery:
      case OrderStatus.OutForDelivery:
      case OrderStatus.Delivered: return 'status-ready';
      default: return 'status-out';
    }
  }

  getTypeText(type: OrderType): string {
    return type === OrderType.CustomCommission ? 'Custom Commission' : 'Standard Order';
  }
}
