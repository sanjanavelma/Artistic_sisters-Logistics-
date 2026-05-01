import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ArtworkService } from '../../core/services/artwork.service';
import { OrderService } from '../../core/services/order.service';
import { PaymentService } from '../../core/services/payment.service';
import { AuthService } from '../../core/services/auth.service';
import { Artwork, OrderType, PaymentMode } from '../../core/models/models';

@Component({
  selector: 'app-order',
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="page-header">
      <div class="container text-center">
        <h1 class="fade-up">Checkout</h1>
        <div class="divider"></div>
        <p class="page-sub fade-up-2">Complete your purchase details</p>
      </div>
    </div>
    
    <section class="section">
      <div class="container checkout-grid">
        
        <!-- Left Side: Order Form -->
        <div class="checkout-form-col">
          <h3>Shipping & Billing</h3>
          <form [formGroup]="checkoutForm">
            <div class="form-group">
              <label>Full Name</label>
              <input class="form-control" formControlName="name" />
            </div>
            <div class="form-group">
              <label>Email Address</label>
              <input type="email" class="form-control" formControlName="email" />
            </div>
            <div class="form-group">
              <label>Shipping Address</label>
              <textarea class="form-control" formControlName="address" rows="3"></textarea>
            </div>
            
            <h3 style="margin-top:24px">Payment Method</h3>
            <div class="payment-options">
              <label class="payment-opt" [class.selected]="paymentMethod === 'Razorpay'">
                <input type="radio" name="payment" value="Razorpay" (change)="paymentMethod = 'Razorpay'" [checked]="paymentMethod === 'Razorpay'">
                <div>
                  <strong>Pay Online (Razorpay)</strong>
                  <p>Pay securely via Cards, UPI (GPay/PhonePe), or Netbanking</p>
                </div>
              </label>
              
              <label class="payment-opt" [class.selected]="paymentMethod === 'COD'">
                <input type="radio" name="payment" value="COD" (change)="paymentMethod = 'COD'" [checked]="paymentMethod === 'COD'">
                <div>
                  <strong>Cash On Delivery (COD)</strong>
                  <p>Pay cash to our delivery agent at your door</p>
                </div>
              </label>
            </div>
          </form>
        </div>

        <!-- Right Side: Order Summary -->
        <div class="checkout-summary-col">
          <div class="summary-card">
            <h3>Order Summary</h3>
            
            @if (loadingArt) {
              <p>Loading artwork...</p>
            } @else if (artwork) {
              <div class="summary-item">
                <img [src]="artwork.imageUrl || 'https://via.placeholder.com/60'" class="summary-img" />
                <div class="summary-item-details">
                  <strong>{{ artwork.title }}</strong>
                  <span>{{ artwork.medium }} · Qty: 1</span>
                </div>
                <div class="summary-item-price">₹{{ artwork.price | number }}</div>
              </div>
              
              <div class="summary-totals">
                <div class="summary-row"><span>Subtotal</span><span>₹{{ artwork.price | number }}</span></div>
                <div class="summary-row"><span>Delivery Fee</span><span>₹{{ deliveryFee | number }}</span></div>
                <div class="summary-row total-row"><span>Total</span><span>₹{{ (artwork.price + deliveryFee) | number }}</span></div>
              </div>
              
              <button class="btn btn-primary place-order-btn" [disabled]="placingOrder || checkoutForm.invalid" (click)="placeOrder()">
                {{ placingOrder ? 'Processing...' : (paymentMethod === 'Razorpay' ? 'Secure Checkout via UPI' : 'Place order (COD)') }}
              </button>
              
              @if (error) {
                <p class="text-danger mt-2">{{ error }}</p>
              }
            } @else {
              <p class="text-danger">Failed to load item.</p>
            }
          </div>
        </div>

      </div>
    </section>

    <!-- Removed MOCK UI Modals, Razorpay native JS overlay will capture focus here -->
    
    <!-- Success overlay -->
    <div class="modal-overlay" *ngIf="orderSuccess">
       <div class="modal-card" style="text-align:center">
         <div style="font-size: 3rem; color: #16a34a">✅</div>
         <h2 style="margin-bottom:8px">Order Placed Successfully!</h2>
         <p style="color:#64748b; margin-bottom: 24px;">Your order has been confirmed and placed in the queue.</p>
         <button class="btn btn-primary" (click)="goToDashboard()">Go to Dashboard</button>
       </div>
    </div>
  `,
  styles: [`
    .page-header { background: linear-gradient(160deg, #f7ede8, #fcf4f0); padding: 120px 24px 64px; }
    .page-sub { color: var(--text-muted); margin-top: 8px; }
    
    .checkout-grid { display: grid; grid-template-columns: 3fr 2fr; gap: 40px; }
    
    .form-group { margin-bottom: 16px; text-align: left; }
    .form-group label { display: block; font-weight: 600; margin-bottom: 6px; font-size: 0.85rem; color: var(--text-heading); }
    .form-control { width: 100%; padding: 10px 14px; border: 1.5px solid #e2e8f0; border-radius: 8px; font-size: 0.95rem; }
    
    .payment-options { display: flex; flex-direction: column; gap: 12px; }
    .payment-opt {
      display: flex; align-items: flex-start; gap: 12px;
      padding: 16px; border: 1.5px solid #e2e8f0; border-radius: 12px;
      cursor: pointer; transition: all 0.2s; background: white;
    }
    .payment-opt.selected { border-color: var(--pink); background: #fffcfc; }
    .payment-opt input { margin-top: 4px; }
    .payment-opt p { margin: 2px 0 0; font-size: 0.8rem; color: var(--text-muted); }
    
    .summary-card { background: #f8fafc; padding: 24px; border-radius: 16px; border: 1px solid #e2e8f0; }
    .summary-item { display: flex; gap: 12px; margin-bottom: 24px; }
    .summary-img { width: 70px; height: 70px; object-fit: cover; border-radius: 8px; }
    .summary-item-details { flex: 1; display: flex; flex-direction: column; }
    .summary-item-details span { font-size: 0.8rem; color: var(--text-muted); margin-top: 4px; }
    .summary-item-price { font-weight: 700; color: var(--text-heading); }
    
    .summary-totals { border-top: 1px solid #e2e8f0; padding-top: 16px; margin-bottom: 24px; }
    .summary-row { display: flex; justify-content: space-between; margin-bottom: 12px; font-size: 0.9rem; }
    .total-row { font-size: 1.1rem; font-weight: 700; color: var(--text-heading); border-top: 1px solid #e2e8f0; padding-top: 12px; }
    
    .place-order-btn { width: 100%; padding: 14px; font-size: 1rem; }
    
    .modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.6); backdrop-filter: blur(4px); display: flex; align-items: center; justify-content: center; z-index: 1000; }
    .razorpay-mock-card { background: white; width: 100%; max-width: 400px; border-radius: 12px; overflow: hidden; box-shadow: 0 20px 40px rgba(0,0,0,0.2); }
    .rzp-header { background: #0f172a; color: white; padding: 16px; font-size: 0.9rem; }
    .rzp-body { padding: 32px 24px; text-align: center; }
    .modal-card { background: white; padding: 40px; border-radius: 16px; max-width: 400px; width: 100%; }
    
    @media (max-width: 800px) { .checkout-grid { grid-template-columns: 1fr; } }
  `]
})
export class OrderComponent implements OnInit {
  checkoutForm!: FormGroup;
  artwork: Artwork | null = null;
  loadingArt = true;
  deliveryFee = 1500;
  paymentMethod: 'Razorpay' | 'COD' = 'Razorpay';
  
  placingOrder = false;
  error = '';
  
  // Razorpay state
  razorpayOrderId = '';
  pendingSystemOrderId = '';
  verifying = false;
  orderSuccess = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private artSvc: ArtworkService,
    private orderSvc: OrderService,
    private paymentSvc: PaymentService,
    private auth: AuthService
  ) {}

  ngOnInit() {
    const artId = this.route.snapshot.queryParamMap.get('artId');
    if (!artId) {
      this.error = 'No artwork selected. Please go to portfolio.';
      this.loadingArt = false;
      return;
    }

    this.artSvc.getById(artId).subscribe({
      next: (art) => {
        this.artwork = art;
        this.loadingArt = false;
      },
      error: () => {
        this.error = 'Failed to load artwork details.';
        this.loadingArt = false;
      }
    });

    const usr = this.auth.currentUser;
    this.checkoutForm = this.fb.group({
      name: [usr?.name || '', Validators.required],
      email: [(usr as any)?.email || '', [Validators.required, Validators.email]],
      address: ['', Validators.required]
    });
  }

  placeOrder() {
    if (this.checkoutForm.invalid || !this.artwork) return;
    this.placingOrder = true;
    this.error = '';

    if (this.paymentMethod === 'COD') {
      // COD: place order immediately
      this.createSystemOrder(PaymentMode.CashOnDelivery, () => {
        this.placingOrder = false;
        this.orderSuccess = true;
      });
    } else {
      // Razorpay: open payment gateway FIRST, create order only after success
      this.startRazorpayFirst();
    }
  }

  /** For COD and post-payment: creates the actual system order */
  private createSystemOrder(pMode: PaymentMode, onSuccess: () => void) {
    this.orderSvc.placeOrder({
      customerId: this.auth.currentUser?.customerId || '',
      customerName: this.checkoutForm.value.name,
      customerEmail: this.checkoutForm.value.email,
      shippingAddress: this.checkoutForm.value.address,
      type: OrderType.ReadyMade,
      paymentMode: pMode,
      items: [{
        artworkId: this.artwork!.id,
        artworkTitle: this.artwork!.title,
        quantity: 1,
        unitPrice: this.artwork!.price + this.deliveryFee
      }]
    }).subscribe({
      next: (res: any) => {
        const isSuccess = res.success !== undefined ? res.success : res.Success;
        if (!isSuccess) {
          this.error = res.message || res.Message || 'Failed to place order.';
          this.placingOrder = false;
          return;
        }
        this.pendingSystemOrderId = res.orderId || res.OrderId;
        onSuccess();
      },
      error: (err) => {
        const body = err.error as any;
        this.error = body?.message || body?.Message || 'Failed to place order. Please try again.';
        this.placingOrder = false;
      }
    });
  }

  /** Step 1 for Razorpay: create a Razorpay session (no system order yet) */
  startRazorpayFirst() {
    const totalAmount = this.artwork!.price + this.deliveryFee;
    
    // generates a valid GUID safely without relying on crypto.randomUUID
    const tempId = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        let r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });

    this.paymentSvc.createRazorpayOrder(totalAmount, tempId).subscribe({
      next: (rzpInfo: any) => {
        this.razorpayOrderId = rzpInfo.razorpayOrderId || rzpInfo.RazorpayOrderId;

        const options = {
          key: 'rzp_test_Sb4Wzq4ppxTZPM',
          amount: totalAmount * 100,
          currency: rzpInfo.currency || 'INR',
          name: 'Artistic Sisters',
          description: `Payment for ${this.artwork!.title}`,
          image: '/assets/logo.png',
          order_id: this.razorpayOrderId,
          handler: (response: any) => {
            // Payment succeeded — NOW create the system order then verify
            this.createSystemOrder(PaymentMode.Card, () => {
              this.verifyRazorpaySuccess(
                response.razorpay_payment_id,
                response.razorpay_order_id,
                response.razorpay_signature
              );
            });
          },
          prefill: {
            name: this.checkoutForm.value.name,
            contact: (this.auth.currentUser as any)?.phone || '9999999999',
            email: this.checkoutForm.value.email
          },
          theme: { color: '#e47a8c' },
          modal: {
            ondismiss: () => {
              // User cancelled — NO order is created
              this.placingOrder = false;
              this.error = 'Payment cancelled. No order was placed.';
            }
          }
        };

        this.waitForRazorpay().then(() => {
          const rzp = new (window as any).Razorpay(options);
          rzp.on('payment.failed', (response: any) => {
            // Payment failed — NO order is created
            this.error = 'Payment failed: ' + (response.error?.description || 'Unknown error');
            this.placingOrder = false;
          });
          rzp.open();
        }).catch(() => {
          this.error = 'Could not load Razorpay. Please check your internet connection and try again.';
          this.placingOrder = false;
        });
      },
      error: (err) => {
        const body = err.error as any;
        this.error = body?.message || body?.error || 'Failed to connect to Payment Gateway. Is the backend running?';
        this.placingOrder = false;
      }
    });
  }
  
  verifyRazorpaySuccess(paymentId: string, razorpayOrderId: string, signature: string, retries = 0) {
    this.verifying = true;
    this.placingOrder = true;
    
    this.paymentSvc.verifyRazorpayPayment(this.pendingSystemOrderId, razorpayOrderId, paymentId, signature).subscribe({
      next: () => {
        this.verifying = false;
        this.orderSuccess = true;
        this.placingOrder = false;
      },
      error: (err) => {
        if (err.status === 404 && retries < 10) {
          // The async OrderPlacedEvent hasn't been consumed by PaymentService yet. Retry in 1s.
          setTimeout(() => this.verifyRazorpaySuccess(paymentId, razorpayOrderId, signature, retries + 1), 1000);
          return;
        }
        this.verifying = false;
        this.error = 'Payment signature verification failed. Please contact support.';
        this.placingOrder = false;
      }
    });
  }
  
  waitForRazorpay(maxRetries = 20): Promise<void> {
    return new Promise((resolve, reject) => {
      let retries = 0;
      const checkInterval = setInterval(() => {
        if ((window as any).Razorpay) {
          clearInterval(checkInterval);
          resolve();
        } else {
          retries++;
          if (retries >= maxRetries) {
            clearInterval(checkInterval);
            reject(new Error('Razorpay SDK timed out.'));
          }
        }
      }, 500); // Check every 500ms
    });
  }

  goToDashboard() {
    this.router.navigate(['/customer/dashboard']);
  }
}


