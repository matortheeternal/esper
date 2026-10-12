## Summary
<!-- One insight/fix per PR. What production behavior changes, and why. -->

## Production changes
- <!-- e.g. `esper/defs/TES5/Formats/AliasFormat.cs` — what changed and the xEdit/esp.json evidence -->

## Verification
- [ ] `dotnet build esper.sln` — 6/6 projects, 0 warnings, 0 errors
- [ ] `dotnet test Tests/Tests.csproj` — passed: ___, skipped: ___, failed: 0
- [ ] Coverage re-run (only if touching deciders/formats/counters):
  `dotnet test --settings coverlet.runsettings --collect:"XPlat Code Coverage"`

## Fixture policy
- [ ] Real-fixture data preferred (local masters load with `Assert.Ignore` when missing)
- [ ] Any hand-crafted test binaries are in-memory/temp only and NOT treated as authoritative
- [ ] No new committed binary fixtures

## Reviewer notes
<!-- Anything the human reviewer should look at closely: tradeoffs, known quirks,
follow-ups deliberately left out of scope. -->
