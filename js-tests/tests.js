const assert = require('node:assert/strict');
const path = require('path');
const dotnet = require('node-api-dotnet/net10.0');

dotnet.load(path.resolve(__dirname, 'esper/esper.dll'));

const Games = dotnet.esper.Games;
const Session = dotnet.esper.setup.Session;
const SessionOptions = dotnet.esper.setup.SessionOptions;
const session = new Session(Games.TES5, new SessionOptions());
const pluginPath = path.resolve(__dirname, '../fixtures/AllRecords.esp');
const plugin = session.pluginManager.LoadPlugin(pluginPath);

assert.equal(plugin.name, 'AllRecords.esp');
assert.equal(plugin.records.length, 122);
console.log(`Loaded ${plugin.name}: ${plugin.records.length} records`);
