import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import { fileURLToPath, URL } from 'node:url';
import Components from 'unplugin-vue-components/vite'
import {BootstrapVueNextResolver} from 'unplugin-vue-components/resolvers'

const target = "https://www.fantasycritic.games";

export default defineConfig({
  plugins: [
    vue(),
    Components({
      resolvers: [BootstrapVueNextResolver()],
    }),],
  server: {
    port: 5001,
    strictPort: true,
    proxy: {
      "/api": {
        target: target,
        changeOrigin: true,
      },
      "/Account": {
        target: target,
        changeOrigin: true,
      },
      "/updatehub": {
        target: target,
        ws: true,
        changeOrigin: true,
      },
      "/.well-known": {
        target: target,
        changeOrigin: true,
      },
      "/css": {
        target: target,
        changeOrigin: true,
      },
      "/img": {
        target: target,
        changeOrigin: true,
      },
      "/js": {
        target: target,
        changeOrigin: true,
      },
      "/lib": {
        target: target,
        changeOrigin: true,
      },
    }
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  css: {
    devSourcemap: true
  }
})