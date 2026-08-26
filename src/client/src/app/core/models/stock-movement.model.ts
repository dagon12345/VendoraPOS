export type StockMovementReason = 'InitialStock' | 'Restock' | 'Adjustment' | 'Waste' | 'Sale';

export interface StockMovement {
  id: string;
  productId: string;
  quantityDelta: number;
  reason: StockMovementReason;
  note: string | null;
  createdAtUtc: string;
}

export interface RecordStockMovementRequest {
  quantityDelta: number;
  reason: StockMovementReason;
  note?: string | null;
}
