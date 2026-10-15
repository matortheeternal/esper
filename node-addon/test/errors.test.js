const { describe, it } = require("node:test");
const assert = require("node:assert/strict");
const { esp, fixture, findGame, withSession, withPlugin } = require("./helpers");

const tes5 = findGame("TES5");
const gamePath = tes5.detected ? tes5.gamePath : fixture();

describe("errors", () => {
  it("rejects out-of-range records", () => {
    withSession("TES5", gamePath, session => {
      withPlugin(session, fixture("AllRecords.esp"), plugin => {
        assert.throws(() => esp.getRecord(plugin, 999999), /out of range/);
        assert.throws(() => esp.getRecord(plugin, -1), /out of range/);
      });
    });
  });

  it("rejects out-of-range children", () => {
    withSession("TES5", gamePath, session => {
      withPlugin(session, fixture("AllRecords.esp"), plugin => {
        assert.throws(() => esp.getChild(plugin, 999999), /out of range/);
      });
    });
  });

  it("rejects record accessors on non-records", () => {
    withSession("TES5", gamePath, session => {
      withPlugin(session, fixture("AllRecords.esp"), plugin => {
        assert.throws(() => esp.recordFormId(plugin), /not a record/);
        assert.throws(() => esp.recordEditorId(plugin), /not a record/);
        assert.throws(() => esp.recordSignature(plugin), /not a record/);
      });
    });
  });

  it("rejects missing plugins", () => {
    withSession("TES5", gamePath, session => {
      assert.throws(
        () => esp.loadPlugin(session, "DoesNotExist.esp"),
        /./);
    });
  });

  it("reports zero children for leaves", () => {
    withSession("TES5", gamePath, session => {
      withPlugin(session, fixture("AllRecords.esp"), plugin => {
        const rec = esp.getRecord(plugin, 1);
        try {
          const fields = [];
          const total = esp.childCount(rec);
          for (let i = 0; i < total; i++) fields.push(esp.getChild(rec, i));
          try {
            const value = fields.find(f => esp.elementKind(f) === "value")
              ?? fields.flatMap(f => {
                const kids = [];
                for (let i = 0; i < esp.childCount(f); i++)
                  kids.push(esp.getChild(f, i));
                return kids;
              }).find(k => esp.elementKind(k) === "value");
            assert.ok(value !== undefined);
            assert.equal(esp.childCount(value), 0);
            assert.equal(esp.elementValue(value).length > 0, true);
          } finally {
            for (const f of fields) esp.free(f);
          }
        } finally {
          esp.free(rec);
        }
      });
    });
  });
});
