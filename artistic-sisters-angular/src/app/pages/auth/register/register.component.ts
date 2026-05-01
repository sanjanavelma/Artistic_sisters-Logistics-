import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

const authStyles = `
  .auth-page { display:grid; grid-template-columns:1fr 1fr; min-height:100vh; padding-top:70px; }
  .auth-left {
    background:linear-gradient(160deg,#f7ede8,#f0e8f4);
    display:flex; flex-direction:column; justify-content:center; align-items:center;
    padding:48px; gap:32px;
  }
  .auth-brand { text-align:center; }
  .brand-logo { width:72px; height:72px; border-radius:50%; overflow:hidden; margin:0 auto 16px; border:3px solid var(--pink-light); }
  .brand-logo img { width:100%; height:100%; object-fit:cover; }
  .auth-brand h2 { margin-bottom:8px; }
  .auth-brand p  { color:var(--text-muted); font-size:0.93rem; line-height:1.7; max-width:280px; }
  .auth-art-preview { width:100%; max-width:340px; border-radius:var(--radius-lg); overflow:hidden; box-shadow:var(--shadow-lg); }
  .auth-art-preview img { width:100%; aspect-ratio:4/3; object-fit:cover; display:block; }
  .auth-right { display:flex; align-items:center; justify-content:center; padding:48px 24px; background:var(--white); overflow-y:auto; }
  .auth-card { width:100%; max-width:480px; }
  .auth-card h2 { margin-bottom:6px; }
  .auth-sub { color:var(--text-muted); font-size:0.9rem; margin-bottom:28px; }
  .auth-submit { width:100%; justify-content:center; padding:13px !important; font-size:1rem !important; margin-top:4px; }
  .auth-submit:disabled { opacity:0.7; cursor:not-allowed; }
  .form-row { display:grid; grid-template-columns:1fr 1fr; gap:16px; }
  .input-eye { position:relative; }
  .input-eye .form-control { padding-right:44px; }
  .eye-btn { position:absolute; right:12px; top:50%; transform:translateY(-50%); background:none; border:none; cursor:pointer; font-size:1rem; }
  .auth-divider { display:flex; align-items:center; gap:12px; margin:24px 0; }
  .auth-divider::before,.auth-divider::after { content:''; flex:1; height:1px; background:var(--border); }
  .auth-divider span { font-size:0.82rem; color:var(--text-muted); }
  .auth-switch { text-align:center; font-size:0.88rem; color:var(--text-muted); }
  .auth-switch a { color:var(--pink); font-weight:500; }
  .auth-switch a:hover { text-decoration:underline; }
  .btn-spinner { width:16px; height:16px; border:2px solid rgba(255,255,255,0.4); border-top-color:#fff; border-radius:50%; animation:spin .7s linear infinite; display:inline-block; }
  @media(max-width:768px){ .auth-page{grid-template-columns:1fr;} .auth-left{display:none;} .form-row{grid-template-columns:1fr;} }
`;

@Component({
  selector: 'app-register',
  imports: [FormsModule, CommonModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-left">
        <div class="auth-brand">
          <div class="brand-logo">
            <img src="https://i.postimg.cc/NjcqPR8B/logo-artistic-sisters.jpg" alt="Logo" />
          </div>
          <h2>Join us today</h2>
          <p>Create an account to browse artworks, place orders and commission custom pieces.</p>
        </div>
        <div class="auth-art-preview">
          <img src="https://i.postimg.cc/GtTH1B7d/artgallery.jpg" alt="Art gallery" />
        </div>
      </div>

      <div class="auth-right">
        <div class="auth-card">
          <h2>Create Account</h2>
          <p class="auth-sub">Fill in your details to get started</p>

          @if (error)   { <div class="alert alert-error">{{ error }}</div> }
          @if (success) { <div class="alert alert-success">✅ Registered! Redirecting...</div> }

          <form (ngSubmit)="submit()">
            <div class="form-row">
              <div class="form-group">
                <label>Full Name</label>
                <input class="form-control" type="text" [(ngModel)]="form.name" name="name" placeholder="Your full name" required />
              </div>
              <div class="form-group">
                <label>Phone Number</label>
                <input class="form-control" type="tel" [(ngModel)]="form.phone" name="phone" placeholder="9876543210" required />
              </div>
            </div>
            <div class="form-group">
              <label>Email Address</label>
              <input class="form-control" type="email" [(ngModel)]="form.email" name="email" placeholder="you@example.com" required />
            </div>
            <div class="form-group">
              <label>Delivery Address</label>
              <input class="form-control" type="text" [(ngModel)]="form.address" name="address" placeholder="Your delivery address" required />
            </div>
            <div class="form-group">
              <label>Password</label>
              <div class="input-eye">
                <input class="form-control" [type]="showPw ? 'text' : 'password'" [(ngModel)]="form.password" name="password" placeholder="Min 8 characters" required minlength="8" />
                <button type="button" class="eye-btn" (click)="showPw=!showPw">{{ showPw ? '🙈' : '👁️' }}</button>
              </div>
            </div>
            <button type="submit" class="btn btn-primary auth-submit" [disabled]="loading">
              @if (loading) { <span class="btn-spinner"></span> } @else { Create Account }
            </button>
          </form>

          <div class="auth-divider"><span>or</span></div>
          <p class="auth-switch">
            Already have an account? <a routerLink="/auth/login">Sign in</a>
          </p>
        </div>
      </div>
    </div>
  `,
  styles: [`${authStyles}`]
})
export class RegisterComponent {
  form = { name:'', email:'', password:'', phone:'', address:'' };
  loading = false; error = ''; success = false; showPw = false;
  constructor(private auth: AuthService, private router: Router) {}

  submit() {
    if (!this.form.name || !this.form.email || !this.form.password || !this.form.phone || !this.form.address)
      { this.error = 'Please fill all fields'; return; }
    this.loading = true; this.error = '';
    this.auth.register(this.form).subscribe({
      next: (res: any) => {
        this.loading = false;
        const isSuccess = res.success !== undefined ? res.success : res.Success;
        if (isSuccess) { this.success = true; setTimeout(() => this.router.navigate(['/portfolio']), 1500); }
        else this.error = res.message || res.Message || 'Registration failed.';
      },
      error: (err) => { 
        this.loading = false; 
        const errorBody = err.error as any;
        this.error = errorBody?.message || errorBody?.Message || err.message || 'Server error. Is the backend running?'; 
      }
    });
  }
}
