import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), 'VITE_')
  const API_TARGET = env.VITE_API_TARGET || 'http://localhost:5185'

  return {
    plugins: [vue()],
    server: {
      port: 5173,
      proxy: {
        '/api': {
          target: API_TARGET,
          changeOrigin: true,
          secure: false
        },
        '/hubs': {
          target: API_TARGET,
          changeOrigin: true,
          secure: false,
          ws: true
        }
      }
    }
  }
})
