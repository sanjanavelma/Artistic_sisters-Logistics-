import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../core/services/order.service';
import { LogisticsService } from '../../core/services/logistics.service';
import { AdminOrderDto, DeliveryAssignmentDto, OrderStatus, OrderType, PaymentMode } from '../../core/models/models';

@Component({
  selector: 'app-admin-dashboard',
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="admin-header">
      <div class="container">
        <div class="header-content">
          <div>
            <div class="role-badge">⚙️ Admin</div>
            <h1>Admin Dashboard</h1>
            <p>All orders and delivery assignments across the platform</p>
            <div style="display: flex; gap: 10px;">
              <button (click)="openAgentModal()" class="register-agent-btn">
                ➕ Add Agent (Logistics)
              </button>
              <button (click)="openVehicleModal()" class="register-agent-btn" style="background: rgba(59,130,246,0.15); border-color: rgba(59,130,246,0.4);">
                🚗 Add Vehicle
              </button>
            </div>
          </div>
          <div class="header-stats">
            <div class="stat-card">
              <span class="stat-num">{{ orders.length }}</span>
              <span class="stat-label">Total Orders</span>
            </div>
            <div class="stat-card">
              <span class="stat-num">{{ activeDeliveries }}</span>
              <span class="stat-label">Active Deliveries</span>
            </div>
            <div class="stat-card">
              <span class="stat-num">₹{{ totalRevenue | number:'1.0-0' }}</span>
              <span class="stat-label">Total Revenue</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <section class="section">
      <div class="container">

        <!-- Tabs -->
        <div class="tabs">
          <button class="tab-btn" [class.active]="activeTab === 'orders'" (click)="activeTab = 'orders'" id="tab-orders">
            📋 All Orders
          </button>
          <button class="tab-btn" [class.active]="activeTab === 'deliveries'" (click)="activeTab = 'deliveries'" id="tab-deliveries">
            🚚 Delivery Assignments
          </button>
        </div>

        <!-- ─────────────── ORDERS TAB ─────────────── -->
        <div *ngIf="activeTab === 'orders'">

          <!-- Filter bar -->
          <div class="filter-bar">
            <input class="search-input" type="text" [(ngModel)]="searchText" placeholder="Search by order ID or customer..." />
            <select class="filter-select" [(ngModel)]="statusFilter">
              <option value="">All Statuses</option>
              <option value="0">Pending</option>
              <option value="1">Confirmed</option>
              <option value="2">In Production</option>
              <option value="3">Ready for Delivery</option>
              <option value="4">Out for Delivery</option>
              <option value="5">Delivered</option>
              <option value="6">Cancelled</option>
            </select>
          </div>

          <div *ngIf="loadingOrders" class="loading-state">
            <div class="spinner"></div><p>Loading orders...</p>
          </div>

          <div *ngIf="!loadingOrders && filteredOrders.length === 0" class="empty-state">
            <div class="empty-icon">📋</div>
            <h3>No orders found</h3>
          </div>

          <div class="orders-table-wrap" *ngIf="!loadingOrders && filteredOrders.length > 0">
            <table class="orders-table">
              <thead>
                <tr>
                  <th>Order ID</th>
                  <th>Customer</th>
                  <th>Type</th>
                  <th>Status</th>
                  <th>Amount</th>
                  <th>Payment</th>
                  <th>Placed On</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let order of filteredOrders">
                  <td><span class="mono">{{ order.id | slice:0:8 | uppercase }}</span></td>
                  <td><span class="mono small">{{ order.customerId | slice:0:8 }}</span></td>
                  <td>{{ getTypeText(order.type) }}</td>
                  <td>
                    <select
                      class="status-select"
                      [ngClass]="getStatusClass(order.status)"
                      [(ngModel)]="order.status"
                      (change)="updateOrderStatus(order, $event)"
                      [disabled]="updatingOrder === order.id">
                      <option [ngValue]="0" class="s-pending">Pending</option>
                      <option [ngValue]="1" class="s-confirmed">Confirmed</option>
                      <option [ngValue]="2" class="s-production">In Production</option>
                      <option [ngValue]="3" class="s-ready">Ready to Ship</option>
                      <option [ngValue]="4" class="s-out">Out for Delivery</option>
                      <option [ngValue]="5" class="s-delivered">Delivered</option>
                      <option [ngValue]="6" class="s-cancelled">Cancelled</option>
                    </select>
                  </td>
                  <td class="amount">₹{{ order.totalAmount }}</td>
                  <td>{{ getPaymentText(order.paymentMode) }}</td>
                  <td>{{ order.placedAt | date:'mediumDate' }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- ─────────────── DELIVERIES TAB ─────────────── -->
        <div *ngIf="activeTab === 'deliveries'">

          <div *ngIf="loadingDeliveries" class="loading-state">
            <div class="spinner"></div><p>Loading assignments...</p>
          </div>

          <div *ngIf="!loadingDeliveries && assignments.length === 0" class="empty-state">
            <div class="empty-icon">🚚</div>
            <h3>No delivery assignments yet</h3>
          </div>

          <div class="delivery-grid" *ngIf="!loadingDeliveries && assignments.length > 0">
            <div class="delivery-card" *ngFor="let a of assignments">
              <div class="dc-header">
                <div class="dc-id">
                  <span class="label">ORDER</span>
                  <strong>{{ a.orderId | slice:0:8 | uppercase }}</strong>
                </div>
                <div class="dc-status" [ngClass]="getDelivStatusClass(a.statusText)">
                  {{ a.statusText }}
                </div>
              </div>
              <div class="dc-body">
                <div class="dc-row">
                  <span class="dc-label">👤 Agent</span>
                  <span class="dc-val">{{ a.agent.name }}</span>
                </div>
                <div class="dc-row">
                  <span class="dc-label">📞 Phone</span>
                  <span class="dc-val">{{ a.agent.phone }}</span>
                </div>
                <div class="dc-row">
                  <span class="dc-label">🚗 Vehicle</span>
                  <span class="dc-val">{{ a.vehicle.registrationNumber }} · {{ a.vehicle.vehicleType }}</span>
                </div>
                <div class="dc-row">
                  <span class="dc-label">📅 Assigned</span>
                  <span class="dc-val">{{ a.assignedAt | date:'mediumDate' }}</span>
                </div>

                <div class="sla-bar" [class.sla-warn]="isSlaUrgent(a.slaDeadline)">
                  <span>SLA: {{ a.slaDeadline | date:'medium' }}</span>
                  <span class="sla-flag" *ngIf="isSlaUrgent(a.slaDeadline)">⚠️ URGENT</span>
                </div>
              </div>
            </div>
          </div>

        </div>
      </div>
    </section>

    <!-- Toast Notification -->
    <div *ngIf="toastMsg" class="toast" [class.toast-error]="toastError">
      {{ toastMsg }}
    </div>

    <!-- Modals -->
    <div class="modal-overlay" *ngIf="showAgentModal || showVehicleModal">
      <!-- Agent Modal -->
      <div class="modal-card" *ngIf="showAgentModal">
        <h2>Add Agent</h2>
        <p class="modal-sub">Register agent directly into Logistics Database.</p>
        <form (ngSubmit)="submitAgent()">
          <div class="form-group">
            <label>Full Name</label>
            <input class="form-control" [(ngModel)]="agentForm.name" name="name" required />
          </div>
          <div class="form-group">
            <label>Phone</label>
            <input class="form-control" [(ngModel)]="agentForm.phone" name="phone" required />
          </div>
          <div class="form-group">
            <label>Email</label>
            <input class="form-control" type="email" [(ngModel)]="agentForm.email" name="email" required />
          </div>
          <div class="modal-actions">
            <button type="button" class="tab-btn" style="border:none;" (click)="showAgentModal = false">Cancel</button>
            <button type="submit" class="register-agent-btn bg-blue" [disabled]="loadingModal" style="color:white; background:#3b82f6; border:none; padding:10px 16px;">
              {{ loadingModal ? 'Saving...' : 'Add Agent' }}
            </button>
          </div>
        </form>
      </div>

      <!-- Vehicle Modal -->
      <div class="modal-card" *ngIf="showVehicleModal">
        <h2>Add Vehicle</h2>
        <p class="modal-sub">Add a new delivery vehicle.</p>
        <form (ngSubmit)="submitVehicle()">
          <div class="form-group">
            <label>Registration Number</label>
            <input class="form-control" [(ngModel)]="vehicleForm.registrationNumber" name="reg" placeholder="MH12 AB 1234" required />
          </div>
          <div class="form-group">
            <label>Vehicle Type</label>
            <input class="form-control" [(ngModel)]="vehicleForm.vehicleType" name="type" placeholder="Van, Bike" required />
          </div>
          <div class="modal-actions">
            <button type="button" class="tab-btn" style="border:none;" (click)="showVehicleModal = false">Cancel</button>
            <button type="submit" class="register-agent-btn" [disabled]="loadingModal" style="color:white; background:#3b82f6; border:none; padding:10px 16px;">
              {{ loadingModal ? 'Saving...' : 'Add Vehicle' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .admin-header {
      background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
      padding: 120px 24px 64px; color: white;
    }
    .header-content { display: flex; justify-content: space-between; align-items: flex-start; flex-wrap: wrap; gap: 32px; }
    .role-badge {
      display: inline-flex; background: rgba(255,255,255,0.12);
      padding: 4px 12px; border-radius: 20px; font-size: 0.8rem;
      font-weight: 600; color: #94a3b8; margin-bottom: 8px;
    }
    .admin-header h1 { font-size: 2.5rem; font-weight: 700; margin: 0 0 8px; }
    .admin-header p { color: #64748b; margin: 0 0 16px; }
    .register-agent-btn {
      display: inline-flex; align-items: center; gap: 6px;
      background: rgba(255,255,255,0.12); color: white;
      border: 1px solid rgba(255,255,255,0.25); border-radius: 10px;
      padding: 9px 18px; font-size: 0.88rem; font-weight: 600;
      text-decoration: none; transition: all 0.2s;
      backdrop-filter: blur(4px);
    }
    .register-agent-btn:hover {
      background: rgba(255,255,255,0.22); border-color: rgba(255,255,255,0.4);
    }

    .header-stats { display: flex; gap: 16px; flex-wrap: wrap; }
    .stat-card {
      background: rgba(255,255,255,0.07); border: 1px solid rgba(255,255,255,0.1);
      border-radius: 14px; padding: 20px 28px; display: flex; flex-direction: column;
      align-items: center; min-width: 120px;
    }
    .stat-num { font-size: 1.8rem; font-weight: 700; color: white; }
    .stat-label { font-size: 0.78rem; color: #64748b; margin-top: 2px; text-align: center; }

    /* Tabs */
    .tabs { display: flex; gap: 4px; margin-bottom: 28px; border-bottom: 1px solid var(--border); }
    .tab-btn {
      padding: 12px 24px; border: none; background: none; cursor: pointer;
      font-size: 0.95rem; font-weight: 600; color: var(--text-muted);
      border-bottom: 2px solid transparent; margin-bottom: -1px;
      transition: all 0.2s; font-family: var(--sans);
    }
    .tab-btn:hover { color: var(--text-heading); }
    .tab-btn.active { color: #3b82f6; border-bottom-color: #3b82f6; }

    /* Filter bar */
    .filter-bar { display: flex; gap: 12px; margin-bottom: 20px; flex-wrap: wrap; }
    .search-input {
      flex: 1; min-width: 200px; padding: 10px 16px;
      border: 1.5px solid var(--border); border-radius: 10px;
      font-size: 0.9rem; font-family: var(--sans);
    }
    .search-input:focus { outline: none; border-color: #3b82f6; }
    .filter-select {
      padding: 10px 14px; border: 1.5px solid var(--border); border-radius: 10px;
      font-size: 0.9rem; font-family: var(--sans); background: white; cursor: pointer;
    }

    /* Table */
    .orders-table-wrap { overflow-x: auto; border-radius: 14px; box-shadow: 0 4px 20px rgba(0,0,0,0.05); }
    .orders-table { width: 100%; border-collapse: collapse; background: white; }
    .orders-table thead { background: #f8fafc; }
    .orders-table th {
      padding: 14px 18px; text-align: left; font-size: 0.78rem;
      font-weight: 700; color: #94a3b8; text-transform: uppercase; letter-spacing: 0.06em;
      border-bottom: 1px solid #e2e8f0;
    }
    .orders-table td {
      padding: 14px 18px; font-size: 0.9rem; border-bottom: 1px solid #f1f5f9;
      color: var(--text-body);
    }
    .orders-table tr:last-child td { border-bottom: none; }
    .orders-table tr:hover td { background: #f8fafc; }
    .mono { font-family: monospace; font-weight: 600; color: #1e293b; }
    .small { font-size: 0.8rem; }
    .amount { font-weight: 700; color: var(--pink); }

    .status-select {
      padding: 4px 10px; border-radius: 20px; font-size: 0.75rem; font-weight: 600;
      border: 1px solid transparent; cursor: pointer; font-family: inherit;
      appearance: none; -webkit-appearance: none;
      background-image: url('data:image/svg+xml;utf8,<svg fill="black" height="24" viewBox="0 0 24 24" width="24" xmlns="http://www.w3.org/2000/svg"><path d="M7 10l5 5 5-5z"/><path d="M0 0h24v24H0z" fill="none"/></svg>');
      background-repeat: no-repeat; background-position-x: 100%; background-position-y: 50%;
      padding-right: 24px; transition: all 0.2s;
    }
    .status-select:focus { outline: none; box-shadow: 0 0 0 2px rgba(59,130,246,0.3); }

    .s-pending    { background-color: #fef9c3; color: #854d0e; }
    .s-confirmed  { background-color: #dbeafe; color: #1e40af; }
    .s-production { background-color: #e0e7ff; color: #3730a3; }
    .s-ready      { background-color: #d1fae5; color: #065f46; }
    .s-out        { background-color: #e0f2fe; color: #075985; }
    .s-delivered  { background-color: #dcfce7; color: #14532d; }
    .s-cancelled  { background-color: #fee2e2; color: #991b1b; }

    /* Delivery cards */
    .delivery-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 20px; }
    .delivery-card {
      background: white; border-radius: 16px;
      box-shadow: 0 6px 24px rgba(0,0,0,0.05); overflow: hidden;
      border: 1px solid rgba(0,0,0,0.04);
    }
    .dc-header {
      display: flex; justify-content: space-between; align-items: center;
      padding: 18px 20px; background: #f8fafc; border-bottom: 1px solid #e2e8f0;
    }
    .dc-id { display: flex; flex-direction: column; }
    .label { font-size: 0.7rem; color: #94a3b8; font-weight: 700; letter-spacing: 0.08em; }
    .dc-status { padding: 4px 12px; border-radius: 20px; font-size: 0.75rem; font-weight: 600; }
    .ds-pickedup      { background: #dbeafe; color: #1e40af; }
    .ds-intransit     { background: #fef9c3; color: #854d0e; }
    .ds-outfordelivery{ background: #e0f2fe; color: #075985; }
    .ds-delivered     { background: #dcfce7; color: #14532d; }
    .ds-failed        { background: #fee2e2; color: #991b1b; }
    .ds-other         { background: #f1f5f9; color: #475569; }

    .dc-body { padding: 18px 20px; display: flex; flex-direction: column; gap: 10px; }
    .dc-row { display: flex; justify-content: space-between; align-items: center; font-size: 0.88rem; }
    .dc-label { color: #94a3b8; font-size: 0.8rem; }
    .dc-val { font-weight: 500; color: #1e293b; }

    .sla-bar {
      padding: 8px 12px; background: #f8fafc; border-radius: 8px;
      font-size: 0.8rem; color: #64748b; display: flex; justify-content: space-between;
      margin-top: 4px; border: 1px solid #e2e8f0;
    }
    .sla-bar.sla-warn { background: #fef2f2; border-color: #fecaca; color: #dc2626; }
    .sla-flag { font-weight: 700; }

    /* Loading / Empty (shared) */
    .loading-state { text-align: center; padding: 48px; color: var(--text-muted); }
    .spinner {
      width: 36px; height: 36px; border: 3px solid #e5e7eb;
      border-top-color: #3b82f6; border-radius: 50%;
      animation: spin 0.8s linear infinite; margin: 0 auto 16px;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
    .empty-state { text-align: center; padding: 60px; background: white; border-radius: 16px; border: 1px dashed var(--border); }
    .empty-icon { font-size: 56px; margin-bottom: 16px; }

    /* Modal */
    .modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.6); backdrop-filter: blur(4px); display: flex; align-items: center; justify-content: center; z-index: 1000; }
    .modal-card { background: white; border-radius: 16px; padding: 32px; width: 100%; max-width: 440px; box-shadow: 0 20px 40px rgba(0,0,0,0.2); }
    .modal-card h2 { margin: 0 0 8px; font-size: 1.5rem; color: #1e293b; }
    .modal-sub { color: #64748b; font-size: 0.9rem; margin-bottom: 24px; }
    .form-group { margin-bottom: 16px; display: flex; flex-direction: column; gap: 6px; }
    .form-group label { font-size: 0.85rem; font-weight: 600; color: #475569; }
    .form-control { padding: 10px 14px; border: 1.5px solid #cbd5e1; border-radius: 8px; font-family: inherit; font-size: 0.95rem; }
    .form-control:focus { outline: none; border-color: #3b82f6; box-shadow: 0 0 0 3px rgba(59,130,246,0.1); }
    .modal-actions { display: flex; justify-content: flex-end; gap: 12px; margin-top: 28px; }
    /* Toast */
    .toast {
      position: fixed; top: 80px; right: 24px; z-index: 9999;
      background: #2d6a4f; color: #fff; padding: 12px 20px; border-radius: 8px;
      font-size: 0.9rem; font-weight: 500; box-shadow: 0 4px 20px rgba(0,0,0,0.15);
      animation: slideIn 0.3s ease;
    }
    .toast.toast-error { background: #fee2e2; color: #991b1b; }
    @keyframes slideIn {
      from { opacity: 0; transform: translateX(20px); }
      to   { opacity: 1; transform: translateX(0); }
    }
  `]
})
export class AdminDashboardComponent implements OnInit {
  activeTab: 'orders' | 'deliveries' = 'orders';
  orders: AdminOrderDto[] = [];
  assignments: DeliveryAssignmentDto[] = [];
  loadingOrders = true;
  loadingDeliveries = false;
  searchText = '';
  statusFilter = '';

  updatingOrder = '';
  toastMsg = '';
  toastError = false;

  // Modals
  showAgentModal = false;
  showVehicleModal = false;
  loadingModal = false;
  agentForm = { name: '', phone: '', email: '' };
  vehicleForm = { registrationNumber: '', vehicleType: '' };

  get filteredOrders(): AdminOrderDto[] {
    return this.orders.filter(o => {
      const matchSearch = !this.searchText ||
        o.id.toLowerCase().includes(this.searchText.toLowerCase()) ||
        o.customerId?.toLowerCase().includes(this.searchText.toLowerCase());
      const matchStatus = !this.statusFilter || o.status.toString() === this.statusFilter;
      return matchSearch && matchStatus;
    });
  }

  get totalRevenue(): number {
    return this.orders.reduce((sum, o) => sum + o.totalAmount, 0);
  }

  get activeDeliveries(): number {
    return this.assignments.filter(a => a.status !== 4 && a.status !== 5).length;
  }

  constructor(
    private orderService: OrderService,
    private logisticsService: LogisticsService
  ) {}

  ngOnInit() {
    this.loadOrders();
    this.loadDeliveries();
  }

  loadOrders() {
    this.loadingOrders = true;
    this.orderService.getAllOrders().subscribe({
      next: (data) => { this.orders = data; this.loadingOrders = false; },
      error: (err) => { this.loadingOrders = false; this.showToast(err?.error?.message || 'Failed to load orders', true); }
    });
  }

  loadDeliveries() {
    this.loadingDeliveries = true;
    this.logisticsService.getAssignments().subscribe({
      next: (data) => { this.assignments = data; this.loadingDeliveries = false; },
      error: (err) => { this.loadingDeliveries = false; this.showToast(err?.error?.message || 'Failed to load assignments', true); }
    });
  }

  getStatusText(status: OrderStatus): string {
    const map: Record<number, string> = {
      0: 'Pending', 1: 'Confirmed', 2: 'In Production',
      3: 'Ready to Ship', 4: 'Out for Delivery', 5: 'Delivered', 6: 'Cancelled'
    };
    return map[status] ?? 'Unknown';
  }

  getStatusClass(status: OrderStatus): string {
    const map: Record<number, string> = {
      0: 's-pending', 1: 's-confirmed', 2: 's-production', 3: 's-ready',
      4: 's-out', 5: 's-delivered', 6: 's-cancelled'
    };
    return map[status] ?? '';
  }

  getTypeText(type: OrderType): string {
    return type === OrderType.CustomCommission ? 'Commission' : 'Standard';
  }

  getPaymentText(mode: PaymentMode): string {
    const map: Record<number, string> = { 0: 'Card', 1: 'Cash on Delivery', 2: 'Bank Transfer' };
    return map[mode] ?? 'Unknown';
  }

  getDelivStatusClass(statusText: string): string {
    switch (statusText?.toLowerCase()) {
      case 'pickedup': return 'ds-pickedup';
      case 'intransit': return 'ds-intransit';
      case 'outfordelivery': return 'ds-outfordelivery';
      case 'delivered': return 'ds-delivered';
      case 'failed': return 'ds-failed';
      default: return 'ds-other';
    }
  }

  isSlaUrgent(deadline: Date): boolean {
    const d = new Date(deadline);
    return (d.getTime() - Date.now()) < 2 * 3600 * 1000;
  }

  openAgentModal() {
    this.agentForm = { name: '', phone: '', email: '' };
    this.showAgentModal = true;
  }

  openVehicleModal() {
    this.vehicleForm = { registrationNumber: '', vehicleType: '' };
    this.showVehicleModal = true;
  }

  submitAgent() {
    if (!this.agentForm.name || !this.agentForm.phone || !this.agentForm.email) return;
    this.loadingModal = true;
    this.logisticsService.addAgent(this.agentForm).subscribe({
      next: () => {
        this.loadingModal = false;
        this.showAgentModal = false;
        alert('Agent added to Logistics DB successfully.');
      },
      error: (err) => {
        this.loadingModal = false;
        alert('Failed to add agent.');
        console.error(err);
      }
    });
  }

  submitVehicle() {
    if (!this.vehicleForm.registrationNumber || !this.vehicleForm.vehicleType) return;
    this.loadingModal = true;
    this.logisticsService.addVehicle(this.vehicleForm).subscribe({
      next: () => {
        this.loadingModal = false;
        this.showVehicleModal = false;
        alert('Vehicle added successfully.');
      },
      error: (err) => {
        this.loadingModal = false;
        alert('Failed to add vehicle.');
        console.error(err);
      }
    });
  }

  updateOrderStatus(order: AdminOrderDto, event: any) {
    const newStatus = parseInt(event.target.value, 10);
    this.updatingOrder = order.id;
    
    this.orderService.updateOrderStatus(order.id, newStatus).subscribe({
      next: (res) => {
        this.updatingOrder = '';
        this.showToast('✅ Order status updated successfully!');
        if (newStatus === 3) {
           // Reload deliveries slightly delayed because saga takes a second
           setTimeout(() => this.loadDeliveries(), 1500);
        }
      },
      error: (err) => {
        this.updatingOrder = '';
        this.showToast(err?.error?.message || '❌ Failed to update order status. Please ensure backend is running.', true);
        this.loadOrders(); // Revert local state to actual state
      }
    });
  }

  showToast(msg: string, isError = false) {
    this.toastMsg = msg;
    this.toastError = isError;
    setTimeout(() => { this.toastMsg = ''; }, 3500);
  }
}
