export interface CreateOrderItem {
    productId: string;
    productName: string;
    unitPrice: number;
    quantity: number;
}

export interface CreateOrderRequest {
    customerId: string;
    items: CreateOrderItem[];
}

export interface OrderItemResponse {
    productId: string;
    productName: string;
    unitPrice: number;
    quantity: number;
    subtotal: number;
}

export interface OrderResponse {
    id: string;
    customerId: string;
    status: 'Pending' | 'Processing' | 'Approved' | 'Failed' | 'Canceled';
    totalAmount: number;
    createdAt: string;
    updatedAt: string | null;
    items: OrderItemResponse[];
}