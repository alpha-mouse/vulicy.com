import React, { createContext, useContext, useEffect, useState } from 'react';
import * as Sentry from "@sentry/react";
import { Config } from '../types/config';
import { api } from '../utils/api';

interface ConfigContextType {
  config: Config | null;
  loading: boolean;
}

const ConfigContext = createContext<ConfigContextType | undefined>(undefined);

const LOCAL_STORAGE_KEY = 'vulicy_config';

export const ConfigProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [config, setConfig] = useState<Config | null>(() => {
    const cached = localStorage.getItem(LOCAL_STORAGE_KEY);
    return cached ? JSON.parse(cached) : null;
  });
  const [loading, setLoading] = useState(!config);

  useEffect(() => {
    const fetchConfig = async () => {
      try {
        const newConfig = await api.get<Config>('/api/config');

        setConfig(prev => {
          if (JSON.stringify(newConfig) !== JSON.stringify(prev)) {
            localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(newConfig));
            return newConfig;
          }
          return prev;
        });
      } catch (error) {
        console.error('Failed to fetch config:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchConfig();
  }, []);

  useEffect(() => {
    if (config?.sentryFeDsn) {
      Sentry.init({
        dsn: config.sentryFeDsn,
        environment: config.environment,
        sendDefaultPii: false,
      });
    }
  }, [config?.sentryFeDsn, config?.environment]);

  return (
    <ConfigContext.Provider value={{ config, loading }}>
      {children}
    </ConfigContext.Provider>
  );
};

export const useConfig = () => {
  const context = useContext(ConfigContext);
  if (context === undefined) {
    throw new Error('useConfig must be used within a ConfigProvider');
  }
  return context;
};
