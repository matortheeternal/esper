const native = require("./esper-addon.node");

const xelib = {
  verbose: false,
  logger: console,
  HandleGroup: null,

  elementTypes: ["etFile", "etMainRecord", "etGroupRecord", "etSubRecord",
    "etSubRecordStruct", "etSubRecordArray", "etSubRecordUnion", "etArray",
    "etStruct", "etValue", "etFlag", "etStringListTerminator", "etUnion",
    "etStructChapter"],
  defTypes: ["dtRecord", "dtSubRecord", "dtSubRecordArray", "dtSubRecordStruct",
    "dtSubRecordUnion", "dtString", "dtLString", "dtLenString", "dtByteArray",
    "dtInteger", "dtIntegerFormater", "dtIntegerFormaterUnion", "dtFlag",
    "dtFloat",     "dtArray", "dtStruct", "dtUnion", "dtResolvable", "dtEmpty", "dtStructChapter"],
  smashTypes: ["stUnknown", "stRecord", "stString", "stInteger", "stFlag",
    "stFloat", "stStruct", "stUnsortedArray", "stUnsortedStructArray",
    "stSortedArray", "stSortedStructArray", "stByteArray", "stUnion"],
  valueTypes: ["vtUnknown", "vtBytes", "vtNumber", "vtString", "vtText",
    "vtReference", "vtFlags", "vtEnum", "vtColor", "vtArray", "vtStruct"],

  games: {
    TES4: "Oblivion", FO3: "Fallout 3", FNV: "Fallout New Vegas",
    TES5: "Skyrim", SSE: "Skyrim Special Edition", FO4: "Fallout 4",
  },
};

function track(handle) {
  if (xelib.HandleGroup) xelib.HandleGroup.push(handle);
  return handle;
}

function handles(values) {
  return values.map(track);
}

Object.assign(xelib, {
  Release: id => native.free(id),

  CreateHandleGroup: () => {
    xelib.HandleGroup = [];
  },
  FreeHandleGroup: () => {
    for (const h of xelib.HandleGroup) {
      try {
        native.free(h);
      } catch (e) {
        if (xelib.verbose) xelib.logger.info(`double free skipped: ${e.message}`);
      }
    }
    xelib.HandleGroup = null;
  },
  WithHandleGroup: callback => {
    xelib.CreateHandleGroup();
    try {
      return callback();
    } finally {
      xelib.FreeHandleGroup();
    }
  },
  OutsideHandleGroup: callback => {
    const group = xelib.HandleGroup;
    xelib.HandleGroup = null;
    try {
      return callback();
    } finally {
      xelib.HandleGroup = group;
    }
  },
  WithHandle: (handle, callback) => {
    try {
      return callback(handle);
    } finally {
      native.free(handle);
    }
  },
  WithHandles: (handles, callback) => {
    try {
      return callback(...handles);
    } finally {
      for (const h of handles) native.free(h);
    }
  },
  WithEachHandle: (handleArray, callback) => handleArray.map(h =>
    xelib.WithHandle(h, () => callback(h))),

  HasElement: (id, path = "") => native.hasElement(id, path),
  GetElement: (id, path = "") => track(native.getElement(id, path)),
  GetElementEx: (id, path = "") => track(native.getElementEx(id, path)),
  GetElements: (id = 0n, path = "") => handles(native.getElements(id, path)),
  AddElement: (id, path = "") => track(native.addElement(id, path)),
  RemoveElement: (id, path = "") => native.removeElement(id, path),
  ElementCount: id => native.childCount(id),
  GetContainer: id => track(native.getContainer(id)),
  ElementEquals: (id1, id2) => id1 === id2,

  Name: id => native.elementLabel(id),
  DisplayName: id => native.elementLabel(id),
  LongName: id => native.elementLabel(id),
  Path: id => native.elementPath(id),
  Signature: id => native.getSignature(id),
  EditorID: id => native.editorID(id),
  GetValue: (id, path = "") => native.getValue(id, path),
  SetValue: (id, path, value) => native.setValue(id, path, value),
  GetIntValue: (id, path = "") => native.getIntValue(id, path),
  SetIntValue: (id, path, value) => native.setIntValue(id, path, value),
  GetUIntValue: (id, path = "") => native.getUIntValue(id, path),
  SetUIntValue: (id, path, value) => native.setUIntValue(id, path, value),
  GetFloatValue: (id, path = "") => native.getFloatValue(id, path),
  SetFloatValue: (id, path, value) => native.setFloatValue(id, path, value),
  DefType: id => xelib.defTypes[native.defType(id)],
  ElementType: id => ({
    file: "etFile", group: "etGroupRecord", record: "etMainRecord",
    struct: "etStruct", array: "etStruct", value: "etValue", element: "etUnion",
  }[native.elementKind(id)] || "etUnion"),
  GetIsModified: id => native.getIsModified(id),

  FileByName: (session, filename) => track(native.fileByName(session, filename)),
  GetFileName: id => native.pluginName(id),
  GetRecordCount: id => native.pluginRecordCount(id),
  SaveFile: (id, filePath = "") => native.savePlugin(id, filePath),

  GetRecord: (plugin, formID) => track(native.getRecordByFormId(plugin, formID)),
  GetFormID: id => native.recordFormId(id),

  LoadPlugin: (session, filename) => track(native.loadPlugin(session, filename)),
  GetLoadOrder: session => JSON.parse(native.loadOrder(session)).map(m => m.name),
  GetActivePlugins: session => JSON.parse(native.loadOrder(session))
    .filter(m => m.active).map(m => m.name),
  GetMasters: id => JSON.parse(native.pluginMasters(id)),
});

module.exports = xelib;
