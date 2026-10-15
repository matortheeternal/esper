# esper-node

NativeAOT Node.js addon and test suite for esper. Replaces `js-tests`,
which bound esper through node-api-dotnet dynamic interop.

## Layout

- `esper-node.csproj` — NativeAOT shared-library project referencing
  `esper.csproj`. Publishes `esper-addon.node`.
- `Addon.cs` — module init, N-API externs, JS marshalling helpers.
- `Api.cs` — exported functions (session, plugins, records, tree,
  values, save).
- `Backend.cs` — esper logic behind the exports plus game detection.
- `roots.xml` — trim roots (definition loading uses reflection).
- `build.js`, `package.json` — build and zero-dependency test runner.
- `test/` — `node:test` suites with no dependencies.

## API

Handles are `BigInt` values referencing live esper objects. `free`
releases any handle. Errors throw JavaScript exceptions.

Session: `detectGames()`, `openSession(gameKey, gamePath)`, `free`.
Plugins: `loadOrder(session)`, `loadPlugin(session, path)`,
`pluginName`, `pluginPath`, `pluginRecordCount`, `pluginMasters`,
`pluginLocalized`, `pluginModified`, `associatedArchives(session, name)`.
Records: `getRecord(plugin, index)`, `recordFormId`, `recordEditorId`,
`recordSignature`. Tree: `childCount`, `getChild`,
`elementKind` (`file/group/record/struct/value/element`),
`elementLabel`, `elementValue`. Write: `savePlugin(plugin, path)`.

Metadata crossings (`detectGames`, `loadOrder`, `pluginMasters`,
`associatedArchives`) return JSON strings; hot paths return handles,
numbers, strings, and booleans with no serialization.

## Run

```sh
npm test            # builds for the host platform, runs all suites
npm run build -- --rid=win-x64   # cross-platform publish
```

`build.js` copies `definitions.zip` next to the addon; tests locate it
by running from the package root. Suites needing installed games skip
when none are detected (`TES5` fixtures cover the rest).

## Definitions and trimming

`definitions.zip` loads at runtime as with every esper host. Reflection
used by definition loading is preserved via `roots.xml`; the addon
itself uses no `dynamic` and no runtime reflection.
