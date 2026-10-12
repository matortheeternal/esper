const fs = require('node:fs');
const path = require('node:path');
const { spawnSync } = require('node:child_process');

const root = path.resolve(__dirname, '..');
const output = path.join(__dirname, 'esper');
const result = spawnSync('dotnet', [
  'publish',
  path.join(root, 'esper/esper.csproj'),
  '--configuration', 'Release',
  '--output', output,
  '--nologo',
  '-p:EnableNodeApiGenerator=false',
], { stdio: 'inherit' });

if (result.error) throw result.error;
if (result.status !== 0) process.exit(result.status ?? 1);

fs.copyFileSync(
  path.join(root, 'definitions.zip'),
  path.join(__dirname, 'definitions.zip'),
);
