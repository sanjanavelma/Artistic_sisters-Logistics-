import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-footer',
  imports: [RouterLink],
  template: `
    <footer class="footer">
      <div class="footer-inner">
        <!-- Brand -->
        <div class="footer-brand">
          <div class="footer-logo">
            <div class="logo-circle">
              <img src="https://i.postimg.cc/NjcqPR8B/logo-artistic-sisters.jpg" alt="Logo" />
            </div>
            <span>Artistic_Sisters</span>
          </div>
          <p class="footer-tagline">Crafting memories, one stroke at a time.</p>
        </div>

        <!-- Quick links -->
        <div class="footer-links">
          <h4>Quick Links</h4>
          <ul>
            <li><a routerLink="/">Home</a></li>
            <li><a routerLink="/about">About</a></li>
            <li><a routerLink="/portfolio">Portfolio</a></li>
            <li><a routerLink="/services">Services</a></li>
            <li><a routerLink="/contact">Contact</a></li>
          </ul>
        </div>

        <!-- Social -->
        <div class="footer-social">
          <h4>Connect</h4>
          <div class="social-icons">
            <a href="https://www.instagram.com/artistic_sisters._/" target="_blank" rel="noopener" class="social-icon" aria-label="Instagram">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
                <rect x="2" y="2" width="20" height="20" rx="5" ry="5"/><path d="M16 11.37A4 4 0 1 1 12.63 8 4 4 0 0 1 16 11.37z"/><line x1="17.5" y1="6.5" x2="17.51" y2="6.5"/>
              </svg>
            </a>
            <a href="mailto:artistic.sisters07@gmail.com" class="social-icon" aria-label="Email">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
                <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/>
              </svg>
            </a>
          </div>
        </div>
      </div>

      <div class="footer-bottom">
        <span>© 2026 Artistic_Sisters. All rights reserved.</span>
        <span class="footer-legal">
          <a href="#">Privacy Policy</a> | <a href="#">Terms of Service</a>
        </span>
      </div>
    </footer>
  `,
  styles: [`
    .footer {
      background: var(--cream-dark);
      border-top: 1px solid var(--border);
      padding: 56px 24px 0;
    }
    .footer-inner {
      max-width: 1200px; margin: 0 auto;
      display: grid; grid-template-columns: 2fr 1fr 1fr;
      gap: 48px; padding-bottom: 40px;
    }
    .footer-logo {
      display: flex; align-items: center; gap: 10px;
      font-family: var(--serif); font-size: 1.1rem;
      font-weight: 600; color: var(--text-heading);
      margin-bottom: 12px;
    }
    .logo-circle {
      width: 36px; height: 36px; border-radius: 50%;
      overflow: hidden; border: 2px solid var(--pink-light);
    }
    .logo-circle img { width: 100%; height: 100%; object-fit: cover; }
    .footer-tagline { font-size: 0.88rem; color: var(--text-muted); line-height: 1.5; }

    .footer-links h4, .footer-social h4 {
      font-family: var(--sans); font-size: 0.8rem; font-weight: 600;
      text-transform: uppercase; letter-spacing: 0.1em;
      color: var(--text-muted); margin-bottom: 16px;
    }
    .footer-links ul { display: flex; flex-direction: column; gap: 8px; }
    .footer-links a {
      font-size: 0.9rem; color: var(--text-body);
      transition: var(--transition);
    }
    .footer-links a:hover { color: var(--pink); }

    .social-icons { display: flex; gap: 12px; }
    .social-icon {
      width: 40px; height: 40px; border-radius: 50%;
      border: 1.5px solid var(--border); background: var(--white);
      display: flex; align-items: center; justify-content: center;
      color: var(--text-body); transition: var(--transition);
    }
    .social-icon:hover { background: var(--pink); color: #fff; border-color: var(--pink); transform: translateY(-2px); }

    .footer-bottom {
      border-top: 1px solid var(--border);
      max-width: 1200px; margin: 0 auto;
      padding: 20px 0;
      display: flex; justify-content: space-between; align-items: center;
      font-size: 0.82rem; color: var(--text-muted);
    }
    .footer-legal a { color: var(--text-muted); transition: var(--transition); }
    .footer-legal a:hover { color: var(--pink); }

    @media (max-width: 768px) {
      .footer-inner { grid-template-columns: 1fr 1fr; gap: 32px; }
      .footer-brand { grid-column: 1 / -1; }
      .footer-bottom { flex-direction: column; gap: 8px; text-align: center; }
    }
    @media (max-width: 480px) {
      .footer-inner { grid-template-columns: 1fr; }
    }
  `]
})
export class FooterComponent {}
