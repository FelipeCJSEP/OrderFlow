import axios from 'axios';
import type { CreateOrderRequest, OrderResponse } from './types';

const api = axios.create({
    baseURL: 'https://localhost:7000/api',
    headers: {
        'Content-Type': 'application/json',
    }
});

export const createOrder = async (payload: CreateOrderRequest): Promise<{ id: string }> => {
    const response = await api.post<{ id: string }>('/Orders', payload);
    return response.data;
};

export const getOrderById = async (id: string): Promise<OrderResponse> => {
    const response = await api.get<OrderResponse>(`/Orders/${id}`);
    return response.data;
};