import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import { api } from '../../api/client';
import type { Cart } from '../../types';

interface CartState {
  cart?: Cart;
  status: 'idle' | 'loading' | 'failed';
  error?: string;
}

const initialState: CartState = { status: 'idle' };

export const fetchCart = createAsyncThunk('cart/fetch', async () => {
  const { data } = await api.get<Cart>('/cart');
  return data;
});

export const addCartItem = createAsyncThunk('cart/addItem', async (payload: { bookId: number; quantity: number }) => {
  const { data } = await api.post<Cart>('/cart/items', payload);
  return data;
});

export const updateCartItem = createAsyncThunk('cart/updateItem', async (payload: { itemId: number; quantity: number }) => {
  const { data } = await api.put<Cart>(`/cart/items/${payload.itemId}`, { quantity: payload.quantity });
  return data;
});

export const removeCartItem = createAsyncThunk('cart/removeItem', async (itemId: number) => {
  const { data } = await api.delete<Cart>(`/cart/items/${itemId}`);
  return data;
});

const cartSlice = createSlice({
  name: 'cart',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchCart.fulfilled, (state, action) => {
        state.cart = action.payload;
        state.status = 'idle';
      })
      .addCase(addCartItem.fulfilled, (state, action) => {
        state.cart = action.payload;
      })
      .addCase(updateCartItem.fulfilled, (state, action) => {
        state.cart = action.payload;
      })
      .addCase(removeCartItem.fulfilled, (state, action) => {
        state.cart = action.payload;
      });
  },
});

export default cartSlice.reducer;
