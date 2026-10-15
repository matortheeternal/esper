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

  it("recounts HEDR record count at save time, not removal time", () => {
    const dir = fs.mkdtempSync(path.join(os.tmpdir(), "esper-hedr-"));
    const out = path.join(dir, "AllRecords.esp");
    let before = 0;
    let beforeRecords = 0;
    try {
      withSession("TES5", gamePath, session => {
        withPlugin(session, fixture("AllRecords.esp"), plugin => {
          const hdr = esp.getChild(plugin, 0);
          try {
            before = esp.getUIntValue(hdr, "HEDR\\Number of Records");
            beforeRecords = esp.pluginRecordCount(plugin);
            const rec = esp.getRecordByFormId(plugin, 0xEB4);
            try {
              assert.equal(esp.removeElement(rec, ""), true);
            } finally {
              esp.free(rec);
            }
            assert.equal(
              esp.getUIntValue(hdr, "HEDR\\Number of Records"), before);
            assert.equal(esp.savePlugin(plugin, out), true);
            assert.equal(
              esp.getUIntValue(hdr, "HEDR\\Number of Records"), before - 1);
          } finally {
            esp.free(hdr);
          }
        });
      });
      withSession("TES5", gamePath, session => {
        withPlugin(session, out, plugin => {
          assert.equal(esp.pluginRecordCount(plugin), beforeRecords - 1);
          const hdr = esp.getChild(plugin, 0);
          try {
            assert.equal(
              esp.getUIntValue(hdr, "HEDR\\Number of Records"), before - 1);
          } finally {
            esp.free(hdr);
          }
        });
      });
    } finally {
      fs.rmSync(dir, { recursive: true });
    }
  });
});
