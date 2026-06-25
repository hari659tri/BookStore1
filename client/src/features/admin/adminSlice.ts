import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import { api } from '../../api/client';
import type { AdminUser, Order } from '../../types';

interface AdminState {
  users: AdminUser[];
  orders: Order[];
}

const initialState: AdminState = { users: [], orders: [] };

export const fetchAdminUsers = createAsyncThunk('admin/users', async () => {
  const { data } = await api.get<AdminUser[]>('/admin/users');
  return data;
});

export const fetchAdminOrders = createAsyncThunk('admin/orders', async () => {
  const { data } = await api.get<Order[]>('/admin/orders');
  return data;
});

export const updateOrderStatus = createAsyncThunk('admin/updateOrderStatus', async (payload: { id: number; status: string }) => {
  const { data } = await api.put<Order>(`/admin/orders/${payload.id}/status`, { status: payload.status });
  return data;
});

const adminSlice = createSlice({
  name: 'admin',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchAdminUsers.fulfilled, (state, action) => {
        state.users = action.payload;
      })
      .addCase(fetchAdminOrders.fulfilled, (state, action) => {
        state.orders = action.payload;
      })
      .addCase(updateOrderStatus.fulfilled, (state, action) => {
        state.orders = state.orders.map((order) => (order.id === action.payload.id ? action.payload : order));
      });
  },
});

export default adminSlice.reducer;
