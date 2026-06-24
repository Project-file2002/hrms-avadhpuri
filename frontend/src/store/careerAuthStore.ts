import { create } from 'zustand';
import type { User } from '../types';
import careerApi, { clearCareerSession, loadCareerSession, saveCareerSession } from '../services/careerApi';

interface CareerAuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  register: (payload: { firstName: string; lastName: string; email: string; password: string; phone?: string }) => Promise<void>;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  loadSession: () => void;
}

export const useCareerAuthStore = create<CareerAuthState>((set) => ({
  user: null,
  token: null,
  isAuthenticated: false,

  register: async (payload) => {
    const res = await careerApi.post('/register', payload);
    saveCareerSession(res.data);
    set({ user: res.data.user, token: res.data.token, isAuthenticated: true });
  },

  login: async (email, password) => {
    const res = await careerApi.post('/login', { email, password });
    saveCareerSession(res.data);
    set({ user: res.data.user, token: res.data.token, isAuthenticated: true });
  },

  logout: () => {
    clearCareerSession();
    set({ user: null, token: null, isAuthenticated: false });
  },

  loadSession: () => {
    const { token, user } = loadCareerSession();
    if (token && user?.roles?.includes('Candidate')) {
      set({ user, token, isAuthenticated: true });
    } else {
      set({ user: null, token: null, isAuthenticated: false });
    }
  },
}));
