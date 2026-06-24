import axios from 'axios';
import { logError } from '../utils/errorLogger';

const api = axios.create({
  baseURL: 'http://localhost:5096/api',
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const url = error.config?.url ?? 'unknown';
    const status = error.response?.status;
    const method = error.config?.method?.toUpperCase() ?? 'UNKNOWN';

    logError(
      `${method} ${url} failed (${status}): ${error.message}`,
      'API',
      error,
      {
        url,
        method,
        status,
        responseData: error.response?.data,
      }
    );

    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      if (!window.location.pathname.startsWith('/careers')) {
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

export default api;
