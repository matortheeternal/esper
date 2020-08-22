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

let oldCoverFlags = [
  "Edge 0-1 wall", 
  "Edge 0-1 ledge cover", 
  "Unknown 3", 
  "Unknown 4", 
  "Edge 0-1 left", 
  "Edge 0-1 right", 
  "Edge 1-2 wall", 
  "Edge 1-2 ledge cover", 
  "Unknown 9", 
  "Unknown 10", 
  "Edge 1-2 left", 
  "Edge 1-2 right", 
  "Unknown 13", 
  "Unknown 14", 
  "Unknown 15", 
  "Unknown 16"
];

let coverFlags = [
  "Edge 0-1 Cover Value 1/4",
  "Edge 0-1 Cover Value 2/4",
  "Edge 0-1 Cover Value 3/4",
  "Edge 0-1 Cover Value 4/4",
  "Edge 0-1 Left",
  "Edge 0-1 Right",
  "Edge 1-2 Cover Value 1/4",
  "Edge 1-2 Cover Value 2/4",
  "Edge 1-2 Cover Value 3/4",
  "Edge 1-2 Cover Value 4/4",
  "Edge 1-2 Left",
  "Edge 1-2 Right",
  "Unknown 13",
  "Unknown 14",
  "Unknown 15",
  "Unknown 16"
];

let formatCoverFlags = function(element) {
  let flags = xelib.GetEnabledFlags(element);
  return flags.map(str => {
    return coverFlags[oldCoverFlags.indexOf(str)];  
  }).join(', ');
};

let isCoverFlags = function(element) {
  return xelib.Name(element) === 'Cover Flags';
};

let formatVTXTCellPosition = function(element) {
  let value = xelib.GetValue(element);
  let n = Math.floor(value / 17);
  let m = value % 17;
  return `${value} -> ${n}:${m}`;
};

let isVTXTCellPosition = function(element) {
  if (xelib.Name(element) !== 'Position') return false;
  return xelib.Name(xelib.GetContainer(element)) === 'Cell';
};

let landFlags = [
  "Vertex Normals / Height Map",
  "Vertex Colours",
  "Layers",
  "Unknown 4",
  "Unknown 5",
  "",
  "",
  "",
  "",
  "",
  "MPCD"
];

let formatLandFlags = function(element) {
  let data = xelib.GetUIntValue(element);
  console.log(data);
  let flags = [];
  for (let i = 0; i < 32; i++) {
    let n = Math.floor((32 - i - 1) / 8) * 8 + i % 8;
    if ((data & Math.pow(2, i)) !== 0) 
        flags.push(landFlags[n] || '');
  }
  return flags.join(', ');
};

let isLandFlags = function(element) {
  if (xelib.Name(element) !== 'DATA - Unknown') return false;
  let sig = xelib.Signature(xelib.GetContainer(element));
  return sig === 'LAND';
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
    if (isCoverFlags(element)) {
      return formatCoverFlags(element);
    } else {
      return xelib.GetEnabledFlags(element).join(', ');
    }
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
  } else if (isVTXTCellPosition(element)) {
  	return formatVTXTCellPosition(element);
  } else if (isLandFlags(element)) { 
    return formatLandFlags(element);
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