# esper

Bethesda plugin file parsing library.  Uses [esp.json](https://github.com/matortheeternal/esp.json) definitions.  Built to offer an API similar to xEdit.

## fixture installation

The `SkyrimEsmTest` requires installing certain files in the fixtures folder:

- `Skyrim.esm` - copy `Skyrim.esm` from the data folder of any PC Skyrim installation to `fixtures/Skyrim.esm`
- `skyrim_strings/skyrim_english.*` - extract the `strings/skyrim_english.*` files from `Skyrim - Interface.bsa` to the `fixtures/skyrim_strings` folder.  You should have three files at the following paths if you do this correctly:
  - `fixtures/skyrim_strings/skyrim_english.dlstrings`
  - `fixtures/skyrim_strings/skyrim_english.ilstrings`
  - `fixtures/skyrim_strings/skyrim_english.strings`