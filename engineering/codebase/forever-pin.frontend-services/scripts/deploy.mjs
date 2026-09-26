import { cp, mkdir, rm } from "node:fs/promises";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const workspace = fileURLToPath(new URL("../", import.meta.url));
const source = new URL("../apps/web/dist/", import.meta.url);
const destination = new URL("../../forever-pin.backend-services/ForeverPin.Api/wwwroot/", import.meta.url);

// The API host serves the web app; each app keeps its own build output.
const build = spawnSync("pnpm", ["--filter", "@foreverpin/web", "build"], {
  cwd: workspace,
  stdio: "inherit",
});
if (build.error) throw build.error;
if (build.status !== 0) process.exit(build.status ?? 1);

await rm(destination, { recursive: true, force: true });
await mkdir(destination, { recursive: true });
await cp(source, destination, { recursive: true });
console.log("Copied apps/web/dist to ForeverPin.Api/wwwroot.");
