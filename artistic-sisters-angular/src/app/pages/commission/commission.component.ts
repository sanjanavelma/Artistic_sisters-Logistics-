import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { OrderService } from '../../core/services/order.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-commission',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './commission.component.html',
  styles: [`
    .page-header { background: linear-gradient(160deg, #f7ede8, #fcf4f0); padding: 120px 24px 64px; }
    .page-sub { color: var(--text-muted); margin-top: 8px; }
    
    .commission-section { padding: 60px 24px; background: #fff; }
    .auth-card {
      background: white;
      border-radius: 16px;
      padding: 40px;
      box-shadow: 0 10px 30px rgba(0,0,0,0.05);
      max-width: 800px;
      margin: 0 auto;
    }
    .form-group { margin-bottom: 24px; text-align: left; }
    .form-label { display: block; font-weight: 500; margin-bottom: 8px; color: var(--text-dark); }
    .form-control {
      width: 100%;
      padding: 12px 16px;
      border: 1px solid rgba(0,0,0,0.1);
      border-radius: 8px;
      font-size: 1rem;
      transition: all 0.3s ease;
      font-family: inherit;
    }
    .form-control:focus {
      outline: none;
      border-color: var(--primary-color);
      box-shadow: 0 0 0 4px rgba(222, 142, 142, 0.1);
    }
    .text-danger { color: #dc3545; font-size: 0.875rem; margin-top: 4px; display: block; }
    
    .grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 24px; }
    @media(max-width: 768px) { .grid-2 { grid-template-columns: 1fr; } }
    
    .btn-submit {
      width: 100%;
      padding: 14px;
      font-size: 1.1rem;
      margin-top: 16px;
    }
    
    .success-message {
      text-align: center;
      padding: 40px 24px;
    }
    .success-icon {
      font-size: 64px;
      color: #28a745;
      margin-bottom: 24px;
    }
  `]
})
export class CommissionComponent implements OnInit {
  commissionForm!: FormGroup;
  isSubmitting = false;
  isSuccess = false;
  errorMessage = '';

  artworkTypes = ['Portrait', 'Landscape', 'Abstract', 'Character Design', 'Other'];
  mediums = ['Oil', 'Acrylic', 'Watercolor', 'Digital', 'Pencil/Charcoal', 'Mixed Media'];
  sizes = ['Small (A5, 8x10)', 'Medium (A4, 11x14)', 'Large (A3, 16x20)', 'Extra Large (A2+)', 'Custom'];

  constructor(
    private fb: FormBuilder,
    private orderService: OrderService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    // Pre-fill fields if available
    const user = this.authService.currentUser;
    const prefillEmail = localStorage.getItem('as_email') || ''; // if stored

    this.commissionForm = this.fb.group({
      customerName: [user?.name || '', [Validators.required, Validators.minLength(2)]],
      customerEmail: [prefillEmail, [Validators.required, Validators.email]],
      artworkType: ['', Validators.required],
      medium: ['', Validators.required],
      size: ['', Validators.required],
      budgetMin: ['', [Validators.required, Validators.min(1)]],
      budgetMax: ['', [Validators.required, Validators.min(1)]],
      referencePhotoUrl: ['', [Validators.pattern('https?://.+')]],
      specialInstructions: ['']
    });
  }

  onSubmit() {
    if (this.commissionForm.invalid) {
      this.commissionForm.markAllAsTouched();
      return;
    }

    const val = this.commissionForm.value;
    if (val.budgetMax < val.budgetMin) {
      this.errorMessage = 'Maximum budget must be greater than or equal to minimum budget.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const req = {
      customerId: this.authService.currentUser?.customerId || '',
      ...val
    };

    this.orderService.placeCommission(req).subscribe({
      next: (res: any) => {
        this.isSubmitting = false;
        const isSuccess = res.success !== undefined ? res.success : res.Success;
        if (isSuccess) {
          this.isSuccess = true;
        } else {
          this.errorMessage = res.message || res.Message || 'Failed to submit request.';
        }
      },
      error: (err) => {
        this.isSubmitting = false;
        this.errorMessage = 'An error occurred while submitting. Please try again.';
        console.error(err);
      }
    });
  }
}
