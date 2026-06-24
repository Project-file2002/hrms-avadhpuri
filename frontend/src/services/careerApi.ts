import axios from 'axios';
import type { LoginResponse } from '../types';

const careerApi = axios.create({
  baseURL: 'http://localhost:5096/api/careers',
  headers: { 'Content-Type': 'application/json' },
});

careerApi.interceptors.request.use((config) => {
  const token = localStorage.getItem('career_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

careerApi.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('career_token');
      localStorage.removeItem('career_user');
      window.dispatchEvent(new Event('career-auth-changed'));
    }
    return Promise.reject(error);
  }
);

export default careerApi;

export function saveCareerSession(data: LoginResponse) {
  localStorage.setItem('career_token', data.token);
  localStorage.setItem('career_user', JSON.stringify(data.user));
  window.dispatchEvent(new Event('career-auth-changed'));
}

export function clearCareerSession() {
  localStorage.removeItem('career_token');
  localStorage.removeItem('career_user');
  window.dispatchEvent(new Event('career-auth-changed'));
}

export function loadCareerSession(): { token: string | null; user: LoginResponse['user'] | null } {
  const token = localStorage.getItem('career_token');
  const userStr = localStorage.getItem('career_user');
  if (!token || !userStr) return { token: null, user: null };
  try {
    return { token, user: JSON.parse(userStr) };
  } catch {
    return { token: null, user: null };
  }
}
