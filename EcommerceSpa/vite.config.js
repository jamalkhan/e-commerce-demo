import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";

function resolveApiBaseUrl(mode) {
  const env = {
    ...loadEnv(mode, process.cwd(), ""),
    ...process.env
  };

  return (
    env.API_HTTPS ||
    env.API_HTTP ||
    env["services__api__https__0"] ||
    env["services__api__http__0"] ||
    env.VITE_API_BASE_URL ||
    "https://sandbox.api.jamal.com"
  );
}

export default defineConfig(({ mode }) => ({
  plugins: [react()],
  define: {
    __API_BASE_URL__: JSON.stringify(resolveApiBaseUrl(mode))
  },
  server: {
    host: "0.0.0.0",
    port: 5173,
    strictPort: true
  }
}));
