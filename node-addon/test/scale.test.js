const { describe, it } = require("node:test");
const assert = require("node:assert/strict");
const {
  esp, findGame, withSession, withPlugin, children, freeAll,
} = require("./helpers");

const tes5 = findGame("TES5");
const needsGame = { skip: !tes5.detected && "needs an installed game" };

describe("scale", needsGame, () => {
  it("loads a full master plugin", { timeout: 300000 }, () => {
    withSession("TES5", tes5.gamePath, session => {
      const started = Date.now();
      withPlugin(session, "Skyrim.esm", plugin => {
        const count = esp.pluginRecordCount(plugin);
        console.log(`    loaded ${count} records in ${Date.now() - started}ms`);
        assert.ok(count > 500000);
        const groups = children(plugin);
        const rest = [...groups];
        try {
          assert.ok(groups.length > 50);
          const at = groups.findIndex(g =>
            esp.elementKind(g) === "group" &&
            esp.elementLabel(g).startsWith("GMST"));
          assert.ok(at >= 0, "GMST group present");
          const gmst = groups[at];
          rest.splice(at, 1);
          try {
            const recs = children(gmst);
            try {
              assert.ok(recs.length > 1000);
              for (const r of recs.slice(0, 50)) {
                assert.equal(esp.elementKind(r), "record");
                assert.equal(esp.recordSignature(r), "GMST");
              }
            } finally {
              freeAll(recs);
            }
          } finally {
            esp.free(gmst);
          }
        } finally {
          freeAll(rest);
        }
      });
    });
  });
});
