namespace esper.defs {
    [JSExport]
    public enum SmashType : byte {
        stUnknown,
        stRecord,
        stString,
        stInteger,
        stFlag,
        stFloat,
        stStruct,
        stUnsortedArray,
        stUnsortedStructArray,
        stSortedArray,
        stSortedStructArray,
        stByteArray,
        stUnion
    }
}
