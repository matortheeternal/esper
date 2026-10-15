const { describe, it } = require("node:test");
const assert = require("node:assert/strict");
const {
  esp, fixture, findGame, withSession, withPlugin, children, freeAll,
} = require("./helpers");

const tes5 = findGame("TES5");
const gamePath = tes5.detected ? tes5.gamePath : fixture();

function firstGroup(plugin, prefix) {
  const file = children(plugin);
  try {
    for (const group of file) {
      if (esp.elementKind(group) === "group" &&
          esp.elementLabel(group).startsWith(prefix))
        return { file, group };
    }
    assert.fail(`no ${prefix} group`);
  } catch (e) {
    freeAll(file);
    throw e;
  }
}

describe("element tree", () => {
  it("exposes files as roots with groups and records", () => {
    withSession("TES5", gamePath, session => {
      withPlugin(session, fixture("AllRecords.esp"), plugin => {
        assert.equal(esp.elementKind(plugin), "file");
        const file = children(plugin);
        try {
          assert.ok(file.length > 100);
          const kinds = new Set(file.map(g => esp.elementKind(g)));
          assert.ok(kinds.has("group"));
          assert.ok(kinds.has("record"));
          for (const g of file)
            assert.ok(esp.elementLabel(g).length > 0);
        } finally {
          freeAll(file);
        }
      });
    });
  });

  it("walks group children down to records", () => {
    withSession("TES5", gamePath, session => {
      withPlugin(session, fixture("AllRecords.esp"), plugin => {
        const { file, group } = firstGroup(plugin, "GMST");
        try {
          assert.ok(esp.childCount(group) > 0);
          const recs = children(group);
          try {
            assert.ok(recs.every(r => esp.elementKind(r) === "record"));
            const labels = recs.map(r => esp.elementLabel(r));
            assert.ok(labels.every(l => /[0-9A-F]{8}/.test(l)));
          } finally {
            freeAll(recs);
          }
        } finally {
          esp.free(group);
          freeAll(file.filter(h => h !== group));
        }
      });
    });
  });

  it("expands a record subtree", () => {
    withSession("TES5", gamePath, session => {
      withPlugin(session, fixture("AllRecords.esp"), plugin => {
        const rec = esp.getRecord(plugin, 1);
        try {
          assert.equal(esp.elementKind(rec), "record");
          assert.match(esp.recordSignature(rec), /^[A-Z0-9]{4}$/);
          assert.match(esp.recordFormId(rec).toString(16).toUpperCase().padStart(8, "0"), /^[0-9A-F]{8}$/);
          const fields = children(rec);
          try {
            assert.ok(fields.length > 0);
            const seen = new Set(fields.map(f => esp.elementKind(f)));
            assert.ok(seen.has("struct") || seen.has("value"));
          } finally {
            freeAll(fields);
          }
        } finally {
          esp.free(rec);
        }
      });
    });
  });

  it("reads value text", () => {
    withSession("TES5", gamePath, session => {
      withPlugin(session, fixture("AllRecords.esp"), plugin => {
        const rec = esp.getRecord(plugin, 0);
        try {
          const leaves = [];
          const walk = handle => {
            for (const child of children(handle)) {
              if (esp.elementKind(child) === "value") leaves.push(child);
              else if (esp.childCount(child) > 0) walk(child);
              else esp.free(child);
            }
          };
          walk(rec);
          try {
            assert.ok(leaves.length > 0);
            assert.ok(leaves.every(l => typeof esp.elementValue(l) === "string"));
          } finally {
            freeAll(leaves);
          }
        } finally {
          esp.free(rec);
        }
      });
    });
  });
});
