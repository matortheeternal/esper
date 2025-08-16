namespace esper.defs {
    [JSExport]
    public enum ConflictType : byte {
        Ignore,
        BenignIfAdded,
        Benign,
        Override,
        Translate,
        Normal,
        NormalIgnoreEmpty,
        Critical,
        FormID
    }
}
