const path = require("node:path");

process.chdir(path.join(__dirname, ".."));

const esp = require("../esper-addon.node");

const root = __dirname;
const fixture = (...parts) => path.join(
  root, "..", "..", "fixtures", ...parts);

function detectGames() {
  return JSON.parse(esp.detectGames());
}

function findGame(key) {
  return detectGames().find(g => g.key === key);
}

function openSession(gameKey, gamePath) {
  return esp.openSession(gameKey, gamePath);
}

function withSession(gameKey, gamePath, fn) {
  const session = openSession(gameKey, gamePath);
  try {
    return fn(session);
  } finally {
    esp.free(session);
  }
}

function loadPlugin(session, file) {
  return esp.loadPlugin(session, file);
}

function withPlugin(session, file, fn) {
  const plugin = loadPlugin(session, file);
  try {
    return fn(plugin);
  } finally {
    esp.free(plugin);
  }
}

function children(handle) {
  const out = [];
  const total = esp.childCount(handle);
  for (let i = 0; i < total; i++)
    out.push(esp.getChild(handle, i));
  return out;
}

function freeAll(handles) {
  for (const h of handles) esp.free(h);
}

module.exports = {
  esp, fixture, detectGames, findGame,
  openSession, withSession, loadPlugin, withPlugin,
  children, freeAll,
};
