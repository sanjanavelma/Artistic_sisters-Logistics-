import { Routes } from '@angular/router';
import { authGuard, artistGuard, deliveryGuard, adminGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent) },
  { path: 'about', loadComponent: () => import('./pages/about/about.component').then(m => m.AboutComponent) },
  { path: 'portfolio', loadComponent: () => import('./pages/portfolio/portfolio.component').then(m => m.PortfolioComponent) },
  { path: 'services', loadComponent: () => import('./pages/services/services.component').then(m => m.ServicesComponent) },
  { path: 'contact', loadComponent: () => import('./pages/contact/contact.component').then(m => m.ContactComponent) },
  { path: 'auth/login', loadComponent: () => import('./pages/auth/login/login.component').then(m => m.LoginComponent) },
  { path: 'auth/register', loadComponent: () => import('./pages/auth/register/register.component').then(m => m.RegisterComponent) },
  { path: 'auth/register-agent', loadComponent: () => import('./pages/auth/register-agent/register-agent.component').then(m => m.RegisterAgentComponent) },
  { path: 'order', loadComponent: () => import('./pages/order/order.component').then(m => m.OrderComponent), canActivate: [authGuard] },
  { path: 'commission', loadComponent: () => import('./pages/commission/commission.component').then(m => m.CommissionComponent), canActivate: [authGuard] },
  { path: 'track/:orderId', loadComponent: () => import('./pages/track-order/track-order.component').then(m => m.TrackOrderComponent), canActivate: [authGuard] },
  { path: 'artist', loadComponent: () => import('./pages/artist/artist.component').then(m => m.ArtistComponent), canActivate: [artistGuard] },
  { path: 'customer/dashboard', loadComponent: () => import('./pages/customer/customer-dashboard.component').then(m => m.CustomerDashboardComponent), canActivate: [authGuard] },
  { path: 'delivery/dashboard', loadComponent: () => import('./pages/delivery/delivery-dashboard.component').then(m => m.DeliveryDashboardComponent), canActivate: [deliveryGuard] },
  { path: 'admin/dashboard', loadComponent: () => import('./pages/admin/admin-dashboard.component').then(m => m.AdminDashboardComponent), canActivate: [adminGuard] },
  { path: '**', redirectTo: '' }
];

