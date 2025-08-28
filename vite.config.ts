// vite.config.ts
import { defineConfig } from "vite";
import fable from "vite-plugin-fable";

export default defineConfig({
  plugins: [
    // Point to your fsproj. If yours is elsewhere, adjust the path.
    fable({ fsproj: "src/src.fsproj" })
  ],
});
