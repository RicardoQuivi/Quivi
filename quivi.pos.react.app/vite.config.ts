import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import svgr from "vite-plugin-svgr";
import path from 'path';

// https://vitejs.dev/config/
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
    ],
    server: {
        port: 3011,
    },
    resolve: {
        alias: {
            ui: path.resolve(__dirname, '../Quivi.Pos.React.Core/src'),
        },
    },
})
