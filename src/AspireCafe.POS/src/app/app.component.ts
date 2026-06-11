import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { CartService } from './core/services/cart.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="app">
      <header class="topbar">
        <div class="brand">
          <div class="brand-mark">
            <span class="bean">☕</span>
          </div>
          <div>
            <div class="brand-name">AspireCafe</div>
            <div class="brand-tag">Point of Sale</div>
          </div>
        </div>

        <nav class="nav">
          <a routerLink="/menu" routerLinkActive="active" class="nav-link">
            <span class="dot"></span> Menu &amp; Order
          </a>
          <a routerLink="/payment" routerLinkActive="active" class="nav-link"
             [class.disabled]="cart.itemCount() === 0">
            <span class="dot"></span> Payment
          </a>
          <a routerLink="/orders" routerLinkActive="active" class="nav-link">
            <span class="dot"></span> Active Orders
          </a>
        </nav>

        <div class="status">
          <div class="status-card">
            <div class="status-label">Table</div>
            <div class="status-value">{{ cart.tableNumber() }}</div>
          </div>
          <div class="status-card">
            <div class="status-label">Items</div>
            <div class="status-value">{{ cart.itemCount() }}</div>
          </div>
          <div class="status-card highlight">
            <div class="status-label">Subtotal</div>
            <div class="status-value">\${{ cart.subtotal().toFixed(2) }}</div>
          </div>
        </div>
      </header>

      <main class="main">
        <router-outlet />
      </main>
    </div>
  `,
  styles: [`
    .app {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
    }

    .topbar {
      display: grid;
      grid-template-columns: auto 1fr auto;
      align-items: center;
      gap: 32px;
      padding: 18px 32px;
      background: linear-gradient(180deg, rgba(46, 37, 31, 0.95) 0%, rgba(36, 28, 23, 0.85) 100%);
      backdrop-filter: blur(12px);
      border-bottom: 1px solid var(--line);
      position: sticky;
      top: 0;
      z-index: 50;
    }

    .brand {
      display: flex;
      align-items: center;
      gap: 14px;
    }

    .brand-mark {
      width: 48px;
      height: 48px;
      border-radius: 14px;
      background: linear-gradient(135deg, var(--accent) 0%, var(--espresso) 100%);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.6rem;
      box-shadow: 0 6px 16px var(--accent-glow);
    }

    .brand-name {
      font-family: var(--font-display);
      font-size: 1.4rem;
      font-weight: 800;
      color: var(--text-primary);
      letter-spacing: -0.01em;
    }

    .brand-tag {
      font-size: 0.7rem;
      letter-spacing: 0.15em;
      text-transform: uppercase;
      color: var(--text-muted);
      font-weight: 600;
    }

    .nav {
      display: flex;
      gap: 6px;
      justify-content: center;
    }

    .nav-link {
      display: inline-flex;
      align-items: center;
      gap: 8px;
      padding: 10px 18px;
      border-radius: 999px;
      color: var(--text-secondary);
      text-decoration: none;
      font-weight: 500;
      font-size: 0.92rem;
      transition: all 0.18s ease;
      border: 1px solid transparent;
    }

    .nav-link:hover:not(.disabled) {
      color: var(--text-primary);
      background: var(--bg-elevated);
    }

    .nav-link.active {
      color: var(--accent);
      background: var(--accent-glow);
      border-color: rgba(212, 165, 116, 0.3);
    }

    .nav-link.active .dot { background: var(--accent); }
    .nav-link.disabled { opacity: 0.4; pointer-events: none; }

    .dot {
      width: 6px;
      height: 6px;
      border-radius: 50%;
      background: var(--text-muted);
    }

    .status {
      display: flex;
      gap: 10px;
    }

    .status-card {
      padding: 8px 16px;
      background: var(--bg-surface);
      border: 1px solid var(--line);
      border-radius: 12px;
      text-align: center;
      min-width: 80px;
    }

    .status-card.highlight {
      background: linear-gradient(135deg, rgba(212, 165, 116, 0.12), rgba(212, 165, 116, 0.04));
      border-color: rgba(212, 165, 116, 0.4);
    }

    .status-label {
      font-size: 0.68rem;
      letter-spacing: 0.1em;
      text-transform: uppercase;
      color: var(--text-muted);
      font-weight: 600;
    }

    .status-value {
      font-family: var(--font-display);
      font-size: 1.15rem;
      font-weight: 700;
      color: var(--text-primary);
      margin-top: 2px;
    }

    .status-card.highlight .status-value { color: var(--accent); }

    .main {
      flex: 1;
      padding: 28px 32px 48px;
      max-width: 1600px;
      width: 100%;
      margin: 0 auto;
    }

    @media (max-width: 900px) {
      .topbar { grid-template-columns: 1fr; gap: 14px; padding: 14px 18px; }
      .status { justify-content: center; }
      .main { padding: 18px; }
    }
  `],
})
export class AppComponent {
  protected readonly cart = inject(CartService);
}
