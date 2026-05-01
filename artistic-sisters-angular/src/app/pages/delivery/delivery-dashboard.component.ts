import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { interval, Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { LogisticsService } from '../../core/services/logistics.service';
import { DeliveryAssignmentDto, DeliveryAgent } from '../../core/models/models';

@Component({
  selector: 'app-delivery-dashboard',
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <div class="container">
        <div class="header-content">
          <div class="header-left">
            <div class="role-badge">🚚 Delivery Agent</div>
              <h1>My Deliveries</h1>
              <p *ngIf="!agentInfo">Update order status in real time</p>
              <p *ngIf="agentInfo" class="agent-welcome">
                Welcome back, <strong>{{ agentInfo.name }}</strong> ({{ agentInfo.email }})
              </p>
            </div>
          <div class="live-indicator" [class.active]="isPolling">
            <span class="pulse-dot"></span>
            {{ isPolling ? 'Live Tracking Active' : 'Tracking Paused' }}
          </div>
        </div>
      </div>
    </div>

    <section class="section">
      <div class="container">

        <!-- Agent ID Input (until auth is hooked up for this role) -->
        <div class="agent-setup-card" *ngIf="!agentId">
          <div class="setup-icon">🪪</div>
          <h3>Enter Your Agent ID</h3>
          <p>Paste your Agent ID to load your assignments</p>
          <div class="setup-form">
            <input
              id="agent-id-input"
              type="text"
              class="form-input"
              [(ngModel)]="agentIdInput"
              placeholder="e.g. 3fa85f64-5717-4562-b3fc-2c963f66afa6"
            />
            <button class="btn btn-primary" (click)="loadAssignments()">Load My Orders</button>
          </div>
        </div>

        <!-- Assignments List -->
        <div *ngIf="agentId">
          <div class="section-header">
            <h2>Active Assignments <span class="count-badge">{{ activeCount }}</span></h2>
            <button class="btn btn-ghost" (click)="reset()">← Change Agent</button>
          </div>

          <div *ngIf="loading" class="loading-state">
            <div class="spinner"></div>
            <p>Loading your assignments...</p>
          </div>

          <div *ngIf="!loading && assignments.length === 0" class="empty-state">
            <div class="empty-icon">📦</div>
            <h3>No Active Deliveries</h3>
            <p>You don't have any orders assigned right now.</p>
          </div>

          <div class="assignments-grid" *ngIf="!loading && assignments.length > 0">
            <div class="assignment-card" *ngFor="let item of assignments">
              <div class="card-header">
                <div class="order-info">
                  <span class="label">ORDER</span>
                  <strong>{{ item.orderId | slice:0:8 | uppercase }}</strong>
                </div>
                <div class="status-chip" [ngClass]="getStatusClass(item.statusText)">
                  {{ item.statusText }}
                </div>
              </div>

              <div class="card-body">
                <div class="info-grid">
                  <div class="info-item">
                    <span class="info-label">🚗 Vehicle</span>
                    <span class="info-val">{{ item.vehicle.registrationNumber }} ({{ item.vehicle.vehicleType }})</span>
                  </div>
                  <div class="info-item">
                    <span class="info-label">📅 Assigned</span>
                    <span class="info-val">{{ item.assignedAt | date:'mediumDate' }}</span>
                  </div>
                  <div class="info-item">
                    <span class="info-label">⏰ SLA Deadline</span>
                    <span class="info-val" [class.sla-warn]="isSlaUrgent(item.slaDeadline)">
                      {{ item.slaDeadline | date:'medium' }}
                    </span>
                  </div>

                </div>

                <!-- Update Panel -->
                <div class="update-panel">
                  <div class="update-row">
                    <div class="select-wrap">
                      <label>Update Status</label>
                      <select class="form-select" [(ngModel)]="selectedStatus[item.orderId]">
                        <option value="1">Picked Up</option>
                        <option value="2">In Transit</option>
                        <option value="3">Out for Delivery</option>
                        <option value="4">Delivered</option>
                        <option value="5">Failed</option>
                      </select>
                    </div>
                    <button
                      class="btn btn-primary update-btn"
                      (click)="updateStatus(item)"
                      [disabled]="updating[item.orderId]">
                      {{ updating[item.orderId] ? 'Updating...' : 'Update Status' }}
                    </button>
                  </div>


                  <div class="feedback" *ngIf="feedback[item.orderId]" [class.success]="feedback[item.orderId].ok">
                    {{ feedback[item.orderId].msg }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

      </div>
    </section>
  `,
  styles: [`
    .page-header {
      background: linear-gradient(135deg, #1e293b 0%, #334155 100%);
      padding: 120px 24px 64px;
      color: white;
    }
    .header-content { display: flex; justify-content: space-between; align-items: flex-end; flex-wrap: wrap; gap: 16px; }
    .header-left { display: flex; flex-direction: column; gap: 8px; }
    .role-badge {
      display: inline-flex; align-items: center; gap: 6px;
      background: rgba(255,255,255,0.15); padding: 4px 12px;
      border-radius: 20px; font-size: 0.8rem; font-weight: 600;
      width: fit-content; color: #94a3b8;
    }
    .page-header h1 { font-size: 2.5rem; font-weight: 700; margin: 0; color: white; }
    .page-header p { color: #94a3b8; margin: 0; }
    .live-indicator {
      display: flex; align-items: center; gap: 8px;
      background: rgba(255,255,255,0.1); padding: 8px 16px;
      border-radius: 20px; font-size: 0.85rem; color: #94a3b8;
      border: 1px solid rgba(255,255,255,0.15);
    }
    .live-indicator.active { color: #4ade80; border-color: rgba(74,222,128,0.3); background: rgba(74,222,128,0.08); }
    .pulse-dot {
      width: 8px; height: 8px; border-radius: 50%; background: #94a3b8;
      animation: pulse 2s infinite;
    }
    .live-indicator.active .pulse-dot { background: #4ade80; }
    @keyframes pulse { 0%, 100% { opacity: 1; transform: scale(1); } 50% { opacity: 0.5; transform: scale(1.3); } }

    /* Agent Setup */
    .agent-setup-card {
      max-width: 560px; margin: 0 auto; text-align: center;
      background: white; border-radius: 20px; padding: 48px;
      box-shadow: 0 20px 60px rgba(0,0,0,0.06);
    }
    .setup-icon { font-size: 56px; margin-bottom: 16px; }
    .agent-setup-card h3 { font-size: 1.4rem; margin-bottom: 8px; }
    .agent-setup-card p { color: var(--text-muted); margin-bottom: 24px; }
    .setup-form { display: flex; flex-direction: column; gap: 12px; }
    .form-input {
      width: 100%; padding: 12px 16px; border: 1.5px solid var(--border);
      border-radius: 10px; font-size: 0.95rem; font-family: var(--sans);
      transition: border-color 0.2s; box-sizing: border-box;
    }
    .form-input:focus { outline: none; border-color: #3b82f6; }
    .form-input.sm { padding: 8px 12px; font-size: 0.85rem; }

    /* Section header */
    .section-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
    .section-header h2 { font-size: 1.4rem; font-weight: 700; display: flex; align-items: center; gap: 10px; }
    .count-badge {
      background: #3b82f6; color: white; border-radius: 20px;
      padding: 2px 10px; font-size: 0.8rem; font-weight: 600;
    }

    /* Loading */
    .loading-state { text-align: center; padding: 48px; color: var(--text-muted); }
    .spinner {
      width: 36px; height: 36px; border: 3px solid #e5e7eb;
      border-top-color: #3b82f6; border-radius: 50%;
      animation: spin 0.8s linear infinite; margin: 0 auto 16px;
    }
    @keyframes spin { to { transform: rotate(360deg); } }

    /* Empty */
    .empty-state { text-align: center; padding: 60px; background: white; border-radius: 16px; border: 1px dashed var(--border); }
    .empty-icon { font-size: 56px; margin-bottom: 16px; }

    /* Assignments Grid */
    .assignments-grid { display: grid; gap: 24px; }
    .assignment-card {
      background: white; border-radius: 16px;
      box-shadow: 0 8px 32px rgba(0,0,0,0.05);
      overflow: hidden; border: 1px solid rgba(0,0,0,0.04);
    }
    .card-header {
      display: flex; justify-content: space-between; align-items: center;
      padding: 20px 24px; background: #f8fafc;
      border-bottom: 1px solid #e2e8f0;
    }
    .order-info { display: flex; flex-direction: column; }
    .label { font-size: 0.7rem; color: #94a3b8; font-weight: 700; letter-spacing: 0.08em; }
    .status-chip { padding: 5px 14px; border-radius: 20px; font-size: 0.78rem; font-weight: 600; }
    .s-pickedup  { background: #dbeafe; color: #1e40af; }
    .s-intransit { background: #fef9c3; color: #854d0e; }
    .s-outfordelivery { background: #e0f2fe; color: #075985; }
    .s-delivered { background: #dcfce7; color: #14532d; }
    .s-failed    { background: #fee2e2; color: #991b1b; }
    .s-other     { background: #f1f5f9; color: #475569; }

    .card-body { padding: 24px; }
    .info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 24px; }
    .info-item { display: flex; flex-direction: column; gap: 3px; }
    .info-label { font-size: 0.78rem; color: #94a3b8; font-weight: 600; }
    .info-val { font-size: 0.9rem; font-weight: 500; color: #1e293b; }
    .gps-val { font-family: monospace; color: #3b82f6; }
    .sla-warn { color: #dc2626 !important; font-weight: 700 !important; }

    /* Update Panel */
    .update-panel { border-top: 1px solid #f1f5f9; padding-top: 20px; display: flex; flex-direction: column; gap: 12px; }
    .update-row { display: flex; align-items: flex-end; gap: 12px; flex-wrap: wrap; }
    .select-wrap { display: flex; flex-direction: column; gap: 6px; flex: 1; min-width: 180px; }
    .select-wrap label { font-size: 0.82rem; font-weight: 600; color: #64748b; }
    .form-select {
      padding: 10px 14px; border: 1.5px solid #e2e8f0; border-radius: 10px;
      font-size: 0.9rem; font-family: var(--sans); background: #f8fafc;
      cursor: pointer;
    }
    .form-select:focus { outline: none; border-color: #3b82f6; }
    .update-btn { white-space: nowrap; }



    .btn-outline {
      border: 1.5px solid #3b82f6; color: #3b82f6; background: transparent;
      padding: 9px 16px; border-radius: 8px; font-weight: 600;
      cursor: pointer; transition: all 0.2s; font-family: var(--sans); font-size: 0.88rem;
    }
    .btn-outline:hover { background: #3b82f6; color: white; }
    .btn-outline:disabled { opacity: 0.5; cursor: not-allowed; }

    .feedback {
      padding: 8px 14px; border-radius: 8px; font-size: 0.85rem;
      background: #fee2e2; color: #991b1b;
    }
    .feedback.success { background: #dcfce7; color: #14532d; }

    .agent-welcome { color: #e2e8f0; margin: 0; font-size: 1.1rem; }
    .agent-welcome strong { color: #f8fafc; }

    @media (max-width: 600px) {
      .info-grid { grid-template-columns: 1fr; }
      .update-row { flex-direction: column; align-items: stretch; }
    }
  `]
})
export class DeliveryDashboardComponent implements OnInit, OnDestroy {
  agentId = '';
  agentIdInput = '';
  assignments: DeliveryAssignmentDto[] = [];
  agentInfo: DeliveryAgent | null = null;
  loading = false;
  isPolling = false;

  selectedStatus: Record<string, number> = {};
  updating: Record<string, boolean> = {};
  updatingGPS: Record<string, boolean> = {};
  feedback: Record<string, { ok: boolean; msg: string }> = {};

  private pollSub?: Subscription;

  get activeCount(): number {
    return this.assignments.filter(a => a.status !== 4 && a.status !== 5).length;
  }

  constructor(private logisticsService: LogisticsService) {}

  ngOnInit() {
    // Try to pick agent ID from localStorage (if auth was hooked up)
    const storedId = localStorage.getItem('as_id');
    const role = localStorage.getItem('as_role');
    if (storedId && role === 'DeliveryAgent') {
      this.agentId = storedId;
      this.fetchAgentInfo();
      this.startPolling();
    }
  }

  loadAssignments() {
    const id = this.agentIdInput.trim();
    if (!id) return;
    this.agentId = id;
    this.fetchAgentInfo();
    this.startPolling();
  }

  fetchAgentInfo() {
    this.logisticsService.getAgentById(this.agentId).subscribe({
      next: (data) => this.agentInfo = data,
      error: () => this.agentInfo = null
    });
  }

  reset() {
    this.agentId = '';
    this.agentIdInput = '';
    this.agentInfo = null;
    this.assignments = [];
    this.stopPolling();
  }

  private startPolling() {
    this.loading = true;
    this.fetchAssignments();
    this.isPolling = true;
    this.pollSub = interval(5000).pipe(
      switchMap(() => this.logisticsService.getAgentAssignments(this.agentId))
    ).subscribe({
      next: (data) => { this.assignments = data; this.initDefaults(); },
      error: (err) => console.error('Poll error', err)
    });
  }

  private fetchAssignments() {
    this.logisticsService.getAgentAssignments(this.agentId).subscribe({
      next: (data) => {
        this.assignments = data;
        this.loading = false;
        this.initDefaults();
      },
      error: () => { this.loading = false; }
    });
  }

  private initDefaults() {
    this.assignments.forEach(a => {
      if (!this.selectedStatus[a.orderId]) this.selectedStatus[a.orderId] = a.status;
    });
  }

  private stopPolling() {
    this.pollSub?.unsubscribe();
    this.isPolling = false;
  }

  ngOnDestroy() { this.stopPolling(); }

  updateStatus(item: DeliveryAssignmentDto) {
    const status = Number(this.selectedStatus[item.orderId]);
    this.updating[item.orderId] = true;
    this.feedback[item.orderId] = { ok: false, msg: '' };

    this.logisticsService.updateStatus(item.orderId.toString(), status).subscribe({
      next: () => {
        this.updating[item.orderId] = false;
        this.feedback[item.orderId] = { ok: true, msg: '✅ Status updated successfully!' };
        this.fetchAssignments();
      },
      error: (err) => {
        this.updating[item.orderId] = false;
        const msg = err?.error?.message || 'Failed to update status.';
        this.feedback[item.orderId] = { ok: false, msg: '❌ ' + msg };
      }
    });
  }



  getStatusClass(statusText: string): string {
    switch (statusText?.toLowerCase()) {
      case 'pickedup': return 's-pickedup';
      case 'intransit': return 's-intransit';
      case 'outfordelivery': return 's-outfordelivery';
      case 'delivered': return 's-delivered';
      case 'failed': return 's-failed';
      default: return 's-other';
    }
  }

  isSlaUrgent(deadline: Date): boolean {
    const d = new Date(deadline);
    const now = new Date();
    const hoursLeft = (d.getTime() - now.getTime()) / (1000 * 3600);
    return hoursLeft < 2;
  }
}
