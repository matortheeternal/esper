const { describe, it } = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const os = require("node:os");
const path = require("node:path");
const { esp, fixture, findGame, withSession, withPlugin } = require("./helpers");

const tes5 = findGame("TES5");
const gamePath = tes5.detected ? tes5.gamePath : fixture();

describe("write", () => {
  it("round-trips byte-identical", () => {
    withSession("TES5", gamePath, session => {
      withPlugin(session, fixture("AllRecords.esp"), plugin => {
        const out = path.join(fs.mkdtempSync(
          path.join(os.tmpdir(), "esper-node-")), "AllRecords.esp");
        assert.equal(esp.savePlugin(plugin, out), true);
        assert.deepEqual(fs.readFileSync(out), fs.readFileSync(fixture("AllRecords.esp")));
        fs.rmSync(path.dirname(out), { recursive: true });
      });
    });
  });

  it("reports unmodified files clean", () => {
    withSession("TES5", gamePath, session => {
      withPlugin(session, fixture("AllRecords.esp"), plugin => {
        assert.equal(esp.pluginModified(plugin), false);
      });
    });
  });
});
