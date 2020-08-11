let serializeArray = function(array) {
  let output = [];
  xelib.GetElements(array).forEach(element => {
    output.push(serializeElement(element));
  });
  return output;
};

let serializeStruct = function(struct) {
  let output = {};
  xelib.GetElements(struct).forEach(element => {
    let path = xelib.Name(element);
    output[path] = serializeElement(element);
  });
  return output;
};

let serializeElement = function(element) {
  let vt = xelib.ValueType(element);
  if (vt === xelib.vtArray) {
    return serializeArray(element);
  } else if (vt === xelib.vtStruct) {
    return serializeStruct(element);
  } else if (vt === xelib.vtFlags) {
    return xelib.GetEnabledFlags(element).join(', ');
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
  xelib.GetElements(rec).forEach(element => {
    let path = xelib.LocalPath(element);
    output[path] = serializeElement(element);
  });
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
  fh.saveJsonFile(outputPath, output[key], true);
});