import axios from 'axios';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7287/api/v1';

export const api = axios.create({
  baseURL: apiBaseUrl,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('bookStoreToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
