let recordViewNodes = zedit.GetRecordViewNodes();
let conflictClassExpr = /^c[ta]-.*/;
let conflictSet = {
  ct: xelib.conflictthis,
  ca: xelib.conflictAll
};

let getConflictType = function(item) {
  let classes = item.class.split(' ');
  for (let k of classes) {
    let match = k.match(conflictClassExpr);
    if (!match) continue;
    return match[0].toCamelCase();
  }
  return null;
};

let output = recordViewNodes.filter(node => {
  return node.handles.find(n => n);
}).map(node => {
  let cellConflicts = node.cells.slice(1).map(cell => {
    return getConflictType(cell) || 'ctUnknown';
  });
  let rowConflict = getConflictType(node) || 'caUnknown';
  return { label: node.label, rowConflict, cellConflicts };
});

fh.saveJsonFile('conflicts.json', output);