# Testing notes (tribal knowledge)

How to run the esper test suite without wasting hours. Earned the hard way.

## Self-building runs only

- Always run `dotnet test` WITHOUT `--no-build`. The test DLL goes stale
  silently (source edits that "built clean" never compiled; focused runs
  then execute old code and lie to you).
- When in doubt, compare mtimes: `Tests.dll` must be newer than every
  source you touched. Rebuild explicitly if not.
- VSTest filter gotcha: `|` in `--filter` matches only the FIRST condition
  in this adapter. Use single `Name~` filters, one run per fixture.

## Verify every write and edit

- Untracked temp files (`Tmp*`, `Probe*`) evaporate in this environment.
  After writing one, verify with `ls` immediately — never trust the
  write-OK alone, and read-backs can show phantom content.
- Same for edits: `grep` the new text after every edit call. Under load,
  edits report OK without landing. Build failures are the backstop, not
  the tool result.
- Temp probe pattern: write a small NUnit fixture that scans records and
  `Console.WriteLine`s what you need, run it, grep the log, DELETE THE FILE
  before committing. Never leave probes in the tree.

## Memory discipline (the OOM crash)

- A full-plugin sweep without per-record cache clearing once OOM-crashed
  the host machine. Rule: every record walk clears
  `rec._internalElements` / `rec._unexpectedSubrecords` in a `finally`
  (see `TestHelpers.ScanRecords` / `FindRecord` — use them, don't
  hand-roll walks).
- Full-master sweeps run under a 2 GiB memory cgroup:
  `systemd-run --user --pipe --wait --collect -p MemoryMax=2G ...`
  This is not optional for Skyrim.esm-scale files.

## Fixture-first session discipline

- Three modes via `ESPER_TEST_MODE`: `fixture` (default, committed
  `fixtures/auto/*.fixture.esp`, ~8s suite), `game` (real ESMs via
  dotenv `ESPER_<GAME>_DATA` paths, ~30s+), `build` (ESM fallback +
  sidecar `fixtures/auto/<Base>.missing` for misses).
- `Session` construction parses ALL definition JSON (~1s+ each). Never
  build sessions per-test: `Synthetic.GetSession(game)` is the shared
  cache. Same for plugin loads: `Fixture.Master` / `Synthetic.GetPlugin`
  for read-only; fresh `LoadPlugin` only for tests that mutate (with
  try/finally restore, or preferably the dirty-check harness if it lands).
- After regenerating fixtures (`ESPER_GENERATE_FIXTURES=1`), REBUILD
  before running tests — the build copies fixtures into the output dir,
  and the runner reads the copies, not the repo files.
- Game ESMs live outside the repo (dotenv paths, git-ignored). Committed
  fixtures are small curated plugins; master sweeps stay env-gated and
  never run in CI.

## Coverage and status

- `scripts/coverage.sh` runs suite + coverlet + reportgenerator.
  Use `coverlet.runsettings` (esper-only; balsa/Tests excluded).
- `.ai/status.*` regenerates from the newest cobertura + trx.
  `.ai/` is git-excluded local tooling; `scripts/status-manual.json`
  is the committed counterpart.

## Conventions that bite

- No source comments in new code (reviewer policy); whitespace: 90-col
  limit, closing paren on its own line when breaking calls.
- One insight per branch, branched off `update`; never commit to
  `update`/`master` directly; never force-push.
- Real-fixture data over synthetic; authoritative oracles are game ESMs
  and xEdit sources — never lock in a guess (see the `banditB`/`BanditB`
  revision lesson: bundled and Steam masters differ).
