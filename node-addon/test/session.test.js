const { describe, it } = require("node:test");
const assert = require("node:assert/strict");
const { esp, detectGames, findGame } = require("./helpers");

describe("session", () => {
  it("detects games with stable shape", () => {
    const games = detectGames();
    assert.ok(Array.isArray(games));
    assert.equal(games.length, 7);
    for (const g of games) {
      assert.equal(typeof g.key, "string");
      assert.equal(typeof g.fullName, "string");
      assert.equal(typeof g.esmName, "string");
      assert.equal(typeof g.detected, "boolean");
      assert.equal(typeof g.esmPresent, "boolean");
    }
    assert.deepEqual(
      games.map(g => g.key).sort(),
      ["FNV", "FO3", "FO4", "SF", "SSE", "TES4", "TES5"]);
  });

  it("opens and frees a session", () => {
    const tes5 = findGame("TES5");
    assert.ok(tes5.detected, "TES5 must be detected on this machine");
    const session = esp.openSession("TES5", tes5.gamePath);
    assert.equal(typeof session, "bigint");
    assert.ok(session !== 0n);
    assert.equal(esp.free(session), true);
  });

  it("rejects unknown game keys", () => {
    assert.throws(() => esp.openSession("NOPE", "/tmp"), /unknown gameKey/);
  });

  it("rejects missing game paths", () => {
    assert.throws(
      () => esp.openSession("TES5", "/tmp/definitely-not-a-game"),
      /gamePath not found/);
  });
});
