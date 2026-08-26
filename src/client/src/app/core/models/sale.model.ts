export type PaymentMethod = 'Cash' | 'Card';

export interface SaleLine {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
  /** Units of this line individually returned/voided so far - a partial refund, independent of
   *  the sale's own isVoided flag. See Sale.VoidLine on the backend. */
  voidedQuantity: number;
  activeQuantity: number;
  activeLineTotal: number;
}

export interface Sale {
  id: string;
  createdAtUtc: string;
  paymentMethod: PaymentMethod;
  totalAmount: number;
  amountTendered: number | null;
  changeDue: number | null;
  isVoided: boolean;
  voidedAtUtc: string | null;
  voidReason: string | null;
  /** Sum of all per-line partial refunds (0 if none have been made). */
  refundedAmount: number;
  netTotal: number;
  lines: SaleLine[];
}

export interface SaleLineRequest {
  productId: string;
  quantity: number;
}

export interface CreateSaleRequest {
  lines: SaleLineRequest[];
  paymentMethod: PaymentMethod;
  amountTendered?: number | null;
}

export interface VoidSaleRequest {
  reason?: string | null;
}

export interface VoidLineRequest {
  productId: string;
  quantity: number;
  reason?: string | null;
}

export interface RestoreLineRequest {
  productId: string;
  quantity: number;
  reason?: string | null;
}
