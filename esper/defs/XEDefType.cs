namespace esper.defs {
    [JSExport]
    public enum XEDefType : byte {
        dtRecord,
        dtSubRecord,
        dtSubRecordArray,
        dtSubRecordUnion,
        dtString,
        dtLString,
        dtLenString,
        dtByteArray,
        dtInteger,
        dtIntegerFormater,
        dtIntegerFormaterUnion,
        dtFlag,
        dtFloat,
        dtArray,
        dtStruct,
        dtUnion,
        dtResolvable, // unused
        dtEmpty,
        dtStructChapter // unused
    }
}
