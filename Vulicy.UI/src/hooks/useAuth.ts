import { useState, useEffect, useCallback } from 'react';
import type { User } from '../types/feature';

const AUTH_STORAGE_KEY = 'vulicy_user';

interface UseAuthResult {
  user: User | null;
  isLoading: boolean;
  isAdmin: boolean;
  login: (returnUrl?: string) => void;
  logout: () => Promise<void>;
  clearAuthState: () => void;
}

export const useAuth = (): UseAuthResult => {
  const [user, setUser] = useState<User | null>(() => {
    try {
      const stored = localStorage.getItem(AUTH_STORAGE_KEY);
      return stored ? JSON.parse(stored) : null;
    } catch {
      return null;
    }
  });
  const [isLoading, setIsLoading] = useState(true);

  const clearAuthState = useCallback(() => {
    localStorage.removeItem(AUTH_STORAGE_KEY);
    setUser(null);
  }, []);

  const validateSession = useCallback(async () => {
    try {
      const response = await fetch('/api/auth/me');
      if (response.ok) {
        const userData: User = await response.json();
        localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(userData));
        setUser(userData);
      } else if (response.status === 401) {
        clearAuthState();
      }
    } catch (error) {
      console.error('Failed to validate session:', error);
      // Keep existing localStorage state on network error
    } finally {
      setIsLoading(false);
    }
  }, [clearAuthState]);

  useEffect(() => {
    validateSession();
  }, [validateSession]);

  const login = useCallback((returnUrl?: string) => {
    const url = returnUrl
      ? `/api/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`
      : '/api/auth/login';
    window.location.href = url;
  }, []);

  const logout = useCallback(async () => {
    clearAuthState();
    window.location.href = '/api/auth/logout';
  }, [clearAuthState]);

  return {
    user,
    isLoading,
    isAdmin: user?.isAdmin ?? false,
    login,
    logout,
    clearAuthState,
  };
};
