/** A single store-wide number, not per-product - see product-list and stock-alerts. */
export const LOW_STOCK_THRESHOLD = 10;

/** Products expiring within this many days count as "expiring soon". */
export const EXPIRING_SOON_DAYS = 7;

export function isLowStock(quantityOnHand: number): boolean {
  return quantityOnHand <= LOW_STOCK_THRESHOLD;
}

export function daysUntil(isoDate: string): number {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const target = new Date(isoDate);
  target.setHours(0, 0, 0, 0);
  return Math.round((target.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
}

export function isExpired(isoDate: string): boolean {
  return daysUntil(isoDate) < 0;
}

export function isExpiringSoon(isoDate: string): boolean {
  const days = daysUntil(isoDate);
  return days >= 0 && days <= EXPIRING_SOON_DAYS;
}
