import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  build: {
    outDir: '../Vulicy.Web/wwwroot',
    emptyOutDir: true,
  },
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5165',
        changeOrigin: true,
        secure: false,
        timeout: 0,
        proxyTimeout: 0,
        agent: new (await import('http')).Agent({
          keepAlive: true,
          maxSockets: 100,
        }),
      }
    }
  }
})
