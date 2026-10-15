# xEdit parity

`xelib-compat.js` maps the xEditLib script surface onto `esper-addon.node`
so existing userscript logic ports with mechanical edits. Native exports
use esper naming; the compat layer restores xelib naming, handle groups,
and enum names. Intentional divergences are listed below, not hidden.

## Covered

| xelib | native | notes |
|---|---|---|
| `HasElement` | `hasElement(h, path?)` | |
| `GetElement` | `getElement(h, path?)` | missing resolves to `0n`, no throw |
| `GetElementEx` | `getElementEx(h, path?)` | missing throws |
| `GetElements` | `getElements(h, path?)` | returns `BigInt[]`; free each member |
| `GetContainer` | `getContainer(h)` | missing resolves to `0n` |
| `AddElement` | `addElement(h, path?)` | get-or-create; returns existing when present |
| `RemoveElement` | `removeElement(h, path?)` | missing path returns `false`, no throw |
| `ElementCount` | `childCount` | pre-existing export |
| `Name`/`DisplayName`/`LongName` | `elementLabel` | pre-existing export |
| `Path` | `elementPath` | `\`-separated, same convention as xelib |
| `Signature` | `getSignature` | |
| `EditorID` | `editorID` | alias of `recordEditorId` |
| `GetValue`/`SetValue` | `getValue`/`setValue(h, path, value)` | missing reads as `""` |
| `GetIntValue`/`SetIntValue` | `getIntValue`/`setIntValue` | lossy narrowing caller's responsibility |
| `GetUIntValue`/`SetUIntValue` | `getUIntValue`/`setUIntValue` | |
| `GetFloatValue`/`SetFloatValue` | `getFloatValue`/`setFloatValue` | |
| `DefType` | `defType` | integer `XEDefType` index; compat maps to `dt*` name |
| `GetIsModified` | `getIsModified` | |
| `FileByName` | `fileByName(session, name)` | missing resolves to `0n` |
| `GetFileName`/`GetRecordCount` | `pluginName`/`pluginRecordCount` | pre-existing exports |
| `SaveFile` | `savePlugin(h, path?)` | empty path reuses the source path |
| `GetRecord` | `getRecordByFormId(plugin, formId)` | missing resolves to `0n` |
| `GetFormID` | `recordFormId` | pre-existing export |
| `LoadPlugin` | `loadPlugin(session, path)` | pre-existing export |
| `GetLoadOrder`/`GetActivePlugins` | `loadOrder(session)` | compat parses the JSON once per call |
| `GetMasters` | `pluginMasters` | pre-existing export |
| handle groups | `CreateHandleGroup`/`FreeHandleGroup`/`WithHandleGroup`/`OutsideHandleGroup`/`WithHandle`/`WithHandles`/`WithEachHandle` | double free inside group release is skipped, not fatal |
| `ElementEquals` | identity compare | handle identity, see divergences |
| `Release` | `free` | pre-existing export |

Enum tables (`elementTypes`, `defTypes`, `smashTypes`, `valueTypes`,
`games`) are present for script compatibility. `smashTypes` and
`valueTypes` have no native counterpart yet; `ElementType` maps esper
kinds onto `et*` names approximately.

## Intentional divergences

- Files are not roots. Esper parents each file under a `Root` struct,
  so `GetContainer(file)` returns that struct instead of null.
- `ElementEquals` compares handle identity. Each lookup pins a new
  handle, so two lookups of the same element compare unequal until a
  handle cache exists.
- Setup is explicit. xelib's implicit global session becomes
  `openSession`/`loadPlugin` handles; there is no `SetGameMode` or
  `LoadPlugins` emulation in the compat layer.
- `AddElement` is get-or-create and creation support varies by record
  type (structures that cannot contain the path throw instead of
  coercing).
- `savePlugin` with an empty path writes back to the source file.
  Tests always pass an explicit path so fixtures are never dirtied.
- `RemoveElement` is idempotent. A missing path returns `false`
  (ensure-absent); required elements return `false`; files and roots
  still throw.

## Not covered (future)

record creation, conflict/combine resolution,
record serialization, search/filter iteration, master-list editing,
resource extraction, and string-localization editing.
