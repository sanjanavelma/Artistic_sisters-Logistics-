import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService } from '../../core/services/order.service';
import { LogisticsService } from '../../core/services/logistics.service';
import { CustomerOrderDto, OrderStatus, TrackingInfo } from '../../core/models/models';

@Component({
  selector: 'app-track-order',
  imports: [CommonModule, RouterLink],
  template: `
    <div class="page-header">
      <div class="container text-center">
        <h1 class="fade-up">Track Order</h1>
        <div class="divider"></div>
        <p class="page-sub fade-up-2">Live logistics tracking</p>
      </div>
    </div>
    
    <section class="section tracking-section">
      <div class="container">
        <!-- Error & Loading states -->
        <div *ngIf="loading" class="text-center p-5">
          <p class="text-muted">Loading order details...</p>
        </div>
        
        <div *ngIf="error" class="error-state">
          <h3>Oops!</h3>
          <p>{{ error }}</p>
          <a routerLink="/customer/dashboard" class="btn btn-outline-primary mt-3">Back to Dashboard</a>
        </div>

        <div *ngIf="!loading && !error && order" class="tracking-card">
          <div class="tracking-header">
            <div>
              <span class="label">ORDER ID</span>
              <h4>{{ order.id | uppercase }}</h4>
            </div>
            <div class="text-right">
              <span class="label">EXPECTED ARRIVAL</span>
              <h4>{{ getDeliveryEstimate() }}</h4>
            </div>
          </div>
          
          <div class="tracking-body">
            <h5 class="mb-4">Delivery Timeline</h5>
            
            <div class="timeline">
              <div class="timeline-step" *ngFor="let phase of phases; let i = index" 
                   [class.active]="phase.active" [class.current]="isCurrentPhase(i)">
                <div class="step-icon">
                  <span *ngIf="phase.active && !isCurrentPhase(i)">✓</span>
                  <div *ngIf="isCurrentPhase(i)" class="pulse"></div>
                </div>
                <div class="step-content">
                  <h6 class="step-title">{{ phase.label }}</h6>
                  <p class="step-desc" *ngIf="phase.active && phase.detail">{{ phase.detail }}</p>
                </div>
              </div>
            </div>

            <!-- Optional LIVE Tracking Map / GPS Coordinates if Available -->
            <div *ngIf="trackingInfo" class="live-tracking mt-5">
              <h6>📍 Live Logistics Update</h6>
              <div class="gps-card">
                <p><strong>Agent Status:</strong> En route to destination</p>
                <p class="text-muted mb-0">Location: {{ trackingInfo.latitude }}, {{ trackingInfo.longitude }}</p>
                <p class="text-muted mb-0"><small>Updated: {{ trackingInfo.updatedAt | date:'mediumTime' }}</small></p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  `,
  styles: [`
    .page-header { background: linear-gradient(160deg, #f7ede8, #fcf4f0); padding: 120px 24px 64px; }
    .page-sub { color: var(--text-muted); margin-top: 8px; }
    .divider { margin: 16px auto; width: 60px; height: 3px; background: var(--pink); border-radius: 2px; }
    
    .tracking-section { background: #fafafb; padding: 60px 20px; min-height: 50vh; }
    .text-right { text-align: right; }
    .mb-4 { margin-bottom: 24px; }
    .mt-5 { margin-top: 32px; }
    .mb-0 { margin-bottom: 0; }
    
    .tracking-card {
      background: white; border-radius: 16px; box-shadow: 0 10px 30px rgba(0,0,0,0.05);
      max-width: 800px; margin: 0 auto; overflow: hidden;
    }
    
    .tracking-header {
      padding: 30px; background: var(--pink); color: white;
      display: flex; justify-content: space-between; align-items: center;
    }
    .tracking-header .label { font-size: 0.75rem; font-weight: 600; letter-spacing: 0.05em; opacity: 0.8; }
    .tracking-header h4 { margin: 4px 0 0; color: white; font-size: 1.25rem; font-family: var(--sans); }
    
    .tracking-body { padding: 40px; }
    
    /* Vertical Timeline */
    .timeline { display: flex; flex-direction: column; position: relative; gap: 30px; }
    .timeline::before {
      content: ''; position: absolute; left: 16px; top: 10px; bottom: 10px;
      width: 2px; background: #e5e7eb; z-index: 1;
    }
    
    .timeline-step { display: flex; gap: 24px; position: relative; z-index: 2; opacity: 0.5; }
    .timeline-step.active { opacity: 1; }
    
    .step-icon {
      width: 34px; height: 34px; border-radius: 50%; background: #e5e7eb;
      display: flex; align-items: center; justify-content: center;
      color: white; font-weight: bold; font-size: 0.9rem; flex-shrink: 0;
      border: 4px solid white; transition: all 0.3s;
    }
    .timeline-step.active .step-icon { background: var(--pink); }
    .timeline-step.current .step-icon { background: white; border-color: var(--pink); border-width: 3px; position: relative; }
    
    .pulse {
      width: 14px; height: 14px; background: var(--pink); border-radius: 50%;
      box-shadow: 0 0 0 0 rgba(222, 142, 142, 0.7); animation: pulse 1.5s infinite;
    }
    
    .step-content { padding-top: 6px; }
    .step-title { margin: 0; font-size: 1.05rem; font-weight: 600; color: var(--text-heading); }
    .step-desc { margin: 4px 0 0; font-size: 0.85rem; color: var(--text-muted); }
    
    .live-tracking {
      background: #fdfaf8; border: 1px solid #f2e6e1; border-radius: 12px; padding: 20px;
    }
    .gps-card { margin-top: 12px; font-size: 0.95rem; }
    
    .error-state { text-align: center; padding: 60px 20px; background: white; border-radius: 16px; border: 1px solid #ffdde1; }
    .btn-outline-primary {
      display: inline-block; background: transparent; border: 2px solid var(--pink);
      color: var(--pink); padding: 10px 24px; border-radius: 8px; font-weight: 600;
      text-decoration: none; transition: 0.3s;
    }
    .btn-outline-primary:hover { background: var(--pink); color: white; }

    @keyframes pulse {
      0% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(222, 142, 142, 0.7); }
      70% { transform: scale(1); box-shadow: 0 0 0 10px rgba(222, 142, 142, 0); }
      100% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(222, 142, 142, 0); }
    }
  `]
})
export class TrackOrderComponent implements OnInit, OnDestroy {
  orderId = '';
  order: CustomerOrderDto | null = null;
  loading = true;
  error = '';
  
