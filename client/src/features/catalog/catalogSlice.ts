import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import { api } from '../../api/client';
import type { Author, Book, Category, PagedResult } from '../../types';

interface CatalogState {
  books?: PagedResult<Book>;
  authors: Author[];
  categories: Category[];
  selectedBook?: Book;
  status: 'idle' | 'loading' | 'failed';
  error?: string;
}

const initialState: CatalogState = {
  authors: [],
  categories: [],
  status: 'idle',
};

export const fetchBooks = createAsyncThunk('catalog/fetchBooks', async (params?: Record<string, string | number | boolean | undefined>) => {
  const { data } = await api.get<PagedResult<Book>>('/books', { params });
  return data;
});

export const fetchBook = createAsyncThunk('catalog/fetchBook', async (id: number) => {
  const { data } = await api.get<Book>(`/books/${id}`);
  return data;
});

export const fetchCatalogLookups = createAsyncThunk('catalog/fetchLookups', async () => {
  const [authors, categories] = await Promise.all([api.get<Author[]>('/authors'), api.get<Category[]>('/categories')]);
  return { authors: authors.data, categories: categories.data };
});

const catalogSlice = createSlice({
  name: 'catalog',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchBooks.pending, (state) => {
        state.status = 'loading';
      })
      .addCase(fetchBooks.fulfilled, (state, action) => {
        state.status = 'idle';
        state.books = action.payload;
      })
      .addCase(fetchBooks.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.error.message;
      })
      .addCase(fetchBook.fulfilled, (state, action) => {
        state.selectedBook = action.payload;
      })
      .addCase(fetchCatalogLookups.fulfilled, (state, action) => {
        state.authors = action.payload.authors;
        state.categories = action.payload.categories;
      });
  },
});

export default catalogSlice.reducer;
