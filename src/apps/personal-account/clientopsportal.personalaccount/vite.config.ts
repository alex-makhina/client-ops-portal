import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import { resolve } from 'path'

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    envDir: resolve(__dirname, '../../'),
    server: {
        port: 62000,
    }
})
