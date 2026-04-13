import { Component, OnInit, HostListener } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive, CommonModule],
  template: `
    <nav class="navbar" [class.scrolled]="scrolled">
      <div class="nav-container">
        <!-- Logo -->
        <a routerLink="/" class="nav-logo">
          <div class="logo-circle">
            <img src="https://i.postimg.cc/NjcqPR8B/logo-artistic-sisters.jpg" alt="Artistic Sisters Logo" />
          </div>
          <span class="logo-text">Artistic<span class="underscore">_</span>Sisters</span>
        </a>

        <!-- Desktop Nav -->
        <ul class="nav-links" [class.open]="menuOpen">
          @if (!user || (user.role !== 'DeliveryAgent' && user.role !== 'Artist')) {
            <li><a routerLink="/"         routerLinkActive="active" [routerLinkActiveOptions]="{exact:true}" (click)="closeMenu()">Home</a></li>
            <li><a routerLink="/about"    routerLinkActive="active" (click)="closeMenu()">About</a></li>
            <li><a routerLink="/portfolio" routerLinkActive="active" (click)="closeMenu()">Portfolio</a></li>
            <li><a routerLink="/services" routerLinkActive="active" (click)="closeMenu()">Services</a></li>
            <li><a routerLink="/contact"  routerLinkActive="active" (click)="closeMenu()">Contact</a></li>
          }

          <!-- Auth links -->
          @if (!user) {
            <li class="nav-auth">
              <a routerLink="/auth/login" class="btn btn-ghost nav-btn" (click)="closeMenu()">Login</a>
              <a routerLink="/auth/register" class="btn btn-primary nav-btn" (click)="closeMenu()">Register</a>
            </li>
          } @else {
            <li class="nav-auth">
              @if (user.role === 'Artist') {
                <a routerLink="/artist" class="btn btn-ghost nav-btn" (click)="closeMenu()">🎨 Dashboard</a>
                <a routerLink="/admin/dashboard" class="btn btn-ghost nav-btn" (click)="closeMenu()">⚙️ Admin</a>
              }
              @if (user.role === 'DeliveryAgent') {
                <a routerLink="/delivery/dashboard" class="btn btn-ghost nav-btn" (click)="closeMenu()">🚚 My Deliveries</a>
              }
              <div class="user-menu">
                <button class="user-chip" (click)="toggleUser($event)">
                  <span class="user-avatar">{{ user.name.charAt(0) }}</span>
                  <span>{{ user.name.split(' ')[0] }}</span>
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 9l6 6 6-6"/></svg>
                </button>
                @if (userMenuOpen) {
                  <div class="user-dropdown">
                    <span class="user-role-badge">{{ user.role }}</span>
                    @if (user.role === 'Customer') {
                      <a routerLink="/customer/dashboard" class="dropdown-item" (click)="closeMenu()">Dashboard</a>
                    }
                    @if (user.role === 'Artist') {
                      <a routerLink="/artist" class="dropdown-item" (click)="closeMenu()">Artist Dashboard</a>
                      <a routerLink="/admin/dashboard" class="dropdown-item" (click)="closeMenu()">Admin Dashboard</a>
                    }
                    @if (user.role === 'DeliveryAgent') {
                      <a routerLink="/delivery/dashboard" class="dropdown-item" (click)="closeMenu()">My Deliveries</a>
                    }
                    <button class="dropdown-item logout" (click)="logout()">Sign out</button>
                  </div>
                }
              </div>
            </li>
          }
        </ul>

        <!-- Hamburger -->
        <button class="hamburger" (click)="toggleMenu()" [class.active]="menuOpen" aria-label="Menu">
          <span></span><span></span><span></span>
        </button>
      </div>
    </nav>

    <!-- Mobile backdrop -->
    @if (menuOpen) {
      <div class="nav-backdrop" (click)="closeMenu()"></div>
    }
  `,
  styles: [`
    .navbar {
      position: fixed; top: 0; left: 0; right: 0; z-index: 100;
      background: rgba(250,247,244,0.92);
      backdrop-filter: blur(12px);
      -webkit-backdrop-filter: blur(12px);
      border-bottom: 1px solid transparent;
      transition: all 0.3s ease;
    }
    .navbar.scrolled {
      border-bottom-color: var(--border);
      box-shadow: 0 2px 20px rgba(60,40,30,0.06);
    }
    .nav-container {
      max-width: 1400px; margin: 0 auto; padding: 0 32px; /* Expanding max-width to match hero and increasing padding */
      display: flex; align-items: center; justify-content: space-between;
      height: 90px; /* Thicker navbar */
    }
    .nav-logo {
      display: flex; align-items: center; gap: 14px; text-decoration: none;
    }
    .logo-circle {
      width: 54px; height: 54px; border-radius: 50%;
      overflow: hidden; border: 2px solid var(--pink-light);
      flex-shrink: 0;
    }
    .logo-circle img { width: 100%; height: 100%; object-fit: cover; }
    .logo-text {
      font-family: var(--serif); font-size: 1.7rem; font-weight: 600;
      color: var(--text-heading); letter-spacing: -0.01em;
    }
    .logo-text .underscore { color: var(--pink); }

    .nav-links {
      display: flex; align-items: center; gap: 8px; list-style: none; /* Slightly bigger gap */
    }
    .nav-links li a:not(.btn) {
      padding: 8px 18px; border-radius: 6px; font-size: 1.15rem; /* Larger links */
      font-weight: 500; color: var(--text-body); transition: var(--transition);
      position: relative;
    }
    .nav-links li a:not(.btn):hover { color: var(--text-heading); }
    .nav-links li a.active {
      color: var(--pink);
    }
    .nav-links li a.active::after {
      content: ''; position: absolute; bottom: -2px; left: 18px; right: 18px;
      height: 2px; background: var(--pink); border-radius: 1px;
    }
    .nav-auth { display: flex; align-items: center; gap: 12px; margin-left: 20px; }
    .nav-btn { padding: 10px 22px !important; font-size: 1.05rem !important; }

    /* User chip */
    .user-menu { position: relative; }
    .user-chip {
      display: flex; align-items: center; gap: 8px;
      padding: 8px 16px; border-radius: var(--radius-full);
      border: 1.5px solid var(--border); background: var(--white);
      font-size: 1.05rem; font-weight: 500; color: var(--text-heading);
      cursor: pointer; transition: var(--transition);
    }
    .user-chip:hover { border-color: var(--pink); }
    .user-avatar {
      width: 34px; height: 34px; border-radius: 50%; background: var(--pink);
      color: #fff; display: flex; align-items: center; justify-content: center;
      font-size: 0.95rem; font-weight: 600;
    }
    .user-dropdown {
      position: absolute; top: calc(100% + 8px); right: 0;
      background: var(--white); border: 1px solid var(--border);
      border-radius: var(--radius-md); padding: 12px;
      box-shadow: var(--shadow-md); min-width: 160px;
      display: flex; flex-direction: column; gap: 8px;
    }
    .user-role-badge {
      font-size: 0.75rem; font-weight: 600; color: var(--pink);
      text-transform: uppercase; letter-spacing: 0.08em;
      padding: 3px 8px; background: var(--pink-light);
      border-radius: var(--radius-full); text-align: center;
    }
    .dropdown-item {
      padding: 8px 10px; border-radius: var(--radius-sm);
      font-size: 0.88rem; font-weight: 500; cursor: pointer;
      transition: var(--transition); text-align: left;
      background: none; border: none; font-family: var(--sans);
    }
    .dropdown-item.logout { color: #b91c1c; }
    .dropdown-item.logout:hover { background: #fef2f2; }

    /* Hamburger */
    .hamburger { display: none; flex-direction: column; gap: 5px; padding: 6px; cursor: pointer; background: none; border: none; }
    .hamburger span { display: block; width: 22px; height: 2px; background: var(--text-heading); border-radius: 2px; transition: var(--transition); }
    .hamburger.active span:nth-child(1) { transform: translateY(7px) rotate(45deg); }
    .hamburger.active span:nth-child(2) { opacity: 0; }
    .hamburger.active span:nth-child(3) { transform: translateY(-7px) rotate(-45deg); }

    .nav-backdrop { position: fixed; inset: 0; z-index: 99; }

    @media (max-width: 768px) {
      .hamburger { display: flex; }
      .nav-links {
        position: fixed; top: 70px; left: 0; right: 0;
        background: var(--cream); border-top: 1px solid var(--border);
        flex-direction: column; align-items: stretch;
        padding: 16px 24px 24px; gap: 4px;
        transform: translateY(-110%); opacity: 0;
        transition: transform 0.3s ease, opacity 0.3s ease;
        z-index: 100; pointer-events: none;
      }
      .nav-links.open { transform: translateY(0); opacity: 1; pointer-events: all; }
      .nav-links li a:not(.btn) { display: block; padding: 12px 0; }
      .nav-auth { flex-direction: column; align-items: stretch; margin-left: 0; margin-top: 8px; }
      .nav-btn { text-align: center; }
    }
  `]
})
export class NavbarComponent implements OnInit {
  scrolled    = false;
  menuOpen    = false;
  userMenuOpen = false;
  user: { name: string; role: string; customerId: string } | null = null;

  constructor(private auth: AuthService) {}

  ngOnInit() {
    this.auth.user$.subscribe(u => this.user = u);
  }

  @HostListener('window:scroll')
  onScroll() { this.scrolled = window.scrollY > 20; }

  toggleMenu()  { this.menuOpen = !this.menuOpen; this.userMenuOpen = false; }
  closeMenu()   { this.menuOpen = false; }
  toggleUser(e: Event) { e.stopPropagation(); this.userMenuOpen = !this.userMenuOpen; }
  logout()      { this.auth.logout(); this.menuOpen = false; this.userMenuOpen = false; }

  @HostListener('document:click')
  onDocClick() { this.userMenuOpen = false; }
}
