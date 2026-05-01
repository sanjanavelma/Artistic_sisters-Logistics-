import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ArtworkService } from '../../core/services/artwork.service';
import { AddArtworkRequest, Artwork } from '../../core/models/models';

@Component({
  selector: 'app-artist',
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <div class="container text-center">
        <h1 class="fade-up">Artist Dashboard</h1>
        <div class="divider"></div>
        <p class="page-sub fade-up-2">Manage your portfolio and upload new works</p>
      </div>
    </div>

    <section class="section artist-dashboard">
      <div class="container">

        <!-- Tabs -->
        <div class="tabs fade-up">
          <button class="tab-btn" [class.active]="activeTab === 'upload'" (click)="activeTab = 'upload'">Upload Artwork</button>
          <button class="tab-btn" [class.active]="activeTab === 'manage'" (click)="activeTab = 'manage'">Manage Portfolio</button>
        </div>

        <!-- Upload Form Tab -->
        @if (activeTab === 'upload') {
          <div class="card dashboard-card fade-up">
            <h3>Add New Artwork</h3>
            <p class="text-muted" style="margin-bottom:24px">Fill in the details below to publish a new piece to your gallery.</p>

            @if (successMsg) { <div class="alert alert-success">{{ successMsg }}</div> }
            @if (errMsg)     { <div class="alert alert-error">{{ errMsg }}</div> }

            <form (ngSubmit)="submitArtwork()">
              <div class="form-row">
                <div class="form-group">
                  <label>Title</label>
                  <input class="form-control" type="text" [(ngModel)]="form.title" name="title" required placeholder="E.g. Sunrise in the Valley" />
                </div>
                <div class="form-group">
                  <label>Price (₹)</label>
                  <input class="form-control" type="number" [(ngModel)]="form.price" name="price" required min="0" />
                </div>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label>Artwork Type</label>
                  <select class="form-control" [(ngModel)]="form.artworkType" name="type" required>
                    <option value="" disabled>Select Type</option>
                    <option value="Portrait">Portrait</option>
                    <option value="Landscape">Landscape</option>
                    <option value="Abstract">Abstract</option>
                    <option value="CustomCommission">Custom Commission</option>
                  </select>
                </div>
                <div class="form-group">
                  <label>Medium</label>
                  <select class="form-control" [(ngModel)]="form.medium" name="medium" required>
                    <option value="" disabled>Select Medium</option>
                    <option value="Pencil">Pencil / Charcoal</option>
                    <option value="Watercolor">Watercolor</option>
                    <option value="Oil">Oil / Acrylic</option>
                    <option value="Digital">Digital</option>
                  </select>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label>Dimensions</label>
                  <input class="form-control" type="text" [(ngModel)]="form.dimensions" name="dimensions" placeholder="E.g. 24x36 inches" />
                </div>
                <div class="form-group">
                  <label>Artwork Code (Optional SKU)</label>
                  <input class="form-control" type="text" [(ngModel)]="form.artworkCode" name="code" placeholder="E.g. ART-001" />
                </div>
              </div>

              <div class="form-group">
                <label>Image URL</label>
                <input class="form-control" type="url" [(ngModel)]="form.imageUrl" name="image" placeholder="https://i.postimg.cc/..." required />
              </div>

              <div class="form-group">
                <label>Description</label>
                <textarea class="form-control" [(ngModel)]="form.description" name="desc" rows="4" required placeholder="Tell the story behind this piece..."></textarea>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label>Available Quantity</label>
                  <input class="form-control" type="number" [(ngModel)]="form.availableQuantity" name="qty" required min="1" />
                </div>
                <div class="form-group">
                  <label>Estimated Completion Days</label>
                  <input class="form-control" type="number" [(ngModel)]="form.estimatedCompletionDays" name="days" required min="0" />
                </div>
              </div>

              <div class="form-checkbox" style="margin-bottom: 24px;">
                <label>
                  <input type="checkbox" [(ngModel)]="form.isCustomizable" name="customizable" />
                  Is this artwork customizable for buyers?
                </label>
              </div>

              <button type="submit" class="btn btn-primary" [disabled]="uploading">
                @if (uploading) { <span class="btn-spinner"></span> } @else { Publish Artwork }
              </button>
            </form>
          </div>
        }

        <!-- Manage Portfolio Tab -->
        @if (activeTab === 'manage') {

          <!-- Toast notification -->
          @if (toastMsg) {
            <div class="toast" [class.toast-error]="toastIsError">{{ toastMsg }}</div>
          }

          <div class="dashboard-gallery fade-up">
            @if (loadingArt) {
              <div class="text-center" style="padding:40px;"><div class="spinner"></div></div>
            } @else if (myArtworks.length === 0) {
              <div class="empty-state text-center card" style="padding:60px;">
                <div style="font-size:3rem; margin-bottom:16px;">🖼️</div>
                <h3>Your portfolio is empty</h3>
                <p class="text-muted">Start by uploading your first artwork.</p>
                <button class="btn btn-outline" style="margin-top:16px" (click)="activeTab = 'upload'">Go to Upload</button>
              </div>
            } @else {
              <div class="portfolio-grid">
                @for (art of myArtworks; track art.id) {
                  <div class="card art-card" [class.card-coming-soon]="art.isComingSoon">

                    <!-- Image + status badges -->
                    <div class="art-img-wrap">
                      <img [src]="art.imageUrl || 'https://via.placeholder.com/400x300?text=No+Image'" [alt]="art.title" loading="lazy" />
                      @if (art.isComingSoon) {
                        <div class="status-pill pill-amber">
                          <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
                          Coming Soon
                        </div>
                      } @else if (!art.isAvailable) {
                        <div class="status-pill pill-dark">Sold Out</div>
                      } @else {
                        <div class="status-pill pill-green">● Live</div>
                      }
                    </div>

                    <!-- Card body -->
                    <div class="art-body">
                      <div class="art-header-row">
                        <h4 class="art-title">{{ art.title }}</h4>
                        <span class="badge">{{ art.artworkType }}</span>
                      </div>
                      <p class="art-meta-text">{{ art.medium }} · {{ art.dimensions }}</p>
                      <p class="art-meta-text">Stock: {{ art.availableQuantity }} | {{ art.isCustomizable ? 'Customizable' : 'Fixed' }}</p>
                      <div class="art-price-row">
                        <span class="art-price">₹{{ art.price | number }}</span>
                        <span class="art-code">{{ art.artworkCode }}</span>
                      </div>

                      <!-- Action buttons -->
                      <div class="action-row">
                        <!-- Coming Soon toggle -->
                        <button
                          class="action-btn"
                          [class.btn-amber]="!art.isComingSoon"
                          [class.btn-green]="art.isComingSoon"
                          (click)="toggleCs(art)"
                          [disabled]="loadingIds.has(art.id)">
                          @if (loadingIds.has(art.id)) {
                            <span class="btn-spinner sm"></span>
                          } @else if (art.isComingSoon) {
                            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="20 6 9 17 4 12"/></svg>
                            Mark Available
                          } @else {
                            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
                            Coming Soon
                          }
                        </button>

                        <!-- Remove button -->
                        <button class="action-btn btn-red" (click)="openDeleteModal(art)" [disabled]="loadingIds.has(art.id)">
                          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14H6L5 6"/><path d="M10 11v6M14 11v6"/><path d="M9 6V4h6v2"/></svg>
                          Remove
                        </button>
                      </div>
                    </div>

                  </div>
                }
              </div>
            }
          </div>
        }

      </div>
    </section>

    <!-- Remove Confirmation Modal -->
    @if (confirmArt) {
      <div class="modal-backdrop" (click)="confirmArt = null">
        <div class="modal-card" (click)="$event.stopPropagation()">
          <div class="modal-icon">🗑️</div>
          <h3>Remove Artwork?</h3>
          <p>Are you sure you want to remove <strong>"{{ confirmArt.title }}"</strong> from the gallery?
             The artwork will be hidden from buyers but can be restored by an admin.</p>
          <div class="modal-actions">
            <button class="btn btn-outline" (click)="confirmArt = null">Cancel</button>
            <button class="btn btn-danger" (click)="confirmDelete()" [disabled]="deleteLoading">
              @if (deleteLoading) { Removing... } @else { Yes, Remove }
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .page-header { background: linear-gradient(160deg, #f7ede8, #fcf4f0); padding: 120px 24px 64px; }
    .page-sub { color: var(--text-muted); margin-top: 8px; font-size: 1.05rem; }

    .tabs { display:flex; gap:16px; margin-bottom: 32px; justify-content:center; }
    .tab-btn {
      background: none; border: none; padding: 10px 24px; font-size: 1rem;
      font-weight: 500; color: var(--text-muted); cursor: pointer;
      border-bottom: 2px solid transparent; transition: var(--transition);
    }
    .tab-btn:hover { color: var(--text-body); }
    .tab-btn.active { color: var(--pink); border-bottom-color: var(--pink); }

    /* Upload form */
    .dashboard-card { max-width: 800px; margin: 0 auto; padding: 40px; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
    .form-checkbox label { display: flex; align-items: center; gap: 10px; cursor: pointer; font-size: 0.95rem; color: var(--text-body); }
    .form-checkbox input { width: 18px; height: 18px; accent-color: var(--pink); }
    .btn-spinner { width:16px; height:16px; border:2px solid rgba(255,255,255,0.4); border-top-color:#fff; border-radius:50%; animation:spin .7s linear infinite; display:inline-block; }
    .btn-spinner.sm { width:12px; height:12px; }

    /* Toast */
    .toast {
      position: fixed; top: 80px; right: 24px; z-index: 9999;
      background: #2d6a4f; color: #fff;
      padding: 12px 20px; border-radius: 8px;
      font-size: 0.9rem; font-weight: 500;
      box-shadow: 0 4px 20px rgba(0,0,0,0.15);
      animation: slideIn 0.3s ease;
    }
    .toast.toast-error { background: #c62828; }
    @keyframes slideIn {
      from { opacity: 0; transform: translateX(20px); }
      to   { opacity: 1; transform: translateX(0); }
    }

    /* Portfolio grid */
    .portfolio-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 24px; }
    .art-card {
      transition: transform 0.2s ease, box-shadow 0.2s ease;
    }
    .art-card:hover { transform: translateY(-3px); box-shadow: 0 8px 28px rgba(0,0,0,0.10); }
    .card-coming-soon { border: 2px dashed #f0a500 !important; }

    /* Image wrap */
    .art-img-wrap { position: relative; aspect-ratio: 4/3; overflow: hidden; background: #eee; }
    .art-img-wrap img { width: 100%; height: 100%; object-fit: cover; transition: transform 0.4s ease; }
    .art-card:hover .art-img-wrap img { transform: scale(1.04); }

    /* Status pills */
    .status-pill {
      position: absolute; top: 10px; left: 10px;
      display: inline-flex; align-items: center; gap: 5px;
      font-size: 0.7rem; font-weight: 700;
      padding: 3px 10px; border-radius: 99px;
    }
    .pill-amber { background: #f0a500; color: #fff; }
    .pill-dark  { background: #3d3230; color: #fff; }
    .pill-green { background: #2d6a4f; color: #fff; }

    /* Card body */
    .art-body { padding: 16px; }
    .art-header-row { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 6px; gap: 8px; }
    .art-title { font-size: 1rem; margin: 0; flex: 1; }
    .art-meta-text { font-size: 0.8rem; color: var(--text-muted); margin: 2px 0; }
    .art-price-row { display: flex; justify-content: space-between; align-items: center; margin: 10px 0 14px; }
    .art-price { font-weight: 700; font-size: 1rem; color: var(--pink); }
    .art-code { font-size: 0.75rem; color: var(--text-muted); }

    /* Action row */
    .action-row { display: flex; gap: 8px; }
    .action-btn {
      flex: 1; display: inline-flex; align-items: center; justify-content: center; gap: 5px;
      padding: 8px 10px; border: none; border-radius: 8px;
      font-size: 0.78rem; font-weight: 600; cursor: pointer;
      transition: filter 0.18s ease, transform 0.15s ease;
    }
    .action-btn:hover:not(:disabled) { filter: brightness(1.12); transform: translateY(-1px); }
    .action-btn:disabled { opacity: 0.55; cursor: not-allowed; }
    .btn-amber { background: #fff3cd; color: #856404; border: 1px solid #f0c040; }
    .btn-green { background: #d1fae5; color: #065f46; border: 1px solid #6ee7b7; }
    .btn-red   { background: #fee2e2; color: #991b1b; border: 1px solid #fca5a5; flex: 0 0 auto; }

    /* Modal */
    .modal-backdrop {
      position: fixed; inset: 0; background: rgba(0,0,0,0.45);
      display: flex; align-items: center; justify-content: center;
      z-index: 8888; padding: 20px;
    }
    .modal-card {
      background: #fff; border-radius: 16px;
      padding: 36px 32px; max-width: 420px; width: 100%;
      text-align: center; box-shadow: 0 20px 60px rgba(0,0,0,0.2);
      animation: popIn 0.25s ease;
    }
    @keyframes popIn {
      from { opacity: 0; transform: scale(0.9); }
      to   { opacity: 1; transform: scale(1); }
    }
    .modal-icon { font-size: 3rem; margin-bottom: 12px; }
    .modal-card h3 { margin-bottom: 10px; }
    .modal-card p  { color: var(--text-muted); font-size: 0.95rem; line-height: 1.6; margin-bottom: 28px; }
    .modal-actions { display: flex; gap: 12px; justify-content: center; }
    .btn-danger {
      background: #dc2626; color: #fff; border: none;
      border-radius: var(--radius); padding: 10px 22px;
      font-size: 0.9rem; font-weight: 600; cursor: pointer; transition: background 0.2s;
    }
    .btn-danger:hover:not(:disabled) { background: #b91c1c; }
    .btn-danger:disabled { opacity: 0.6; cursor: not-allowed; }

    @media (max-width: 768px) {
      .form-row { grid-template-columns: 1fr; gap:0; }
      .portfolio-grid { grid-template-columns: 1fr 1fr; }
      .dashboard-card { padding: 24px; }
    }
    @media (max-width: 500px) { .portfolio-grid { grid-template-columns: 1fr; } }
  `]
})
export class ArtistComponent implements OnInit {
  activeTab: 'upload' | 'manage' = 'upload';

  form: AddArtworkRequest = {
    title: '', description: '', price: 0,
    artworkType: '', medium: '', dimensions: '',
    artworkCode: '', imageUrl: '', availableQuantity: 1,
    isCustomizable: false, estimatedCompletionDays: 0
  };

  uploading  = false;
  successMsg = '';
  errMsg     = '';

  myArtworks: Artwork[] = [];
  loadingArt = false;

  confirmArt: Artwork | null = null;
  deleteLoading = false;
  loadingIds    = new Set<string>(); // tracks per-card loading state

  toastMsg     = '';
  toastIsError = false;

  constructor(private artworkService: ArtworkService) {}

  ngOnInit() {
    this.loadMyArtworks();
  }

  submitArtwork() {
    this.uploading = true;
    this.successMsg = '';
    this.errMsg = '';

    this.artworkService.add(this.form).subscribe({
      next: () => {
        this.uploading = false;
        this.successMsg = 'Artwork published successfully!';
        this.form = {
          title: '', description: '', price: 0,
          artworkType: '', medium: '', dimensions: '',
          artworkCode: '', imageUrl: '', availableQuantity: 1,
          isCustomizable: false, estimatedCompletionDays: 0
        };
        this.loadMyArtworks();
        setTimeout(() => this.activeTab = 'manage', 1500);
      },
      error: (err) => {
        this.uploading = false;
        this.errMsg = err?.error?.message || 'Failed to publish artwork. Server might be offline.';
        console.error(err);
      }
    });
  }

  loadMyArtworks() {
    this.loadingArt = true;
    this.artworkService.getAll().subscribe({
      next: (arts) => { this.myArtworks = arts; this.loadingArt = false; },
      error: (err) => { this.loadingArt = false; this.showToast(err?.error?.message || 'Failed to load artworks', true); }
    });
  }

  // ── Coming Soon toggle ────────────────────────────────────────────
  toggleCs(art: Artwork) {
    this.loadingIds.add(art.id);
    const next = !art.isComingSoon;

    this.artworkService.toggleComingSoon(art.id, next).subscribe({
      next: (res: any) => {
        art.isComingSoon = next;
        art.isAvailable  = art.availableQuantity > 0 && !next;
        this.loadingIds.delete(art.id);
        this.showToast(res.message || res.Message || (next ? 'Marked as Coming Soon' : 'Artwork is now live'));
      },
      error: (err) => {
        this.loadingIds.delete(art.id);
        this.showToast(err?.error?.message || err?.error?.Message || 'Could not update status — is the server running?', true);
      }
    });
  }

  // ── Delete artwork ────────────────────────────────────────────────
  openDeleteModal(art: Artwork) {
    this.confirmArt = art;
  }

  confirmDelete() {
    if (!this.confirmArt) return;
    this.deleteLoading = true;

    this.artworkService.deleteArtwork(this.confirmArt.id).subscribe({
      next: (res: any) => {
        this.showToast(res.message || res.Message || 'Artwork removed from the gallery');
        this.myArtworks = this.myArtworks.filter(a => a.id !== this.confirmArt!.id);
        this.confirmArt  = null;
        this.deleteLoading = false;
      },
      error: (err) => {
        this.showToast(err?.error?.message || err?.error?.Message || 'Could not remove artwork — is the server running?', true);
        this.confirmArt  = null;
        this.deleteLoading = false;
      }
    });
  }

  // ── Toast helper ──────────────────────────────────────────────────
  private showToast(msg: string, isError = false) {
    this.toastMsg    = msg;
    this.toastIsError = isError;
    setTimeout(() => { this.toastMsg = ''; }, 3500);
  }
}
