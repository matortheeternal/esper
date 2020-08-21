let getConditionEnum = function(element) {
  let data = xelib.GetUIntValue(element);
  let r = data & 0xE0;
  if (r == 0) return "Equal to";
  if (r == 0x20) return "Not equal to";
  if (r == 0x40) return "Greater than";
  if (r == 0x60) return "Greater than or equal to";
  if (r == 0x80) return "Less than";
  if (r == 0xA0) return "Less than or equal to";
  return "<Unknown Compare operator>";
};

let getConditionFlags = function(element) {
  let data = xelib.GetUIntValue(element);
  let r = data & 0x1F;
  let flags = [];
  if (r & 1) flags.push("Or");
  if (r & 2) flags.push("Use aliases");
  if (r & 4) flags.push("Use global");
  if (r & 8) flags.push("Use packdata");
  if (r & 16) flags.push("Swap Subject and Target");
  return flags.join(', ');
};

let formatConditionType = function(element) {
  let type = getConditionEnum(element);
  let flags = getConditionFlags(element);
  return flags != "" ? `${type} / ${flags}` : type;
};

let isConditionType = function(element) {
  if (xelib.Name(element) !== 'Type') return false;
  let container = xelib.GetContainer(element);
  return xelib.Name(container) === 'CTDA - ';
};

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

let masterNames;

let getTargetFileName = function(nativeFid) {
  let ordinal = nativeFid >> 24;
  return masterNames[ordinal];
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
    let nativeFid = xelib.GetUIntValue(element);
    if (nativeFid === 0) return '{Null:000000}';
    let filename = getTargetFileName(nativeFid);
    let localFid = nativeFid & 0xFFFFFF;
    return `{${filename}:${xelib.Hex(localFid, 6)}}`;
  } else if (isConditionType(element)) {
    return formatConditionType(element);
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
  masterNames = xelib.GetElements(plugin, 'File Header\\Master Files')
      .map(m => xelib.GetValue(m, 'MAST'));
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