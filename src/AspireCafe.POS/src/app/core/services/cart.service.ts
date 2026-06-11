import { Injectable, computed, signal } from '@angular/core';
import { CartLine, MenuItem } from '../models/models';

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly _lines = signal<CartLine[]>([]);
  private readonly _tableNumber = signal<number>(1);
  private readonly _serverName = signal<string>('Server');

  readonly lines = this._lines.asReadonly();
  readonly tableNumber = this._tableNumber.asReadonly();
  readonly serverName = this._serverName.asReadonly();

  readonly itemCount = computed(() =>
    this._lines().reduce((sum, l) => sum + l.quantity, 0)
  );

  readonly subtotal = computed(() =>
    +this._lines().reduce((sum, l) => sum + l.unitPrice * l.quantity, 0).toFixed(2)
  );

  readonly tax = computed(() => +(this.subtotal() * 0.07).toFixed(2));
  readonly total = computed(() => +(this.subtotal() + this.tax()).toFixed(2));

  setTable(n: number) { this._tableNumber.set(n); }
  setServer(name: string) { this._serverName.set(name); }

  add(item: MenuItem) {
    const lines = [...this._lines()];
    const idx = lines.findIndex((l) => l.menuItemId === item.id);
    if (idx >= 0) {
      lines[idx] = { ...lines[idx], quantity: lines[idx].quantity + 1 };
    } else {
      lines.push({
        menuItemId: item.id,
        name: item.name,
        unitPrice: item.price,
        quantity: 1,
        notes: '',
      });
    }
    this._lines.set(lines);
  }

  increment(menuItemId: string) {
    this._lines.update((lines) =>
      lines.map((l) => (l.menuItemId === menuItemId ? { ...l, quantity: l.quantity + 1 } : l))
    );
  }

  decrement(menuItemId: string) {
    this._lines.update((lines) =>
      lines
        .map((l) => (l.menuItemId === menuItemId ? { ...l, quantity: l.quantity - 1 } : l))
        .filter((l) => l.quantity > 0)
    );
  }

  remove(menuItemId: string) {
    this._lines.update((lines) => lines.filter((l) => l.menuItemId !== menuItemId));
  }

  updateNotes(menuItemId: string, notes: string) {
    this._lines.update((lines) =>
      lines.map((l) => (l.menuItemId === menuItemId ? { ...l, notes } : l))
    );
  }

  clear() { this._lines.set([]); }
}
