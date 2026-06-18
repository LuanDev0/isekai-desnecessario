import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { ProfileService } from './profile.service';

export const authGuard: CanActivateFn = () => {
  const auth    = inject(AuthService);
  const router  = inject(Router);

  if (auth.isLogado()) return true;

  return router.createUrlTree(['/cadastro']);
};

export const guestGuard: CanActivateFn = () => {
  const auth    = inject(AuthService);
  const profile = inject(ProfileService);
  const router  = inject(Router);

  if (auth.isLogado() && profile.getSavedId()) {
    return router.createUrlTree(['/']);
  }

  return true;
};
