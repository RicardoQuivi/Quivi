import { defineConfig } from 'tsup'

export default defineConfig({
    entry: ['src/index.ts'],
    format: ['esm', 'cjs'],
    dts: true,
    splitting: false,         // required for React Native & Node
    sourcemap: true,
    clean: true,
    // external: ['react']
})