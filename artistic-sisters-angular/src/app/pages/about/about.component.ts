import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-about',
  imports: [RouterLink],
  template: `
    <!-- Page Header -->
    <div class="page-header">
      <div class="container text-center">
        <h1 class="fade-up">About Us</h1>
        <div class="divider"></div>
        <p class="page-sub fade-up-2">The story behind Artistic Sisters</p>
      </div>
    </div>

    <!-- Our Story -->
    <section class="section">
      <div class="container">
        <div class="story-grid">
          <div class="story-text fade-up">
            <span class="eyebrow">Our Story</span>
            <h2>Two sisters, one passion</h2>
            <div class="divider" style="margin:16px 0 24px;"></div>
            <p>
              Artistic Sisters was born from a shared love of bringing images to life on canvas. Growing up in a home
              filled with colour and creativity, we always found ourselves sketching faces, scenes and memories that
              felt too beautiful to fade away.
            </p>
            <p style="margin-top:16px">
              What started as personal art journals and gifted portraits grew into a full creative venture —
              a place where we transform your most cherished moments into timeless artwork, crafted entirely by hand
              with traditional and realistic techniques.
            </p>
            <p style="margin-top:16px">
              Every piece we create carries a piece of our heart. Whether it's a portrait, a landscape, or a custom
              commission close to your soul — we pour skill, patience and love into every single stroke.
            </p>
            <a routerLink="/portfolio" class="btn btn-outline" style="margin-top:28px">See Our Work</a>
          </div>

          <div class="story-photos fade-up-2">
            <div class="photo-stack">
              <div class="photo-card photo-1">
                <img src="https://i.postimg.cc/4dLJyJ7y/me-in-dehli.jpg" alt="Sanjana Velma" />
                <div class="photo-label">
                  <span class="photo-name">Sanjana Velma</span>
                  <span class="photo-role">Co-founder & Artist</span>
                </div>
              </div>
              <div class="photo-card photo-2">
                <img src="https://i.postimg.cc/8CzBWHX5/sathvika.jpg" alt="Sathvika Velma" />
                <div class="photo-label">
                  <span class="photo-name">Sathvika Velma</span>
                  <span class="photo-role">Co-founder & Artist</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Values -->
    <section class="section values-section">
      <div class="container">
        <h2 class="text-center section-title">What We Stand For</h2>
        <div class="divider"></div>
        <div class="values-grid">
          <div class="value-card fade-up">
            <div class="value-icon">🎨</div>
            <h4>Authenticity</h4>
            <p>Every piece is entirely hand-crafted — no digital shortcuts. Pure skill, pure art.</p>
          </div>
          <div class="value-card fade-up-2">
            <div class="value-icon">💝</div>
            <h4>Personal Touch</h4>
            <p>We work closely with you to capture the exact emotion and story you want preserved.</p>
          </div>
          <div class="value-card fade-up-3">
            <div class="value-icon">⏱️</div>
            <h4>Dedication</h4>
            <p>We never rush. Each artwork gets the time, attention and love it deserves.</p>
          </div>
          <div class="value-card fade-up">
            <div class="value-icon">✨</div>
            <h4>Excellence</h4>
            <p>We won't deliver anything we wouldn't be proud to hang in our own home.</p>
          </div>
        </div>
      </div>
    </section>

    <!-- Inside Our Studio -->
    <section class="section studio-section">
      <div class="container">
        <div class="studio-inner">
          <div class="studio-text fade-up">
            <span class="eyebrow">Inside Our Studio</span>
            <h2>Where the magic happens</h2>
            <div class="divider" style="margin:16px 0 24px;"></div>
            <p>
              Our studio is our sanctuary — a place where creativity flourishes, ideas take shape, and paint meets canvas.
              It's filled with inspiration, from reference books and colour swatches to the gentle hum of focused artistry.
            </p>
            <p style="margin-top:16px">
              We invite you to glimpse into our world where imagination comes to life. Every corner tells a story,
              and every brush stroke is a conversation between artist and canvas.
            </p>
          </div>
          <div class="studio-img-wrap fade-up-2">
            <img src="https://i.postimg.cc/WbHWPd4H/gallery.jpg" alt="Inside our studio" />
          </div>
        </div>
      </div>
    </section>
  `,
  styles: [`
    .page-header {
      background: linear-gradient(160deg, #f7ede8, #fcf4f0);
      padding: 120px 24px 64px;
    }
    .page-sub { color: var(--text-muted); font-size: 1.05rem; margin-top: 8px; }
    .eyebrow {
      display: block; font-size: 0.78rem; font-weight: 600;
      text-transform: uppercase; letter-spacing: 0.12em;
      color: var(--pink); margin-bottom: 12px;
    }

    /* Story */
    .story-grid {
      display: grid; grid-template-columns: 1fr 1fr;
      gap: 64px; align-items: center;
    }
    .story-text h2 { margin-bottom: 4px; }
    .story-text p { color: var(--text-body); line-height: 1.8; }

    .photo-stack {
      display: grid; grid-template-columns: 1fr 1fr; gap: 16px;
    }
    .photo-card {
      border-radius: var(--radius-md); overflow: hidden;
      box-shadow: var(--shadow-md); position: relative;
    }
    .photo-card img { width: 100%; aspect-ratio: 3/4; object-fit: cover; display: block; }
    .photo-2 { margin-top: 32px; }
    .photo-label {
      position: absolute; bottom: 0; left: 0; right: 0;
      background: linear-gradient(transparent, rgba(40,20,20,0.75));
      padding: 24px 14px 14px; color: #fff;
    }
    .photo-name { display: block; font-family: var(--serif); font-size: 0.95rem; font-weight: 600; }
    .photo-role { display: block; font-size: 0.75rem; opacity: 0.85; margin-top: 2px; }

    /* Values */
    .values-section { background: var(--cream-dark); }
    .values-grid {
      display: grid; grid-template-columns: repeat(4, 1fr); gap: 20px; margin-top: 8px;
    }
    .value-card {
      background: var(--white); border-radius: var(--radius-md);
      padding: 28px 20px; text-align: center;
      box-shadow: var(--shadow-sm); transition: var(--transition);
    }
    .value-card:hover { box-shadow: var(--shadow-md); transform: translateY(-3px); }
    .value-icon { font-size: 2rem; margin-bottom: 12px; }
    .value-card h4 { margin-bottom: 8px; font-family: var(--serif); }
    .value-card p { font-size: 0.88rem; color: var(--text-muted); line-height: 1.6; }

    /* Studio */
    .studio-section { background: var(--white); }
    .studio-inner {
      display: grid; grid-template-columns: 1fr 1fr;
      gap: 56px; align-items: center;
    }
    .studio-img-wrap {
      border-radius: var(--radius-lg); overflow: hidden;
      box-shadow: var(--shadow-lg);
    }
    .studio-img-wrap img { width: 100%; aspect-ratio: 4/3; object-fit: cover; display: block; }

    @media (max-width: 900px) {
      .story-grid, .studio-inner { grid-template-columns: 1fr; gap: 36px; }
      .values-grid { grid-template-columns: 1fr 1fr; }
    }
    @media (max-width: 480px) {
      .values-grid { grid-template-columns: 1fr; }
      .photo-stack { grid-template-columns: 1fr; }
      .photo-2 { margin-top: 0; }
    }
  `]
})
export class AboutComponent {}
