const { describe, it } = require("node:test");
const assert = require("node:assert/strict");
const { esp, fixture, findGame, withSession, withPlugin } = require("./helpers");

const tes5 = findGame("TES5");
const gamePath = tes5.detected ? tes5.gamePath : fixture();
const needsGame = { skip: !tes5.detected && "needs an installed game" };

describe("plugins", () => {
  it("lists load order masters-first", needsGame, () => {
    withSession("TES5", gamePath, session => {
      const order = JSON.parse(esp.loadOrder(session));
      const names = order.map(m => m.name);
      assert.ok(names.includes("Skyrim.esm"));
      assert.ok(order[0].name === "Skyrim.esm");
      const skyrim = order.find(m => m.name === "Skyrim.esm");
      assert.equal(skyrim.active, true);
      assert.deepEqual(skyrim.masters, []);
      for (const m of order) {
        assert.equal(typeof m.name, "string");
        assert.equal(typeof m.active, "boolean");
        assert.ok(Array.isArray(m.masters));
      }
    });
  });

  it("loads a fixture plugin", () => {
    withSession("TES5", gamePath, session => {
      withPlugin(session, fixture("AllRecords.esp"), plugin => {
        assert.equal(esp.pluginName(plugin), "AllRecords.esp");
        assert.equal(esp.pluginRecordCount(plugin), 122);
        assert.deepEqual(JSON.parse(esp.pluginMasters(plugin)), ["Skyrim.esm"]);
        assert.equal(esp.pluginLocalized(plugin), false);
        assert.equal(esp.pluginModified(plugin), false);
        assert.ok(esp.pluginPath(plugin).endsWith("AllRecords.esp"));
      });
    });
  });

  it("resolves associated archives", needsGame, () => {
    withSession("TES5", gamePath, session => {
      withPlugin(session, "Skyrim.esm", plugin => {
        const archives = JSON.parse(esp.associatedArchives(
          session, esp.pluginName(plugin)));
        assert.ok(Array.isArray(archives));
      });
    });
  });
});
