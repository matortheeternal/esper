const { describe, it } = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const os = require("node:os");
const path = require("node:path");
const {
  esp, fixture, findGame, withSession, withPlugin, children, freeAll,
} = require("./helpers");

const tes5 = findGame("TES5");
const gamePath = tes5.detected ? tes5.gamePath : fixture();
const NPC_FID = 0xEB4;

function withNpc(fn) {
  withSession("TES5", gamePath, session => {
    withPlugin(session, fixture("AllRecords.esp"), plugin => {
      const rec = esp.getRecordByFormId(plugin, NPC_FID);
      try {
        fn({ session, plugin, rec });
      } finally {
        esp.free(rec);
      }
    });
  });
}

function valueLeaves(handle, cap = 200) {
  const found = [];
  const walk = h => {
    for (const child of children(h)) {
      if (found.length >= cap) {
        esp.free(child);
        continue;
      }
      if (esp.elementKind(child) === "value") found.push(child);
      else if (esp.childCount(child) > 0) {
        walk(child);
        esp.free(child);
      } else esp.free(child);
    }
  };
  walk(handle);
  return found;
}

describe("xedit parity: navigation", () => {
  it("resolves elements by path", () => {
    withNpc(({ rec }) => {
      assert.equal(esp.hasElement(rec, "EDID"), true);
      assert.equal(esp.hasElement(rec, "ZZZZ-Nothing"), false);
      const el = esp.getElement(rec, "EDID");
      try {
        assert.match(esp.elementLabel(el), /EDID/);
      } finally {
        esp.free(el);
      }
      const self = esp.getElement(rec);
      try {
        assert.equal(esp.elementKind(self), "record");
      } finally {
        esp.free(self);
      }
    });
  });

  it("resolves throwing paths and lists children", () => {
    withNpc(({ rec }) => {
      const el = esp.getElementEx(rec, "EDID");
      try {
        assert.match(esp.elementLabel(el), /EDID/);
      } finally {
        esp.free(el);
      }
      assert.throws(() => esp.getElementEx(rec, "ZZZZ-Nothing"));
      const kids = esp.getElements(rec);
      try {
        assert.equal(kids.length, esp.childCount(rec));
        assert.ok(kids.every(k => typeof k === "bigint"));
        assert.ok(kids.length > 0);
      } finally {
        freeAll(kids);
      }
    });
  });

  it("navigates to containers and reports paths", () => {
    withNpc(({ plugin, rec }) => {
      const group = esp.getContainer(rec);
      try {
        assert.match(esp.elementLabel(group), /Non-Player/);
      } finally {
        esp.free(group);
      }
      const root = esp.getContainer(plugin);
      try {
        assert.equal(esp.elementKind(root), "struct");
        assert.equal(esp.elementLabel(root), "Root");
      } finally {
        esp.free(root);
      }
      assert.match(esp.elementPath(rec), /AllRecords\.esp/);
    });
  });

  it("gets or creates subrecords idempotently", () => {
    withNpc(({ rec }) => {
      const before = esp.childCount(rec);
      const el = esp.addElement(rec, "EDID");
      try {
        assert.match(esp.elementLabel(el), /EDID/);
      } finally {
        esp.free(el);
      }
      assert.equal(esp.childCount(rec), before);
    });
  });
});

describe("xedit parity: identity", () => {
  it("reports editor ids, signatures, and form ids", () => {
    withNpc(({ rec }) => {
      assert.equal(esp.editorID(rec), "dunTransmogrifyDremora");
      assert.equal(esp.getSignature(rec), "NPC_");
      assert.equal(esp.recordFormId(rec), NPC_FID);
      assert.equal(esp.getIsModified(rec), false);
      assert.equal(esp.defType(rec), 0);
    });
  });

  it("finds loaded files by name", () => {
    withNpc(({ session, plugin }) => {
      const found = esp.fileByName(session, "AllRecords.esp");
      try {
        assert.equal(esp.elementLabel(found), esp.elementLabel(plugin));
      } finally {
        esp.free(found);
      }
      assert.equal(esp.fileByName(session, "Nope.esp"), 0n);
    });
  });
});