  trackingInfo: TrackingInfo | null = null;
  trackingInterval: any;

  phases = [
    { label: 'Order Confirmed', active: false, detail: 'We received your order.' },
    { label: 'Packed', active: false, detail: 'Order is ready in our studio.' },
    { label: 'Shipped', active: false, detail: 'Handed over to logistics partner.' },
    { label: 'In Transit', active: false, detail: 'Traveling to your city.' },
    { label: 'Reached Local Hub', active: false, detail: 'Arrived at your local delivery hub.' },
    { label: 'Out for Delivery', active: false, detail: 'The agent is on the way.' },
    { label: 'Delivered', active: false, detail: 'Successfully delivered to your address.' }
  ];

  constructor(
    private route: ActivatedRoute, 
    private orderService: OrderService,
    private logisticsService: LogisticsService
  ) {
    this.orderId = this.route.snapshot.paramMap.get('orderId') || '';
  }

  ngOnInit() {
    if (!this.orderId) {
      this.error = 'Invalid Tracking Link.';
      this.loading = false;
      return;
    }

    this.orderService.getOrderById(this.orderId).subscribe({
      next: (orderData) => {
        this.order = orderData;
        this.calculatePhases(orderData.status);
        this.loading = false;

        // If the order is out for delivery, stream GPS
        if (orderData.status === OrderStatus.OutForDelivery) {
          this.fetchLiveTracking();
          this.trackingInterval = setInterval(() => this.fetchLiveTracking(), 10000);
        }
      },
      error: (err) => {
        console.error(err);
        this.error = 'Unable to fetch your order. Please check your tracking ID.';
        this.loading = false;
      }
    });
  }

  ngOnDestroy() {
    if (this.trackingInterval) clearInterval(this.trackingInterval);
  }

  calculatePhases(status: OrderStatus) {
    // Determine the furthest "index" to light up
    let progressIndex = 0;
    
    switch (status) {
      case OrderStatus.Pending: progressIndex = 0; break;
      case OrderStatus.Confirmed: progressIndex = 0; break;
      case OrderStatus.InProduction: progressIndex = 1; break;
      case OrderStatus.ReadyForDelivery: progressIndex = 2; break; // Shipped
      // Simulate transit states if status is ReadyForDelivery 
      // (in a real app, logistics webhooks would increment this uniquely)
      case OrderStatus.OutForDelivery: progressIndex = 5; break;
      case OrderStatus.Delivered: progressIndex = 6; break;
      case OrderStatus.Cancelled: progressIndex = -1; break;
    }

    // Set active states
    for (let i = 0; i <= progressIndex; i++) {
        if (this.phases[i]) {
            this.phases[i].active = true;
        }
    }
  }

  isCurrentPhase(index: number): boolean {
    // The current active phase is the LAST active phase in the list
    if (!this.phases[index].active) return false;
    if (index === this.phases.length - 1) return true; // It's delivered
    return !this.phases[index + 1].active; // It's active, but the next one isn't
  }

  fetchLiveTracking() {
    this.logisticsService.track(this.orderId).subscribe({
      next: (info) => this.trackingInfo = info,
      error: () => { /* silently ignore if tracker isn't online */ }
    });
  }

  getDeliveryEstimate(): string {
    if (!this.order) return '';
    if (this.order.status === OrderStatus.Delivered) return 'Delivered';
    const d = new Date(this.order.placedAt);
    d.setDate(d.getDate() + 5); // Standard 5 day delivery
    return d.toDateString();
  }
}
