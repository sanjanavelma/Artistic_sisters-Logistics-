import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

const authStyles = `
  .auth-page { display:grid; grid-template-columns:1fr 1fr; min-height:100vh; padding-top:70px; }
  .auth-left { background:linear-gradient(160deg,#0f172a,#1e3a5f); display:flex; flex-direction:column; justify-content:center; align-items:center; padding:48px; gap:32px; }
  .auth-brand { text-align:center; }
  .brand-logo { width:72px; height:72px; border-radius:50%; overflow:hidden; margin:0 auto 16px; border:3px solid rgba(255,255,255,0.2); }
  .brand-logo img { width:100%; height:100%; object-fit:cover; }
  .auth-brand h2 { margin-bottom:8px; color:white; }
  .auth-brand p  { color:#94a3b8; font-size:0.93rem; line-height:1.7; max-width:280px; }
  .delivery-icon { font-size:96px; filter:drop-shadow(0 8px 24px rgba(0,0,0,0.4)); }
  .auth-right { display:flex; align-items:center; justify-content:center; padding:48px 24px; background:var(--white); overflow-y:auto; }
  .auth-card { width:100%; max-width:480px; }
  .auth-card h2 { margin-bottom:6px; }
  .auth-sub { color:var(--text-muted); font-size:0.9rem; margin-bottom:20px; }
  .agent-info-banner { background:#eff6ff; border:1px solid #bfdbfe; border-radius:var(--radius-sm); padding:12px 14px; display:flex; gap:10px; align-items:flex-start; margin-bottom:24px; }
  .agent-info-banner p { font-size:0.85rem; color:#1e40af; line-height:1.6; }
  .auth-submit { width:100%; justify-content:center; padding:13px !important; font-size:1rem !important; margin-top:4px; background:#1e293b !important; }
  .auth-submit:disabled { opacity:0.7; cursor:not-allowed; }
  .auth-submit:hover:not(:disabled) { background:#334155 !important; }
  .form-row { display:grid; grid-template-columns:1fr 1fr; gap:16px; }
  .input-eye { position:relative; }
  .input-eye .form-control { padding-right:44px; }
  .eye-btn { position:absolute; right:12px; top:50%; transform:translateY(-50%); background:none; border:none; cursor:pointer; font-size:1rem; }
  .auth-switch { text-align:center; font-size:0.88rem; color:var(--text-muted); }
  .auth-switch a { color:#3b82f6; font-weight:500; }
  .btn-spinner { width:16px; height:16px; border:2px solid rgba(255,255,255,0.4); border-top-color:#fff; border-radius:50%; animation:spin .7s linear infinite; display:inline-block; }
  @keyframes spin { to { transform:rotate(360deg); } }
  @media(max-width:768px){ .auth-page{grid-template-columns:1fr;} .auth-left{display:none;} .form-row{grid-template-columns:1fr;} }
`;

@Component({
  selector: 'app-register-agent',
  imports: [FormsModule, CommonModule, RouterLink],
  template: `
    <div class="auth-page">
      <!-- Left panel -->
      <div class="auth-left">
        <div class="auth-brand">
          <div class="brand-logo">
            <img src="https://i.postimg.cc/NjcqPR8B/logo-artistic-sisters.jpg" alt="Logo" />
          </div>
          <h2>Join as Delivery Agent</h2>
          <p>Register to start managing deliveries and updating order locations in real time.</p>
        </div>
        <div class="delivery-icon">🚚</div>
      </div>

      <!-- Right panel -->
      <div class="auth-right">
        <div class="auth-card">
          <h2>Agent Registration</h2>
          <p class="auth-sub">Create your delivery agent account</p>

          @if (error)   { <div class="alert alert-error">{{ error }}</div> }
          @if (success) { <div class="alert alert-success">🚚 Account created! Redirecting to your dashboard...</div> }

          <div class="agent-info-banner">
            <span>ℹ️</span>
            <p>As a delivery agent you can view your assigned orders, update delivery status and push real-time GPS location to customers.</p>
          </div>

          <form (ngSubmit)="submit()">
            <div class="form-row">
              <div class="form-group">
                <label>Full Name</label>
                <input id="agent-name" class="form-control" type="text"
                  [(ngModel)]="form.name" name="name"
                  placeholder="Your full name" required />
              </div>
              <div class="form-group">
                <label>Phone Number</label>
                <input id="agent-phone" class="form-control" type="tel"
                  [(ngModel)]="form.phone" name="phone"
                  placeholder="9876543210" required />
              </div>
            </div>

            <div class="form-group">
              <label>Email Address</label>
              <input id="agent-email" class="form-control" type="email"
                [(ngModel)]="form.email" name="email"
                placeholder="agent@example.com" required />
            </div>

            <div class="form-group">
              <label>Home Address</label>
              <input id="agent-address" class="form-control" type="text"
                [(ngModel)]="form.address" name="address"
                placeholder="Your address" required />
            </div>

            <div class="form-group">
              <label>Password</label>
              <div class="input-eye">
                <input id="agent-password" class="form-control"
                  [type]="showPw ? 'text' : 'password'"
                  [(ngModel)]="form.password" name="password"
                  placeholder="Min 8 characters" required minlength="8" />
                <button type="button" class="eye-btn" (click)="showPw=!showPw">
                  {{ showPw ? '🙈' : '👁️' }}
                </button>
              </div>
            </div>

            <button id="register-agent-btn" type="submit"
              class="btn btn-primary auth-submit" [disabled]="loading">
              @if (loading) { <span class="btn-spinner"></span> }
              @else { 🚚 Register as Delivery Agent }
            </button>
          </form>

          <p class="auth-switch" style="margin-top:20px">
            Already have an account? <a routerLink="/auth/login">Sign in</a>
          </p>
        </div>
      </div>
    </div>
  `,
  styles: [`${authStyles}`]
})
export class RegisterAgentComponent {
  form = { name: '', email: '', password: '', phone: '', address: '' };
  loading = false;
  error = '';
  success = false;
  showPw = false;

  constructor(private auth: AuthService, private router: Router) {}

  submit() {
    if (!this.form.name || !this.form.email || !this.form.password
      || !this.form.phone || !this.form.address) {
      this.error = 'Please fill all fields';
      return;
    }
    this.loading = true;
    this.error = '';

    this.auth.registerAgent(this.form).subscribe({
      next: res => {
        this.loading = false;
        if (res.success) {
          this.success = true;
          setTimeout(() => this.router.navigate(['/delivery/dashboard']), 1500);
        } else {
          this.error = res.message;
        }
      },
      error: () => {
        this.loading = false;
        this.error = 'Server error. Is the backend running?';
      }
    });
  }
}
