import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";

export default defineConfig({
  plugins: [vue()],
  build: {
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (!id.includes("node_modules")) {
            return undefined;
          }

          if (id.includes("/@primeuix/")) {
            return "prime-theme";
          }

          if (id.includes("/primeicons/")) {
            return "prime-icons";
          }

          return undefined;
        },
      },
    },
  },
});
