import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite';
import path from "node:path";
import viteHtmlResolveAlias from 'vite-plugin-html-resolve-alias';

// https://vitejs.dev/config
export default defineConfig({
    root: path.join(__dirname, "src", "renderer"),
    plugins: [
      react(),
      viteHtmlResolveAlias()
    ],
    resolve: {
      alias: {
          '@': path.resolve(__dirname, 'src'),
          '@img': path.resolve(__dirname, 'src/img/')
      }
    },
    build: {
      assetsDir: "assets",
      emptyOutDir: true,
      rollupOptions: {
        output: {
            entryFileNames: "${assetDir}/js/[name]-[hash].js",
            assetFileNames: "${assetDir}/[name]-[hash][extname]",
            chunkFileNames: "${assetDir}/js/[name]-[hash]-chunk.js"
        }
      }
    }
  });
