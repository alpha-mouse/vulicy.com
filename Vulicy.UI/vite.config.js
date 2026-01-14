import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5165',
        changeOrigin: true,
        secure: false,
        timeout: 0,  // Disable proxy timeout
        proxyTimeout: 0,  // Disable proxy timeout
        // Configure HTTP agent to allow more concurrent connections
        agent: new (await import('http')).Agent({
          keepAlive: true,
          maxSockets: 100,  // Allow many concurrent requests
        }),
      }
    }
  }
})
