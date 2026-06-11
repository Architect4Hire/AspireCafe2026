import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { OrderService } from '../../core/services/order.service';
import { PaymentService } from '../../core/services/payment.service';
import { Order, Payment, TipOption } from '../../core/models/models';

type Stage = 'review' | 'processing' | 'complete';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="payment-page">
      @if (stage() === 'review') {
        <div class="payment-grid">
          <section class="left">
            <div class="card section">
              <h2>Tip</h2>
              <p class="muted">Automatic calculation — pick a preset or set your own.</p>

              <div class="tip-grid">
                @for (opt of tipOptions(); track opt.percent) {
                  <button class="tip-card"
                          [class.active]="!isCustomTip() && selectedTipPercent() === opt.percent"
                          (click)="selectTip(opt)">
                    <div class="tip-pct">{{ opt.percent }}%</div>
                    <div class="tip-amt">\${{ opt.amount.toFixed(2) }}</div>
                    <div class="tip-label">{{ opt.label }}</div>
                  </button>
                }
              </div>

              <div class="custom-tip">
                <button class="filter-pill" [class.active]="isCustomTip()" (click)="toggleCustomTip()">
                  Custom Amount
                </button>
                @if (isCustomTip()) {
                  <div class="custom-input">
                    <span class="prefix">$</span>
                    <input type="number" min="0" step="0.01"
                           [ngModel]="customTipAmount()"
                           (ngModelChange)="setCustomTip($event)" />
                  </div>
                }
              </div>
            </div>

            <div class="card section">
              <h2>Payment Method</h2>
              <div class="method-grid">
                @for (m of methods; track m.value) {
                  <button class="method-card"
                          [class.active]="selectedMethod() === m.value"
                          (click)="selectMethod(m.value)">
                    <div class="method-icon">{{ m.icon }}</div>
                    <div class="method-label">{{ m.label }}</div>
                  </button>
                }
              </div>

              @if (selectedMethod() === 'CreditCard' || selectedMethod() === 'DebitCard') {
                <div class="card-input">
                  <label>
                    <span>Last 4 digits</span>
                    <input type="text" maxlength="4" pattern="[0-9]{4}"
                           placeholder="1234"
                           [ngModel]="last4()"
                           (ngModelChange)="setLast4($event)" />
                  </label>
                </div>
              }
            </div>
          </section>

          <aside class="right">
            <div class="card receipt">
              <div class="receipt-head">
                <h2>Order Summary</h2>
                <div class="table-badge">Table {{ cart.tableNumber() }}</div>
              </div>

              <div class="receipt-lines">
                @for (line of cart.lines(); track line.menuItemId) {
                  <div class="r-line">
                    <span class="qty-pill">{{ line.quantity }}×</span>
                    <span class="r-name">{{ line.name }}</span>
                    <span class="r-amt">\${{ (line.unitPrice * line.quantity).toFixed(2) }}</span>
                  </div>
                }
              </div>

              <div class="receipt-totals">
                <div class="t-row"><span>Subtotal</span><strong>\${{ cart.subtotal().toFixed(2) }}</strong></div>
                <div class="t-row"><span>Tax (7%)</span><strong>\${{ cart.tax().toFixed(2) }}</strong></div>
                <div class="t-row tip-row">
                  <span>Tip
                    @if (!isCustomTip()) {
                      <span class="tip-badge">{{ selectedTipPercent() }}%</span>
                    }
                  </span>
                  <strong>\${{ effectiveTipAmount().toFixed(2) }}</strong>
                </div>
                <div class="t-row grand">
                  <span>Total Due</span>
                  <strong>\${{ grandTotal().toFixed(2) }}</strong>
                </div>
              </div>

              <button class="btn btn-primary pay-btn"
                      [disabled]="!canPay()"
                      (click)="submit()">
                Pay \${{ grandTotal().toFixed(2) }} →
              </button>
              <button class="btn btn-ghost block" (click)="back()">← Back to Menu</button>
            </div>
          </aside>
        </div>
      } @else if (stage() === 'processing') {
        <div class="processing">
          <div class="processing-card">
            <div class="big-spinner"></div>
            <h2>Processing Payment</h2>
            <p>Authorizing your payment of \${{ grandTotal().toFixed(2) }}…</p>
          </div>
        </div>
      } @else if (stage() === 'complete' && completedPayment()) {
        <div class="success">
          <div class="success-card">
            <div class="check">✓</div>
            <h1>Payment Complete</h1>
            <p class="muted">Order routed to Table {{ cart.tableNumber() }}</p>

            <div class="receipt-summary">
              <div class="r-row"><span>Subtotal</span><span>\${{ completedPayment()!.subtotal.toFixed(2) }}</span></div>
              <div class="r-row"><span>Tax</span><span>\${{ completedPayment()!.taxAmount.toFixed(2) }}</span></div>
              <div class="r-row"><span>Tip ({{ completedPayment()!.tipPercent.toFixed(0) }}%)</span><span>\${{ completedPayment()!.tipAmount.toFixed(2) }}</span></div>
              <div class="r-row grand"><span>Total Paid</span><span>\${{ completedPayment()!.total.toFixed(2) }}</span></div>
              <div class="auth">
                <div class="auth-row"><span>Method</span><strong>{{ completedPayment()!.method }}</strong></div>
                <div class="auth-row"><span>Auth Code</span><strong>{{ completedPayment()!.authorizationCode }}</strong></div>
                @if (completedPayment()!.last4) {
                  <div class="auth-row"><span>Card</span><strong>•••• {{ completedPayment()!.last4 }}</strong></div>
                }
              </div>
            </div>

            <button class="btn btn-primary block" (click)="newOrder()">Start New Order</button>
            <button class="btn btn-ghost block" (click)="viewOrders()">View All Active Orders</button>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .payment-page { max-width: 1300px; margin: 0 auto; }

    .payment-grid {
      display: grid;
      grid-template-columns: 1fr 420px;
      gap: 22px;
    }

    .section { margin-bottom: 20px; }
    .muted { color: var(--text-muted); font-size: 0.88rem; margin-top: 4px; margin-bottom: 16px; }

    .tip-grid {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 10px;
      margin-bottom: 18px;
    }

    .tip-card {
      padding: 18px 10px;
      background: var(--bg-elevated);
      border: 2px solid var(--line);
      border-radius: var(--radius-md);
      text-align: center;
      transition: all 0.18s ease;
      color: var(--text-primary);
    }

    .tip-card:hover { border-color: var(--accent); transform: translateY(-2px); }

    .tip-card.active {
      background: linear-gradient(135deg, rgba(212, 165, 116, 0.18), rgba(212, 165, 116, 0.06));
      border-color: var(--accent);
      box-shadow: 0 6px 20px var(--accent-glow);
    }

    .tip-pct {
      font-family: var(--font-display);
      font-size: 1.6rem;
      font-weight: 800;
      color: var(--accent);
    }

    .tip-amt { font-weight: 700; margin-top: 4px; }
    .tip-label { font-size: 0.72rem; color: var(--text-muted); margin-top: 2px; letter-spacing: 0.05em; text-transform: uppercase; }

    .custom-tip { display: flex; gap: 12px; align-items: center; flex-wrap: wrap; }

    .filter-pill {
      padding: 9px 18px;
      border-radius: 999px;
      background: var(--bg-elevated);
      color: var(--text-secondary);
      border: 1px solid var(--line);
      font-weight: 500;
      font-size: 0.9rem;
    }

    .filter-pill.active { background: var(--accent); color: var(--bg-deep); border-color: var(--accent); }

    .custom-input {
      display: flex;
      align-items: center;
      background: var(--bg-deep);
      border: 1px solid var(--line);
      border-radius: 8px;
      padding: 0 10px;
    }

    .custom-input .prefix { color: var(--text-muted); font-weight: 600; }

    .custom-input input {
      background: transparent;
      border: none;
      color: var(--text-primary);
      padding: 10px 8px;
      font-size: 1rem;
      width: 100px;
      font-family: inherit;
    }

    .custom-input input:focus { outline: none; }

    .method-grid {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 10px;
      margin-bottom: 16px;
    }

    .method-card {
      padding: 18px 10px;
      background: var(--bg-elevated);
      border: 2px solid var(--line);
      border-radius: var(--radius-md);
      transition: all 0.18s ease;
      color: var(--text-primary);
    }

    .method-card:hover { border-color: var(--accent); }
    .method-card.active {
      background: var(--accent-glow);
      border-color: var(--accent);
    }

    .method-icon { font-size: 1.8rem; margin-bottom: 6px; }
    .method-label { font-weight: 600; font-size: 0.88rem; }

    .card-input { margin-top: 8px; }
    .card-input label { display: flex; flex-direction: column; gap: 4px; }
    .card-input span {
      font-size: 0.7rem;
      letter-spacing: 0.08em;
      text-transform: uppercase;
      color: var(--text-muted);
      font-weight: 600;
    }
    .card-input input {
      padding: 10px 12px;
      background: var(--bg-deep);
      border: 1px solid var(--line);
      border-radius: 8px;
      color: var(--text-primary);
      font-size: 1rem;
      font-family: inherit;
      max-width: 200px;
    }

    .right { position: sticky; top: 110px; }

    .receipt-head {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
    }

    .table-badge {
      background: var(--accent-glow);
      color: var(--accent);
      padding: 6px 14px;
      border-radius: 999px;
      font-weight: 700;
      border: 1px solid rgba(212, 165, 116, 0.35);
      font-size: 0.9rem;
    }

    .receipt-lines { padding: 6px 0 14px; border-bottom: 1px dashed var(--line); margin-bottom: 14px; }

    .r-line {
      display: grid;
      grid-template-columns: auto 1fr auto;
      gap: 10px;
      padding: 6px 0;
      align-items: center;
    }

    .qty-pill {
      background: var(--bg-elevated);
      padding: 2px 8px;
      border-radius: 6px;
      font-size: 0.78rem;
      font-weight: 700;
      color: var(--accent);
    }

    .r-name { color: var(--text-secondary); }
    .r-amt { font-weight: 600; }

    .receipt-totals { padding: 4px 0 14px; }

    .t-row {
      display: flex;
      justify-content: space-between;
      padding: 6px 0;
      color: var(--text-secondary);
      font-size: 0.94rem;
    }

    .tip-badge {
      background: var(--accent-glow);
      color: var(--accent);
      padding: 1px 8px;
      border-radius: 4px;
      font-size: 0.7rem;
      margin-left: 6px;
      font-weight: 700;
    }

    .t-row.grand {
      border-top: 1px solid var(--line);
      margin-top: 8px;
      padding-top: 12px;
      color: var(--text-primary);
      font-size: 1.25rem;
    }

    .t-row.grand strong {
      color: var(--accent);
      font-family: var(--font-display);
      font-size: 1.6rem;
    }

    .pay-btn { width: 100%; justify-content: center; padding: 16px; font-size: 1.05rem; margin-bottom: 8px; }
    .block { width: 100%; justify-content: center; }

    /* Processing */
    .processing { display: flex; justify-content: center; padding-top: 80px; }
    .processing-card { text-align: center; }
    .big-spinner {
      width: 80px;
      height: 80px;
      border: 5px solid var(--line);
      border-top-color: var(--accent);
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin: 0 auto 24px;
    }
    .processing-card p { color: var(--text-muted); margin-top: 8px; }

    @keyframes spin { to { transform: rotate(360deg); } }

    /* Success */
    .success { display: flex; justify-content: center; padding-top: 30px; }
    .success-card {
      max-width: 460px;
      width: 100%;
      text-align: center;
      background: var(--bg-surface);
      border: 1px solid var(--line);
      border-radius: var(--radius-lg);
      padding: 36px 28px;
      box-shadow: var(--shadow-lg);
    }

    .check {
      width: 76px;
      height: 76px;
      border-radius: 50%;
      background: linear-gradient(135deg, var(--success), #5d9b54);
      color: white;
      font-size: 2.5rem;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto 18px;
      box-shadow: 0 8px 24px rgba(127, 176, 105, 0.4);
      animation: pop 0.4s cubic-bezier(0.34, 1.56, 0.64, 1);
    }

    @keyframes pop { from { transform: scale(0); } to { transform: scale(1); } }

    .success h1 { margin-bottom: 4px; }
    .success .muted { margin-bottom: 24px; }

    .receipt-summary { text-align: left; margin-bottom: 24px; }
    .r-row { display: flex; justify-content: space-between; padding: 6px 0; color: var(--text-secondary); }
    .r-row.grand {
      border-top: 1px solid var(--line);
      margin-top: 10px;
      padding-top: 14px;
      color: var(--text-primary);
      font-weight: 700;
      font-size: 1.15rem;
    }
    .r-row.grand span:last-child { color: var(--accent); }

    .auth {
      background: var(--bg-deep);
      border-radius: 10px;
      padding: 12px 14px;
      margin-top: 14px;
    }

    .auth-row {
      display: flex;
      justify-content: space-between;
      padding: 4px 0;
      font-size: 0.86rem;
    }

    .auth-row span { color: var(--text-muted); }

    @media (max-width: 1000px) {
      .payment-grid { grid-template-columns: 1fr; }
      .right { position: static; }
      .tip-grid, .method-grid { grid-template-columns: repeat(2, 1fr); }
    }
  `],
})
export class PaymentComponent implements OnInit {
  protected readonly cart = inject(CartService);
  private readonly orderService = inject(OrderService);
  private readonly paymentService = inject(PaymentService);
  private readonly router = inject(Router);

  protected readonly stage = signal<Stage>('review');
  protected readonly tipOptions = signal<TipOption[]>([]);
  protected readonly selectedTipPercent = signal<number>(18);
  protected readonly customTipAmount = signal<number>(0);
  protected readonly isCustomTip = signal<boolean>(false);
  protected readonly selectedMethod = signal<string>('CreditCard');
  protected readonly last4 = signal<string>('');
  protected readonly completedPayment = signal<Payment | null>(null);

  protected readonly methods = [
    { value: 'CreditCard', label: 'Credit', icon: '💳' },
    { value: 'DebitCard', label: 'Debit', icon: '💳' },
    { value: 'Cash', label: 'Cash', icon: '💵' },
    { value: 'MobileWallet', label: 'Mobile', icon: '📱' },
  ];

  protected readonly effectiveTipAmount = computed(() => {
    if (this.isCustomTip()) return +this.customTipAmount();
    const opt = this.tipOptions().find((o) => o.percent === this.selectedTipPercent());
    return opt?.amount ?? 0;
  });

  protected readonly grandTotal = computed(() =>
    +(this.cart.subtotal() + this.cart.tax() + this.effectiveTipAmount()).toFixed(2)
  );

  protected readonly canPay = computed(() => {
    if (this.cart.itemCount() === 0) return false;
    const method = this.selectedMethod();
    if (method === 'CreditCard' || method === 'DebitCard') {
      return /^\d{4}$/.test(this.last4());
    }
    return true;
  });

  ngOnInit() {
    if (this.cart.itemCount() === 0) {
      this.router.navigate(['/menu']);
      return;
    }
    this.paymentService.getTipSuggestions(this.cart.subtotal()).subscribe({
      next: (s) => this.tipOptions.set(s.options),
      error: () => {
        // Fallback to client-side suggestions if API is unreachable
        const sub = this.cart.subtotal();
        this.tipOptions.set([
          { percent: 15, amount: +(sub * 0.15).toFixed(2), label: 'Good' },
          { percent: 18, amount: +(sub * 0.18).toFixed(2), label: 'Great' },
          { percent: 20, amount: +(sub * 0.20).toFixed(2), label: 'Excellent' },
          { percent: 25, amount: +(sub * 0.25).toFixed(2), label: 'Outstanding' },
        ]);
      },
    });
  }

  selectTip(opt: TipOption) {
    this.isCustomTip.set(false);
    this.selectedTipPercent.set(opt.percent);
  }

  toggleCustomTip() {
    this.isCustomTip.update((v) => !v);
    if (this.isCustomTip() && this.customTipAmount() === 0) {
      this.customTipAmount.set(this.effectiveTipAmount());
    }
  }

  setCustomTip(amount: number) { this.customTipAmount.set(amount || 0); }

  selectMethod(m: string) { this.selectedMethod.set(m); }
  setLast4(v: string) { this.last4.set(v.replace(/\D/g, '').slice(0, 4)); }

  submit() {
    if (!this.canPay()) return;
    this.stage.set('processing');

    // Step 1: submit the order (order routing by table number)
    this.orderService
      .submit({
        tableNumber: this.cart.tableNumber(),
        serverName: this.cart.serverName(),
        items: this.cart.lines().map((l) => ({
          menuItemId: l.menuItemId,
          name: l.name,
          unitPrice: l.unitPrice,
          quantity: l.quantity,
          notes: l.notes,
        })),
      })
      .subscribe({
        next: (order: Order) => {
          // Step 2: process payment for the created order
          this.paymentService
            .process({
              orderId: order.id,
              tableNumber: order.tableNumber,
              subtotal: order.subtotal,
              taxAmount: order.taxAmount,
              tipPercent: this.isCustomTip() ? undefined : this.selectedTipPercent(),
              tipAmount: this.isCustomTip() ? this.effectiveTipAmount() : undefined,
              method: this.selectedMethod(),
              last4: this.last4(),
            })
            .subscribe({
              next: (payment) => {
                this.completedPayment.set(payment);
                this.stage.set('complete');
              },
              error: () => this.stage.set('review'),
            });
        },
        error: () => this.stage.set('review'),
      });
  }

  back() { this.router.navigate(['/menu']); }
  newOrder() { this.cart.clear(); this.router.navigate(['/menu']); }
  viewOrders() { this.router.navigate(['/orders']); }
}
