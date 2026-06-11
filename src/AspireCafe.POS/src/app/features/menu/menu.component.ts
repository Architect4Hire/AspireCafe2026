import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { MenuService } from '../../core/services/menu.service';
import { MenuItem } from '../../core/models/models';

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="menu-page">
      <section class="menu-area">
        <div class="page-header">
          <div>
            <h1>Today's Menu</h1>
            <p class="subtitle">Tap items to add to the order</p>
          </div>
          <div class="filters">
            <button class="filter-pill" [class.active]="selectedCategory() === 'All'"
                    (click)="selectCategory('All')">All</button>
            @for (cat of categories(); track cat) {
              <button class="filter-pill" [class.active]="selectedCategory() === cat"
                      (click)="selectCategory(cat)">{{ cat }}</button>
            }
          </div>
        </div>

        @if (loading()) {
          <div class="loading">
            <div class="spinner"></div>
            <span>Brewing the menu…</span>
          </div>
        } @else if (error()) {
          <div class="error-state">
            <strong>Couldn't reach the Menu API.</strong>
            <p>Make sure the AspireCafe.AppHost is running. ({{ error() }})</p>
          </div>
        } @else {
          <div class="grid">
            @for (item of visibleItems(); track item.id) {
              <article class="item-card" (click)="addToCart(item)">
                <div class="item-image" [style.background]="bgFor(item)">
                  <span class="item-emoji">{{ emojiFor(item) }}</span>
                  @if (!item.isAvailable) {
                    <div class="unavailable">86'd</div>
                  }
                </div>
                <div class="item-body">
                  <div class="item-row">
                    <h3 class="item-name">{{ item.name }}</h3>
                    <div class="item-price">\${{ item.price.toFixed(2) }}</div>
                  </div>
                  <p class="item-desc">{{ item.description }}</p>
                  <div class="item-meta">
                    <span class="chip">{{ item.category }}</span>
                    <span class="chip">{{ item.prepTimeMinutes }} min</span>
                  </div>
                </div>
                <div class="item-add">+</div>
              </article>
            }
          </div>
        }
      </section>

      <aside class="cart-aside">
        <div class="cart-card">
          <div class="cart-head">
            <h2>Current Order</h2>
            <span class="chip accent">{{ cart.itemCount() }} items</span>
          </div>

          <div class="table-row">
            <label>
              <span>Table #</span>
              <input type="number" min="1" max="999"
                     [ngModel]="cart.tableNumber()"
                     (ngModelChange)="cart.setTable($event)" />
            </label>
            <label>
              <span>Server</span>
              <input type="text" maxlength="40"
                     [ngModel]="cart.serverName()"
                     (ngModelChange)="cart.setServer($event)" />
            </label>
          </div>

          <div class="cart-lines">
            @if (cart.lines().length === 0) {
              <div class="empty">
                <div class="empty-icon">🛒</div>
                <p>No items yet. Tap a menu item to start.</p>
              </div>
            }
            @for (line of cart.lines(); track line.menuItemId) {
              <div class="cart-line">
                <div class="line-info">
                  <div class="line-name">{{ line.name }}</div>
                  <div class="line-price">\${{ line.unitPrice.toFixed(2) }} each</div>
                </div>
                <div class="qty">
                  <button (click)="cart.decrement(line.menuItemId)">−</button>
                  <span>{{ line.quantity }}</span>
                  <button (click)="cart.increment(line.menuItemId)">+</button>
                </div>
                <div class="line-total">\${{ (line.unitPrice * line.quantity).toFixed(2) }}</div>
              </div>
            }
          </div>

          <div class="cart-totals">
            <div class="t-row"><span>Subtotal</span><strong>\${{ cart.subtotal().toFixed(2) }}</strong></div>
            <div class="t-row"><span>Tax (7%)</span><strong>\${{ cart.tax().toFixed(2) }}</strong></div>
            <div class="t-row total"><span>Total</span><strong>\${{ cart.total().toFixed(2) }}</strong></div>
          </div>

          <button class="btn btn-primary block"
                  [disabled]="cart.itemCount() === 0"
                  (click)="proceedToPayment()">
            Proceed to Payment →
          </button>
          @if (cart.itemCount() > 0) {
            <button class="btn btn-danger block" (click)="cart.clear()">Clear Order</button>
          }
        </div>
      </aside>
    </div>
  `,
  styles: [`
    .menu-page {
      display: grid;
      grid-template-columns: 1fr 380px;
      gap: 28px;
      align-items: start;
    }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-end;
      margin-bottom: 22px;
      flex-wrap: wrap;
      gap: 18px;
    }

    .subtitle { color: var(--text-muted); margin-top: 4px; font-size: 0.92rem; }

    .filters { display: flex; gap: 6px; flex-wrap: wrap; }

    .filter-pill {
      padding: 8px 16px;
      border-radius: 999px;
      background: var(--bg-surface);
      color: var(--text-secondary);
      border: 1px solid var(--line);
      font-weight: 500;
      font-size: 0.88rem;
      transition: all 0.18s ease;
    }

    .filter-pill:hover { color: var(--text-primary); border-color: var(--accent); }
    .filter-pill.active {
      background: var(--accent);
      color: var(--bg-deep);
      border-color: var(--accent);
    }

    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
      gap: 16px;
    }

    .item-card {
      position: relative;
      background: var(--bg-surface);
      border: 1px solid var(--line);
      border-radius: var(--radius-md);
      overflow: hidden;
      cursor: pointer;
      transition: all 0.2s ease;
      display: flex;
      flex-direction: column;
    }

    .item-card:hover {
      transform: translateY(-3px);
      border-color: var(--accent);
      box-shadow: var(--shadow-md);
    }

    .item-image {
      height: 130px;
      position: relative;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .item-emoji { font-size: 3.5rem; filter: drop-shadow(0 4px 8px rgba(0,0,0,0.3)); }

    .unavailable {
      position: absolute;
      top: 10px;
      right: 10px;
      background: var(--danger);
      color: white;
      padding: 4px 10px;
      border-radius: 6px;
      font-size: 0.72rem;
      font-weight: 700;
      letter-spacing: 0.05em;
    }

    .item-body { padding: 14px 16px; flex: 1; }

    .item-row {
      display: flex;
      justify-content: space-between;
      align-items: baseline;
      gap: 12px;
      margin-bottom: 6px;
    }

    .item-name {
      font-family: var(--font-display);
      font-size: 1.1rem;
      color: var(--text-primary);
    }

    .item-price {
      font-weight: 700;
      color: var(--accent);
      font-size: 1.05rem;
    }

    .item-desc {
      font-size: 0.84rem;
      color: var(--text-muted);
      line-height: 1.4;
      margin-bottom: 10px;
      min-height: 2.4em;
    }

    .item-meta { display: flex; gap: 6px; flex-wrap: wrap; }

    .item-add {
      position: absolute;
      top: 12px;
      left: 12px;
      width: 32px;
      height: 32px;
      border-radius: 50%;
      background: var(--accent);
      color: var(--bg-deep);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.4rem;
      font-weight: 700;
      opacity: 0;
      transition: opacity 0.18s;
    }

    .item-card:hover .item-add { opacity: 1; }

    /* Cart panel */
    .cart-aside { position: sticky; top: 110px; }

    .cart-card {
      background: var(--bg-surface);
      border: 1px solid var(--line);
      border-radius: var(--radius-md);
      padding: 22px;
      box-shadow: var(--shadow-md);
    }

    .cart-head {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
    }

    .table-row {
      display: grid;
      grid-template-columns: 1fr 1.5fr;
      gap: 10px;
      margin-bottom: 16px;
    }

    .table-row label {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .table-row label span {
      font-size: 0.7rem;
      letter-spacing: 0.08em;
      text-transform: uppercase;
      color: var(--text-muted);
      font-weight: 600;
    }

    .table-row input {
      padding: 10px 12px;
      background: var(--bg-deep);
      border: 1px solid var(--line);
      border-radius: 8px;
      color: var(--text-primary);
      font-size: 0.95rem;
      font-family: inherit;
    }

    .table-row input:focus {
      outline: none;
      border-color: var(--accent);
    }

    .cart-lines {
      max-height: 320px;
      overflow-y: auto;
      margin-bottom: 16px;
      padding-right: 4px;
    }

    .empty {
      text-align: center;
      padding: 30px 10px;
      color: var(--text-muted);
    }

    .empty-icon { font-size: 2.4rem; margin-bottom: 8px; opacity: 0.5; }

    .cart-line {
      display: grid;
      grid-template-columns: 1fr auto auto;
      gap: 10px;
      align-items: center;
      padding: 10px 0;
      border-bottom: 1px solid var(--line);
    }

    .cart-line:last-child { border-bottom: none; }

    .line-name { font-weight: 600; color: var(--text-primary); font-size: 0.92rem; }
    .line-price { font-size: 0.76rem; color: var(--text-muted); }
    .line-total { font-weight: 700; color: var(--accent); min-width: 56px; text-align: right; }

    .qty {
      display: flex;
      align-items: center;
      gap: 8px;
      background: var(--bg-deep);
      border-radius: 8px;
      padding: 2px;
    }

    .qty button {
      width: 24px;
      height: 24px;
      border-radius: 6px;
      background: var(--bg-elevated);
      color: var(--text-primary);
      font-weight: 700;
      font-size: 0.9rem;
    }

    .qty button:hover { background: var(--accent); color: var(--bg-deep); }
    .qty span { min-width: 18px; text-align: center; font-weight: 600; }

    .cart-totals { padding: 14px 0; border-top: 1px dashed var(--line); margin-bottom: 14px; }

    .t-row {
      display: flex;
      justify-content: space-between;
      padding: 4px 0;
      color: var(--text-secondary);
      font-size: 0.92rem;
    }

    .t-row.total {
      border-top: 1px solid var(--line);
      margin-top: 8px;
      padding-top: 12px;
      color: var(--text-primary);
      font-size: 1.1rem;
    }

    .t-row.total strong { color: var(--accent); font-family: var(--font-display); }

    .block { width: 100%; justify-content: center; margin-bottom: 8px; }

    .loading, .error-state {
      padding: 40px;
      text-align: center;
      color: var(--text-muted);
    }

    .error-state { color: var(--danger); }
    .error-state p { color: var(--text-muted); margin-top: 6px; font-size: 0.88rem; }

    .spinner {
      width: 36px;
      height: 36px;
      border: 3px solid var(--line);
      border-top-color: var(--accent);
      border-radius: 50%;
      animation: spin 0.9s linear infinite;
      margin: 0 auto 12px;
    }

    @keyframes spin { to { transform: rotate(360deg); } }

    @media (max-width: 1100px) {
      .menu-page { grid-template-columns: 1fr; }
      .cart-aside { position: static; }
    }
  `],
})
export class MenuComponent implements OnInit {
  protected readonly cart = inject(CartService);
  private readonly menuService = inject(MenuService);
  private readonly router = inject(Router);

  protected readonly items = signal<MenuItem[]>([]);
  protected readonly loading = signal<boolean>(true);
  protected readonly error = signal<string | null>(null);
  protected readonly selectedCategory = signal<string>('All');

  protected readonly categories = computed(() => {
    const set = new Set(this.items().map((i) => i.category));
    return Array.from(set).sort();
  });

  protected readonly visibleItems = computed(() => {
    const cat = this.selectedCategory();
    return cat === 'All' ? this.items() : this.items().filter((i) => i.category === cat);
  });

  ngOnInit() {
    this.menuService.getMenu().subscribe({
      next: (items) => { this.items.set(items); this.loading.set(false); },
      error: (err) => { this.error.set(err.message ?? 'Unknown error'); this.loading.set(false); },
    });
  }

  selectCategory(c: string) { this.selectedCategory.set(c); }

  addToCart(item: MenuItem) {
    if (!item.isAvailable) return;
    this.cart.add(item);
  }

  proceedToPayment() {
    if (this.cart.itemCount() === 0) return;
    this.router.navigate(['/payment']);
  }

  protected bgFor(item: MenuItem): string {
    const palettes: Record<string, string> = {
      Coffee: 'linear-gradient(135deg, #4a2c1a 0%, #2e1810 100%)',
      Tea: 'linear-gradient(135deg, #3a4a2c 0%, #1e2a18 100%)',
      Pastry: 'linear-gradient(135deg, #5a3e22 0%, #3a2614 100%)',
      Food: 'linear-gradient(135deg, #4a3320 0%, #2e1f12 100%)',
    };
    return palettes[item.category] ?? 'linear-gradient(135deg, #3e2e22 0%, #241a14 100%)';
  }

  protected emojiFor(item: MenuItem): string {
    const map: Record<string, string> = {
      'Espresso': '☕',
      'Cappuccino': '☕',
      'Latte': '🥛',
      'Cold Brew': '🧊',
      'Croissant': '🥐',
      'Blueberry Muffin': '🧁',
      'Avocado Toast': '🥑',
      'Caprese Panini': '🥪',
      'Matcha Latte': '🍵',
      'Chai Latte': '🍵',
    };
    return map[item.name] ?? '🍽️';
  }
}
