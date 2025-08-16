namespace esper.elements {
    [JSExport]
    public enum CopyOptions : byte {
        AsNewRecord = 1,
        CopyChildGroups = 2,
        CopyMaster = 4,
        CopyPreviousOverride = 8,
        CopyKnownOverride = 16
    }
}
