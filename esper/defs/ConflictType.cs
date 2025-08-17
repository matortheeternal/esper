namespace esper.defs {
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
