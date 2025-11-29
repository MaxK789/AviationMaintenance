import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// поправь порт под свой WebApi (посмотри в Rider, обычно 5001/5242 и т.п.)
const API_TARGET = 'http://localhost:5185'

export default defineConfig({
  plugins: [vue()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: API_TARGET,
        changeOrigin: true,
        secure: false
      }
    }
  }
})
