import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-services',
  imports: [RouterLink],
  template: `
    <div class="page-header">
      <div class="container text-center">
        <h1 class="fade-up">Our Services</h1>
        <div class="divider"></div>
        <p class="page-sub fade-up-2">Handcrafted art experiences tailored to you</p>
      </div>
    </div>

    <section class="section">
      <div class="container">
        <div class="services-grid">
          @for (svc of services; track svc.title) {
            <div class="svc-card card">
              <div class="svc-icon">{{ svc.icon }}</div>
              <h3>{{ svc.title }}</h3>
              <p class="svc-desc">{{ svc.description }}</p>
              <ul class="svc-list">
                @for (item of svc.items; track item) {
                  <li>
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="var(--pink)" stroke-width="2.5"><path d="M20 6L9 17l-5-5"/></svg>
                    {{ item }}
                  </li>
                }
              </ul>
              <a [routerLink]="svc.route" class="btn btn-outline svc-btn">{{ svc.cta }}</a>
            </div>
          }
        </div>
      </div>
    </section>

    <!-- Process Section -->
    <section class="section process-section">
      <div class="container">
        <h2 class="text-center section-title">How It Works</h2>
        <div class="divider"></div>
        <div class="steps-grid">
          @for (step of steps; track step.num) {
            <div class="step-card">
              <div class="step-num">{{ step.num }}</div>
              <h4>{{ step.title }}</h4>
              <p>{{ step.desc }}</p>
            </div>
          }
        </div>
      </div>
    </section>
  `,
  styles: [`
    .page-header { background: linear-gradient(160deg,#f7ede8,#fcf4f0); padding:120px 24px 64px; }
    .page-sub { color:var(--text-muted); margin-top:8px; }

    .services-grid { display:grid; grid-template-columns:1fr 1fr; gap:28px; }
    .svc-card { padding:36px 32px; display:flex; flex-direction:column; gap:12px; }
    .svc-icon { font-size:2.4rem; }
    .svc-card h3 { font-size:1.3rem; margin:0; }
    .svc-desc { color:var(--text-muted); font-size:0.95rem; line-height:1.7; flex:1; }
    .svc-list { display:flex; flex-direction:column; gap:8px; margin:4px 0; }
    .svc-list li { display:flex; align-items:center; gap:8px; font-size:0.9rem; color:var(--text-body); }
    .svc-btn { align-self:flex-start; }

    .process-section { background:var(--cream-dark); }
    .steps-grid { display:grid; grid-template-columns:repeat(4,1fr); gap:20px; margin-top:8px; }
    .step-card { text-align:center; padding:28px 16px; }
    .step-num {
      width:48px; height:48px; border-radius:50%; background:var(--pink);
      color:#fff; font-family:var(--serif); font-size:1.3rem; font-weight:600;
      display:flex; align-items:center; justify-content:center; margin:0 auto 16px;
    }
    .step-card h4 { margin-bottom:8px; }
    .step-card p  { font-size:0.87rem; color:var(--text-muted); line-height:1.6; }

    @media(max-width:900px){ .services-grid{grid-template-columns:1fr;} .steps-grid{grid-template-columns:1fr 1fr;} }
    @media(max-width:540px){ .steps-grid{grid-template-columns:1fr;} }
  `]
})
export class ServicesComponent {
  services = [
    {
      icon: '🖼️', title: 'Ready-Made Artworks',
      description: 'Explore our collection of completed, gallery-ready pieces available for immediate purchase. Each artwork is original, hand-crafted and certified.',
      items: ['Original hand-crafted pieces','Multiple sizes & mediums','Immediate availability','Safe packaging & delivery'],
      route: '/portfolio', cta: 'Browse Gallery'
    },
    {
      icon: '✍️', title: 'Custom Commission',
      description: 'Have a specific vision in mind? We\'ll create a bespoke piece tailored entirely to your requirements — from portraits to landscapes to abstract.',
      items: ['Portraits – individual & family','Pet portraits','Landscape & scenery','Abstract & conceptual art'],
      route: '/commission', cta: 'Request Commission'
    },
    {
      icon: '🎁', title: 'Gift Artworks',
      description: 'Surprise a loved one with a truly unique and personal gift. We offer special gift packaging and personalised dedication notes.',
      items: ['Personalised portraits','Anniversary & wedding gifts','Birthday surprises','Corporate gifting'],
      route: '/contact', cta: 'Get in Touch'
    },
    {
      icon: '📐', title: 'Mediums We Work In',
      description: 'We work across a wide range of traditional and contemporary mediums, choosing the best technique for your vision and budget.',
      items: ['Pencil & charcoal sketching','Watercolour painting','Oil & acrylic painting','Digital art'],
      route: '/contact', cta: 'Ask About Mediums'
    }
  ];

  steps = [
    { num:'01', title:'Reach Out', desc:'Contact us with your idea, reference photos, and budget.' },
    { num:'02', title:'Consultation', desc:'We discuss your vision, size, medium and timeline in detail.' },
    { num:'03', title:'Creation', desc:'We craft your artwork with care — updates shared along the way.' },
    { num:'04', title:'Delivery', desc:'Your framed, packaged artwork arrives safely at your door.' }
  ];
}
