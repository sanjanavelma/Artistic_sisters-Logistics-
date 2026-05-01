import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  imports: [RouterLink],
  template: `
    <!-- Hero -->
    <section class="hero">
      <div class="hero-bg"></div>
      <div class="hero-overlay"></div>
      <div class="hero-content fade-up">
        <h1>Welcome to<br>Artistic Sisters</h1>
        <p class="hero-sub">Bringing your cherished moments and visions to life through traditional and realistic art.</p>
        <div class="hero-actions">
          <a href="http://localhost:5050" target="_blank" class="btn btn-solid-pink hero-btn" style="display: inline-flex; align-items: center;">
            🪄 Try AI Art Lab
          </a>
          <a routerLink="/contact" class="btn btn-white-pink hero-btn">Get in Touch</a>
        </div>
      </div>
    </section>

    <!-- About snippet -->
    <section class="section about-snippet">
      <div class="container text-center">
        <h2 class="section-title fade-up">Art from the Heart</h2>
        <div class="divider"></div>
        <p class="about-text fade-up-2">
          Artistic_Sisters is a creative venture born from a shared passion for the timeless beauty of traditional and
          realistic art. We specialize in creating bespoke artworks, from intricate portraits that capture the essence
          of a soul, to vibrant paintings that tell a story. Each piece is crafted with love, dedication, and a keen
          eye for detail.
        </p>
        <a routerLink="/about" class="learn-more fade-up-3">
          Learn More About Us
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M5 12h14M12 5l7 7-7 7"/></svg>
        </a>
      </div>
    </section>

    <!-- Featured Works -->
    <section class="section featured-bg">
      <div class="container">
        <h2 class="section-title text-center fade-up">Featured Works</h2>
        <div class="divider"></div>
        <div class="featured-grid">
          <div class="card featured-card fade-up">
            <div class="featured-img-wrap">
              <img src="https://i.postimg.cc/P5gN6Dgz/mithal-portrait.jpg" alt="Portrait of Dr. Ashok Mittal" loading="lazy" />
            </div>
            <div class="featured-body">
              <h3>Portrait of Dr. Ashok Mittal</h3>
              <p>Portrait of Dr. Ashok Mittal, founder and chancellor of Lovely Professional University and Member of Rajya Sabha.</p>
            </div>
          </div>

          <div class="card featured-card fade-up-2">
            <div class="featured-img-wrap">
              <img src="https://i.postimg.cc/GtTH1B7d/artgallery.jpg" alt="ELYSIUM 2.0 Art Exhibition" loading="lazy" />
            </div>
            <div class="featured-body">
              <h3>ELYSIUM 2.0 Art Exhibition</h3>
              <p>Our art stall at ELYSIUM 2.0, St. Mary's College of Pharmacy, Hyderabad — filled with creativity and great vibes!</p>
            </div>
          </div>
        </div>

        <div class="text-center" style="margin-top:40px">
          <a routerLink="/portfolio" class="btn btn-outline">
            Explore Full Portfolio
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M5 12h14M12 5l7 7-7 7"/></svg>
          </a>
        </div>
      </div>
    </section>

    <!-- CTA Banner -->
    <section class="cta-banner">
      <div class="container text-center">
        <h2>Ready to commission your own artwork?</h2>
        <p>Tell us your vision — we'll bring it to life with love and artistry.</p>
        <div class="cta-actions">
          <a routerLink="/commission" class="btn btn-primary">Request a Commission</a>
          <a routerLink="/portfolio"  class="btn btn-ghost">Browse Gallery</a>
        </div>
      </div>
    </section>
  `,
  styles: [`
    /* Hero */
    /* Hero */
    .hero {
      position: relative; 
      display: flex; flex-direction: column; align-items: center; justify-content: center;
      text-align: center; overflow: hidden;
      margin: 90px auto 40px; /* Flush top, centered with margin auto for side borders */
      padding: 90px 20px 100px; /* Reduced internal padding above text and below buttons */
      max-width: 1280px; 
      width: calc(100% - 80px); /* Brings back the white side borders! */
      border-radius: 4px;
    }
    .hero-bg {
      position: absolute; inset: -40px; /* inset to hide blur safely */
      background: url('https://i.postimg.cc/7GjHfNKw/shop1.jpg') no-repeat;
      background-size: 120%; /* Scaled down from 155% but still slightly larger than cover */
      background-position: center 30%;
      filter: blur(12px);
      z-index: 0;
    }
    .hero-overlay {
      position: absolute; inset: 0;
      background: rgba(245, 215, 225, 0.48); /* Pink tint fade */
      z-index: 1;
    }
    .hero-content {
      position: relative; max-width: 800px; padding: 0 24px; z-index: 2;
    }
    .hero-content h1 {
      font-family: 'Times New Roman', Times, serif;
      font-size: 6rem; /* 125% zoomed look */
      color: #3b3531;
      margin-bottom: 24px; line-height: 1.05;
      font-weight: 700;
      text-shadow: 0 2px 14px rgba(255,255,255,0.9);
      letter-spacing: 1px;
    }
    .hero-sub {
      font-size: 1.5rem; color: #4a4542;
      max-width: 700px; margin: 0 auto 40px; line-height: 1.6;
      font-weight: 500;
    }
    .hero-actions { display: flex; gap: 24px; justify-content: center; flex-wrap: wrap; }
    
    .btn-solid-pink {
      background-color: #eea4cc;
      color: #111;
      border: none;
      border-radius: 6px;
      font-weight: 500;
      box-shadow: 0 4px 15px rgba(238, 164, 204, 0.4);
      transition: background-color 0.2s ease, transform 0.2s ease;
    }
    .btn-solid-pink:hover {
      background-color: #e595c2;
      color: #000;
      transform: translateY(-2px);
    }
    
    .btn-white-pink {
      background-color: rgba(255, 255, 255, 0.85);
      color: #eea4cc;
      border: none;
      border-radius: 6px;
      font-weight: 500;
      box-shadow: 0 4px 15px rgba(0, 0, 0, 0.05);
      transition: background-color 0.2s ease, transform 0.2s ease;
    }
    .btn-white-pink:hover {
      background-color: #fff;
      transform: translateY(-2px);
    }
    
    .hero-btn { padding: 18px 46px !important; font-size: 1.25rem !important; }

    /* About snippet */
    .about-snippet { background: var(--white); }
    .about-text {
      max-width: 680px; margin: 0 auto 28px;
      font-size: 1.05rem; line-height: 1.8; color: var(--text-body);
    }
    .learn-more {
      display: inline-flex; align-items: center; gap: 6px;
      color: var(--pink); font-weight: 500; font-size: 0.95rem;
      transition: var(--transition);
    }
    .learn-more:hover { gap: 10px; }

    /* Featured */
    .featured-bg { background: var(--cream); }
    .featured-grid {
      display: grid; grid-template-columns: 1fr 1fr;
      gap: 28px; margin-top: 12px;
    }
    .featured-card { cursor: default; }
    .featured-img-wrap {
      aspect-ratio: 4/3; overflow: hidden;
    }
    .featured-img-wrap img {
      width: 100%; height: 100%; object-fit: cover;
      transition: transform 0.5s ease;
    }
    .featured-card:hover .featured-img-wrap img { transform: scale(1.04); }
    .featured-body { padding: 20px; }
    .featured-body h3 { margin-bottom: 8px; font-size: 1.1rem; }
    .featured-body p  { font-size: 0.9rem; color: var(--text-muted); line-height: 1.6; }

    /* CTA */
    .cta-banner {
      background: linear-gradient(135deg, #f9eff2 0%, #f4ecf8 100%);
      padding: 72px 24px;
      border-top: 1px solid var(--border);
    }
    .cta-banner h2 { margin-bottom: 12px; }
    .cta-banner p  { color: var(--text-muted); margin-bottom: 32px; font-size: 1.05rem; }
    .cta-actions   { display: flex; gap: 14px; justify-content: center; flex-wrap: wrap; }

    @media (max-width: 640px) {
      .featured-grid { grid-template-columns: 1fr; }
      .hero { min-height: 80vh; }
    }
  `]
})
export class HomeComponent {}
