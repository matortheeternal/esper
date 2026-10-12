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

### js tests

`js-tests/` smoke-tests Node interop via dynamic
[node-api-dotnet](https://github.com/microsoft/node-api-dotnet)
loading (`dotnet.load` on the `net10.0` host entry), which is the
cross-platform default.  Run:

    cd js-tests && npm install && npm test

This publishes the full `esper.dll` dependency closure and asserts a
real plugin load.  The legacy static `[JSExport]` surface is opt-in
only: build with `-p:EnableNodeApiGenerator=true` (Windows).

## fixture installation

The `SkyrimEsmTest` requires installing certain files in the fixtures folder:

- `Skyrim.esm` - copy `Skyrim.esm` from the data folder of any PC Skyrim installation to `fixtures/Skyrim.esm`
- `Dragonborn.esm` - copy `Dragonborn.esm` from the data folder of any PC Skyrim installation to `fixtures/Dragonborn.esm`
- `skyrim_strings/skyrim_english.*` - extract the `strings/skyrim_english.*` files from `Skyrim - Interface.bsa` to the `fixtures/skyrim_strings` folder.  You should have three files at the following paths if you do this correctly:
  - `fixtures/skyrim_strings/skyrim_english.dlstrings`
  - `fixtures/skyrim_strings/skyrim_english.ilstrings`
  - `fixtures/skyrim_strings/skyrim_english.strings`