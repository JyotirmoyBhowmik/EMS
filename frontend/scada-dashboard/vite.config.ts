import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [react()],
    resolve: {
        alias: {
            '@': path.resolve(__dirname, './src'),
        },
    },
    server: {
        port: 3000,
        proxy: {
            '/api': {
                target: process.env.REACT_APP_API_URL || 'http://localhost:5000',
                changeOrigin: true,
            },
        },
    },
    build: {
        outDir: 'build',
        sourcemap: true,
    },
})
