# esper

Bethesda plugin file parsing library.  Uses [esp.json](https://github.com/matortheeternal/esp.json) definitions.  Built to offer an API similar to xEdit.

## testing

Requires the .NET 10 SDK.  Run the managed suite with
`dotnet test Tests/Tests.csproj`.

### fixture policy

Only game ESMs and Creation Kit ESPs are authoritative oracles.
Hand-crafted synthetic fixtures exercise parser/round-trip behavior
but are never treated as truth about real record layouts.

Tests that need proprietary masters not present locally skip with a
clear `Assert.Ignore` naming the missing file instead of failing.

### master decode sweeps

Exhaustive per-record decode sweeps over local masters are opt-in:

    ESPER_RUN_MASTER_TESTS=1 dotnet test Tests/Tests.csproj

They decode every record in every available master and report
per-signature counts plus preserved unexpected subrecords.

### code coverage

Coverage uses coverlet.  Collect a Cobertura report with:

    dotnet test Tests/Tests.csproj --collect:"XPlat Code Coverage"

The report is written under `TestResults/<guid>/coverage.cobertura.xml`.

### node tests

`node-addon/` builds esper as a NativeAOT Node.js addon
(`esper-addon.node`) and exercises it through `node:test` suites with
no dependencies. Run:

    cd node-addon && npm test

This publishes the addon for the host platform and asserts session,
load order, plugin, tree, value, write, and scale coverage against
real plugin data.

## fixture installation

The `SkyrimEsmTest` requires installing certain files in the fixtures folder:

- `Skyrim.esm` - copy `Skyrim.esm` from the data folder of any PC Skyrim installation to `fixtures/Skyrim.esm`
- `Dragonborn.esm` - copy `Dragonborn.esm` from the data folder of any PC Skyrim installation to `fixtures/Dragonborn.esm`
- `skyrim_strings/skyrim_english.*` - extract the `strings/skyrim_english.*` files from `Skyrim - Interface.bsa` to the `fixtures/skyrim_strings` folder.  You should have three files at the following paths if you do this correctly:
  - `fixtures/skyrim_strings/skyrim_english.dlstrings`
  - `fixtures/skyrim_strings/skyrim_english.ilstrings`
  - `fixtures/skyrim_strings/skyrim_english.strings`

### Local game data folders (optional)

Tests resolve game masters in this order: the `ESPER_<GAME>_DATA` folder
(`TES4`, `FO3`, `FNV`, `TES5`, `SSE`, `FO4`, `SF`), then `fixtures/`.
Copy `.env.example` to `.env` and set the folders you have, e.g.
`ESPER_TES5_DATA=/mnt/elements/SteamLibrary/.../Skyrim/Data`. Real
environment variables take precedence over `.env`. `.env` is git-ignored;
only `.env.example` is committed. Tests that need a missing master skip
themselves with the filename.

### Test modes (default: fixture)

`ESPER_TEST_MODE` selects how tests access game masters:

- unset (default): `fixture` — tests use the small checked-in plugins in
  `fixtures/auto/` (`<Base>.fixture.esp`, mastered to the real ESM name).
  Absent masters auto-materialize as in-session dummies, and missing
  records resolve through them via record injection (see below).
- `game`: tests load the real ESMs from the folders above (or `fixtures/`).
- `build`: like `game`, but each fixture-missing record is appended to
  `fixtures/auto/<Base>.missing`; run the gated generator test with
  `ESPER_GENERATE_FIXTURES=1` to bake them into `fixtures/auto/`.

Record injection (xEdit semantics): a record whose file-ID slot has no
loaded master registers into that slot's plugin as an injected record, and
`GetRecordByFormId` falls back to it after the exact search misses. This is
what lets mastered fixture plugins resolve references against dummy
masters. Disable per session with
`new SessionOptions { allowInjections = false }`.