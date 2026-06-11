import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { Order } from '../../core/models/models';
import { OrderService } from '../../core/services/order.service';

@Component({
  selector: 'app-table-routing',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="orders-page">
      <div class="page-header">
        <div>
          <h1>Active Orders</h1>
          <p class="subtitle">Routed to tables — refreshes every 5 seconds</p>
        </div>
        <button class="btn btn-ghost" (click)="reload()">↻ Refresh</button>
      </div>

      @if (loading()) {
        <div class="loading">
          <div class="spinner"></div>
          <span>Loading orders…</span>
        </div>
      } @else if (error()) {
        <div class="error-state">Couldn't reach the Orders API. ({{ error() }})</div>
      } @else if (orders().length === 0) {
        <div class="empty-state">
          <div class="empty-icon">🍽️</div>
          <h2>No active orders</h2>
          <p>Submitted orders will appear here automatically.</p>
        </div>
      } @else {
        <div class="orders-grid">
          @for (order of orders(); track order.id) {
            <article class="order-card" [attr.data-status]="order.status.toLowerCase()">
              <header class="order-head">
                <div class="table-label">
                  <span class="table-num">{{ order.tableNumber }}</span>
                  <span class="table-text">Table</span>
                </div>
                <div class="status-pill" [attr.data-status]="order.status.toLowerCase()">
                  {{ order.status }}
                </div>
              </header>

              <div class="server">Server: <strong>{{ order.serverName }}</strong></div>

              <ul class="items">
                @for (item of order.items; track item.id) {
                  <li>
                    <span class="qty">{{ item.quantity }}×</span>
                    <span class="name">{{ item.name }}</span>
                    @if (item.notes) { <em class="notes">— {{ item.notes }}</em> }
                  </li>
                }
              </ul>

              <footer class="order-foot">
                <div class="totals">
                  <span class="total-label">Total</span>
                  <span class="total-val">\${{ order.total.toFixed(2) }}</span>
                </div>
                <div class="actions">
                  @if (order.status === 'Submitted') {
                    <button class="btn-mini start" (click)="advance(order, 'Preparing')">Start</button>
                  }
                  @if (order.status === 'Preparing') {
                    <button class="btn-mini ready" (click)="advance(order, 'Ready')">Ready</button>
                  }
                  @if (order.status === 'Ready') {
                    <button class="btn-mini deliver" (click)="advance(order, 'Delivered')">Deliver</button>
                  }
                </div>
              </footer>
            </article>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .orders-page { max-width: 1500px; margin: 0 auto; }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-end;
      margin-bottom: 22px;
    }

    .subtitle { color: var(--text-muted); font-size: 0.9rem; margin-top: 4px; }

    .orders-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(310px, 1fr));
      gap: 16px;
    }

    .order-card {
      background: var(--bg-surface);
      border: 1px solid var(--line);
      border-radius: var(--radius-md);
      padding: 18px;
      display: flex;
      flex-direction: column;
      gap: 12px;
      position: relative;
      overflow: hidden;
    }

    .order-card::before {
      content: '';
      position: absolute;
      top: 0; left: 0;
      width: 4px;
      height: 100%;
      background: var(--text-muted);
    }

    .order-card[data-status="submitted"]::before { background: var(--accent); }
    .order-card[data-status="preparing"]::before { background: var(--warning); }
    .order-card[data-status="ready"]::before { background: var(--success); }

    .order-head {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .table-label { display: flex; align-items: baseline; gap: 8px; }

    .table-num {
      font-family: var(--font-display);
      font-size: 2.2rem;
      font-weight: 800;
      color: var(--accent);
      line-height: 1;
    }

    .table-text {
      font-size: 0.72rem;
      letter-spacing: 0.15em;
      text-transform: uppercase;
      color: var(--text-muted);
      font-weight: 600;
    }

    .status-pill {
      padding: 5px 12px;
      border-radius: 999px;
      font-size: 0.74rem;
      font-weight: 700;
      letter-spacing: 0.05em;
      text-transform: uppercase;
      background: var(--bg-elevated);
      color: var(--text-secondary);
      border: 1px solid var(--line);
    }

    .status-pill[data-status="submitted"] {
      background: var(--accent-glow);
      color: var(--accent);
      border-color: rgba(212, 165, 116, 0.35);
    }
    .status-pill[data-status="preparing"] {
      background: rgba(224, 169, 109, 0.15);
      color: var(--warning);
      border-color: rgba(224, 169, 109, 0.35);
    }
    .status-pill[data-status="ready"] {
      background: rgba(127, 176, 105, 0.15);
      color: var(--success);
      border-color: rgba(127, 176, 105, 0.35);
    }

    .server { font-size: 0.85rem; color: var(--text-muted); }

    .items {
      list-style: none;
      border-top: 1px dashed var(--line);
      border-bottom: 1px dashed var(--line);
      padding: 10px 0;
      display: flex;
      flex-direction: column;
      gap: 5px;
    }

    .items li {
      display: flex;
      gap: 8px;
      font-size: 0.92rem;
      color: var(--text-secondary);
    }

    .items .qty {
      background: var(--bg-elevated);
      padding: 1px 7px;
      border-radius: 4px;
      font-weight: 700;
      color: var(--accent);
      font-size: 0.78rem;
      min-width: 28px;
      text-align: center;
    }

    .items .name { color: var(--text-primary); }
    .items .notes { color: var(--text-muted); font-style: italic; font-size: 0.82rem; }

    .order-foot {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .total-label {
      font-size: 0.72rem;
      letter-spacing: 0.08em;
      text-transform: uppercase;
      color: var(--text-muted);
      margin-right: 6px;
    }

    .total-val {
      font-family: var(--font-display);
      font-size: 1.3rem;
      font-weight: 700;
      color: var(--accent);
    }

    .actions { display: flex; gap: 6px; }

    .btn-mini {
      padding: 7px 14px;
      border-radius: 8px;
      font-weight: 600;
      font-size: 0.82rem;
      transition: all 0.16s ease;
    }

    .btn-mini.start { background: var(--accent); color: var(--bg-deep); }
    .btn-mini.ready { background: var(--warning); color: var(--bg-deep); }
    .btn-mini.deliver { background: var(--success); color: var(--bg-deep); }
    .btn-mini:hover { transform: translateY(-1px); filter: brightness(1.08); }

    .empty-state {
      text-align: center;
      padding: 80px 20px;
      color: var(--text-muted);
    }
    .empty-state h2 { color: var(--text-primary); margin-bottom: 6px; }
    .empty-icon { font-size: 4rem; opacity: 0.5; margin-bottom: 14px; }

    .loading, .error-state {
      padding: 40px;
      text-align: center;
      color: var(--text-muted);
    }
    .error-state { color: var(--danger); }

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
  `],
})
export class TableRoutingComponent implements OnInit, OnDestroy {
  private readonly orderService = inject(OrderService);

  protected readonly orders = signal<Order[]>([]);
  protected readonly loading = signal<boolean>(true);
  protected readonly error = signal<string | null>(null);

  private timer?: ReturnType<typeof setInterval>;

  ngOnInit() {
    this.reload();
    this.timer = setInterval(() => this.reload(), 5000);
  }

  ngOnDestroy() {
    if (this.timer) clearInterval(this.timer);
  }

  reload() {
    this.orderService.getActive().subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.loading.set(false);
        this.error.set(null);
      },
      error: (err) => {
        this.error.set(err.message ?? 'Unknown error');
        this.loading.set(false);
      },
    });
  }

  advance(order: Order, next: string) {
    this.orderService.updateStatus(order.id, next).subscribe({
      next: () => this.reload(),
    });
  }
}
