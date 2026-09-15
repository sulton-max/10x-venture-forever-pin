import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import mkcert from "vite-plugin-mkcert";
import path from "node:path";

// ForeverPin frontend build and development server.
export default defineConfig(({ command, mode }) => ({
  // Use HTTPS for secure cookies in development; VITE_HTTPS=false opts into HTTP.
  plugins: [
    react(),
    tailwindcss(),
    ...(command === "serve" && mode !== "test" && process.env.VITE_HTTPS !== "false" ? [mkcert()] : []),
  ],
  server: {
    port: 7024,
    // Proxy to the local HTTPS API, accepting its dev certificate and preserving the browser origin.
    proxy: {
      "/api": { target: "https://localhost:7020", changeOrigin: false, secure: false },
      "/health": { target: "https://localhost:7020", changeOrigin: false, secure: false },
    },
  },
  // Prod-ish: build straight into the Api's wwwroot so the backend serves the SPA.
  build: {
    outDir: "../forever-pin.backend-services/ForeverPin.Api/wwwroot",
    emptyOutDir: true,
  },
  resolve: {
    alias: {
      "@": path.resolve(import.meta.dirname, "./src"),
    },
  },
  // Unit tests mirror the source tree.
  test: {
    include: ["tests/**/*.test.{ts,tsx}"],
  },
}));