describe("xedit parity: values", () => {
  it("round-trips string values", () => {
    withNpc(({ rec }) => {
      const current = esp.getValue(rec, "EDID");
      assert.equal(current, "dunTransmogrifyDremora");
      assert.equal(esp.setValue(rec, "EDID", current), true);
      assert.equal(esp.getValue(rec, "EDID"), current);
    });
  });

  it("round-trips typed values", () => {
    withNpc(({ rec }) => {
      const leaves = valueLeaves(rec);
      try {
        const readers = [
          [esp.getIntValue, esp.setIntValue],
          [esp.getUIntValue, esp.setUIntValue],
          [esp.getFloatValue, esp.setFloatValue],
        ];
        let covered = 0;
        for (const [get, set] of readers) {
          const leaf = leaves.find(l => {
            try {
              get(l, "");
              return true;
            } catch {
              return false;
            }
          });
          if (!leaf) continue;
          const value = get(leaf, "");
          assert.equal(typeof value, "number");
          assert.equal(set(leaf, "", value), true);
          assert.equal(get(leaf, ""), value);
          covered++;
        }
        assert.ok(covered > 0);
      } finally {
        freeAll(leaves);
      }
    });
  });

  it("saves to an explicit path byte-identical", () => {
    withNpc(({ plugin }) => {
      const out = path.join(fs.mkdtempSync(
        path.join(os.tmpdir(), "esper-parity-")), "AllRecords.esp");
      try {
        assert.equal(esp.savePlugin(plugin, out), true);
        assert.deepEqual(
          fs.readFileSync(out), fs.readFileSync(fixture("AllRecords.esp")));
      } finally {
        fs.rmSync(path.dirname(out), { recursive: true });
      }
    });
  });
});

describe("xedit parity: removal", () => {
  it("removes optional subrecords and marks the tree modified", () => {
    withNpc(({ rec, plugin }) => {
      assert.equal(esp.removeElement(rec, "EDID"), true);
      assert.equal(esp.hasElement(rec, "EDID"), false);
      assert.equal(esp.getIsModified(rec), true);
      assert.equal(esp.getIsModified(plugin), true);
    });
  });

  it("is idempotent for missing elements", () => {
    withNpc(({ rec }) => {
      assert.equal(esp.removeElement(rec, "ZZZZ-Nothing"), false);
      assert.equal(esp.removeElement(rec, "EDID"), true);
      assert.equal(esp.removeElement(rec, "EDID"), false);
    });
  });

  it("refuses required elements without throwing", () => {
    withNpc(({ rec }) => {
      const before = esp.childCount(rec);
      assert.equal(esp.removeElement(rec, "ACBS"), false);
      assert.equal(esp.hasElement(rec, "ACBS"), true);
      assert.equal(esp.childCount(rec), before);
    });
  });

  it("refuses files and roots", () => {
    withNpc(({ plugin }) => {
      assert.throws(() => esp.removeElement(plugin, ""));
      const root = esp.getContainer(plugin);
      try {
        assert.throws(() => esp.removeElement(root, ""));
      } finally {
        esp.free(root);
      }
    });
  });

  it("evicts removed records from the plugin index", () => {
    withNpc(({ plugin }) => {
      const before = esp.pluginRecordCount(plugin);
      const rec = esp.getRecordByFormId(plugin, NPC_FID);
      try {
        assert.equal(esp.removeElement(rec, ""), true);
      } finally {
        esp.free(rec);
      }
      assert.equal(esp.pluginRecordCount(plugin), before - 1);
      assert.equal(esp.getRecordByFormId(plugin, NPC_FID), 0n);
      assert.equal(esp.getIsModified(plugin), true);
    });
  });

  it("round-trips a removed record through save and reload", () => {
    const dir = fs.mkdtempSync(path.join(os.tmpdir(), "esper-remove-"));
    const out = path.join(dir, "AllRecords.esp");
    let before = 0;
    try {
      withSession("TES5", gamePath, session => {
        withPlugin(session, fixture("AllRecords.esp"), plugin => {
          before = esp.pluginRecordCount(plugin);
          const rec = esp.getRecordByFormId(plugin, NPC_FID);
          try {
            assert.equal(esp.removeElement(rec, ""), true);
          } finally {
            esp.free(rec);
          }
          assert.equal(esp.savePlugin(plugin, out), true);
        });
      });
      withSession("TES5", gamePath, session => {
        withPlugin(session, out, plugin => {
          assert.equal(esp.pluginRecordCount(plugin), before - 1);
          assert.equal(esp.getRecordByFormId(plugin, NPC_FID), 0n);
          const again = path.join(dir, "AllRecords2.esp");
          assert.equal(esp.savePlugin(plugin, again), true);
          assert.deepEqual(fs.readFileSync(again), fs.readFileSync(out));
        });
      });
    } finally {
      fs.rmSync(dir, { recursive: true });
    }
  });
});
