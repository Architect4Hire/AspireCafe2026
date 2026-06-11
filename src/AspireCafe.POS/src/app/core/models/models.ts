export interface MenuItem {
  id: string;
  name: string;
  description: string;
  price: number;
  category: string;
  imageUrl: string;
  isAvailable: boolean;
  prepTimeMinutes: number;
}

export interface CartLine {
  menuItemId: string;
  name: string;
  unitPrice: number;
  quantity: number;
  notes: string;
}

export interface OrderItem {
  id: string;
  menuItemId: string;
  name: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
  notes: string;
}

export interface Order {
  id: string;
  tableNumber: number;
  serverName: string;
  status: string;
  subtotal: number;
  taxAmount: number;
  total: number;
  createdUtc: string;
  updatedUtc: string;
  items: OrderItem[];
}

export interface OrderSubmit {
  tableNumber: number;
  serverName: string;
  items: {
    menuItemId: string;
    name: string;
    unitPrice: number;
    quantity: number;
    notes: string;
  }[];
}

export interface TipOption {
  percent: number;
  amount: number;
  label: string;
}

export interface TipSuggestion {
  subtotal: number;
  options: TipOption[];
}

export interface Payment {
  id: string;
  orderId: string;
  tableNumber: number;
  subtotal: number;
  taxAmount: number;
  tipAmount: number;
  tipPercent: number;
  total: number;
  method: string;
  status: string;
  last4: string;
  authorizationCode: string;
  createdUtc: string;
}

export interface PaymentSubmit {
  orderId: string;
  tableNumber: number;
  subtotal: number;
  taxAmount: number;
  tipPercent?: number;
  tipAmount?: number;
  method: string;
  last4: string;
}
