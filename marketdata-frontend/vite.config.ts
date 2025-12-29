import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import type { ViteUserConfigExport } from "vitest/config";

export default defineConfig({
  plugins: [react()],
  test: {
    environment: "jsdom",
    setupFiles: "./src/setupTests.ts",
    globals: true
  }
} satisfies ViteUserConfigExport);

