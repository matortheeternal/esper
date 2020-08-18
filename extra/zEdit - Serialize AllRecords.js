let GetName = function(element) {
  return xelib.Name(element).replace(/Filename/g, "FileName");
};

let serializeContainer = function(container) {
  let output = [];
  xelib.GetElements(container).forEach(element => {
    let a = serializeElement(element);
    let o = { name: GetName(element) };
    if (typeof a !== 'string') {
      o.elements = a;
    } else {
      o.value = a;
    }
    output.push(o);
  });
  return output;
};

let serializeElement = function(element) {
  let vt = xelib.ValueType(element);
  if (vt === xelib.vtArray) {
    return serializeContainer(element);
  } else if (vt === xelib.vtStruct) {
    return serializeContainer(element);
  } else if (vt === xelib.vtFlags) {
    return xelib.GetEnabledFlags(element).join(', ');
  } else if (xelib.ElementCount(element) > 0) {
    return serializeContainer(element);
  } else if (vt === xelib.vtReference) {
    let localFid = xelib.GetUIntValue(element) & 0xFFFFFF;
    if (localFid === 0) return '{Null:000000}';
    let rec = xelib.GetLinksTo(element);
    let filename = rec > 0
    	? xelib.Name(xelib.GetElementFile(xelib.GetMasterRecord(rec)))
        : 'Null';
    return `{${filename}:${xelib.Hex(localFid, 6)}}`;
  }
  return xelib.GetValue(element);
};

let serializeRecord = function(rec) {
  let output = {};
  if (xelib.HasElement(rec, 'Child Group')) {
    let childGroup = xelib.GetElement(rec, 'Child Group');
    output["Child Group"] = serializeGroup(childGroup);
  }
  output.elements = serializeContainer(rec);
  return output;
};

let serializeGroup = function(group) {
  let output = {};
  xelib.GetElements(group).forEach(element => {
    let et = xelib.ElementType(element);
    if (et === xelib.etMainRecord) {
      let fid = xelib.GetHexFormID(element, true);
      output[fid] = serializeRecord(element);
    } else if (et === xelib.etGroupRecord) {
      let key = xelib.Name(element);
      output[key] = serializeGroup(element);
    }
  });
  return output;
};

let serializePlugin = function(plugin) {
  let output = {};
  xelib.GetElements(plugin).forEach(element => {
    let et = xelib.ElementType(element);
    console.log(et);
    if (et === xelib.etMainRecord) {
      output["File Header"] = serializeRecord(element);
    } else if (et === xelib.etGroupRecord) {
      let label = xelib.Signature(element);
      if (label === 'NAVI') return;
      output[label] = serializeGroup(element);
    }
  });
  return output;
};

let targetPlugin = xelib.FileByName('AllRecords.esp');
let output = serializePlugin(targetPlugin);
fh.jetpack.dir('AllRecords.esp');
let folderPath = fh.jetpack.path('AllRecords.esp');
Object.keys(output).forEach(key => {
  let filename = `${key}.json`;
  let outputPath = fh.jetpack.path(folderPath, filename);
  fh.saveJsonFile(outputPath, output[key]);
});