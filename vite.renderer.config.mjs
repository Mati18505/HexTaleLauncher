import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite';
import path from "node:path";

// https://vitejs.dev/config
export default defineConfig({
    root: path.join(__dirname, "src", "renderer"),
    plugins: [
      react()
    ]
  });
