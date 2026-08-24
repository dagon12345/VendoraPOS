export interface Product {
  id: string;
  sku: string;
  name: string;
  description: string | null;
  price: number;
  quantityOnHand: number;
  isActive: boolean;
}

export interface CreateProductRequest {
  sku: string;
  name: string;
  price: number;
  initialQuantity: number;
  description?: string | null;
}

export interface UpdateProductRequest {
  name: string;
  price: number;
  description?: string | null;
}

export type ProductAuditAction = 'Edited' | 'Activated' | 'Deactivated';

export interface ProductAuditLog {
  id: string;
  productId: string;
  action: ProductAuditAction;
  summary: string;
  createdAtUtc: string;
}
