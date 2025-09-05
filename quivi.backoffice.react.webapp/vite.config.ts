import { defineConfig } from "vite";
import plugin from "@vitejs/plugin-react";
import svgr from "vite-plugin-svgr";
import tailwindcss from '@tailwindcss/vite'
import { VitePWA } from 'vite-plugin-pwa'

// https://vite.dev/config/
export default defineConfig({
    plugins: [
        plugin(),
        svgr({
            svgrOptions: {
                icon: true,
                // This will transform your SVG to a React component
                exportType: "named",
                namedExport: "ReactComponent",
            },
        }),
        tailwindcss(),
        VitePWA({
            registerType: 'autoUpdate',
            manifest: {
                name: 'Quivi Backoffice',
                short_name: 'Quivi Backoffice',
                start_url: '/',
                display: 'standalone',
                background_color: '#1C3A11',
                theme_color: '#1C3A11',
                icons: [
                    {
                        src: 'icons/quivi-192x192.png',
                        sizes: '192x192',
                        type: 'image/png'
                    },
                    {
                        src: 'icons/quivi-512x512.png',
                        sizes: '512x512',
                        type: 'image/png'
                    },
                    {
                        src: 'icons/quivi-maskable.png',
                        sizes: '512x512',
                        type: 'image/png',
                        purpose: 'any maskable'
                    }
                ],
                screenshots: [
                    {
                        src: 'screenshots/wide.png',
                        sizes: '1280x720',
                        form_factor: 'wide'
                    },
                    {
                        src: 'screenshots/narrow.png',
                        sizes: '540x720',
                        form_factor: 'narrow'
                    }
                ]
            }
        })
    ],
    server: {
        host: true,
        port: 3010,
        watch: {
            usePolling: true,
        }
    }
});