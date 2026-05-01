import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-contact',
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <div class="container text-center">
        <h1 class="fade-up">Get in Touch</h1>
        <div class="divider"></div>
        <p class="page-sub fade-up-2">We'd love to hear from you</p>
      </div>
    </div>

    <section class="section">
      <div class="container">
        <div class="contact-grid">

          <!-- Left: Contact Info -->
          <div class="contact-info fade-up">
            <!-- Brand social -->
            <div class="info-block">
              <h3>Artistic Sisters</h3>
              <p class="brand-tagline">Crafting memories, one stroke at a time.</p>
              <div class="contact-links">
                <a href="https://www.instagram.com/artistic_sisters._/" target="_blank" rel="noopener" class="contact-link">
                  <span class="link-icon">
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
                      <rect x="2" y="2" width="20" height="20" rx="5" ry="5"/><path d="M16 11.37A4 4 0 1 1 12.63 8 4 4 0 0 1 16 11.37z"/><line x1="17.5" y1="6.5" x2="17.51" y2="6.5"/>
                    </svg>
                  </span>
                  <span>&#64;artistic_sisters._</span>
                </a>
                <a href="mailto:artistic.sisters07@gmail.com" class="contact-link">
                  <span class="link-icon">
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
                      <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/>
                    </svg>
                  </span>
                  <span>artistic.sisters07&#64;gmail.com</span>
                </a>
              </div>
            </div>

            <div class="divider-h"></div>

            <!-- Artists -->
            <div class="artists-section">
              <h3>Meet the Artists</h3>
              <div class="artists-list">
                @for (artist of artists; track artist.name) {
                  <div class="artist-card">
                    <div class="artist-avatar">{{ artist.name.charAt(0) }}</div>
                    <div class="artist-info">
                      <h4>{{ artist.name }}</h4>
                      <div class="artist-contacts">
                        <a href="tel:{{ artist.phone }}" class="artist-link">
                          <span>📱</span> {{ artist.phone }}
                        </a>
                        <a [href]="artist.insta" target="_blank" rel="noopener" class="artist-link">
                          <span>📸</span> Instagram
                        </a>
                        <a [href]="'mailto:' + artist.email" class="artist-link">
                          <span>✉️</span> {{ artist.email }}
                        </a>
                        @if (artist.linkedin) {
                          <a [href]="artist.linkedin" target="_blank" rel="noopener" class="artist-link">
                            <span>💼</span> LinkedIn
                          </a>
                        }
                      </div>
                    </div>
                  </div>
                }
              </div>
            </div>
          </div>

          <!-- Right: Message Form -->
          <div class="contact-form-wrap fade-up-2">
            <div class="card contact-form-card">
              <h3>Send us a message</h3>
              <p class="form-intro">Have a question or want to commission something special? Fill in the form below and we'll get back to you soon.</p>

              @if (sent) {
                <div class="alert alert-success">
                  ✅ Message sent! We'll be in touch soon.
                </div>
              }

              <form (submit)="send($event)">
                <div class="form-row">
                  <div class="form-group">
                    <label>Your Name</label>
                    <input class="form-control" type="text" [(ngModel)]="form.name" placeholder="Your name" required name="name" />
                  </div>
                  <div class="form-group">
                    <label>Email Address</label>
                    <input class="form-control" type="email" [(ngModel)]="form.email" placeholder="you@example.com" required name="email" />
                  </div>
                </div>
                <div class="form-group">
                  <label>Subject</label>
                  <input class="form-control" type="text" [(ngModel)]="form.subject" placeholder="Commission enquiry / General question" name="subject" />
                </div>
                <div class="form-group">
                  <label>Message</label>
                  <textarea class="form-control" [(ngModel)]="form.message" rows="5" placeholder="Tell us about your idea..." required name="message"></textarea>
                </div>
                @if (sendError) {
                  <div class="alert alert-error" style="margin-bottom:12px;">{{ sendError }}</div>
                }
                <button type="submit" class="btn btn-primary" style="width:100%;justify-content:center" [disabled]="sending">
                  @if (sending) { <span class="btn-spinner"></span> Sending… }
                  @else {
                    Send Message
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="22" y1="2" x2="11" y2="13"/><polygon points="22 2 15 22 11 13 2 9 22 2"/></svg>
                  }
                </button>
              </form>
            </div>
          </div>
        </div>
      </div>
    </section>
  `,
  styles: [`
    .page-header { background:linear-gradient(160deg,#f7ede8,#fcf4f0); padding:120px 24px 64px; }
    .page-sub { color:var(--text-muted); margin-top:8px; }

    .contact-grid { display:grid; grid-template-columns:1fr 1.2fr; gap:48px; align-items:start; }

    .info-block h3 { margin-bottom:6px; }
    .brand-tagline { color:var(--text-muted); font-size:0.9rem; margin-bottom:20px; }
    .contact-links { display:flex; flex-direction:column; gap:12px; }
    .contact-link {
      display:flex; align-items:center; gap:12px;
      color:var(--text-body); font-size:0.93rem; transition:var(--transition);
    }
    .contact-link:hover { color:var(--pink); }
    .link-icon {
      width:36px; height:36px; border-radius:50%;
      background:var(--cream-dark); border:1px solid var(--border);
      display:flex; align-items:center; justify-content:center;
      flex-shrink:0; transition:var(--transition);
    }
    .contact-link:hover .link-icon { background:var(--pink); color:#fff; border-color:var(--pink); }

    .divider-h { height:1px; background:var(--border); margin:28px 0; }

    .artists-section h3 { margin-bottom:20px; }
    .artists-list { display:flex; flex-direction:column; gap:20px; }
    .artist-card { display:flex; gap:14px; align-items:flex-start; }
    .artist-avatar {
      width:44px; height:44px; border-radius:50%; background:var(--pink);
      color:#fff; display:flex; align-items:center; justify-content:center;
      font-family:var(--serif); font-size:1.1rem; font-weight:600; flex-shrink:0;
    }
    .artist-info h4 { margin-bottom:8px; font-size:0.97rem; }
    .artist-contacts { display:flex; flex-direction:column; gap:5px; }
    .artist-link {
      display:flex; align-items:center; gap:6px;
      font-size:0.83rem; color:var(--text-muted); transition:var(--transition);
    }
    .artist-link:hover { color:var(--pink); }

    .contact-form-card { padding:36px; }
    .contact-form-card h3 { margin-bottom:6px; }
    .form-intro { color:var(--text-muted); font-size:0.9rem; margin-bottom:24px; line-height:1.6; }
    .form-row { display:grid; grid-template-columns:1fr 1fr; gap:16px; }

    @media(max-width:900px){ .contact-grid{grid-template-columns:1fr;} }
    @media(max-width:480px){ .form-row{grid-template-columns:1fr;} }
    .btn-spinner { width:14px; height:14px; border:2px solid rgba(255,255,255,0.4); border-top-color:#fff; border-radius:50%; animation:spin .7s linear infinite; display:inline-block; margin-right:6px; vertical-align:middle; }
    @keyframes spin { to { transform:rotate(360deg); } }
  `]
})
export class ContactComponent {
  sent      = false;
  sending   = false;
  sendError = '';
  form = { name:'', email:'', subject:'', message:'' };

  private readonly NOTIFY_URL = 'http://localhost:5000/api/notifications/contact';

  artists = [
    {
      name:'Sanjana Velma', phone:'8897532632',
      insta:'https://www.instagram.com/_sanjana_velma_/',
      email:'sanjanavelma27@gmail.com',
      linkedin:'https://www.linkedin.com/in/-sanjanareddy-velma'
    },
    {
      name:'Sathvika Velma', phone:'9652832632',
      insta:'https://www.instagram.com/sathvika_velma/',
      email:'velumasathvika@gmail.com',
      linkedin: null
    }
  ];

  constructor(private http: HttpClient) {}

  send(e: Event) {
    e.preventDefault();
    if (!this.form.name || !this.form.email || !this.form.message) {
      this.sendError = 'Please fill in your name, email and message.';
      return;
    }
    this.sending   = true;
    this.sendError = '';

    this.http.post<{ success: boolean; message: string }>(this.NOTIFY_URL, this.form)
      .subscribe({
        next: () => {
          this.sending = false;
          this.sent    = true;
          this.form    = { name:'', email:'', subject:'', message:'' };
          setTimeout(() => this.sent = false, 6000);
        },
        error: () => {
          this.sending   = false;
          this.sendError = 'Could not send the message. Please try again or email us directly.';
        }
      });
  }
}
