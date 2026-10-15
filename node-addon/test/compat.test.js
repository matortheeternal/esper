const { describe, it } = require("node:test");
const assert = require("node:assert/strict");
const xelib = require("../xelib-compat");
const { esp, fixture, findGame } = require("./helpers");

const tes5 = findGame("TES5");
const gamePath = tes5.detected ? tes5.gamePath : fixture();

function openAllRecords() {
  const session = esp.openSession("TES5", gamePath);
  const plugin = esp.loadPlugin(session, fixture("AllRecords.esp"));
  return { session, plugin };
}

function closeAll({ session, plugin }) {
  esp.free(plugin);
  esp.free(session);
}

describe("xelib-compat: handle groups", () => {
  it("tracks and frees handles as a group", () => {
    const { session, plugin } = openAllRecords();
    try {
      xelib.WithHandleGroup(() => {
        const rec = xelib.GetRecord(plugin, 0xEB4);
        xelib.GetElement(rec, "EDID");
        assert.ok(xelib.HandleGroup.length >= 2);
        return esp.editorID(rec);
      });
      assert.equal(xelib.HandleGroup, null);
    } finally {
      closeAll({ session, plugin });
    }
  });

  it("lets handles escape the group", () => {
    const { session, plugin } = openAllRecords();
    try {
      let kept = 0n;
      xelib.WithHandleGroup(() => {
        kept = xelib.OutsideHandleGroup(() =>
          xelib.GetRecord(plugin, 0xEB4));
        assert.equal(xelib.HandleGroup.length, 0);
      });
      try {
        assert.equal(xelib.EditorID(kept), "dunTransmogrifyDremora");
      } finally {
        xelib.Release(kept);
      }
    } finally {
      closeAll({ session, plugin });
    }
  });

  it("compares handle identity, not element identity", () => {
    const { session, plugin } = openAllRecords();
    try {
      const a = esp.getRecordByFormId(plugin, 0xEB4);
      const b = esp.getRecordByFormId(plugin, 0xEB4);
      try {
        assert.equal(xelib.ElementEquals(a, a), true);
        assert.equal(xelib.ElementEquals(a, b), false);
      } finally {
        esp.free(a);
        esp.free(b);
      }
    } finally {
      closeAll({ session, plugin });
    }
  });
});

describe("xelib-compat: element surface", () => {
  it("mirrors names, values, and types", () => {
    const { session, plugin } = openAllRecords();
    try {
      xelib.WithHandleGroup(() => {
        const rec = xelib.GetRecord(plugin, 0xEB4);
        assert.equal(xelib.EditorID(rec), "dunTransmogrifyDremora");
        assert.equal(xelib.Signature(rec), "NPC_");
        assert.equal(xelib.GetValue(rec, "EDID"), "dunTransmogrifyDremora");
        assert.equal(xelib.DefType(rec), "dtRecord");
        assert.equal(xelib.ElementType(rec), "etMainRecord");
        assert.equal(xelib.GetIsModified(rec), false);
        assert.equal(xelib.GetFormID(rec), 0xEB4);
        assert.match(xelib.Path(rec), /AllRecords\.esp/);
        const container = xelib.GetContainer(rec);
        assert.match(xelib.Name(container), /Non-Player/);
        const kids = xelib.GetElements(rec);
        assert.equal(kids.length, xelib.ElementCount(rec));
        const seen = xelib.WithEachHandle(kids.slice(0, 3),
          h => xelib.Name(h));
        assert.equal(seen.length, Math.min(3, kids.length));
      });
    } finally {
      closeAll({ session, plugin });
    }
  });

  it("mirrors file and session helpers", () => {
    const { session, plugin } = openAllRecords();
    try {
      xelib.WithHandleGroup(() => {
        const found = xelib.FileByName(session, "AllRecords.esp");
        assert.equal(xelib.GetFileName(found), "AllRecords.esp");
        assert.equal(xelib.GetRecordCount(found), xelib.GetRecordCount(plugin));
        const order = xelib.GetLoadOrder(session);
        assert.ok(order.includes("Skyrim.esm"));
      });
    } finally {
      closeAll({ session, plugin });
    }
  });

  it("removes elements idempotently", () => {
    const { session, plugin } = openAllRecords();
    try {
      const rec = xelib.GetRecord(plugin, 0xEB4);
      try {
        assert.equal(xelib.RemoveElement(rec, "ZZZZ-Nothing"), false);
        assert.equal(xelib.RemoveElement(rec, "EDID"), true);
        assert.equal(xelib.RemoveElement(rec, "EDID"), false);
        assert.equal(xelib.HasElement(rec, "EDID"), false);
      } finally {
        xelib.Release(rec);
      }
    } finally {
      closeAll({ session, plugin });
    }
  });
});
