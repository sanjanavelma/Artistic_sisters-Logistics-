import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ArtworkService } from '../../core/services/artwork.service';
import { Artwork } from '../../core/models/models';

@Component({
  selector: 'app-portfolio',
  imports: [CommonModule, RouterLink],
  template: `
    <!-- Header -->
    <div class="page-header">
      <div class="container text-center">
        <h1 class="fade-up">Our Portfolio</h1>
        <div class="divider"></div>
        <p class="page-sub fade-up-2">A collection of our passion projects and commissioned works.</p>
      </div>
    </div>

    <section class="section">
      <div class="container">
        <!-- Filters -->
        <div class="filters fade-up">
          <span class="filter-label">Filter by:</span>
          <div class="chip-group">
            <button class="chip" [class.active]="activeType === ''" (click)="setType('')">All</button>
            <button class="chip" [class.active]="activeType === 'Portrait'" (click)="setType('Portrait')">Portraits</button>
            <button class="chip" [class.active]="activeType === 'Landscape'" (click)="setType('Landscape')">Landscape</button>
            <button class="chip" [class.active]="activeType === 'Abstract'" (click)="setType('Abstract')">Abstract</button>
            <button class="chip" [class.active]="activeType === 'CustomCommission'" (click)="setType('CustomCommission')">Commissions</button>
          </div>
          <div class="chip-group" style="margin-left:auto">
            <button class="chip" [class.active]="activeMedium === ''" (click)="setMedium('')">All Media</button>
            <button class="chip" [class.active]="activeMedium === 'Pencil'" (click)="setMedium('Pencil')">Pencil</button>
            <button class="chip" [class.active]="activeMedium === 'Watercolor'" (click)="setMedium('Watercolor')">Watercolor</button>
            <button class="chip" [class.active]="activeMedium === 'Oil'" (click)="setMedium('Oil')">Oil</button>
            <button class="chip" [class.active]="activeMedium === 'Digital'" (click)="setMedium('Digital')">Digital</button>
          </div>
        </div>

        <!-- Loading -->
        @if (loading) {
          <div class="text-center" style="padding:60px 0">
            <div class="spinner"></div>
            <p style="color:var(--text-muted)">Loading artworks...</p>
          </div>
        }

        <!-- Error -->
        @if (error) {
          <div class="empty-state">
            <div class="empty-icon">⚠️</div>
            <h3>Couldn't load artworks</h3>
            <p>{{ error }}</p>
            <button class="btn btn-outline" (click)="load()">Try again</button>
          </div>
        }

        <!-- Empty -->
        @if (!loading && !error && artworks.length === 0) {
          <div class="empty-state">
            <div class="empty-icon">🎨</div>
            <h3>Gallery Coming Soon</h3>
            <p>Our artworks will appear here once the artist adds them. Check back soon!</p>
          </div>
        }

        <!-- Grid -->
        @if (!loading && !error && artworks.length > 0) {
          <div class="portfolio-grid">
            @for (art of artworks; track art.id) {
              <div class="card art-card" [class.card-coming-soon]="art.isComingSoon">
                <div class="art-img-wrap">
                  <img [src]="art.imageUrl || 'https://via.placeholder.com/400x300?text=No+Image'"
                       [alt]="art.title" loading="lazy" />
                  @if (art.isComingSoon) {
                    <div class="status-badge coming-soon-badge">
                      <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
                      Coming Soon
                    </div>
                  } @else if (!art.isAvailable) {
                    <div class="status-badge sold-badge">Sold Out</div>
                  }
                  @if (art.isCustomizable) {
                    <div class="custom-badge">Customizable</div>
                  }
                </div>
                <div class="art-body">
                  <span class="badge">{{ art.artworkType }}</span>
                  <h4 class="art-title">{{ art.title }}</h4>
                  <p class="art-desc">{{ art.description }}</p>
                  <div class="art-meta">
                    <span class="art-medium">{{ art.medium }} · {{ art.dimensions }}</span>
                    <span class="art-price">₹{{ art.price | number }}</span>
                  </div>
                  @if (art.isAvailable && !art.isComingSoon) {
                    <a [routerLink]="['/order']" [queryParams]="{ artId: art.id }" class="btn btn-primary art-btn">Order Now</a>
                  } @else if (art.isComingSoon) {
                    <button class="btn art-btn btn-coming-soon" disabled>
                      <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
                      Coming Soon
                    </button>
                  }
                </div>
              </div>
            }
          </div>
        }
      </div>
    </section>
  `,
  styles: [`
    .page-header {
      background: linear-gradient(160deg, #f7ede8, #fcf4f0);
      padding: 120px 24px 64px;
    }
    .page-sub { color: var(--text-muted); margin-top: 8px; }

    .filters {
      display: flex; align-items: center; gap: 10px;
      flex-wrap: wrap; margin-bottom: 36px;
    }
    .filter-label { font-size: 0.85rem; font-weight: 600; color: var(--text-muted); white-space: nowrap; }
    .chip-group   { display: flex; gap: 8px; flex-wrap: wrap; }

    .portfolio-grid {
      display: grid; grid-template-columns: repeat(3, 1fr);
      gap: 24px;
    }
    .art-card { position: relative; cursor: default; transition: transform 0.2s ease, box-shadow 0.2s ease; }
    .art-card:hover { transform: translateY(-2px); box-shadow: 0 8px 28px rgba(0,0,0,0.09); }
    .card-coming-soon { border: 2px dashed #f0a500 !important; opacity: 0.9; }

    .art-img-wrap {
      position: relative; aspect-ratio: 3/4; overflow: hidden;
    }
    .art-img-wrap img {
      width: 100%; height: 100%; object-fit: cover;
      transition: transform 0.5s ease;
    }
    .art-card:hover .art-img-wrap img { transform: scale(1.04); }

    .status-badge {
      position: absolute; top: 10px; left: 10px;
      display: inline-flex; align-items: center; gap: 5px;
      font-size: 0.72rem; font-weight: 700; padding: 4px 10px;
      border-radius: var(--radius-full);
    }
    .sold-badge        { background: #3d3230; color: #fff; }
    .coming-soon-badge { background: #f0a500; color: #fff; }
    .custom-badge {
      position: absolute; top: 10px; right: 10px;
      font-size: 0.72rem; font-weight: 600; padding: 3px 10px;
      border-radius: var(--radius-full);
      background: var(--pink); color: #fff;
    }

    .art-body { padding: 16px; }
    .art-title { margin: 8px 0 6px; font-size: 1rem; }
    .art-desc  { font-size: 0.83rem; color: var(--text-muted); line-height: 1.5; margin-bottom: 10px;
                 display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
    .art-meta  { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; }
    .art-medium { font-size: 0.78rem; color: var(--text-muted); }
    .art-price  { font-weight: 700; color: var(--text-heading); font-size: 1rem; }
    .art-btn    { width: 100%; justify-content: center; padding: 9px !important; font-size: 0.88rem !important; }
    .btn-coming-soon {
      background: #fff3cd; color: #856404;
      border: 1px solid #f0c040;
      display: flex; align-items: center; justify-content: center; gap: 6px;
      cursor: not-allowed; opacity: 0.85;
    }

    .empty-state { text-align: center; padding: 80px 20px; }
    .empty-icon  { font-size: 4rem; margin-bottom: 16px; }
    .empty-state h3 { margin-bottom: 8px; }
    .empty-state p  { color: var(--text-muted); }

    @media (max-width: 900px) { .portfolio-grid { grid-template-columns: 1fr 1fr; } }
    @media (max-width: 540px) { .portfolio-grid { grid-template-columns: 1fr; } }
  `]
})
export class PortfolioComponent implements OnInit {
  artworks: Artwork[] = [];
  loading      = false;
  error        = '';
  activeType   = '';
  activeMedium = '';

  constructor(private artworkSvc: ArtworkService) {}

  ngOnInit() { this.load(); }

  load() {
    this.loading = true; this.error = '';
    this.artworkSvc.getAll(this.activeType || undefined, this.activeMedium || undefined).subscribe({
      next:  arts => { this.artworks = arts; this.loading = false; },
      error: ()   => { this.error = 'Could not connect to the server. Please make sure the backend is running.'; this.loading = false; }
    });
  }

  setType(t: string)   { this.activeType = t;   this.load(); }
  setMedium(m: string) { this.activeMedium = m; this.load(); }
}
