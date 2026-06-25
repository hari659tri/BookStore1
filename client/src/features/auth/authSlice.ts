import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import { api } from '../../api/client';
import type { UserSession } from '../../types';

interface AuthState {
  session: UserSession | null;
  status: 'idle' | 'loading' | 'failed';
  error?: string;
}

const savedToken = localStorage.getItem('bookStoreToken');
const savedSession = localStorage.getItem('bookStoreSession');

const initialState: AuthState = {
  session: savedToken && savedSession ? JSON.parse(savedSession) : null,
  status: 'idle',
};

export const login = createAsyncThunk('auth/login', async (payload: { email: string; password: string }) => {
  const { data } = await api.post<UserSession>('/auth/login', payload);
  return data;
});

export const register = createAsyncThunk(
  'auth/register',
  async (payload: { firstName: string; lastName: string; email: string; password: string }) => {
    const { data } = await api.post<UserSession>('/auth/register', payload);
    return data;
  },
);

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    logout(state) {
      state.session = null;
      localStorage.removeItem('bookStoreToken');
      localStorage.removeItem('bookStoreSession');
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(login.pending, (state) => {
        state.status = 'loading';
        state.error = undefined;
      })
      .addCase(login.fulfilled, (state, action) => {
        state.status = 'idle';
        state.session = action.payload;
        localStorage.setItem('bookStoreToken', action.payload.token);
        localStorage.setItem('bookStoreSession', JSON.stringify(action.payload));
      })
      .addCase(login.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.error.message;
      })
      .addCase(register.fulfilled, (state, action) => {
        state.status = 'idle';
        state.session = action.payload;
        localStorage.setItem('bookStoreToken', action.payload.token);
        localStorage.setItem('bookStoreSession', JSON.stringify(action.payload));
      });
  },
});

export const { logout } = authSlice.actions;
export default authSlice.reducer;
