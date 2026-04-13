import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const router = inject(Router);
  const authService = inject(AuthService);
  if (authService.isLoggedIn) return true;
  router.navigate(['/auth/login']);
  return false;
};

export const artistGuard: CanActivateFn = () => {
  const router = inject(Router);
  const token  = localStorage.getItem('as_token');
  const role   = localStorage.getItem('as_role');
  if (token && role === 'Artist') return true;
  router.navigate(['/']);
  return false;
};

export const deliveryGuard: CanActivateFn = () => {
  const router = inject(Router);
  const token  = localStorage.getItem('as_token');
  const role   = localStorage.getItem('as_role');
  if (token && role === 'DeliveryAgent') return true;
  router.navigate(['/auth/login']);
  return false;
};

export const adminGuard: CanActivateFn = () => {
  const router = inject(Router);
  const token  = localStorage.getItem('as_token');
  const role   = localStorage.getItem('as_role');
  if (token && role === 'Artist') return true;
  router.navigate(['/auth/login']);
  return false;
};
