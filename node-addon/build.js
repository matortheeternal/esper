const { execFileSync } = require("node:child_process");
const fs = require("node:fs");
const path = require("node:path");

const ridArg = process.argv.find(a => a.startsWith("--rid="));
const rid = ridArg
  ? ridArg.slice("--rid=".length)
  : { linux: "linux-x64", win32: "win-x64", darwin: "darwin-arm64" }[process.platform]
    || `linux-${process.arch}`;
const ext = { "linux-x64": ".so", "win-x64": ".dll", "darwin-arm64": ".dylib", "darwin-x64": ".dylib" }[rid]
  || ".so";

execFileSync("dotnet", ["publish", "-c", "Release", "-r", rid, "--nologo", "-v", "q"], {
  cwd: __dirname,
  stdio: "inherit",
});
const built = path.join(__dirname, "bin", "Release", "net10.0", rid, "publish", `esper-addon${ext}`);
fs.copyFileSync(built, path.join(__dirname, "esper-addon.node"));
fs.copyFileSync(
  path.join(__dirname, "..", "definitions.zip"),
  path.join(__dirname, "definitions.zip"));
const { size } = fs.statSync(path.join(__dirname, "esper-addon.node"));
console.log(`esper-addon.node ready (${(size / 1048576).toFixed(1)} MB, ${rid}).`);
