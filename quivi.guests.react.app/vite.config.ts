import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import svgr from "vite-plugin-svgr";

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    svgr({
      svgrOptions: {
        icon: true,

        // This will transform your SVG to a React component
        exportType: "named",
        namedExport: "ReactComponent",

        svgProps: { role: 'img' },
        prettier: false,
        svgo: true,
        svgoConfig: {
          plugins: [
            { removeViewBox: false },
            { removeAttrs: { attrs: '(style|title)' } }
          ]
        }
      },
    }),
  ],
  server: {
    host: true,
    port: 3012,
    watch: {
      usePolling: true,
    }
  },
})