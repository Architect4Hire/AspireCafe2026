import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'menu', pathMatch: 'full' },
  {
    path: 'menu',
    loadComponent: () => import('./features/menu/menu.component').then((m) => m.MenuComponent),
  },
  {
    path: 'payment',
    loadComponent: () => import('./features/payment/payment.component').then((m) => m.PaymentComponent),
  },
  {
    path: 'orders',
    loadComponent: () => import('./features/table-routing/table-routing.component').then((m) => m.TableRoutingComponent),
  },
  { path: '**', redirectTo: 'menu' },
];
