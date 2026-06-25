import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import { api } from '../../api/client';
import type { Order } from '../../types';

interface OrdersState {
  orders: Order[];
  selected?: Order;
  status: 'idle' | 'loading' | 'failed';
}

const initialState: OrdersState = { orders: [], status: 'idle' };

export const fetchOrders = createAsyncThunk('orders/fetch', async () => {
  const { data } = await api.get<Order[]>('/orders');
  return data;
});

export const placeOrder = createAsyncThunk('orders/place', async () => {
  const { data } = await api.post<Order>('/orders');
  return data;
});

const ordersSlice = createSlice({
  name: 'orders',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchOrders.fulfilled, (state, action) => {
        state.orders = action.payload;
      })
      .addCase(placeOrder.fulfilled, (state, action) => {
        state.selected = action.payload;
        state.orders.unshift(action.payload);
      });
  },
});

export default ordersSlice.reducer;
