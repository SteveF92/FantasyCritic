import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { fileURLToPath, URL } from 'node:url';
import vue from '@vitejs/plugin-vue';
import { BootstrapVueNextResolver } from 'bootstrap-vue-next/resolvers';
import Components from 'unplugin-vue-components/vite';
import { defineConfig } from 'vite';

// The paths the web app serves itself, which the dev server passes through to it.
const backEndPaths = ['/api', '/Account', '/updatehub', '/.well-known', '/css', '/img', '/js', '/lib', '/swagger'];

function proxyTo(target, secure) {
  return Object.fromEntries(backEndPaths.map((path) => [path, { target, changeOrigin: true, secure, ws: path === '/updatehub' }]));
}

// `npm start`, which the web app launches: HTTPS with the ASP.NET Core dev certificate that
// aspnetcore-https.js exports, on the port the web app's SpaProxyServerUrl expects.
function localServer() {
  const certificateFolder = process.env.APPDATA ? `${process.env.APPDATA}/ASP.NET/https` : `${process.env.HOME}/.aspnet/https`;
  const certificateName = process.env.npm_package_name;
  const target = process.env.ASPNETCORE_HTTPS_PORT
    ? `https://localhost:${process.env.ASPNETCORE_HTTPS_PORT}`
    : process.env.ASPNETCORE_URLS
      ? process.env.ASPNETCORE_URLS.split(';')[0]
      : 'https://localhost:4554';

  return {
    https: {
      key: readFileSync(join(certificateFolder, `${certificateName}.key`)),
      cert: readFileSync(join(certificateFolder, `${certificateName}.pem`))
    },
    port: 44477,
    strictPort: true,
    proxy: proxyTo(target, false)
  };
}

// `npm run client` and `npm run preview`: the client app on its own, against the production API.
function productionServer() {
  return {
    port: 5001,
    strictPort: true,
    proxy: proxyTo('https://www.fantasycritic.games', true)
  };
}

export default defineConfig(({ command, mode }) => ({
  plugins: [
    vue(),
    Components({
      dirs: [],
      resolvers: [BootstrapVueNextResolver()]
    })
  ],
  server: command === 'serve' && mode === 'development' ? localServer() : mode === 'client' ? productionServer() : undefined,
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  css: {
    devSourcemap: true,
    preprocessorOptions: {
      scss: {
        quietDeps: true
      }
    }
  },
  test: {
    environment: 'jsdom',
    include: ['tests/**/*.test.{js,ts}']
  }
}));
